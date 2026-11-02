using SwiftlyS2.Shared.Players;

namespace MenuApi.API;

public enum MenuCloseReason
{
    UserExit,        // 0 tuşu veya ESC / R ile kapatıldı
    ItemSelected,    // Bir seçenek seçildi ve menü kapanacak şekilde ayarlandı
    Timeout,         // Otomatik kapanma süresi doldu
    PlayerDisconnect,// Oyuncu sunucudan ayrıldı
    PlayerDeath,     // Oyuncu öldü
    Overridden,      // Başka bir menü açıldığı için eskisi kapandı
    PluginUnload     // Eklenti kapandı
}

public sealed class MenuContext
{
    public required IPlayer Player { get; init; }
    public required IMenu Menu { get; init; }
    public IMenuItem? Item { get; init; }
    public int SlotIndex { get; init; } = -1;
    public int PageIndex { get; init; } = 0;
    public bool CloseMenu { get; set; } = true;
    public bool Handled { get; set; } = true;
}

