using Microsoft.EntityFrameworkCore;
using SeyirMobil.Api.Data;
using SeyirMobil.Api.Models;

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
    var vehicle = new Vehicle
    {
        Plaka = request.Plaka,
        TotalKm = request.TotalKm,
        KayitTrh = DateTime.Now
    };
    db.Vehicles.Add(vehicle);
    await db.SaveChangesAsync();
    return Results.Created($"/api/vehicles/{vehicle.AracId}", vehicle);
})
.WithName("CreateVehicle");

app.Run();

record CreateVehicleRequest(string Plaka, decimal TotalKm);
