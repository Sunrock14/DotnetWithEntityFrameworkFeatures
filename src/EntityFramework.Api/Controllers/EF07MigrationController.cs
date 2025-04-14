using EntityFramework.Api.Datas.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EntityFramework.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EF07MigrationController : ControllerBase
{
    private readonly NorthwindContext _context;

    public EF07MigrationController(NorthwindContext context)
    {
        _context = context;
    }

    // Migration bilgilerini getir
    [HttpGet("info")]
    public IActionResult GetMigrationInformation()
    {
        var migrator = _context.Database.GetService<IMigrator>();
        var historyRepository = _context.Database.GetService<IHistoryRepository>();
        
        // Veritabanında uygulanan migrationları al
        var appliedMigrations = _context.Database.GetAppliedMigrations().ToList();
        
        // Bekleyen migrationları al
        var pendingMigrations = _context.Database.GetPendingMigrations().ToList();

        return Ok(new 
        {
            DatabaseProvider = _context.Database.ProviderName,
            ConnectionString = _context.Database.GetConnectionString(),
            AppliedMigrations = appliedMigrations,
            PendingMigrations = pendingMigrations,
            CanConnect = _context.Database.CanConnect()
        });
    }

    // Migration tablosunu görüntüle
    [HttpGet("history")]
    public async Task<IActionResult> GetMigrationHistory()
    {
        // Migration geçmişine erişmek için ham SQL kullanımı - FormattableString için $ işareti eklendi
        var migrationHistory = await _context.Database
            .SqlQuery<MigrationHistoryEntry>($"SELECT * FROM __EFMigrationsHistory")
            .ToListAsync();

        return Ok(migrationHistory);
    }

    // Veritabanı oluşturma
    [HttpPost("ensure-created")]
    public IActionResult EnsureDatabaseCreated()
    {
        try
        {
            bool wasCreated = _context.Database.EnsureCreated();
            
            return Ok(new 
            { 
                Success = true, 
                WasCreated = wasCreated,
                Message = wasCreated 
                    ? "Veritabanı başarıyla oluşturuldu." 
                    : "Veritabanı zaten mevcut."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new 
            { 
                Success = false, 
                Error = ex.Message 
            });
        }
    }

    // Veritabanını silme
    [HttpPost("ensure-deleted")]
    public IActionResult EnsureDatabaseDeleted()
    {
        try
        {
            bool wasDeleted = _context.Database.EnsureDeleted();
            
            return Ok(new 
            { 
                Success = true, 
                WasDeleted = wasDeleted,
                Message = wasDeleted 
                    ? "Veritabanı başarıyla silindi." 
                    : "Veritabanı mevcut değil."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new 
            { 
                Success = false, 
                Error = ex.Message 
            });
        }
    }

    // Raw SQL kullanımı
    [HttpGet("raw-sql")]
    public async Task<IActionResult> ExecuteRawSql()
    {
        // Tüm sorgu tek bir interpolated string içinde olmalı
        var results = await _context.Database
            .SqlQuery<ProductCategorySummary>($@"
                SELECT TOP 5 p.ProductName, c.CategoryName, p.UnitPrice 
                FROM Products p 
                JOIN Categories c ON p.CategoryID = c.CategoryID 
                ORDER BY p.UnitPrice DESC")
            .ToListAsync();

        return Ok(results);
    }
}

// Migration geçmişini temsil eden sınıf
public class MigrationHistoryEntry
{
    public string MigrationId { get; set; }
    public string ProductVersion { get; set; }
}

// Raw SQL sonucunu temsil eden sınıf
public class ProductCategorySummary
{
    public string ProductName { get; set; }
    public string CategoryName { get; set; }
    public decimal UnitPrice { get; set; }
} 