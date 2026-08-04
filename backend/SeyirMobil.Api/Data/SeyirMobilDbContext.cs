using Microsoft.EntityFrameworkCore;
using SeyirMobil.Api.Models;

namespace SeyirMobil.Api.Data;

public class SeyirMobilDbContext : DbContext
{
    public SeyirMobilDbContext(DbContextOptions<SeyirMobilDbContext> options) : base(options) { }

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<AracHareket> AracHareketleri => Set<AracHareket>();
    public DbSet<User> Users => Set<User>();
}
