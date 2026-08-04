using System.Collections.Concurrent;

namespace SeyirMobil.Api.Services;

// Aktif oturumlarin "son islem zamani"ni tutar - sliding idle timeout icin.
// In-memory (tek backend instance varsayimi, bu projenin kapsaminda yeterli):
// backend yeniden baslatilinca tum oturumlar sifirlanir, kullanicilarin tekrar
// login olmasi gerekir - dahili/staj projesi icin kabul edilebilir bir sinirlama.
public class SessionStore
{
    private readonly ConcurrentDictionary<string, DateTime> _sonIslemZamani = new();

    public void OturumBaslat(string sessionId) =>
        _sonIslemZamani[sessionId] = DateTime.UtcNow;

    // Oturum hala gecerliyse (bulundu VE bosta kalma suresini asmadiysa) son islem
    // zamanini simdiye guncelleyip true doner - her API cagrisi boylece oturumu yeniler.
    public bool DogrulaVeYenile(string sessionId, TimeSpan bostaKalmaSiniri)
    {
        if (!_sonIslemZamani.TryGetValue(sessionId, out var sonIslem))
        {
            return false;
        }
        if (DateTime.UtcNow - sonIslem > bostaKalmaSiniri)
        {
            _sonIslemZamani.TryRemove(sessionId, out _);
            return false;
        }
        _sonIslemZamani[sessionId] = DateTime.UtcNow;
        return true;
    }

    public void OturumBitir(string sessionId) =>
        _sonIslemZamani.TryRemove(sessionId, out _);
}
