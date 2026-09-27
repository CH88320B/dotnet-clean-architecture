using CleanArch.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Infrastructure.Persistence;

public static class DbInitializer
{
    /// <summary>Applies pending migrations (or creates the schema) and seeds demo data on an empty database.</summary>
    public static async Task InitializeAsync(AppDbContext db, CancellationToken ct = default)
    {
        if (db.Database.GetMigrations().Any())
        {
            await db.Database.MigrateAsync(ct);
        }
        else
        {
            await db.Database.EnsureCreatedAsync(ct);
        }

        if (await db.Categories.AnyAsync(ct))
        {
            return;
        }

        var peripherals = new Category("Peripherals");
        var monitors = new Category("Monitors");
        db.Categories.AddRange(peripherals, monitors);
        db.Products.AddRange(
            new Product("KB-001", "Mechanical Keyboard", 89.90m, 25, peripherals.Id, "Hot-swappable, RGB"),
            new Product("MS-001", "Wireless Mouse", 39.50m, 60, peripherals.Id),
            new Product("MN-027", "27\" 4K Monitor", 329.00m, 8, monitors.Id, "IPS, 60Hz, USB-C"),
            new Product("MN-034", "34\" Ultrawide Monitor", 549.00m, 0, monitors.Id));

        await db.SaveChangesAsync(ct);
    }
}
