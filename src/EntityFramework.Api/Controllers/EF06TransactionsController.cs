using EntityFramework.Api.Datas.Contexts;
using EntityFramework.Api.Datas.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntityFramework.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EF06TransactionsController : ControllerBase
{
    private readonly NorthwindContext _context;

    public EF06TransactionsController(NorthwindContext context)
    {
        _context = context;
    }

    // Dahili Transaction Kullanımı
    [HttpPost("orders/create-with-details")]
    public async Task<IActionResult> CreateOrderWithDetails([FromBody] Order order)
    {
        // Veritabanı işlemini başlat
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Ana siparişi ekle
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            
            if (order.OrderDetails == null || !order.OrderDetails.Any())
                return BadRequest("Sipariş detayları gereklidir");
                
            // Sipariş detaylarına sipariş ID'yi atama
            foreach (var detail in order.OrderDetails)
            {
                detail.OrderId = order.OrderId;
                
                // Stok kontrolü
                var product = await _context.Products.FindAsync(detail.ProductId);
                if (product == null)
                    throw new Exception($"Ürün bulunamadı: {detail.ProductId}");
                    
                if (product.UnitsInStock < detail.Quantity)
                    throw new Exception($"Yetersiz stok: {product.ProductName}");
                    
                // Stok güncelleme
                product.UnitsInStock -= detail.Quantity;
            }
            
            await _context.SaveChangesAsync();
            
            // İşlemi onayla
            await transaction.CommitAsync();
            
            return CreatedAtAction(nameof(GetOrderWithDetails), new { id = order.OrderId }, order);
        }
        catch (Exception ex)
        {
            // Hata durumunda işlemi geri al
            await transaction.RollbackAsync();
            return BadRequest($"İşlem hatası: {ex.Message}");
        }
    }
    
    [HttpGet("orders/{id}")]
    public async Task<IActionResult> GetOrderWithDetails(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.OrderId == id);
            
        if (order == null)
            return NotFound($"Sipariş bulunamadı: {id}");
            
        return Ok(order);
    }
    
    // Concurrency Check - İyimser Kilit (Optimistic Locking)
    [HttpPut("products/{id}")]
    public async Task<IActionResult> UpdateProductWithConcurrencyCheck(int id, [FromBody] Product product)
    {
        if (id != product.ProductId)
            return BadRequest("ID uyuşmazlığı");
            
        try
        {
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Eşzamanlılık çakışmasını kontrol et
            var existingProduct = await _context.Products.FindAsync(id);
            
            if (existingProduct == null)
                return NotFound($"Ürün artık mevcut değil: {id}");
                
            // Çakışma bilgisini client'a bildir
            return Conflict(new {
                Message = "Bu kayıt başka bir kullanıcı tarafından değiştirilmiş.",
                CurrentValues = existingProduct,
                YourValues = product
            });
        }
    }
    
    // Açık Transaction API kullanımı
    [HttpPost("bulk-products")]
    public async Task<IActionResult> CreateBulkProducts([FromBody] List<Product> products)
    {
        // Manuel transaction stratejisi
        var strategy = _context.Database.CreateExecutionStrategy();
        
        IActionResult result = null;
        await strategy.ExecuteAsync(async () => {
            // Açık transaction başlat
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                foreach (var product in products)
                {
                    _context.Products.Add(product);
                }
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                result = Ok(new { Message = $"{products.Count} ürün başarıyla eklendi." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                result = BadRequest($"Toplu ekleme işlemi başarısız: {ex.Message}");
            }
        });
        
        return result;
    }
    
    // Isolation Level ile Transaction
    [HttpPost("orders/isolation-test")]
    public async Task<IActionResult> CreateOrderWithIsolation([FromBody] Order order)
    {
        // Belirli bir yalıtım seviyesi ile transaction başlat
        using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        
        try
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return Ok(new { OrderId = order.OrderId });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest($"İşlem hatası: {ex.Message}");
        }
    }
} 