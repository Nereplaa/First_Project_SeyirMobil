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

// Detayli rapor: secilen araliktaki HER gercek okuma, bir onceki okumaya gore
// km farkiyla birlikte. Ilk okumanin Artis'i null (ondan onceki bir okuma
// aralik icinde yok).
public record AracHareketDetayRaporSatiri(
    string AracPlaka,
    DateOnly VeriTarihi,
    decimal KmSayaci,
    decimal? Artis);
