# Entity Framework Core Özellikleri Projesi

Bu proje, Entity Framework Core'un tüm özelliklerini gösteren bir API uygulamasıdır. Northwind veritabanı kullanılarak Entity Framework Core'un temel özellikleri ve ileri düzey teknikleri gösterilmiştir.

## Özellikler

Proje, aşağıdaki Entity Framework Core konularını kapsamaktadır:

1. **Temel CRUD İşlemleri** (`EF01BasicsController`)
   - Entity ekleme, silme, güncelleme, listeleme
   - Asenkron vs. senkron işlemler

2. **Database-First Yaklaşımı** (`EF02DbFirstController`)
   - Var olan veritabanından model oluşturma
   - Scaffold işlemleri
   - Veritabanı şema inceleme

3. **İlişkisel Veri ve Yükleme Stratejileri** (`EF03RelationsController`)
   - One-to-Many ilişkiler
   - Many-to-Many ilişkiler
   - One-to-One ilişkiler
   - Self-referencing ilişkiler
   - Eager Loading (Include)
   - Explicit Loading
   - Lazy Loading açıklaması

4. **LINQ Sorguları ve Optimizasyon** (`EF04QueryingController`)
   - Filtreleme, sıralama, gruplama
   - Projeksiyon (Select)
   - Join işlemleri
   - Aggregation
   - Sayfalama

5. **Performans Optimizasyonu** (`EF05PerformanceController`)
   - AsNoTracking kullanımı
   - Projeksiyon optimizasyonu
   - Split Queries
   - Compiled Queries

6. **Transaction Yönetimi** (`EF06TransactionsController`)
   - Transaction kullanımı
   - Concurrency kontrolü
   - Isolation Level ayarları
   - Execution Strategy

7. **Migration ve Code-First** (`EF07MigrationController`)
   - Migration kavramı
   - Migration tablosu
   - Veritabanı oluşturma/silme
   - Raw SQL kullanımı

## Kurulum

1. Northwind veritabanını yerel SQL Server'ınıza kurun
2. `appsettings.json` dosyasındaki bağlantı dizesini güncelleyin:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.; Database=Northwind; Trusted_Connection=True; TrustServerCertificate=True; Connect Timeout=30; MultipleActiveResultSets=True"
}
```

3. Projeyi çalıştırın:
```bash
cd src/EntityFramework.Api
dotnet run
```

4. Swagger UI üzerinden API'yi test edin: `https://localhost:5001/swagger`

## Proje Yapısı

- **Controllers**: Her bir Entity Framework özelliği için ayrı controller
- **Services**: Entity Framework işlemlerini içeren servis sınıfları
- **Datas/Contexts**: Veritabanı context sınıfları
- **Datas/Entities**: Entity sınıfları

## Teknolojiler

- .NET 9.0
- Entity Framework Core 9.0
- SQL Server
- Swagger/OpenAPI
