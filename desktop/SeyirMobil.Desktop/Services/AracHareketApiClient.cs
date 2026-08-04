using System.Net.Http.Json;
using System.Text.Json;
using SeyirMobil.Desktop.Models;

namespace SeyirMobil.Desktop.Services;

public class AracHareketApiClient
{
    private readonly HttpClient _http;

    public AracHareketApiClient()
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5080/")
        };
    }

    public async Task<List<AracHareketDto>> GetTumHareketlerAsync()
    {
        var result = await _http.GetFromJsonAsync<List<AracHareketDto>>("api/arac-hareketleri");
        return result ?? [];
    }

    public async Task<List<AracPlakaLookupDto>> GetPlakalarAsync()
    {
        var result = await _http.GetFromJsonAsync<List<AracPlakaLookupDto>>("api/arac-hareketleri/plakalar");
        return result ?? [];
    }

    public async Task<List<AracRaporSonucuDto>> GetRaporTopluAsync(List<string> plakalar, DateOnly baslangic, DateOnly bitis)
    {
        var request = new RaporTopluRequestDto(plakalar, baslangic, bitis);
        var response = await _http.PostAsJsonAsync("api/arac-hareketleri/rapor-toplu", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<AracRaporSonucuDto>>();
        return result ?? [];
    }

    public async Task<List<AracHareketDetayRaporSatiriDto>> GetDetayRaporuAsync(List<string> plakalar, DateOnly baslangic, DateOnly bitis)
    {
        var request = new RaporTopluRequestDto(plakalar, baslangic, bitis);
        var response = await _http.PostAsJsonAsync("api/arac-hareketleri/rapor-detay", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<AracHareketDetayRaporSatiriDto>>();
        return result ?? [];
    }

    public async Task<byte[]> ExportHareketlerAsync(List<AracHareketDto> hareketler)
    {
        var response = await _http.PostAsJsonAsync("api/arac-hareketleri/export", hareketler);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<byte[]> ExportRaporAsync(RaporExportRequestDto request)
    {
        var response = await _http.PostAsJsonAsync("api/arac-hareketleri/rapor-export", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<AracHareketSinirlarDto> GetSinirlarAsync(string plaka, DateOnly tarih)
    {
        var url = $"api/arac-hareketleri/sinirlar?plaka={Uri.EscapeDataString(plaka)}&tarih={tarih:yyyy-MM-dd}";
        var result = await _http.GetFromJsonAsync<AracHareketSinirlarDto>(url);
        return result ?? new AracHareketSinirlarDto(false, null, null, null, null);
    }

    public async Task CreateHareketAsync(CreateAracHareketRequestDto request)
    {
        var response = await _http.PostAsJsonAsync("api/arac-hareketleri", request);
        if (!response.IsSuccessStatusCode)
        {
            var mesaj = await OkuHataMesajiAsync(response);
            throw new InvalidOperationException(mesaj);
        }
    }

    public async Task DeleteHareketAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/arac-hareketleri/{id}");
        response.EnsureSuccessStatusCode();
    }

    private static async Task<string> OkuHataMesajiAsync(HttpResponseMessage response)
    {
        try
        {
            var gövde = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(gövde);
            if (doc.RootElement.TryGetProperty("message", out var mesajElemani))
            {
                return mesajElemani.GetString() ?? response.ReasonPhrase ?? "Bilinmeyen hata";
            }
        }
        catch
        {
            // gövde JSON degilse asagida genel mesaja dusulur
        }
        return response.ReasonPhrase ?? "Bilinmeyen hata";
    }
}
