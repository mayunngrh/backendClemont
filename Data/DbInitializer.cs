using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ClemontLoan.Api.Models;

namespace ClemontLoan.Api.Data;

public static class DbInitializer
{
    public const string DemoUserId = "usr-clemont-001";
    public const string DemoUserEmail = "staff@clemont.test";
    public const string DemoUserFullName = "Budi Santoso";
    public const string DemoUserPassword = "Belajar123!";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Ensure database created / migrated
        await context.Database.MigrateAsync();

        // 1. Seed demo user
        var user = await userManager.FindByEmailAsync(DemoUserEmail);
        if (user == null)
        {
            user = new ApplicationUser
            {
                Id = DemoUserId,
                UserName = DemoUserEmail,
                Email = DemoUserEmail,
                EmailConfirmed = true,
                FullName = DemoUserFullName
            };

            var createResult = await userManager.CreateAsync(user, DemoUserPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                throw new Exception($"Gagal membuat demo user: {errors}");
            }
        }

        // 2. Seed 5 initial loan applications belonging to demo user (idempotent)
        var initialApplications = new List<LoanApplication>
        {
            new()
            {
                Id = "la-seed-001",
                ApplicationNumber = "LN-20261025-001",
                FullName = "Siti Rahmawati",
                NationalId = "3201234567890001",
                Phone = "081234567890",
                Email = "siti.rahmawati@contoh.id",
                Amount = 15000000m,
                TenorMonths = 12,
                Purpose = "Modal tambahan untuk pengembangan toko sembako dan etalase frozen food",
                Status = "submitted",
                CreatedAt = DateTime.Parse("2026-10-25T08:30:00.000Z").ToUniversalTime(),
                CreatedByUserId = user.Id
            },
            new()
            {
                Id = "la-seed-002",
                ApplicationNumber = "LN-20261025-002",
                FullName = "Ahmad Fauzi",
                NationalId = "3171012345670005",
                Phone = "082198765432",
                Email = "ahmad.fauzi@contoh.id",
                Amount = 35000000m,
                TenorMonths = 24,
                Purpose = "Pengadaan mesin fotokopi digital dan peralatan cetak offset untuk usaha percetakan",
                Status = "submitted",
                CreatedAt = DateTime.Parse("2026-10-25T09:15:00.000Z").ToUniversalTime(),
                CreatedByUserId = user.Id
            },
            new()
            {
                Id = "la-seed-003",
                ApplicationNumber = "LN-20261025-003",
                FullName = "Dewi Lestari",
                NationalId = "3374023456780002",
                Phone = "085712345678",
                Email = "dewi.lestari@contoh.id",
                Amount = 50000000m,
                TenorMonths = 36,
                Purpose = "Renovasi ruko dan penambahan stok bahan baku konveksi seragam sekolah",
                Status = "submitted",
                CreatedAt = DateTime.Parse("2026-10-25T10:00:00.000Z").ToUniversalTime(),
                CreatedByUserId = user.Id
            },
            new()
            {
                Id = "la-seed-004",
                ApplicationNumber = "LN-20261025-004",
                FullName = "Bambang Prasetyo",
                NationalId = "3578034567890003",
                Phone = "081398761234",
                Email = "bambang.prasetyo@contoh.id",
                Amount = 8000000m,
                TenorMonths = 6,
                Purpose = "Pembelian laptop kerja dan perlengkapan studio desain grafis lepas",
                Status = "submitted",
                CreatedAt = DateTime.Parse("2026-10-25T11:20:00.000Z").ToUniversalTime(),
                CreatedByUserId = user.Id
            },
            new()
            {
                Id = "la-seed-005",
                ApplicationNumber = "LN-20261025-005",
                FullName = "Rina Kusuma",
                NationalId = "5171045678900004",
                Phone = "087812349876",
                Email = "rina.kusuma@contoh.id",
                Amount = 20000000m,
                TenorMonths = 12,
                Purpose = "Perluasan usaha katering harian dan pembelian oven convection gas",
                Status = "submitted",
                CreatedAt = DateTime.Parse("2026-10-25T12:45:00.000Z").ToUniversalTime(),
                CreatedByUserId = user.Id
            }
        };

        foreach (var app in initialApplications)
        {
            if (!await context.LoanApplications.AnyAsync(l => l.ApplicationNumber == app.ApplicationNumber || l.Id == app.Id))
            {
                await context.LoanApplications.AddAsync(app);
            }
        }

        await context.SaveChangesAsync();
    }
}
