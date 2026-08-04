using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SeyirMobil.Api.Data;
using SeyirMobil.Api.Models;
using SeyirMobil.Api.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SeyirMobilDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SeyirMobilDb")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

const string WebClientCorsPolicy = "WebClient";
builder.Services.AddCors(options =>
{
    options.AddPolicy(WebClientCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Auth altyapisi (2026-08-04): login/session/token sistemi. Mevcut arac-hareketleri/vehicles
// endpoint'leri BILINCLI olarak kilitlenmiyor - istemciler (desktop/web) login ekranina
// kavusana kadar oldugu gibi calismaya devam etsinler diye. Sadece /api/auth ve /api/users
// (kullanici yonetimi) korumali.
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["SecretKey"]!));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwtSigningKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(WebClientCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

// Ilk admin kullanicisini seed et (Users tablosu bossa) - boylece login sistemi
// tazeden kurulan bir veritabaninda da elle SQL yazmadan calisir durumda olur.
using (var seedScope = app.Services.CreateScope())
{
    var seedDb = seedScope.ServiceProvider.GetRequiredService<SeyirMobilDbContext>();
    if (!await seedDb.Users.AnyAsync())
    {
        seedDb.Users.Add(new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = "Admin"
        });
        await seedDb.SaveChangesAsync();
    }
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
// Siralama: tarihe gore (en yeni ustte), ayni tarihte birden fazla kayit varsa Id kucuk olan ustte.
app.MapGet("/api/arac-hareketleri", async (SeyirMobilDbContext db) =>
    await db.AracHareketleri
        .OrderByDescending(h => h.VeriTarihi)
        .ThenBy(h => h.Id)
        .ToListAsync())
    .WithName("GetAracHareketleri");

// Bir plaka + tarih icin en yakin onceki/sonraki okumayi (ve ayni tarihte kayit olup olmadigini)
// donuyor - masaustu, yeni kayit eklerken km sayaci icin gecerli araligi buradan hesapliyor.
app.MapGet("/api/arac-hareketleri/sinirlar", async (string plaka, DateOnly tarih, SeyirMobilDbContext db) =>
{
    var ayniTarihVarMi = await db.AracHareketleri
        .AnyAsync(h => h.AracPlaka == plaka && h.VeriTarihi == tarih);

    var onceki = await db.AracHareketleri
        .Where(h => h.AracPlaka == plaka && h.VeriTarihi < tarih)
        .OrderByDescending(h => h.VeriTarihi)
        .FirstOrDefaultAsync();

    var sonraki = await db.AracHareketleri
        .Where(h => h.AracPlaka == plaka && h.VeriTarihi > tarih)
        .OrderBy(h => h.VeriTarihi)
        .FirstOrDefaultAsync();

    return Results.Ok(new AracHareketSinirlar(
        ayniTarihVarMi,
        onceki?.VeriTarihi, onceki?.KmSayaci,
        sonraki?.VeriTarihi, sonraki?.KmSayaci));
})
.WithName("GetAracHareketSinirlari");

// Yeni bir arac hareketi (okuma) ekler - km sayacinin, ayni plakanin en yakin onceki/sonraki
// okumalari arasinda (KESIN sinirlarla, esit degil) kalmasi sunucu tarafinda da dogrulanir -
// client tarafindaki NumericUpDown sinirlamasi sadece UX kolayligi, gercek kaynak burasi.
app.MapPost("/api/arac-hareketleri", async (CreateAracHareketRequest request, SeyirMobilDbContext db) =>
{
    if (request.Hiz < 0 || request.Hiz > 300)
    {
        return Results.BadRequest(new { message = "Hız 0-300 aralığında olmalı." });
    }

    var ayniTarihVarMi = await db.AracHareketleri
        .AnyAsync(h => h.AracPlaka == request.AracPlaka && h.VeriTarihi == request.VeriTarihi);
    if (ayniTarihVarMi)
    {
        return Results.BadRequest(new { message = "Bu plaka için bu tarihte zaten bir kayıt var." });
    }

    var onceki = await db.AracHareketleri
        .Where(h => h.AracPlaka == request.AracPlaka && h.VeriTarihi < request.VeriTarihi)
        .OrderByDescending(h => h.VeriTarihi)
        .FirstOrDefaultAsync();

    var sonraki = await db.AracHareketleri
        .Where(h => h.AracPlaka == request.AracPlaka && h.VeriTarihi > request.VeriTarihi)
        .OrderBy(h => h.VeriTarihi)
        .FirstOrDefaultAsync();

    if (onceki is not null && request.KmSayaci <= onceki.KmSayaci)
    {
        return Results.BadRequest(new
        {
            message = $"Km sayacı, {onceki.VeriTarihi:dd.MM.yyyy} tarihli {onceki.KmSayaci:N2} km değerinden büyük olmalı."
        });
    }
    if (sonraki is not null && request.KmSayaci >= sonraki.KmSayaci)
    {
        return Results.BadRequest(new
        {
            message = $"Km sayacı, {sonraki.VeriTarihi:dd.MM.yyyy} tarihli {sonraki.KmSayaci:N2} km değerinden küçük olmalı."
        });
    }
    if (request.KmSayaci < 0)
    {
        return Results.BadRequest(new { message = "Km sayacı negatif olamaz." });
    }

    var hareket = new AracHareket
    {
        AracId = request.AracId,
        AracPlaka = request.AracPlaka,
        VeriTarihi = request.VeriTarihi,
        Hiz = request.Hiz,
        KmSayaci = request.KmSayaci
    };
    db.AracHareketleri.Add(hareket);
    await db.SaveChangesAsync();
    return Results.Created($"/api/arac-hareketleri/{hareket.Id}", hareket);
})
.WithName("CreateAracHareket");

app.MapDelete("/api/arac-hareketleri/{id:int}", async (int id, SeyirMobilDbContext db) =>
{
    var hareket = await db.AracHareketleri.FindAsync(id);
    if (hareket is null)
    {
        return Results.NotFound();
    }
    db.AracHareketleri.Remove(hareket);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("DeleteAracHareket");

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
    Results.Ok(await HesaplaOzetRaporAsync(request.Plakalar, request.Baslangic, request.Bitis, db)))
    .WithName("GetAracHareketRaporuToplu");

// Detayli rapor: secilen plaka+tarih araligindaki HER gercek okumayi, bir
// onceki okumaya gore km farkiyla birlikte donuyor. Interpolasyon YOK -
// mevcut veri granularitesi aynen kullaniliyor (kullanici onayli mantik,
// 2026-08-04).
app.MapPost("/api/arac-hareketleri/rapor-detay", async (RaporTopluRequest request, SeyirMobilDbContext db) =>
    Results.Ok(await HesaplaDetayRaporAsync(request.Plakalar, request.Baslangic, request.Bitis, db)))
    .WithName("GetAracHareketDetayRaporu");

// Ana liste export'u (2026-08-04) - istemci ekranda o an gosterdigi (filtreli
// olabilir) satirlari gonderir, backend sadece tek tablolu bir .xlsx uretir.
app.MapPost("/api/arac-hareketleri/export", (List<AracHareketExportSatiri> satirlar) =>
{
    using var workbook = new XLWorkbook();
    var sheet = workbook.Worksheets.Add("Araç Hareketleri");

    var satirNo = YazKolonBasliklari(sheet, 1, ["Araç ID", "Araç Plaka", "Veri Tarihi", "Hız", "Km Sayacı"]);
    foreach (var s in satirlar)
    {
        sheet.Cell(satirNo, 1).Value = s.AracId;
        sheet.Cell(satirNo, 2).Value = s.AracPlaka;
        YazTarihHucresi(sheet.Cell(satirNo, 3), s.VeriTarihi);
        sheet.Cell(satirNo, 4).Value = s.Hiz;
        YazKmHucresi(sheet.Cell(satirNo, 5), s.KmSayaci);
        satirNo++;
    }

    sheet.Columns().AdjustToContents();
    return ExcelDosyasiSonucu(workbook, "arac-hareketleri.xlsx");
})
.WithName("ExportAracHareketleri");

// Rapor export'u (2026-08-04) - ozet/detayli VE ayri-plaka-bazli/tumu-birlesik
// modlarinin 4 kombinasyonunu da uretir. Rapor hesabi ayni
// HesaplaOzetRaporAsync/HesaplaDetayRaporAsync fonksiyonlarini kullanir -
// ekrandaki rapor ile export arasinda mantik SAPMASI olmaz.
app.MapPost("/api/arac-hareketleri/rapor-export", async (RaporExportRequest request, SeyirMobilDbContext db) =>
{
    using var workbook = new XLWorkbook();
    var sheet = workbook.Worksheets.Add("Rapor");
    var satirNo = 1;

    if (request.DetayliMi)
    {
        var detaySonuclar = await HesaplaDetayRaporAsync(request.Plakalar, request.Baslangic, request.Bitis, db);
        string[] basliklar = ["Araç Plaka", "Veri Tarihi", "Km Sayacı", "Bir Önceki Okumaya Göre Artış"];

        void YazDetaySatirlari(IEnumerable<AracHareketDetayRaporSatiri> kaynak)
        {
            foreach (var s in kaynak)
            {
                sheet.Cell(satirNo, 1).Value = s.AracPlaka;
                YazTarihHucresi(sheet.Cell(satirNo, 2), s.VeriTarihi);
                YazKmHucresi(sheet.Cell(satirNo, 3), s.KmSayaci);
                if (s.Artis.HasValue)
                {
                    YazKmHucresi(sheet.Cell(satirNo, 4), s.Artis.Value);
                }
                else
                {
                    sheet.Cell(satirNo, 4).Value = "-";
                }
                satirNo++;
            }
        }

        if (request.AyriPlakaBazliMi)
        {
            foreach (var plaka in request.Plakalar)
            {
                satirNo = YazPlakaBasligi(sheet, satirNo, plaka, basliklar.Length);
                satirNo = YazKolonBasliklari(sheet, satirNo, basliklar);
                YazDetaySatirlari(detaySonuclar.Where(s => s.AracPlaka == plaka));
                satirNo++; // bloklar arasi bos satir
            }
        }
        else
        {
            satirNo = YazKolonBasliklari(sheet, satirNo, basliklar);
            YazDetaySatirlari(detaySonuclar);
        }
    }
    else
    {
        var ozetSonuclar = await HesaplaOzetRaporAsync(request.Plakalar, request.Baslangic, request.Bitis, db);
        string[] basliklar = ["Araç Plakası", "Başlangıç Km", "Bitiş Km", "Yapılan Km"];

        void YazOzetSatirlari(IEnumerable<AracRaporSonucu> kaynak)
        {
            foreach (var s in kaynak)
            {
                sheet.Cell(satirNo, 1).Value = s.AracPlaka;
                if (s.BulunduMu)
                {
                    YazKmHucresi(sheet.Cell(satirNo, 2), s.BaslangicKm!.Value);
                    YazKmHucresi(sheet.Cell(satirNo, 3), s.BitisKm!.Value);
                    YazKmHucresi(sheet.Cell(satirNo, 4), s.YapilanKm!.Value);
                }
                else
                {
                    sheet.Cell(satirNo, 2).Value = "Veri yok";
                    sheet.Cell(satirNo, 3).Value = "Veri yok";
                    sheet.Cell(satirNo, 4).Value = "Veri yok";
                }
                satirNo++;
            }
        }

        if (request.AyriPlakaBazliMi)
        {
            foreach (var s in ozetSonuclar)
            {
                satirNo = YazPlakaBasligi(sheet, satirNo, s.AracPlaka, basliklar.Length);
                satirNo = YazKolonBasliklari(sheet, satirNo, basliklar);
                YazOzetSatirlari([s]);
                satirNo++;
            }
        }
        else
        {
            satirNo = YazKolonBasliklari(sheet, satirNo, basliklar);
            YazOzetSatirlari(ozetSonuclar);
        }
    }

    sheet.Columns().AdjustToContents();
    return ExcelDosyasiSonucu(workbook, "rapor.xlsx");
})
.WithName("ExportRapor");

// ---------- Auth (login/session/token) ----------

app.MapPost("/api/auth/login", async (LoginRequest request, SeyirMobilDbContext db, IConfiguration config) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
    if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
    {
        return Results.Unauthorized();
    }

    var (token, expiresAt) = JwtOlustur(user, config);
    return Results.Ok(new LoginResponse(token, expiresAt, user.Username, user.Role));
})
.WithName("Login");

app.MapGet("/api/auth/me", (ClaimsPrincipal principal) =>
    Results.Ok(new
    {
        username = principal.Identity?.Name,
        role = principal.FindFirstValue(ClaimTypes.Role)
    }))
.RequireAuthorization()
.WithName("GetCurrentUser");

// ---------- Kullanici yonetimi (Admin rolu zorunlu - self servis kayit YOK, kasitli) ----------

app.MapGet("/api/users", async (SeyirMobilDbContext db) =>
    await db.Users
        .OrderBy(u => u.Username)
        .Select(u => new UserSummary(u.Id, u.Username, u.Role, u.OlusturmaTarihi))
        .ToListAsync())
.RequireAuthorization(policy => policy.RequireRole("Admin"))
.WithName("GetUsers");

app.MapPost("/api/users", async (CreateUserRequest request, SeyirMobilDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Username))
    {
        return Results.BadRequest(new { message = "Kullanıcı adı boş olamaz." });
    }
    if (request.Password.Length < 6)
    {
        return Results.BadRequest(new { message = "Şifre en az 6 karakter olmalı." });
    }
    if (request.Role != "Admin" && request.Role != "Viewer")
    {
        return Results.BadRequest(new { message = "Rol 'Admin' veya 'Viewer' olmalı." });
    }

    var kullaniciAdiVarMi = await db.Users.AnyAsync(u => u.Username == request.Username);
    if (kullaniciAdiVarMi)
    {
        return Results.BadRequest(new { message = "Bu kullanıcı adı zaten kayıtlı." });
    }

    var user = new User
    {
        Username = request.Username,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        Role = request.Role
    };
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/api/users/{user.Id}", new UserSummary(user.Id, user.Username, user.Role, user.OlusturmaTarihi));
})
.RequireAuthorization(policy => policy.RequireRole("Admin"))
.WithName("CreateUser");

