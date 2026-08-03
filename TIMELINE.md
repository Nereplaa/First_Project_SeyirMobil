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

---

## 🔜 Sıradaki Adımlar

- [ ] ASP.NET Core Web API backend projesinin oluşturulması (`SeyirMobil.Api`)
- [ ] `Vehicles` için temel CRUD endpoint'leri (listeleme, ekleme)
- [ ] WinForms masaüstü istemcisi — backend API'ye bağlanan arayüz
- [ ] Kurumdan gelecek resmi staj/proje gereksinim dokümanının incelenmesi
- [ ] Web istemcisinin eklenmesi (aynı backend API üzerinden)
- [ ] UI/UX iyileştirmeleri
