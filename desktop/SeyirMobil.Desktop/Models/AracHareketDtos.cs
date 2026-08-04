using System.Text.Json.Serialization;

namespace SeyirMobil.Desktop.Models;

public record AracHareketDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("aracId")] int AracId,
    [property: JsonPropertyName("aracPlaka")] string AracPlaka,
    [property: JsonPropertyName("veriTarihi")] DateOnly VeriTarihi,
    [property: JsonPropertyName("hiz")] int Hiz,
    [property: JsonPropertyName("kmSayaci")] decimal KmSayaci);

public record AracHareketSinirlarDto(
    [property: JsonPropertyName("ayniTarihVarMi")] bool AyniTarihVarMi,
    [property: JsonPropertyName("oncekiTarih")] DateOnly? OncekiTarih,
    [property: JsonPropertyName("oncekiKm")] decimal? OncekiKm,
    [property: JsonPropertyName("sonrakiTarih")] DateOnly? SonrakiTarih,
    [property: JsonPropertyName("sonrakiKm")] decimal? SonrakiKm);

public record CreateAracHareketRequestDto(
    [property: JsonPropertyName("aracId")] int AracId,
    [property: JsonPropertyName("aracPlaka")] string AracPlaka,
    [property: JsonPropertyName("veriTarihi")] DateOnly VeriTarihi,
    [property: JsonPropertyName("hiz")] int Hiz,
    [property: JsonPropertyName("kmSayaci")] decimal KmSayaci);

public record AracPlakaLookupDto(
    [property: JsonPropertyName("aracId")] int AracId,
    [property: JsonPropertyName("aracPlaka")] string AracPlaka);

public record RaporTopluRequestDto(
    [property: JsonPropertyName("plakalar")] List<string> Plakalar,
    [property: JsonPropertyName("baslangic")] DateOnly Baslangic,
    [property: JsonPropertyName("bitis")] DateOnly Bitis);

public record AracRaporSonucuDto(
    [property: JsonPropertyName("aracPlaka")] string AracPlaka,
    [property: JsonPropertyName("bulunduMu")] bool BulunduMu,
    [property: JsonPropertyName("baslangicTarihi")] DateOnly? BaslangicTarihi,
    [property: JsonPropertyName("baslangicKm")] decimal? BaslangicKm,
    [property: JsonPropertyName("bitisTarihi")] DateOnly? BitisTarihi,
    [property: JsonPropertyName("bitisKm")] decimal? BitisKm,
    [property: JsonPropertyName("yapilanKm")] decimal? YapilanKm);

public record AracHareketDetayRaporSatiriDto(
    [property: JsonPropertyName("aracPlaka")] string AracPlaka,
    [property: JsonPropertyName("veriTarihi")] DateOnly VeriTarihi,
    [property: JsonPropertyName("kmSayaci")] decimal KmSayaci,
    [property: JsonPropertyName("artis")] decimal? Artis);

public record RaporExportRequestDto(
    [property: JsonPropertyName("plakalar")] List<string> Plakalar,
    [property: JsonPropertyName("baslangic")] DateOnly Baslangic,
    [property: JsonPropertyName("bitis")] DateOnly Bitis,
    [property: JsonPropertyName("detayliMi")] bool DetayliMi,
    [property: JsonPropertyName("ayriPlakaBazliMi")] bool AyriPlakaBazliMi);
