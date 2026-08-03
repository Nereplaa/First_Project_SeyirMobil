# 📅 Seyir Mobil — Proje Timeline'ı

> Bu dosya, "Seyir Mobil - Araç Takip Sistemi" projesinin gelişim sürecini kronolojik olarak,
> tarih/saat bilgisiyle birlikte anlatır — ne yapıldı, neden, sırada ne var. Teknik geliştirme
> notları ayrı ve yerel bir çalışma alanında tutuluyor; burada projenin **nasıl geliştiği**
> anlatılıyor.

---

## 2026-08-03 — Proje Başlangıcı ve Mimari Kararlar

**Staj başladı.** Seyir Mobil bünyesinde, araç kilometre/plaka/kayıt takibi yapan bir sistem
geliştirilmesine karar verildi. İlk aşamada yerel bir veritabanı + masaüstü arayüzü hedefleniyor,
ilerleyen süreçte kurumdan alınacak resmi gereksinimler doğrultusunda web/çoklu platform desteğine
genişletilmesi planlanıyor.

**Mimari karar:** Sistem, birbirinden ayrı ama birlikte çalışan bileşenlerden oluşacak şekilde
tasarlandı:

- **Veritabanı:** SQL Server — sadece backend'den erişilir.
- **Backend:** ASP.NET Core Web API — tüm iş mantığı ve veritabanı erişimi burada toplanır.
- **Masaüstü istemci:** WinForms — sadece backend API'ye HTTP ile bağlanır, veritabanına doğrudan
  erişmez.
- **Web istemci (ileride):** Aynı backend API'yi kullanacak, iş mantığı ikinci kez yazılmayacak.

Bu yaklaşımın amacı: iş mantığının tek bir yerde toplanması, ileride web/mobil eklemenin
kolaylaşması ve veritabanının hiçbir istemciye doğrudan açılmadan güvenli kalması.

### Veritabanı Kurulumu
- SQL Server 2025 Express + SQL Server Management Studio (SSMS) kuruldu.
- `SeyirMobilDb` veritabanı ve `Vehicles` tablosu oluşturuldu (araç ID, plaka, toplam kilometre,
  kayıt tarihi).
- Tabloya 5 adet test (dummy) kaydı eklendi, hem script hem SSMS üzerinden görsel olarak
  doğrulandı.

### 2026-08-03 13:15 — .NET 10 SDK Kurulumu
Backend geliştirmeye başlayabilmek için .NET 10 SDK (güncel LTS sürüm) kuruldu ve doğrulandı.

### 2026-08-03 13:32 — Backend API Çalışıyor: `SeyirMobil.Api`

ASP.NET Core Web API projesi (.NET 10) oluşturuldu ve Entity Framework Core ile `SeyirMobilDb`
veritabanına bağlandı. `Vehicles` için temel uç noktalar (endpoint) yazıldı ve gerçekten
çalıştırılıp test edildi:

- `GET /api/vehicles` — tüm araçları listeler
- `GET /api/vehicles/{id}` — tek bir aracı getirir
- `POST /api/vehicles` — yeni araç ekler

Swagger arayüzü (`/swagger`) ile API'nin tarayıcıdan görsel olarak da denenebilmesi sağlandı.

### 2026-08-03 13:54 — Masaüstü Uygulaması Hazır: Listeleme, Ekleme, Silme

WinForms tabanlı masaüstü uygulaması (`SeyirMobil.Desktop`) geliştirildi ve gerçekten test edildi:

- Araç listesi bir tabloda (grid) gösteriliyor.
- Yeni araç eklenebiliyor — **plaka, gerçek Türkiye plaka formatına** (il kodu + harf + rakam)
  göre doğrulanıyor; toplam kilometre negatif olamıyor.
- Araç silinebiliyor (onay penceresiyle, yanlışlıkla silmeyi önlemek için).
- Liste "Yenile" butonuyla güncellenebiliyor.

Uygulama sadece backend API üzerinden çalışıyor, veritabanına hiç doğrudan bağlanmıyor.

---

## 2026-08-03 14:48 — Kurum Gereksinimi Geldi: Araç Hareket Raporu

Kurumdan resmi proje gereksinimi ulaştı: araçların zaman içindeki periyodik hareket kayıtlarından
(tarih, hız, kilometre sayacı), verilen bir plaka ve tarih aralığı için "o aralıkta kaç km yol
yapıldığını" gösteren bir rapor üretilmesi isteniyor.

Bu doğrultuda:

- Araçların zaman içindeki km sayacı okumalarını tutan yeni bir veri yapısı kuruldu, 100 satırlık
  çeşitli örnek veriyle dolduruldu.
- Backend'e, verilen plaka ve tarih aralığı için başlangıç km / bitiş km / yapılan km hesaplayan
  bir rapor uç noktası eklendi ve gerçek verilerle test edildi.

---

### 2026-08-03 15:14 — Rapor Ekranı ve Ana Ekran Güncellemesi

Masaüstü uygulamasına, kurum gereksiniminin karşılığı olan rapor ekranı eklendi:

- Birden fazla araç aynı anda seçilip tek seferde rapor alınabiliyor.
- Tarih aralığı seçimi kullanıcı hata yapamayacak şekilde tasarlandı (bitiş tarihi, başlangıçtan
  sonraki bir tarih olmak zorunda).
- Uygulamanın ana ekranı artık gerçek görev verisini (araçların zaman içindeki hareket kayıtlarını)
  gösteriyor.
- Arayüz responsive hale getirildi — pencere küçültülse bile hiçbir buton kaybolmuyor, düzen
  kendini otomatik olarak yeniden düzenliyor.

---

## 🔜 Sıradaki Adımlar

- [ ] Web istemcisinin eklenmesi (aynı backend API üzerinden)
- [ ] UI/UX iyileştirmeleri
