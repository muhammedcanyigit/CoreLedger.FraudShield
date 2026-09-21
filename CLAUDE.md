# Core-Ledger & AI Fraud Shield

## Proje Nedir

Production-ready bankacılık mimarisi. CRUD bir para transferi API'si DEĞİL. Gerçek
dünya bankacılık standartlarında çift taraflı defter tutma (double-entry ledger)
ile çalışan ve yapay zeka ile gerçek zamanlı fraud tespiti yapan mikroservis
tabanlı finansal backend.

Amaç: Akbank ve büyük fintech/bankacılık şirketlerinde senior seviye pozisyonlara
girecek özgeçmiş projesi seviyesinde derinlik.

## Neden Bu Mimari (Sorun Tanımı)

Acemi yaklaşım: `UPDATE Users SET Balance = Balance - 100` — bu YASAK, çünkü:
- Audit trail (geçmişe dönük izlenebilirlik) yok olur
- Concurrent işlemlerde bakiye bozulur (race condition)
- Yanlış işlemde veri tutarsızlığı geri döndürülemez

## Vizyon — 3 Temel İlke

1. **Sıfır Hata / Kaybolmayan Veri**: Transfer olduğunda bakiye rakamı asla
   değiştirilmez. Sisteme iki immutable kayıt eklenir: Debit (borç) ve Credit
   (alacak). Bakiye `SELECT SUM(Amount)` ile dinamik hesaplanır.
2. **Ağ Kesintilerine %100 Dayanıklılık (Idempotency)**: Aynı istek
   `Idempotency-Key` header'ı ile tekilleştirilir. Kullanıcı "Gönder"e 2 kez
   basarsa (internet kopması vb.) para 2 kez gitmez.
3. **Milisaniyeler İçinde AI Güvenliği**: Transfer isteği DB'ye yazılmadan önce
   Python ML servisine sorulur. Şüpheli davranışta (örn. gece 03:00, alışılmadık
   konum/tutar) işlem bloklanır.

## Teknik Mimari

### Double-Entry Ledger
- Her hareket iki taraflı: Debit ve Credit
- Denklem: Σ Debit = Σ Credit (sistemdeki toplam her an 0'a eşit olmalı)
- Örnek: Ahmet → Mehmet 500 TL: Ahmet'in hesabı -500 (Debit), Mehmet'in hesabı
  +500 (Credit). Bakiye kayıtları toplanarak (event sourcing mantığı) hesaplanır.

### Idempotency Key
- İstemci `Idempotency-Key: <GUID>` header'ı gönderir
- Backend Redis/PostgreSQL'de kontrol eder:
  - Key yoksa → işlemi yap, sonucu cache'le, yanıtı dön
  - Key varsa → işlemi tekrar çalıştırma, cache'teki yanıtı direkt dön

### Mikroservis İletişimi
- **C# .NET Core**: iş mantığı, EF Core + PostgreSQL, JWT yetkilendirme,
  Idempotency yönetimi
- **Python FastAPI**: ML modeli (Isolation Forest / XGBoost) çalıştırır
- İletişim: REST veya gRPC. C# transfer detaylarını (tutar, saat, cihaz,
  konum) Python'a gönderir → Python 0.00–1.00 arası Fraud Skoru döner
  (>%85 skor → işlem bloklanır / exception)

## Yol Haritası (Proje Anayasası)

| # | Faz | Konu | Durum |
|---|-----|------|-------|
| 1 | Mimari Tasarım | DB şeması: Accounts, LedgerEntries, Transactions, IdempotencyLogs | ⏳ Bekliyor |
| 2 | C# Proje Kurulumu | .NET Core Web API, EF Core & PostgreSQL bağlantısı | ⏳ Bekliyor |
| 3 | Core Ledger Engine | Double-Entry mantığı, atomik Debit/Credit transaction | ⏳ Bekliyor |
| 4 | Idempotency Middleware | Custom header kontrolcüsü + Redis/DB caching | ⏳ Bekliyor |
| 5 | Python AI Servisi | FastAPI kurulumu, Isolation Forest/XGBoost eğitimi | ⏳ Bekliyor |
| 6 | Entegrasyon (gRPC/REST) | C# ↔ Python iletişim katmanı | ⏳ Bekliyor |
| 7 | Fraud Pipeline Entegrasyonu | Transfer akışına AI bloklama mekanizması | ⏳ Bekliyor |
| 8 | Containerization | Docker Compose: C#, Python, PostgreSQL, Redis | ⏳ Bekliyor |
| 9 | Test & Benchmark | Idempotency/Fraud test senaryoları, xUnit/Postman | ⏳ Bekliyor |

## Kurallar / Sabit Talimatlar

- **Kod yazımı**: Kullanıcı açıkça "kod yazmaya başlayabilirsin" demeden kod
  yazılmaz. Önce anlama/kavramları netleştirme aşaması.
- **Git commit/push**: Her adım sonrası GitHub'a push edilir. Commit
  mesajlarında veya contributor bilgisinde **Claude ismi kesinlikle geçmez**.
  Sadece kullanıcının git kimliği (muhammedcanyigit) kullanılır.
- **Repo**: https://github.com/muhammedcanyigit/CoreLedger.FraudShield.git
- **Yerel path**: /Users/canyigit/Desktop/CoreLedger.FraudShield

## İlerleme Günlüğü

### 2026-09-21
- Proje vizyonu ve teknik mimari kullanıcı tarafından detaylıca anlatıldı.
- Repo kurulum sorunu tespit edildi ve çözüldü: `/Users/canyigit` home dizini
  yanlışlıkla farklı bir projenin (Tarim-Projesi) git repo'su olarak
  kullanılıyordu, CoreLedger.FraudShield klasörü bu repo'nun kökü değildi.
  Klasör boştu, temiz şekilde silinip doğru GitHub reposundan
  (muhammedcanyigit/CoreLedger.FraudShield) klonlandı.
- CLAUDE.md oluşturuldu, proje henüz kod aşamasına geçmedi (1. faz: DB şeması
  tasarımı bekliyor).