app.MapDelete("/api/users/{id:int}", async (int id, SeyirMobilDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null)
    {
        return Results.NotFound();
    }
    db.Users.Remove(user);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.RequireAuthorization(policy => policy.RequireRole("Admin"))
.WithName("DeleteUser");

app.Run();

// ---------- JWT uretimi (login endpoint'i kullanir) ----------

static (string Token, DateTime ExpiresAt) JwtOlustur(User user, IConfiguration config)
{
    var jwtSection = config.GetSection("Jwt");
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["SecretKey"]!));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var expiresAt = DateTime.UtcNow.AddMinutes(double.Parse(jwtSection["ExpiryMinutes"]!));

    var claims = new[]
    {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role)
    };

    var token = new JwtSecurityToken(
        issuer: jwtSection["Issuer"],
        audience: jwtSection["Audience"],
        claims: claims,
        expires: expiresAt,
        signingCredentials: creds);

    return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
}

// ---------- Rapor hesaplama (rapor-toplu/rapor-detay VE export endpoint'leri ortak kullanir) ----------

static async Task<List<AracRaporSonucu>> HesaplaOzetRaporAsync(List<string> plakalar, DateOnly baslangic, DateOnly bitis, SeyirMobilDbContext db)
{
    var sonuclar = new List<AracRaporSonucu>();

    foreach (var plaka in plakalar)
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

    return sonuclar;
}

