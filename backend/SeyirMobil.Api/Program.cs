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

app.Run();

record CreateVehicleRequest(string Plaka, decimal TotalKm);
