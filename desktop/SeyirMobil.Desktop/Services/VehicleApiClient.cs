using System.Net.Http.Json;
using SeyirMobil.Desktop.Models;

namespace SeyirMobil.Desktop.Services;

public class VehicleApiClient
{
    private readonly HttpClient _http;

    public VehicleApiClient()
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5080/")
        };
    }

    public async Task<List<VehicleDto>> GetVehiclesAsync()
    {
        var vehicles = await _http.GetFromJsonAsync<List<VehicleDto>>("api/vehicles");
        return vehicles ?? [];
    }

    public async Task<VehicleDto?> CreateVehicleAsync(string plaka, decimal totalKm)
    {
        var request = new CreateVehicleRequestDto { Plaka = plaka, TotalKm = totalKm };
        var response = await _http.PostAsJsonAsync("api/vehicles", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<VehicleDto>();
    }

    public async Task DeleteVehicleAsync(int aracId)
    {
        var response = await _http.DeleteAsync($"api/vehicles/{aracId}");
        response.EnsureSuccessStatusCode();
    }
}
