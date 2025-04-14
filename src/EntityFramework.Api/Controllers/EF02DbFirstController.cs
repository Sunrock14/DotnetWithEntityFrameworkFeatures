using EntityFramework.Api.Datas.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EntityFramework.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EF02DbFirstController : ControllerBase
{
    private readonly NorthwindContext _context;

    public EF02DbFirstController(NorthwindContext context)
    {
        _context = context;
    }

    // Veritabanı şemasını inceleme
    [HttpGet("schema")]
    public IActionResult GetDatabaseSchema()
    {
        var model = _context.Model;
        var entityTypes = model.GetEntityTypes().ToList();
        
        var schemaInfo = new
        {
            DatabaseProvider = _context.Database.ProviderName,
            EntityCount = entityTypes.Count,
            Entities = entityTypes.Select(e => new
            {
                Name = e.Name,
                TableName = e.GetTableName(),
                Schema = e.GetSchema(),
                Properties = e.GetProperties().Select(p => new
                {
                    Name = p.Name,
                    ColumnName = p.GetColumnName(),
                    ClrType = p.ClrType.Name,
                    IsKey = p.IsKey(),
                    IsForeignKey = p.IsForeignKey(),
                    IsRequired = !p.IsNullable
                }).ToList(),
                PrimaryKey = e.FindPrimaryKey()?.Properties.Select(p => p.Name).ToList(),
                ForeignKeys = e.GetForeignKeys().Select(fk => new
                {
                    PrincipalEntity = fk.PrincipalEntityType.Name,
                    PrincipalProperties = fk.PrincipalKey.Properties.Select(p => p.Name).ToList(),
                    DependentProperties = fk.Properties.Select(p => p.Name).ToList(),
                    DeleteBehavior = fk.DeleteBehavior.ToString()
                }).ToList()
            }).ToList()
        };
        
        return Ok(schemaInfo);
    }

    // Veritabanından ters mühendislik süreci bilgileri
    [HttpGet("scaffold-info")]
    public IActionResult GetScaffoldingInfo()
    {
        // Reverse engineering komutunun nasıl kullanılacağının örneği
        // Gerçek bir scaffold işlemi yapmıyoruz, sadece bilgi veriyoruz
        
        var scaffoldCommand = new StringBuilder();
        scaffoldCommand.AppendLine("# Database-First yaklaşımı için Entity Framework tarafından kullanılan scaffold komutu:");
        scaffoldCommand.AppendLine("# Bu komutu dotnet CLI ile kullanabilirsiniz.");
        scaffoldCommand.AppendLine();
        scaffoldCommand.AppendLine("dotnet ef dbcontext scaffold \"Server=.;Database=Northwind;Trusted_Connection=True;TrustServerCertificate=True\" Microsoft.EntityFrameworkCore.SqlServer \\");
        scaffoldCommand.AppendLine("    --context NorthwindContext \\");
        scaffoldCommand.AppendLine("    --context-dir Datas/Contexts \\");
        scaffoldCommand.AppendLine("    --output-dir Datas/Entities \\");
        scaffoldCommand.AppendLine("    --namespace EntityFramework.Api.Datas.Entities \\");
        scaffoldCommand.AppendLine("    --context-namespace EntityFramework.Api.Datas.Contexts \\");
        scaffoldCommand.AppendLine("    --no-onconfiguring \\");
        scaffoldCommand.AppendLine("    --force");
        
        var scaffoldOptions = new
        {
            ScaffoldCommand = scaffoldCommand.ToString(),
            Options = new Dictionary<string, string>
            {
                ["--table"] = "Belirli tabloları dahil etmek için kullanılır (örn: --table Products --table Categories)",
                ["--schema"] = "Belirli bir veritabanı şemasını dahil etmek için kullanılır",
                ["--data-annotations"] = "FluentAPI yerine veri açıklamaları (Data Annotations) kullanılır",
                ["--use-database-names"] = "Veritabanından gelen orijinal sütun ve tablo adlarını korur"
            },
            Notes = new List<string>
            {
                "Database-First yaklaşımında, önce veritabanı oluşturulur ve daha sonra EF Core modelleri oluşturulur.",
                "Bu, özellikle mevcut veritabanlarıyla çalışırken veya veritabanının doğrudan bir DBA tarafından yönetildiği durumlar için uygundur.",
                "Scaffold işlemi ile tüm tablolar, ilişkiler ve diğer veritabanı nesneleri C# sınıflarına dönüştürülür."
            }
        };
        
        return Ok(scaffoldOptions);
    }

    // Saklı prosedür çağırma örneği
    [HttpGet("stored-procedure")]
    public async Task<IActionResult> CallStoredProcedure()
    {
        // Raw SQL kullanarak saklı prosedür çağırma örneği
        // FormattableString olarak kullanmak için $ işareti eklendi
        var results = await _context.Database
            .SqlQuery<CustomerOrderHistory>($"EXEC [dbo].[CustOrderHist] @CustomerID = 'ALFKI'")
            .ToListAsync();
        
        return Ok(new
        {
            StoredProcedureName = "CustOrderHist",
            CustomerID = "ALFKI",
            Results = results
        });
    }

    // Trigger bilgisi
    [HttpGet("triggers")]
    public IActionResult GetTriggerInfo()
    {
        // Not: Gerçek trigger bilgisi için SQL sorgusu çalıştırmak gerekir
        // Bu örnek sadece Northwind'deki trigger'ların kullanımını açıklamaktadır
        
        var triggerInfo = new
        {
            Description = "Veritabanı trigger'ları, belirli olaylar gerçekleştiğinde otomatik olarak çalışan stored procedure'lerdir.",
            Examples = new List<object>
            {
                new
                {
                    Name = "trg_UpdateInventory",
                    Table = "OrderDetails",
                    Event = "INSERT",
                    Action = "Yeni sipariş detayı eklendiğinde ilgili ürünün stoğunu azaltır."
                },
                new
                {
                    Name = "trg_PreventDeletedProducts",
                    Table = "Products",
                    Event = "DELETE",
                    Action = "Ürünleri silmek yerine Discontinued alanını true olarak günceller."
                }
            },
            Notes = "Trigger'lar veritabanı seviyesinde çalıştığı için EF Core'da doğrudan yönetilmezler, ancak EF Core işlemleri trigger'ları tetikleyebilir."
        };
        
        return Ok(triggerInfo);
    }
}

// Saklı prosedür sonucunu temsil eden sınıf
public class CustomerOrderHistory
{
    public string ProductName { get; set; }
    public int Total { get; set; }
} 