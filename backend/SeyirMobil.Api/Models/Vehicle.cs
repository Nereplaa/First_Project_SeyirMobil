using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeyirMobil.Api.Models;

[Table("Vehicles")]
public class Vehicle
{
    [Key]
    [Column("aracid")]
    public int AracId { get; set; }

    [Column("plaka")]
    [Required]
    [MaxLength(15)]
    public string Plaka { get; set; } = string.Empty;

    [Column("totalkm")]
    public decimal TotalKm { get; set; }

    [Column("kayittrh")]
    public DateTime KayitTrh { get; set; }
}
