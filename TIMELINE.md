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

### 2026-08-03 15:41 — Yeni Kayıt Ekleme ve Silme

Ana ekrana, adım adım açılan bir "yeni araç hareketi ekle" akışı eklendi: önce araç seçiliyor,
sonra tarih (bugün önerilir, değiştirilebilir), sonra hız, en son kilometre sayacı. Kilometre
sayacı girilirken sistem, o aracın o tarihe en yakın önceki ve sonraki kayıtlarını otomatik bulup
girilen değerin bu ikisi arasında, gerçekçi kalmasını sağlıyor — böylece kilometre sayacı asla
zaman içinde geriye gitmiyor. Kayıt silme de eklendi.

### 2026-08-03 16:10 — Filtreleme ve Arayüz İyileştirmeleri

Ana ekrandaki listeye bir filtre eklendi — plaka, tarih, hız veya kilometre değerine göre listeyi
daraltmak artık mümkün. Ayrıca arayüz, pencere boyutundan bağımsız olarak her zaman doğru
görünecek şekilde yeniden yapılandırıldı.

---

## 2026-08-03 16:56 — İlk Demo ve Yeni Yol Haritası

Bugüne kadar tamamlanan sistem (araç hareketleri listesi, filtreleme, ekleme/silme, çoklu araç
raporu) Seyir Mobil'den Eren bey'e gösterildi — projenin ilk demosu. Demo sonrası alınan geri
bildirimler doğrultusunda, sıradaki çalışma sırası netleşti:

1. **Detaylı Rapor** — mevcut rapor ekranına, seçilen tarih aralığında gün gün kilometre artışını
   gösteren daha ayrıntılı bir mod eklenecek.
2. **Web uygulaması** — masaüstündeki tüm özellikleri kapsayan, tek sayfalık bir web arayüzü
   geliştirilecek. Önce temel işlevsellik tamamlanacak, görsel/arayüz geliştirmesi ayrı bir
   aşamada ele alınacak.
3. **Arayüz geliştirme** — web altyapısı tamamlandıktan sonra, kurumun önerdiği hazır bileşen
   kütüphaneleriyle (DevExtreme ve muhtemelen Angular) görsel kalite artırılacak.
4. **Docker** — sistemin tek bir komutla kurulup çalıştırılabilmesi için konteynerleştirme, en
   sonda ayrı bir aşamada ele alınacak.

---

## 2026-08-04 08:10 — Detaylı Rapor Özelliği Tamamlandı

İlk demoda alınan geri bildirimin ilk maddesi hayata geçirildi: "Araç Hareket Raporu" ekranına
**"Detaylı Rapor (gün gün)"** seçeneği eklendi. İşaretlendiğinde, seçilen tarih aralığındaki her
gerçek okuma tek tek listeleniyor ve her satır bir öncekine göre ne kadar kilometre yapıldığını
gösteriyor — mevcut özet raporun (başlangıç/bitiş/toplam) yanında, daha ayrıntılı bir alternatif
olarak sunuluyor.

Ayrıca, önceki demoda gündeme gelen iki açık soru netleşti:
- Web arayüzünün **Angular** ile geliştirileceği doğrulandı.
- **DevExtreme** (önerilen hazır arayüz bileşen kütüphanesi) ücretsiz olmadığı araştırıldı ve
  doğrulandı — kurumla lisans konusunun görüşülmesi gerekiyor.

---

## 2026-08-04 09:08 — Web Uygulaması Yayında: Tüm Özellikler Angular'a Taşındı

Kullanıcının Detaylı Rapor özelliğini onaylamasının ardından, masaüstündeki tüm işlevleri kapsayan
bir web arayüzü geliştirildi — araç hareketleri listesi ve filtreleme, yeni kayıt ekleme sihirbazı,
kayıt silme, ve rapor ekranı (özet ve detaylı mod ikisi de). Web arayüzü aynı backend API'sini
kullanıyor; ikinci bir API yazılmadı.

Bu aşamada amaç görsel tasarım değil, işlevsel eksiksizlik — masaüstünde yapılabilen her şeyin
web'de de yapılabilmesi. Görsel/arayüz geliştirmesi (kurumun önerdiği DevExtreme bileşen
kütüphanesiyle) bir sonraki aşamada ayrıca ele alınacak; o aşamada kurumsal lisans netleşene kadar
kişisel bir deneme hesabıyla ilerlenecek.

---

## 2026-08-04 09:52 — Sayfalama ve Excel'e Aktarma (Web + Masaüstü)

Kullanıcı geri bildirimiyle: araç hareketleri listesine sayfalama (sayfa başına gösterilecek kayıt
sayısı seçilebiliyor) ve Excel'e aktarma özelliği eklendi — hem web hem masaüstü uygulamasında.
Bir filtre uygulanmışsa, sadece o filtreye uyan kayıtlar Excel'e aktarılıyor. Rapor ekranında da
benzer şekilde Excel'e aktarma eklendi; kullanıcı isterse her aracı kendi başlığı altında ayrı bir
bölüm olarak, isterse tüm araçları tek bir tabloda dışa aktarabiliyor.

Excel dosyaları backend'de üretiliyor, böylece web ve masaüstü aynı mantığı paylaşıyor — ikisi de
aynı sonucu üretiyor.

---

## 🔜 Sıradaki Adımlar

- [x] Web istemcisinin eklenmesi (aynı backend API üzerinden)
- [x] Sayfalama ve Excel'e aktarma (web + masaüstü)
- [ ] UI/UX iyileştirmeleri (DevExtreme ile)
- [ ] Docker ile konteynerleştirme
