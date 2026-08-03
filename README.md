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

- ✅ Veritabanı kuruldu, `Vehicles` tablosu oluşturuldu ve test verisiyle dolduruldu.
- ✅ .NET 10 SDK kuruldu.
- ✅ Backend API çalışıyor — `Vehicles` için listeleme/ekleme endpoint'leri, Swagger ile test
  edildi.
- 🔄 WinForms masaüstü istemcisi — sırada.

Detaylı ilerleme için bkz. [TIMELINE.md](TIMELINE.md).

## Proje Yapısı

Backend, masaüstü ve veritabanı katmanları en üst seviyede net şekilde ayrılmıştır:

```
├── database/                    ← SQL script'leri (sıralı, ör. 001_..., 002_...)
│   ├── 001_create_vehicles_table.sql
│   └── 002_seed_dummy_data.sql
├── backend/
│   └── SeyirMobil.Api/          ← ASP.NET Core Web API + EF Core (tüm iş mantığı, DB erişimi)
├── desktop/                     ← WinForms masaüstü istemcisi (yakında eklenecek)
│   └── SeyirMobil.Desktop/
├── SeyirMobil.slnx               ← .NET solution (backend + desktop projelerini kapsar)
├── README.md
└── TIMELINE.md
```

## Geliştirici

Alperen Yağmur — Kocaeli Sağlık ve Teknoloji Üniversitesi, Yazılım Mühendisliği — Seyir Mobil
stajyeri.
