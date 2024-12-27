using EntityFramework.Api.Datas.Entities;

namespace EntityFramework.Api.Services.EF03EntityBasics;

public class Basics
{
        
    public void MapMyApiRoutes(WebApplication routes)
    {
        routes.MapGet("/data-add", async (HttpContext context) =>
        {
            var _context = new NorthwindContext();
            Region region = new Region()
            {
                RegionId = 5,
                RegionDescription = "Test"
            };
            // Veri ekleme
            _context.Add(region);
            await _context.SaveChangesAsync();
            return Results.Ok(region);
        });
        routes.MapGet("/save-changes", async (HttpContext context) =>
        {
            // SaveChanges
            // İlgili transactionları db de execute eder
            // Eğer birisi hatalı olursa her işlemi geri alır (rollback)
            var _context = new NorthwindContext();

            Region region = new Region()
            {
                RegionId = 6,
                RegionDescription = "Test"
            };
            _context.Add(region);
            await _context.SaveChangesAsync();
            return Results.Ok(region);
        });
        routes.MapGet("/state-check", async (HttpContext context) =>
        {

            var _context = new NorthwindContext();
            Region region = new Region
            {
                RegionId = 7,
                RegionDescription = "Test2"
            };
            Console.WriteLine(_context.Entry(region).State);
            await _context.AddAsync(region);

            Console.WriteLine(_context.Entry(region).State);
            await _context.SaveChangesAsync();

            Console.WriteLine(_context.Entry(region).State);

            return Results.Ok(region);
        });
        routes.MapGet("/how-to-get-id", async (HttpContext context) =>
        {
            var _context = new NorthwindContext();

            Region region = new()
            {
                RegionDescription = "Test1234"
            };
            await _context.AddAsync(region);
            await _context.SaveChangesAsync();

            //Ürün Id si bu şekilde alınır
            var Id = region.RegionId;
            return Results.Ok(region);

        });
        routes.MapGet("/data-update-change-tracker", async (HttpContext context) =>
        {
            var _context = new NorthwindContext();

            /*
             * ChangeTracker
             * Contextten gelen nesneleri takip eder.
             * Nesnelerde değişiklik var mı kontrol eder.
             * Fakat bu işlem bir maliyettir.
             */
            var regionOld = await _context.Regions.FirstOrDefaultAsync(r => r.RegionId == 1);
            regionOld!.RegionDescription = "Update";

            await _context.SaveChangesAsync();
            return Results.Ok(regionOld);

        });
        routes.MapGet("/data-update-without-change-tracker", async (HttpContext context) =>
        {
            var _context = new NorthwindContext();

            /*
             * Db de var olan bir nesneyi bu şekilde update edebiliriz
             * Fakat Mutlaka ilgili nesnenin Id si verilmelidir.
             */
            Region Region = new()
            {
                RegionId = 1,
                RegionDescription = "Test 234"
            };
            _context.Regions.Update(Region);
            await _context.SaveChangesAsync();
            return Results.Ok(Region);

        });
        routes.MapGet("/multiple-update", async (HttpContext context) =>
        {
            var _context = new NorthwindContext();

            var regions = await _context.Regions.ToListAsync();
            foreach (var item in regions)
            {
                item.RegionDescription = $"update {item.RegionId}";
            }
            await _context.SaveChangesAsync();
            return Results.Ok(regions);
        });
        routes.MapGet("/data-delete", async (HttpContext context) =>
        {
            var _context = new NorthwindContext();

            var region = _context.Regions.FirstOrDefault(r => r.RegionId == 1);
            _context.Regions.Remove(region!);
            await _context.SaveChangesAsync();
            return Results.Ok(region);
        });
        routes.MapGet("/data-delete-without-change-tracker", async (HttpContext context) =>
        {
            var _context = new NorthwindContext();

            Region region = new()
            {
                RegionId = 2
            };
            _context.Remove(region);
            await _context.SaveChangesAsync();
            return Results.Ok(region);
        });
        routes.MapGet("/multiple-delete", async (HttpContext context) =>
        {
            var _context = new NorthwindContext();

            List<Region> region = new List<Region>()
            {
                new Region() { RegionId = 7, RegionDescription = "TESTTT" },
                new Region() { RegionId = 8, RegionDescription = "TESTTTTTTTTT" }
            };
            _context.Regions.RemoveRange(region);
            await _context.SaveChangesAsync();
            return Results.Ok(region);
        });
    }
}