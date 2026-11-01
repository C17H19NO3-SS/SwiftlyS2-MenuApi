# SwiftlyS2 MenuApi (.NET 10)

Counter-Strike 2 için birebir **nostaljik CS:GO / SourceMod tarzı**, **1-7 silah tuşları** (`slot1` - `slot7`), **8 Geri**, **9 İleri**, **0 Çıkış** ve **Custom HUD (`CCSCustomHudLayout`)** destekli modüler Menu API kütüphanesi.

Bu eklenti, sunucudaki diğer tüm eklentilerin (`VIP`, `Admin`, `Shop`, `Bölge Yönetimi` vb.) oyunculara dinamik, etkileşimli ve kesintisiz menüler sunmasını sağlayan **Shared Interface (`IMenuApi`)** servis sağlayıcısıdır.

---

## 🌟 Temel Özellikler

1. **Sıfır Hareket Kısıtlaması (Serbest WASD):**
   - Menü açıkken oyuncunun yürümesi, koşması, zıplaması veya ateş etmesi kesinlikle engellenmez.
2. **Doğrudan Silah Tuşları ile Seçim (`1` - `7`, `8`, `9`, `0`):**
   - `1` - `7`: Menü seçeneğini doğrudan çalıştırır. Oyuncunun elindeki silahı değiştirmesi anında engellenir (`HookResult.Stop`).
   - `8`: **Geri** (Önceki sayfa veya üst menüye dönüş).
   - `9`: **İleri** (Sonraki sayfa).
   - `0`: **Çıkış** (`slot10` / `slot0` ile menüyü anında kapatır).
3. **Sessiz Chat Seçimi:**
   - Menü açıkken sohbete yazılan `1`-`9`, `!1`-`!9` veya `/1`-`/9` komutları sohbet kutusuna yazılmadan (`HookResult.Handled`) yutulur ve menü seçimi olarak işlenir.
4. **Hiyerarşik Alt Menüler (Submenu Stack):**
   - Menüler arası geçişlerde otomatik geçmiş yığını tutulur. Alt menüdeyken `8. Geri` tuşuna basıldığında otomatik olarak bir üst menüye geri dönülür.
5. **Dinamik Temalandırma (`MenuTheme`):**
   - Klasik altın sarısı Valve teması (`MenuTheme.Default`), `Blue`, `Green`, `Red`, `Purple` hazır temaları veya özel HEX renkleri ve piksel font boyutları (`SetFontSizePx`).
6. **Custom Panorama HUD Desteği:**
   - İsteğe bağlı `CCSCustomHudLayout` entity entegrasyonu ile harici Panorama XML layout'larını destekler.

---

## 🔌 Başka Bir Eklentiden Kullanım (Shared API)

`MenuApi`, SwiftlyS2'nin `IInterfaceManager` sistemine kayıtlıdır. Diğer eklentilerinizden kullanmak için:

### 1. Eklentinizde Shared Interface'i Çözümleyin
```csharp
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Plugins;
using MenuApi.API;

namespace MyPlugin;

public class MyPlugin : BasePlugin
{
    public static IMenuApi? MenuApi { get; private set; }

    public MyPlugin(ISwiftlyCore core) : base(core) { }

    public override void UseSharedInterface(IInterfaceManager interfaceManager)
    {
        if (interfaceManager.HasSharedInterface("MenuApi"))
        {
            MenuApi = interfaceManager.GetSharedInterface<IMenuApi>("MenuApi");
            Core.Logger.LogInformation("[MyPlugin] MenuApi arayüzü başarıyla bağlandı.");
        }
    }
}
```

### 2. Menü Oluşturun ve Açın
```csharp
public void OpenVipMenu(IPlayer player)
{
    if (MenuApi == null) return;

    var menu = MenuApi.CreateBuilder("vip.menu", "VIP AYARLARI")
        .SetSubtitle("Özel Yetkiler")
        .SetItemsPerPage(7)
        .SetTimeout(TimeSpan.FromSeconds(30))
        .SetTheme(MenuTheme.Purple)
        .AddToggle("Otomatik Bhop", true, (ctx, val) =>
        {
            ctx.Player.SendChat($"Bhop: {(val ? "[lime]Açık[/]" : "[red]Kapalı[/]")}");
        })
        .AddItem("Can Yenile (+50 HP)", ctx =>
        {
            var pawn = ctx.Player.PlayerPawn;
            if (pawn != null)
            {
                pawn.Health = Math.Min(100, pawn.Health + 50);
                ctx.Player.SendChat($"Canınız: [lime]{pawn.Health} HP[/]");
            }
        })
        .AddSubmenu("Özel Silahlar", ctx =>
        {
            return MenuApi.CreateBuilder("vip.weapons", "SİLAHLAR")
                .AddItem("Altın Deagle", c => c.Player.ExecuteCommand("give weapon_deagle"))
                .AddItem("Özel AWP", c => c.Player.ExecuteCommand("give weapon_awp"))
                .Build();
        })
        .Build();

    menu.Open(player);
}
```

---

## 🎮 Konsol Komutları

- `sw_select <1-10>`: Menüde konsol üzerinden seçim yapar (örn. `sw_select 1`).
- `sw_menuhud <xml_path>`: Özel Panorama XML HUD layoutunu bağlar (Örn: `sw_menuhud panorama/layout/custom_game/menu.xml`).

