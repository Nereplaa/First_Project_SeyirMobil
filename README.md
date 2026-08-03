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
- 🔄 Backend (ASP.NET Core Web API) proje iskeleti — sırada.

Detaylı ilerleme için bkz. [TIMELINE.md](TIMELINE.md).

## Proje Yapısı

```
├── Database/     ← SQL script'leri (sıralı, ör. 001_..., 002_...)
├── README.md
├── TIMELINE.md
└── (SeyirMobil/  ← backend + masaüstü .NET solution, yakında eklenecek)
```

## Geliştirici

Alperen Yağmur — Kocaeli Sağlık ve Teknoloji Üniversitesi, Yazılım Mühendisliği — Seyir Mobil
stajyeri.
