using EntityFramework.Api.Datas.Contexts;
using EntityFramework.Api.Datas.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntityFramework.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EF04QueryingController : ControllerBase
{
    private readonly NorthwindContext _context;

    public EF04QueryingController(NorthwindContext context)
    {
        _context = context;
    }

    // LINQ Where ile Filtreleme
    [HttpGet("products/filter")]
    public async Task<IActionResult> FilterProducts([FromQuery] decimal minPrice, [FromQuery] decimal maxPrice)
    {
        var products = await _context.Products
            .Where(p => p.UnitPrice >= minPrice && p.UnitPrice <= maxPrice)
            .ToListAsync();

        return Ok(products);
    }

    // LINQ OrderBy ile Sıralama
    [HttpGet("products/sort")]
    public async Task<IActionResult> SortProducts([FromQuery] string sortBy, [FromQuery] bool descending = false)
    {
        IQueryable<Product> query = _context.Products;

        query = sortBy.ToLower() switch
        {
            "price" => descending 
                ? query.OrderByDescending(p => p.UnitPrice)
                : query.OrderBy(p => p.UnitPrice),
            "name" => descending
                ? query.OrderByDescending(p => p.ProductName)
                : query.OrderBy(p => p.ProductName),
            "stock" => descending
                ? query.OrderByDescending(p => p.UnitsInStock)
                : query.OrderBy(p => p.UnitsInStock),
            _ => query.OrderBy(p => p.ProductId)
        };

        var products = await query.ToListAsync();
        return Ok(products);
    }

    // LINQ GroupBy ile Gruplama
    [HttpGet("products/by-category")]
    public async Task<IActionResult> GroupProductsByCategory()
    {
        var groupedProducts = await _context.Products
            .Include(p => p.Category)
            .GroupBy(p => p.CategoryId)
            .Select(g => new
            {
                CategoryId = g.Key,
                CategoryName = g.FirstOrDefault().Category.CategoryName,
                ProductCount = g.Count(),
                AveragePrice = g.Average(p => p.UnitPrice),
                Products = g.ToList()
            })
            .ToListAsync();

        return Ok(groupedProducts);
    }

    // LINQ Join ile Birleştirme
    [HttpGet("products/with-suppliers")]
    public async Task<IActionResult> JoinProductsWithSuppliers()
    {
        var productsWithSuppliers = await _context.Products
            .Join(
                _context.Suppliers,
                product => product.SupplierId,
                supplier => supplier.SupplierId,
                (product, supplier) => new
                {
                    Product = product,
                    SupplierName = supplier.CompanyName,
                    SupplierCountry = supplier.Country
                })
            .ToListAsync();

        return Ok(productsWithSuppliers);
    }

    // LINQ Aggregation ile İstatistikler
    [HttpGet("products/stats")]
    public async Task<IActionResult> GetProductStatistics()
    {
        var stats = new
        {
            TotalProducts = await _context.Products.CountAsync(),
            AveragePrice = await _context.Products.AverageAsync(p => p.UnitPrice),
            MaxPrice = await _context.Products.MaxAsync(p => p.UnitPrice),
            MinPrice = await _context.Products.MinAsync(p => p.UnitPrice),
            TotalValue = await _context.Products.SumAsync(p => p.UnitPrice * p.UnitsInStock)
        };

        return Ok(stats);
    }

    // LINQ Any/All ile Varlık Kontrolü
    [HttpGet("orders/need-shipping")]
    public async Task<IActionResult> GetOrdersNeedingShipping()
    {
        var ordersNeedingShipping = await _context.Orders
            .Where(o => o.ShippedDate == null)
            .ToListAsync();

        return Ok(ordersNeedingShipping);
    }

    // LINQ Skip/Take ile Sayfalama
    [HttpGet("products/page")]
    public async Task<IActionResult> GetProductsPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var totalProducts = await _context.Products.CountAsync();
        var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

        var products = await _context.Products
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            Page = page,
            PageSize = pageSize,
            TotalProducts = totalProducts,
            TotalPages = totalPages,
            Products = products
        });
    }
} 