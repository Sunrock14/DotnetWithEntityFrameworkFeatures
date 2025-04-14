using EntityFramework.Api.Datas.Contexts;
using EntityFramework.Api.Datas.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace EntityFramework.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EF05PerformanceController : ControllerBase
{
    private readonly NorthwindContext _context;

    public EF05PerformanceController(NorthwindContext context)
    {
        _context = context;
    }

    // AsNoTracking kullanımı - Performans kazancı
    [HttpGet("products/no-tracking")]
    public async Task<IActionResult> GetProductsWithNoTracking()
    {
        // Süre ölçümü başlat
        var sw = new Stopwatch();
        sw.Start();

        // Takip edilmeyen sonuçlar (sadece okuma için)
        var products = await _context.Products
            .AsNoTracking()
            .ToListAsync();

        sw.Stop();
        
        return Ok(new 
        { 
            ExecutionTime = sw.ElapsedMilliseconds,
            Products = products
        });
    }

    // Normal takip ile sorgu - Karşılaştırma için
    [HttpGet("products/tracking")]
    public async Task<IActionResult> GetProductsWithTracking()
    {
        var sw = new Stopwatch();
        sw.Start();

        // Varsayılan davranış - EF Core sonuçları takip eder
        var products = await _context.Products.ToListAsync();

        sw.Stop();
        
        return Ok(new 
        { 
            ExecutionTime = sw.ElapsedMilliseconds,
            Products = products
        });
    }

    // Select ile projeksiyon kullanımı - Sadece gerekli alanları çekme
    [HttpGet("products/projection")]
    public async Task<IActionResult> GetProductsWithProjection()
    {
        var sw = new Stopwatch();
        sw.Start();

        // Sadece ihtiyaç duyulan alanları seçme
        var products = await _context.Products
            .Select(p => new 
            {
                p.ProductId,
                p.ProductName,
                p.UnitPrice
            })
            .ToListAsync();

        sw.Stop();
        
        return Ok(new 
        { 
            ExecutionTime = sw.ElapsedMilliseconds,
            Products = products
        });
    }

    // Compiled Queries - Tekrar kullanılabilir önceden derlenmiş sorgular
    private static Func<NorthwindContext, int, Task<Product>> GetProductByIdQuery = 
        EF.CompileAsyncQuery<NorthwindContext, int, Product>(
            (ctx, id) => ctx.Products.FirstOrDefault(p => p.ProductId == id));

    [HttpGet("products/{id}/compiled")]
    public async Task<IActionResult> GetProductWithCompiledQuery(int id)
    {
        var sw = new Stopwatch();
        sw.Start();

        // Önceden derlenmiş sorguyu kullanma
        var product = await GetProductByIdQuery(_context, id);

        sw.Stop();
        
        if (product == null)
            return NotFound($"Ürün bulunamadı: {id}");
            
        return Ok(new 
        { 
            ExecutionTime = sw.ElapsedMilliseconds,
            Product = product
        });
    }

    // Split Queries - Büyük veri setleri için sorguları bölme
    [HttpGet("orders/{id}/split-query")]
    public async Task<IActionResult> GetOrderDetailsSplitQuery(int id)
    {
        var sw = new Stopwatch();
        sw.Start();

        var order = await _context.Orders
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .Include(o => o.Customer)
            .AsSplitQuery() // Birden fazla sorgu olarak veritabanına gönderilir
            .FirstOrDefaultAsync(o => o.OrderId == id);

        sw.Stop();
        
        if (order == null)
            return NotFound($"Sipariş bulunamadı: {id}");
            
        return Ok(new 
        { 
            ExecutionTime = sw.ElapsedMilliseconds,
            Order = order
        });
    }

    // Single Query - Karşılaştırma için
    [HttpGet("orders/{id}/single-query")]
    public async Task<IActionResult> GetOrderDetailsSingleQuery(int id)
    {
        var sw = new Stopwatch();
        sw.Start();

        var order = await _context.Orders
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.OrderId == id);

        sw.Stop();
        
        if (order == null)
            return NotFound($"Sipariş bulunamadı: {id}");
            
        return Ok(new 
        { 
            ExecutionTime = sw.ElapsedMilliseconds,
            Order = order
        });
    }

    // Explicit Loading - İhtiyaç duyuldukça veri yükleme
    [HttpGet("customers/{id}/explicit")]
    public async Task<IActionResult> GetCustomerWithExplicitLoading(string id)
    {
        var sw = new Stopwatch();
        sw.Start();

        // Önce sadece müşteriyi yükle
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == id);

        if (customer == null)
            return NotFound($"Müşteri bulunamadı: {id}");

        // Sonra ihtiyaç duyulan ilişkili verileri ayrı ayrı yükle
        await _context.Entry(customer)
            .Collection(c => c.Orders)
            .LoadAsync();
            
        // Sadece gerekli sipariş detaylarını yükle (örneğin: son 5 sipariş)
        var recentOrders = customer.Orders
            .OrderByDescending(o => o.OrderDate)
            .Take(5)
            .ToList();

        sw.Stop();
        
        return Ok(new 
        { 
            ExecutionTime = sw.ElapsedMilliseconds,
            Customer = customer,
            RecentOrders = recentOrders
        });
    }
} 