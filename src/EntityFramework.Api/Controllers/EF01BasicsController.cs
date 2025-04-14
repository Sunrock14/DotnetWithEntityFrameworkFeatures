using EntityFramework.Api.Datas.Contexts;
using EntityFramework.Api.Datas.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntityFramework.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EF01BasicsController : ControllerBase
{
    private readonly NorthwindContext _context;

    public EF01BasicsController(NorthwindContext context)
    {
        _context = context;
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetAllProducts()
    {
        var products = await _context.Products.ToListAsync();
        return Ok(products);
    }

    [HttpGet("products/{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        var product = await _context.Products.FindAsync(id);
        
        if (product == null)
            return NotFound($"Ürün bulunamadı: {id}");
        
        return Ok(product);
    }

    [HttpPost("products")]
    public async Task<IActionResult> AddProduct([FromBody] Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetProductById), new { id = product.ProductId }, product);
    }

    [HttpPut("products/{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
    {
        if (id != product.ProductId)
            return BadRequest("ID uyuşmazlığı");
        
        _context.Entry(product).State = EntityState.Modified;
        
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Products.AnyAsync(p => p.ProductId == id))
                return NotFound($"Ürün bulunamadı: {id}");
            throw;
        }
        
        return NoContent();
    }

    [HttpDelete("products/{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound($"Ürün bulunamadı: {id}");
        
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
} 