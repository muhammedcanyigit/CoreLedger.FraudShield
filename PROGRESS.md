# Değişiklik Günlüğü (Faz Faz)

Bu dosya, projede her fazda tam olarak ne yapıldığını, hangi dosyaların
oluşturulduğunu ve neden o kararların alındığını tutar. `CLAUDE.md` genel
proje rehberi/anayasasıdır; bu dosya ise adım adım "ne oldu" günlüğüdür.

---

## Faz 1 — Mimari Tasarım: Veritabanı Şeması (2026-09-24)

**Durum:** ✅ Tamamlandı

**Oluşturulan dosyalar:**
- `database/schema.sql`

**Ne yapıldı:**
- 5 tablo tasarlandı: `accounts`, `transactions`, `ledger_entries`,
  `balance_snapshots`, `idempotency_logs`
- `accounts` tablosunda bilerek bakiye (Balance) sütunu yok — bakiye her zaman
  `ledger_entries` toplamından hesaplanıyor
- `ledger_entries` immutable tasarlandı (updated_at sütunu yok, sadece INSERT)
- `balance_snapshots` performans amaçlı eklendi: her seferinde tüm ledger'ı
  toplamak yerine son snapshot + sonraki kayıtlar toplanacak
- `transactions` tablosuna `fraud_score` ve `fraud_checked_at` eklendi (AI
  Fraud Shield sonucu audit trail için saklanacak)
- `idempotency_logs` ile aynı isteğin 2 kez işlenmesi engellenecek

**Alınan kararlar:**
- Para birimi: `NUMERIC(18,2)` (decimal), float/double kullanılmadı
  (yuvarlama hatası riski nedeniyle)
- UUID'ler `gen_random_uuid()` ile üretiliyor (pgcrypto extension)

---

## Faz 2 — C# Proje Kurulumu: .NET Web API + EF Core + PostgreSQL (2026-09-24)

**Durum:** ✅ Tamamlandı

**Oluşturulan yapı:**
- `CoreLedger.sln` — çözüm dosyası
- `src/CoreLedger.Domain` — entity sınıfları (Account, Transaction, LedgerEntry,
  BalanceSnapshot, IdempotencyLog) ve enum'lar (LedgerDirection,
  TransactionStatus, TransactionType). Dış paket bağımlılığı yok (saf domain).
- `src/CoreLedger.Infrastructure` — `CoreLedgerDbContext`, her entity için
  Fluent API konfigürasyonu (`Persistence/Configurations/`), EF Core migration
  (`Persistence/Migrations/InitialCreate`), `DependencyInjection.cs` (DI
  extension metodu)
- `src/CoreLedger.Api` — ASP.NET Core Web API (controller tabanlı, minimal API
  değil), Swagger/OpenAPI aktif, şablonun örnek WeatherForecast dosyaları
  silindi

**Neden katmanlı mimari (Domain / Infrastructure / Api)?**
Tek proje yerine 3 ayrı katman: Domain katmanı hiçbir dış pakete (EF Core,
PostgreSQL vs.) bağımlı değil — iş kurallarını veritabanından bağımsız tutar.
Bu, senior seviye .NET projelerinde beklenen standart bir ayrım (Clean
Architecture'ın basitleştirilmiş hali).

**Teknik kararlar / öğrenilenler:**
- **Npgsql.EntityFrameworkCore.PostgreSQL** en son sürümü (10.x) artık .NET 10
  gerektiriyor; projemiz net8.0 hedeflediği için paket 8.0.10'a sabitlendi.
  (Ders: NuGet paketlerinde her zaman hedef framework uyumluluğuna dikkat et.)
- **EFCore.NamingConventions** paketiyle `UseSnakeCaseNamingConvention()`
  kullanıldı ki C# tarafında PascalCase (`AccountNumber`) yazarken veritabanı
  tarafında snake_case (`account_number`) üretilsin — `database/schema.sql`
  ile birebir tutarlı.
  - **Önemli tuzak:** `builder.ToTable("Accounts")` gibi elle PascalCase isim
    verilince, naming convention bu değeri artık dönüştürmüyor (explicit
    config convention'ı eziyor). Çözüm: tablo adlarını da elle snake_case
    (`"accounts"`) yazmak.
- **Enum stratejisi değişti:** `database/schema.sql`'de ilk tasarımda native
  PostgreSQL `ENUM` tipleri vardı. Faz 2'de bundan vazgeçildi, `VARCHAR +
  CHECK constraint` tercih edildi. Neden: PostgreSQL'de mevcut bir ENUM tipine
  sonradan yeni değer eklemek (`ALTER TYPE ... ADD VALUE`) transaction içinde
  çalışmaz ve deployment'ları zorlaştırır; VARCHAR+CHECK tek satırlık bir
  `ALTER TABLE` ile güncellenebiliyor. `database/schema.sql` bu karara göre
  güncellendi.
- **`database/schema.sql`'in rolü değişti:** Artık gerçek şemanın "kaynağı"
  değil, okunabilir tasarım referansı. Gerçek şema artık EF Core migration'ları
  ile üretiliyor (`src/CoreLedger.Infrastructure/Persistence/Migrations`).
  İkisi elle senkron tutulacak.
- Bağlantı dizesi (`ConnectionStrings:DefaultConnection`) sadece
  `appsettings.Development.json`'da, yerel varsayılan değerlerle (localhost,
  postgres/postgres) tutuluyor — gerçek/production bilgisi asla repoya
  yazılmayacak, environment variable (`ConnectionStrings__DefaultConnection`)
  ile ezilecek.

**Doğrulama (canlı PostgreSQL olmadan yapıldı, henüz kurulu değil):**
- `dotnet build` → 0 hata, 0 uyarı
- `dotnet ef migrations add InitialCreate` → başarıyla üretildi, üretilen SQL
  elle incelendi (tablo/sütun adları, FK'ler, CHECK constraint'ler doğru)
- `dotnet run` ile API ayağa kaldırıldı, Swagger UI `200 OK` döndü

**Henüz yapılmadı (sonraki fazlara bırakıldı):**
- Gerçek bir PostgreSQL'e karşı migration'ı uygulamak (`dotnet ef database
  update`) — Faz 8'de Docker Compose ile PostgreSQL container'ı gelince
  yapılacak, ya da kullanıcı yerel PostgreSQL kurarsa daha önce denenebilir
- Ledger business logic (Faz 3), controller/endpoint'ler yok

---
