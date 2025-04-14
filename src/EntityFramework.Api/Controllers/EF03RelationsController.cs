using EntityFramework.Api.Datas.Contexts;
using EntityFramework.Api.Datas.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntityFramework.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EF03RelationsController : ControllerBase
{
    private readonly NorthwindContext _context;

    public EF03RelationsController(NorthwindContext context)
    {
        _context = context;
    }

    // One-to-Many İlişki: Kategori-Ürünler
    [HttpGet("categories/{categoryId}/products")]
    public async Task<IActionResult> GetProductsByCategory(int categoryId)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

        if (category == null)
            return NotFound($"Kategori bulunamadı: {categoryId}");

        return Ok(category.Products);
    }

    // Many-to-Many İlişki: Müşteri-Kullanıcı demografileri
    [HttpGet("customers/{customerId}/demographics")]
    public async Task<IActionResult> GetCustomerDemographics(string customerId)
    {
        var customer = await _context.Customers
            .Include(c => c.CustomerTypes)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);

        if (customer == null)
            return NotFound($"Müşteri bulunamadı: {customerId}");

        return Ok(customer.CustomerTypes);
    }

    // One-to-Many/Eager Loading: Siparişler ve Detaylar
    [HttpGet("orders/{orderId}/details")]
    public async Task<IActionResult> GetOrderWithDetails(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null)
            return NotFound($"Sipariş bulunamadı: {orderId}");

        return Ok(order);
    }

    // Self Referencing İlişki: Çalışanlar ve Yöneticiler
    [HttpGet("employees/{employeeId}/subordinates")]
    public async Task<IActionResult> GetEmployeeSubordinates(int employeeId)
    {
        var employee = await _context.Employees
            .Include(e => e.InverseReportsToNavigation)
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

        if (employee == null)
            return NotFound($"Çalışan bulunamadı: {employeeId}");

        return Ok(employee.InverseReportsToNavigation);
    }

    // Explicit Loading: Tedarikçi ve Ürünleri
    [HttpGet("suppliers/{supplierId}/products-explicit")]
    public async Task<IActionResult> GetSupplierProductsExplicit(int supplierId)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.SupplierId == supplierId);

        if (supplier == null)
            return NotFound($"Tedarikçi bulunamadı: {supplierId}");

        // Tedarikçinin ürünlerini ayrı bir sorgu ile yükleme
        await _context.Entry(supplier)
            .Collection(s => s.Products)
            .LoadAsync();

        return Ok(new
        {
            supplier,
            products = supplier.Products
        });
    }
} 