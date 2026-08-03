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
Masaüstü    Web (ileride)  (ileride: Mobil)
(WinForms)  (React/Blazor)
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
| Web istemci (ileride) | Belirlenmedi |

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

Detaylı ilerleme için bkz. [TIMELINE.md](TIMELINE.md).

## Proje Yapısı

Backend, masaüstü ve veritabanı katmanları en üst seviyede net şekilde ayrılmıştır:

```
├── database/                    ← SQL script'leri (sıralı, ör. 001_..., 002_...)
│   ├── 001_create_vehicles_table.sql
│   ├── 002_seed_dummy_data.sql
│   ├── 003_create_arac_hareketleri_table.sql
│   └── 004_seed_arac_hareketleri_dummy_data.sql
├── backend/
│   └── SeyirMobil.Api/          ← ASP.NET Core Web API + EF Core (tüm iş mantığı, DB erişimi)
├── desktop/
│   └── SeyirMobil.Desktop/      ← WinForms masaüstü istemcisi (araç hareketleri listesi + rapor)
├── SeyirMobil.slnx               ← .NET solution (backend + desktop projelerini kapsar)
├── README.md
└── TIMELINE.md
```

## Geliştirici

Alperen Yağmur — Kocaeli Sağlık ve Teknoloji Üniversitesi, Yazılım Mühendisliği — Seyir Mobil
stajyeri.
