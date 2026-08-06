# 🚚 Seyir Mobil — Araç Takip Sistemi

Araç kilometre, plaka ve kayıt durumlarını takip eden, ölçeklenebilir bir sistem. Seyir Mobil
bünyesinde staj kapsamında geliştirilmektedir.

### 📅 [Proje Timeline'ı → TIMELINE.md](TIMELINE.md)
Projenin nasıl geliştiğini, hangi kararların neden alındığını, tarih/saat bilgisiyle adım adım
görmek için tıklayın.

---

## Hakkında

Sistem ilk aşamada yerel bir veritabanı + masaüstü arayüzü olarak başlıyor, ilerleyen süreçte
kurumdan alınacak geri bildirimler doğrultusunda web ortamına taşınarak çoklu platform destekli
bir yapıya evrilecek.

## Mimari

```
        SQL Server (DB)
             ↑
   ASP.NET Core Web API   ← tüm iş mantığı, tüm veritabanı erişimi burada
             ↑  (HTTP/JSON)
   ┌─────────┼──────────────┐
Masaüstü    Web            (ileride: Mobil)
(WinForms)  (Angular)
```

Hiçbir istemci (masaüstü, web, ileride mobil) veritabanına doğrudan bağlanmaz — sadece backend API
SQL Server'a erişir. İstemciler API'yi HTTP ile çağırır.

## Teknoloji

| Katman | Teknoloji |
|---|---|
| Veritabanı | SQL Server 2025 Express |
| Backend / API | ASP.NET Core Web API (.NET 10) |
| ORM | Entity Framework Core |
| Masaüstü istemci | WinForms (C#) |
| Web istemci | Angular 22 (SPA) |
| Excel export | ClosedXML (MIT, backend'de üretilir) |
| UI komponentleri (ileride) | DevExtreme (ticari lisans — kurumla görüşülecek) |
| Dağıtım (ileride) | Docker (planlanıyor) |

## Güncel Durum

- ✅ Veritabanı kuruldu, temel + asıl görev tabloları oluşturuldu ve test verisiyle dolduruldu.
- ✅ .NET 10 SDK kuruldu.
- ✅ Backend API çalışıyor, Swagger ile test edildi.
- ✅ Kurumdan resmi gereksinim dokümanı geldi — araç hareket/kilometre raporu özelliği.
- ✅ WinForms masaüstü uygulaması: tüm araç hareketlerini listeliyor, çoklu plaka + tarih aralığı
  seçerek "başlangıç km / bitiş km / yapılan km" raporu oluşturuyor. Arayüz responsive — pencere
  daraltılınca kontroller alt satıra kayıyor, kaybolmuyor.
- ✅ Yeni araç hareketi ekleme (adım adım: plaka → tarih → hız → km sayacı — km sayacı, komşu
  kayıtlara göre tutarlı kalacak şekilde otomatik sınırlanıyor) ve kayıt silme.
- ✅ Ana ekranda plaka/tarih/hız/km'ye göre filtreleme.
- ✅ İlk demo Seyir Mobil'e yapıldı, geri bildirim doğrultusunda yol haritası netleşti: sırada
  detaylı rapor (masaüstü) → web uygulaması (temel altyapı) → arayüz geliştirme → Docker.
- ✅ Rapor ekranına "Detaylı Rapor (gün gün)" modu eklendi — seçilen tarih aralığındaki her okumanın
  bir öncekine göre kilometre artışını gösteriyor.
- ✅ Web uygulaması (Angular) yayında — masaüstündeki tüm özellikler (liste/filtre, ekleme/silme,
  rapor özet+detaylı) aynı backend API üzerinden web'de de çalışıyor. Görsel/arayüz geliştirmesi
  bir sonraki aşamada.
- ✅ Araç hareketleri listesine sayfalama eklendi (sayfa başına kayıt sayısı seçilebiliyor), hem
  web hem masaüstünde. Liste ve rapor ekranlarına Excel'e aktarma eklendi — filtre uygulanmışsa
  sadece filtrelenmiş sonuçlar, rapor ekranında araç başına ayrı bölüm veya tek tablo seçenekli.

Detaylı ilerleme için bkz. [TIMELINE.md](TIMELINE.md).

## Proje Yapısı

Backend, masaüstü, web ve veritabanı katmanları en üst seviyede net şekilde ayrılmıştır:

```
├── database/                    ← SQL script'leri (sıralı, ör. 001_..., 002_...)
│   ├── 001_create_vehicles_table.sql        (tarihsel — 006 ile tablo kaldırıldı)
│   ├── 002_seed_dummy_data.sql              (tarihsel — 006 ile tablo kaldırıldı)
│   ├── 003_create_arac_hareketleri_table.sql
│   ├── 004_seed_arac_hareketleri_dummy_data.sql
│   ├── 005_create_users_table.sql
│   └── 006_drop_vehicles_table.sql
├── backend/
│   └── SeyirMobil.Api/          ← ASP.NET Core Web API + EF Core (tüm iş mantığı, DB erişimi)
├── desktop/
│   └── SeyirMobil.Desktop/      ← WinForms masaüstü istemcisi (araç hareketleri listesi + rapor)
├── web/
│   └── seyir-mobil-web/         ← Angular web istemcisi (aynı backend API'yi kullanır)
├── SeyirMobil.slnx               ← .NET solution (backend + desktop projelerini kapsar)
├── README.md
└── TIMELINE.md
```

## Geliştirici

Alperen Yağmur — Kocaeli Sağlık ve Teknoloji Üniversitesi, Yazılım Mühendisliği — Seyir Mobil
stajyeri.
