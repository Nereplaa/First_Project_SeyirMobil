using System.Net.Http.Json;
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
}
