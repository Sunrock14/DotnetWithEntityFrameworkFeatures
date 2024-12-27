namespace EntityFramework.Api.Services.EF04Querying;
public class Queries
{
    private readonly NorthwindContext _context;
    public Queries(NorthwindContext context)
    {
        _context = context;
    }

    public void MapMyApiRoutes(WebApplication routes)
    {
        routes.MapGet("/data-querying", async (HttpContext context) =>
        {
            //Tüm methodları getirir

            //Method Syntax
            var products = await _context.Products.ToListAsync();
            
            //Query Syntax(Linq)
            var products2 = (from Product in products
                             select products).ToList();

            return Results.Ok(products);
        });
    }
}