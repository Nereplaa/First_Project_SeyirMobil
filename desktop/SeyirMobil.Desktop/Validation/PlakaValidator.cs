using System.Text.RegularExpressions;

namespace SeyirMobil.Desktop.Validation;

public static partial class PlakaValidator
{
    // Turkiye plaka formati: 01-81 il kodu + 1-3 harf + rakam (harf sayisina gore rakam sayisi degisir)
    // Harfler: A-Z hariç Q, W, X (Latin alfabesinde yok) ve Ç, Ş, İ, Ö, Ü, Ğ (plakada kullanilmiyor)
    [GeneratedRegex(@"^(0[1-9]|[1-7][0-9]|8[01])([A-PR-VYZ]\d{4,5}|[A-PR-VYZ]{2}\d{3,4}|[A-PR-VYZ]{3}\d{2,3})$")]
    private static partial Regex PlakaPattern();

    public static string Normalize(string plaka) =>
        plaka.Trim().ToUpperInvariant().Replace(" ", "");

    public static bool IsValid(string plaka) => PlakaPattern().IsMatch(Normalize(plaka));
}
