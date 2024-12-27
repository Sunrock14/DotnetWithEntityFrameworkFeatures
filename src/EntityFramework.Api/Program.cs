using EntityFramework.Api.Services.EF03EntityBasics;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Swagger'ý ekleyin  
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Minimal API", Version = "v1" });
});

var conn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<NorthwindContext>(options =>
    options.UseSqlServer(conn));

builder.Services.AddScoped<Queries>();
builder.Services.AddScoped<Basics>();

var app = builder.Build();

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Minimal API V1");
    c.RoutePrefix = "swagger";
});
app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var queries = scope.ServiceProvider.GetRequiredService<Queries>();
    var basics = scope.ServiceProvider.GetRequiredService<Basics>();

    queries.MapMyApiRoutes(app);
    basics.MapMyApiRoutes(app);
}

app.Run();