static async Task<List<AracHareketDetayRaporSatiri>> HesaplaDetayRaporAsync(List<string> plakalar, DateOnly baslangic, DateOnly bitis, SeyirMobilDbContext db)
{
    var sonuc = new List<AracHareketDetayRaporSatiri>();

    foreach (var plaka in plakalar)
    {
        var okumalar = await db.AracHareketleri
            .Where(h => h.AracPlaka == plaka && h.VeriTarihi >= baslangic && h.VeriTarihi <= bitis)
            .OrderBy(h => h.VeriTarihi)
            .ToListAsync();

        decimal? oncekiKm = null;
        foreach (var okuma in okumalar)
        {
            var artis = oncekiKm.HasValue ? okuma.KmSayaci - oncekiKm.Value : (decimal?)null;
            sonuc.Add(new AracHareketDetayRaporSatiri(plaka, okuma.VeriTarihi, okuma.KmSayaci, artis));
            oncekiKm = okuma.KmSayaci;
        }
    }

    return sonuc;
}

// ---------- Excel yazma yardımcıları (ClosedXML) ----------

static int YazKolonBasliklari(IXLWorksheet sheet, int satirNo, IReadOnlyList<string> basliklar)
{
    for (var i = 0; i < basliklar.Count; i++)
    {
        var hucre = sheet.Cell(satirNo, i + 1);
        hucre.Value = basliklar[i];
        hucre.Style.Font.Bold = true;
        hucre.Style.Fill.BackgroundColor = XLColor.FromHtml("#F3F4F6");
    }
    return satirNo + 1;
}

