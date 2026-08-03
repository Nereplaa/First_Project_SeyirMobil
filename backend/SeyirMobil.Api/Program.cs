using Microsoft.EntityFrameworkCore;
using SeyirMobil.Api.Data;
using SeyirMobil.Api.Models;
using SeyirMobil.Api.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SeyirMobilDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SeyirMobilDb")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/vehicles", async (SeyirMobilDbContext db) =>
    await db.Vehicles.OrderBy(v => v.AracId).ToListAsync())
    .WithName("GetVehicles");

app.MapGet("/api/vehicles/{id:int}", async (int id, SeyirMobilDbContext db) =>
    await db.Vehicles.FindAsync(id) is Vehicle vehicle
        ? Results.Ok(vehicle)
        : Results.NotFound())
    .WithName("GetVehicleById");

app.MapPost("/api/vehicles", async (CreateVehicleRequest request, SeyirMobilDbContext db) =>
{
    if (!PlakaValidator.IsValid(request.Plaka))
    {
        return Results.BadRequest(new { message = "Geçersiz plaka formatı. Örnek: 34ABC123 (il kodu 01-81 + 1-3 harf + rakam)." });
    }

    var vehicle = new Vehicle
    {
        Plaka = PlakaValidator.Normalize(request.Plaka),
        TotalKm = request.TotalKm,
        KayitTrh = DateTime.Now
    };
    db.Vehicles.Add(vehicle);
    await db.SaveChangesAsync();
    return Results.Created($"/api/vehicles/{vehicle.AracId}", vehicle);
})
.WithName("CreateVehicle");

app.MapDelete("/api/vehicles/{id:int}", async (int id, SeyirMobilDbContext db) =>
{
    var vehicle = await db.Vehicles.FindAsync(id);
    if (vehicle is null)
    {
        return Results.NotFound();
    }
    db.Vehicles.Remove(vehicle);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("DeleteVehicle");

// Tarih aralığı raporu: verilen plaka + [baslangic, bitis] araliginda ilk ve son km sayaci
// okumasi bulunur, farklari "yapilan km" olarak donulur. Filtreleme/siralama EF Core LINQ
// uzerinden SQL Server'a birakiliyor (SELECT TOP 1 ... ORDER BY) - butun okumalari C#'a
// cekip bellekte aramak yerine, sadece 2 satir istemciye donuyor.
app.MapGet("/api/arac-hareketleri/rapor", async (string plaka, DateOnly baslangic, DateOnly bitis, SeyirMobilDbContext db) =>
{
    var baslangicKayit = await db.AracHareketleri
        .Where(h => h.AracPlaka == plaka && h.VeriTarihi >= baslangic)
        .OrderBy(h => h.VeriTarihi)
        .FirstOrDefaultAsync();

    var bitisKayit = await db.AracHareketleri
        .Where(h => h.AracPlaka == plaka && h.VeriTarihi <= bitis)
        .OrderByDescending(h => h.VeriTarihi)
        .FirstOrDefaultAsync();

    if (baslangicKayit is null || bitisKayit is null)
    {
        return Results.NotFound(new { message = "Bu plaka ve tarih aralığında veri bulunamadı." });
    }

    return Results.Ok(new
    {
        aracPlaka = plaka,
        baslangicTarihi = baslangicKayit.VeriTarihi,
        baslangicKm = baslangicKayit.KmSayaci,
        bitisTarihi = bitisKayit.VeriTarihi,
        bitisKm = bitisKayit.KmSayaci,
        yapilanKm = bitisKayit.KmSayaci - baslangicKayit.KmSayaci
    });
})
.WithName("GetAracHareketRaporu");

// Ana ekranin listesi: tum arac hareketlerini (butun okumalari) donuyor.
app.MapGet("/api/arac-hareketleri", async (SeyirMobilDbContext db) =>
    await db.AracHareketleri
        .OrderBy(h => h.AracId)
        .ThenBy(h => h.VeriTarihi)
        .ToListAsync())
    .WithName("GetAracHareketleri");

// Masaustu rapor ekranindaki plaka secim listesi icin: her aracin benzersiz AracId'si +
// plakasi. Ayni AracId birden cok satirda gectigi icin Distinct() ile tekillestiriliyor.
app.MapGet("/api/arac-hareketleri/plakalar", async (SeyirMobilDbContext db) =>
{
    // EF Core, Select+Distinct+OrderBy zincirini ozel bir record tipine dogrudan SQL'e
    // ceviremiyor (bilinen sinirlama) - once anonim tipe projekte edip SQL'de calistiriyoruz,
    // isimli AracPlakaLookup'a donusturme sorgudan SONRA (bellekte, LINQ to Objects) yapiliyor.
    var plakalar = await db.AracHareketleri
        .Select(h => new { h.AracId, h.AracPlaka })
        .Distinct()
        .OrderBy(p => p.AracPlaka)
        .ToListAsync();

    var sonuc = plakalar.Select(p => new AracPlakaLookup(p.AracId, p.AracPlaka));
    return Results.Ok(sonuc);
})
.WithName("GetAracPlakalari");

// Coklu plaka secimi icin toplu rapor - her plaka icin ayni "ilk/son okuma farki" mantigi
// tekrarlanir, tek bir istekte hepsi donulur (masaustunun N kere API'ye gitmesi yerine).
app.MapPost("/api/arac-hareketleri/rapor-toplu", async (RaporTopluRequest request, SeyirMobilDbContext db) =>
{
    var sonuclar = new List<AracRaporSonucu>();

    foreach (var plaka in request.Plakalar)
    {
        var baslangicKayit = await db.AracHareketleri
            .Where(h => h.AracPlaka == plaka && h.VeriTarihi >= request.Baslangic)
            .OrderBy(h => h.VeriTarihi)
            .FirstOrDefaultAsync();

        var bitisKayit = await db.AracHareketleri
            .Where(h => h.AracPlaka == plaka && h.VeriTarihi <= request.Bitis)
            .OrderByDescending(h => h.VeriTarihi)
            .FirstOrDefaultAsync();

        if (baslangicKayit is null || bitisKayit is null)
        {
            sonuclar.Add(new AracRaporSonucu(plaka, false, null, null, null, null, null));
            continue;
        }

        sonuclar.Add(new AracRaporSonucu(
            plaka,
            true,
            baslangicKayit.VeriTarihi,
            baslangicKayit.KmSayaci,
            bitisKayit.VeriTarihi,
            bitisKayit.KmSayaci,
            bitisKayit.KmSayaci - baslangicKayit.KmSayaci));
    }

    return Results.Ok(sonuclar);
})
.WithName("GetAracHareketRaporuToplu");

app.Run();

record CreateVehicleRequest(string Plaka, decimal TotalKm);
