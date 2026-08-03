using System.Text.Json.Serialization;

namespace SeyirMobil.Desktop.Models;

public class VehicleDto
{
    [JsonPropertyName("aracId")]
    public int AracId { get; set; }

    [JsonPropertyName("plaka")]
    public string Plaka { get; set; } = string.Empty;

    [JsonPropertyName("totalKm")]
    public decimal TotalKm { get; set; }

    [JsonPropertyName("kayitTrh")]
    public DateTime KayitTrh { get; set; }
}

public class CreateVehicleRequestDto
{
    [JsonPropertyName("plaka")]
    public string Plaka { get; set; } = string.Empty;

    [JsonPropertyName("totalKm")]
    public decimal TotalKm { get; set; }
}