// Plaka adini, kolon sayisi kadar hucreyi birlestirerek kalin bir "bölüm başlığı" olarak yazar.
static int YazPlakaBasligi(IXLWorksheet sheet, int satirNo, string plaka, int kolonSayisi)
{
    var hucre = sheet.Cell(satirNo, 1);
    hucre.Value = plaka;
    hucre.Style.Font.Bold = true;
    hucre.Style.Font.FontSize = 13;
    hucre.Style.Fill.BackgroundColor = XLColor.FromHtml("#DBEAFE");
    sheet.Range(satirNo, 1, satirNo, kolonSayisi).Merge();
    return satirNo + 1;
}

static void YazTarihHucresi(IXLCell hucre, DateOnly tarih)
{
    hucre.Value = tarih.ToDateTime(TimeOnly.MinValue);
    hucre.Style.DateFormat.Format = "dd.MM.yyyy";
}

static void YazKmHucresi(IXLCell hucre, decimal km)
{
    hucre.Value = km;
    hucre.Style.NumberFormat.Format = "#,##0.00";
}

static IResult ExcelDosyasiSonucu(XLWorkbook workbook, string dosyaAdi)
{
    using var stream = new MemoryStream();
    workbook.SaveAs(stream);
    return Results.File(
        stream.ToArray(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        dosyaAdi);
}

record CreateVehicleRequest(string Plaka, decimal TotalKm);
