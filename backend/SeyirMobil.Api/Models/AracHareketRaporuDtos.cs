namespace SeyirMobil.Api.Models;

public record AracPlakaLookup(int AracId, string AracPlaka);

public record RaporTopluRequest(List<string> Plakalar, DateOnly Baslangic, DateOnly Bitis);

public record AracRaporSonucu(
    string AracPlaka,
    bool BulunduMu,
    DateOnly? BaslangicTarihi,
    decimal? BaslangicKm,
    DateOnly? BitisTarihi,
    decimal? BitisKm,
    decimal? YapilanKm);
