using EntityFramework.Api.Services.EF01Basics;
using EntityFramework.Api.Services.EF03Relations;
using EntityFramework.Api.Services.EF04Querying;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Controllers ekle
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Swagger'i ekle  
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Entity Framework Özellikleri API", Version = "v1" });
});

// Veritabanı bağlantısını al ve DbContext'i ekle
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<NorthwindContext>(options =>
    options.UseSqlServer(conn));

// Servislerimizi IoC container'a kaydedelim
builder.Services.AddScoped<BasicCrudService>();
builder.Services.AddScoped<RelationshipsService>();
builder.Services.AddScoped<QueryOptimizationService>();

var app = builder.Build();

// Development ortamında hata sayfaları
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Swagger ara katmanları
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Entity Framework Özellikleri API V1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// Controller rotaları
app.MapControllers();

app.Run();