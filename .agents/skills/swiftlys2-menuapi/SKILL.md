---
name: swiftlys2-menuapi
description: >-
  Comprehensive guide and integration skillset for building, styling, and managing interactive
  Counter-Strike 2 (CS2) server menus using SwiftlyS2 MenuApi (.NET 10). Covers Shared API consumption
  (IMenuApi via IInterfaceManager), Fluent MenuBuilder, OOP MenuItems (Action, Toggle, Submenu, Text),
  dynamic MenuTheme customization (colors, font sizes), 1-7 weapon slot & chat selection, multi-page
  pagination, and complete code recipes for other CS2 plugins.
---

# SwiftlyS2 MenuApi Integration Guide & Reference

This skill is the authoritative, production-grade guide for consuming and building interactive in-game menus in Counter-Strike 2 using the **`MenuApi`** plugin on the **SwiftlyS2** (.NET 10) framework.

---

## 1. Quick Architecture & Shared API Overview

`MenuApi` is registered as a first-class **Shared Interface** in SwiftlyS2. Any other plugin running on the server can consume `IMenuApi` without tight binary coupling.

### Key Capabilities
- **Zero Global Hook Overhead:** No global `HookClientCommand` or `HookClientChat` listening; operations are purely command-driven for maximum server performance.
- **Console Bind Support (`sw_select` & `sw_1`..`sw_0`):** Players bind keys cleanly (e.g. `bind 1 "sw_select 1"`, `bind 8 "sw_select 8"`, `bind 0 "sw_select 0"`) or run `sw_1`..`sw_9`, `sw_0`.
- **Chat Selection (`!1`..`!9`, `!0`):** Players can type `!1`..`!7` for options, `!8` for Back, `!9` for Next, `!0` for Exit directly in chat.
- **Dynamic Themes (`MenuTheme`):** Valve nostalgic gold theme, Blue, Green, Red, Purple presets, or custom hex colors and pixel/Panorama font sizes.
- **Hierarchical Submenus:** Built-in history stack (`Stack<(IMenu, int)>`). Pressing `8. Geri` on page 1 of a submenu automatically navigates back to the parent menu.
- **Custom HUD Support:** Optional `CCSCustomHudLayout` entity support for dedicated Panorama XML designs alongside standard CenterHTML.

---

## 2. Consuming `IMenuApi` in Another Plugin

To use `MenuApi` from another SwiftlyS2 plugin (e.g. `VipPlugin`, `AdminPlugin`, `ShopPlugin`), follow the standard SwiftlyS2 Shared Interface lifecycle:

### Step 1: Add Reference or Define the API Interfaces
You can either reference `MenuApi.dll` (with `ExcludeAssets="runtime" PrivateAssets="all"` in `.csproj`) or include `IMenuApi`, `IMenu`, `IMenuBuilder`, `MenuTheme`, `MenuContext` interfaces.

### Step 2: Resolve `IMenuApi` in Plugin Lifecycle
```csharp
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Plugins;
using MenuApi.API;

namespace MyCustomPlugin;

[PluginMetadata(Id = "MyCustomPlugin", Version = "1.0.0", Name = "My Custom Plugin")]
public sealed class MyCustomPlugin : BasePlugin
{
    public static IMenuApi? MenuApi { get; private set; }

    public MyCustomPlugin(ISwiftlyCore core) : base(core) { }

    public override void UseSharedInterface(IInterfaceManager interfaceManager)
    {
        // 1. Resolve MenuApi provided by the MenuApi plugin
        if (interfaceManager.HasSharedInterface("MenuApi"))
        {
            MenuApi = interfaceManager.GetSharedInterface<IMenuApi>("MenuApi");
            Core.Logger.LogInformation("[MyCustomPlugin] MenuApi shared interface resolved successfully.");
        }
        else
        {
            Core.Logger.LogWarning("[MyCustomPlugin] MenuApi shared interface not found! Menus will be disabled.");
        }
    }

    public override void Load(bool hotReload)
    {
        // Register your commands (e.g. !vip, !shop)
    }

    public override void Unload()
    {
        // Cleanup
    }
}
```

---

## 3. Core API Reference

### 3.1 `IMenuApi`
```csharp
public interface IMenuApi
{
    // Create a new menu builder
    IMenuBuilder CreateBuilder(string id);
    IMenuBuilder CreateBuilder(string id, string title);

    // Session management
    bool HasActiveMenu(IPlayer player);
    IMenu? GetActiveMenu(IPlayer player);
    void CloseMenu(IPlayer player, MenuCloseReason reason = MenuCloseReason.UserExit);
    void CloseAll();
}
```

### 3.2 `IMenuBuilder` Fluent API
```csharp
builder
    .SetTitle("ANA MENÜ")                     // Menu title
    .SetSubtitle("Seçiminizi yapınız")        // Optional subtitle
    .SetItemsPerPage(7)                       // Items per page (1 to 7)
    .SetTimeout(TimeSpan.FromSeconds(30))     // Auto-close after inactivity (TimeSpan.Zero to disable)
    .EnableSounds(true)                       // Play CS2 UI sounds on scroll, accept, back
    .EnableSlotSelection(true)                // Enable 1-7, 8 Geri, 9 Ileri, 0 Cikis
    .EnableChatSelection(true)                // Enable typing 1-9 in chat
    .SetTheme(MenuTheme.Default)              // Preset or custom theme
    .ConfigureTheme(theme => { ... })         // In-line theme adjustments
    .SetFontSize(14)                          // Option text font size (pixels or Panorama class)
    .SetTitleFontSize(16)                     // Title font size (pixels or Panorama class)
    .Add(IMenuItem item)                      // Add pre-constructed OOP item
    .AddItem(text, ctx => { ... })            // Quick action item
    .AddItemAsync(text, async ctx => { ... }) // Asynchronous action item
    .AddToggle(text, initialVal, (ctx, v) => {}) // Toggle switch item
    .AddSubmenu(text, submenu)                // Static submenu
    .AddSubmenu(text, ctx => BuildSubmenu())  // Dynamic submenu factory
    .AddText("Bilgi metni", color: "#888888") // Informational separator
    .AddSpacer()                              // Blank space separator
    .Build();                                 // Compiles to IMenu
```

### 3.3 `MenuCloseReason` Enum
- `UserExit`: Closed via `0` key, `ESC`, or console.
- `ItemSelected`: Closed automatically after executing a selectable item (`closeOnSelect = true`).
- `Timeout`: Closed due to inactivity timeout.
- `PlayerDeath`: Automatically closed when the player dies.
- `PlayerDisconnect`: Cleaned up when the player disconnects.
- `Overridden`: Closed because another menu was opened for the player.
- `PluginUnload`: Closed during plugin reload or unload.

---

## 4. Object-Oriented (OOP) Item Elements

`MenuApi` supports modular, reusable item classes under `MenuApi.Items`:

### 1. `MenuItem` / `ActionMenuItem` (Standard Action Item)
Executes a synchronous or asynchronous callback upon selection:
```csharp
var item = new MenuItem("AK-47 ($2700)", context =>
{
    context.Player.SendChat("[gold][Shop][/] [lime]AK-47 satın aldınız![/]");
    context.Player.ExecuteCommand("give weapon_ak47");
}, isEnabled: true, closeOnSelect: true, customColor: "#FFFFFF");
```
- `isEnabled: false`: Renders in grey (`DisabledTextColor`), slot selection plays invalid sound and ignores execution.
- `closeOnSelect: false`: Keeps the menu open after selection (ideal for multi-item buys).
- `customColor`: Overrides theme item color (e.g. `"#FF5555"` for dangerous actions).

### 2. `ToggleMenuItem` (On/Off Switch Item)
Displays live `[AÇIK]` (green) or `[KAPALI]` (red) status badges, automatically flips state on click, and keeps the menu open:
```csharp
var toggle = new ToggleMenuItem("Hasar Bildirimi", initialValue: true, (context, newValue) =>
{
    // Save to player preferences or database
    context.Player.SendChat($"Hasar bildirimi: {(newValue ? "[lime]Açık[/]" : "[red]Kapalı[/]")}");
}, closeOnSelect: false);
```

### 3. `SubmenuMenuItem` (Nested Menus)
Links to a static `IMenu` or dynamically constructs a submenu via factory:
```csharp
// Static submenu:
var subItem = new SubmenuMenuItem("Ses Ayarları", soundMenu);

// Dynamic factory (evaluates player state/permissions at moment of click):
var dynamicSubItem = new SubmenuMenuItem("Admin İşlemleri", context =>
{
    return MenuApi.CreateBuilder("admin.players")
        .SetTitle("OYUNCU LİSTESİ")
        .AddItem("Tümünü Sustur", c => { ... })
        .Build();
});
```
*Note: Submenus automatically display a trailing `»` glyph.*

### 4. `TextMenuItem` (Separators & Info Text)
Non-selectable decorative text or spacers that do not consume a slot number:
```csharp
builder.Add(new TextMenuItem("--- Sunucu Kuralları ---", customColor: "#FFD700"));
builder.Add(new TextMenuItem("Hile ve küfür yasaktır.", customColor: "#AAAAAA"));
builder.AddSpacer();
```

---

## 5. Themes & Styling (`MenuTheme`)

### Built-in Presets
```csharp
MenuTheme.Default  // Nostalgic CS:GO Gold & White (#FFB000, #FFFFFF)
MenuTheme.Blue     // Cyan / Ice Blue (#00BFFF, #1E90FF)
MenuTheme.Green    // Lime / Military Green (#00FF7F, #32CD32)
MenuTheme.Red      // Fiery Orange / Crimson (#FF4500, #FF6347)
MenuTheme.Purple   // VIP Orchid / Purple (#DA70D6, #BA55D3)
```

### Customizing Theme Properties
```csharp
var customTheme = new MenuTheme
{
    TitleColor = "#00FFAA",
    TitlePrefix = "★ [ ",
    TitleSuffix = " ] ★",
    SubtitleColor = "#B0C4DE",
    SlotNumberColor = "#00FFAA",
    ItemTextColor = "#FFFFFF",
    DisabledTextColor = "#555555",
    InfoTextColor = "#888888",
    PageInfoColor = "#888888",
    BackText = "Önceki",
    NextText = "Sonraki",
    ExitText = "Kapat",
    ExitColor = "#FF5555",
    ExitSlotColor = "#FF5555"
};

// Set font sizes via pixels or Panorama classes:
customTheme.SetFontSizePx(13);      // Generates inline font-size and matching Panorama class
customTheme.SetTitleFontSizePx(15); // Generates title styling
```

---

## 6. End-to-End Recipes

### Recipe 1: Complete VIP Menu with Toggles & Submenus
```csharp
public void OpenVipMenu(IPlayer player)
{
    if (MenuApi == null) return;

    var menu = MenuApi.CreateBuilder("vip.main", "VIP AYARLARI")
        .SetSubtitle("Özel Yetkiler ve Yapılandırma")
        .SetItemsPerPage(7)
        .SetTimeout(TimeSpan.FromSeconds(30))
        .SetTheme(MenuTheme.Purple)
        .AddToggle("Otomatik Bhop", true, (ctx, val) =>
        {
            ctx.Player.SendChat($"[purple][VIP][/] Otomatik Bhop: {(val ? "[lime]Açık[/]" : "[red]Kapalı[/]")}");
        })
        .AddToggle("Hasar Efekti", false, (ctx, val) =>
        {
            ctx.Player.SendChat($"[purple][VIP][/] Hasar Efekti: {(val ? "[lime]Açık[/]" : "[red]Kapalı[/]")}");
        })
        .AddItem("Can Yenile (+50 HP)", ctx =>
        {
            var pawn = ctx.Player.PlayerPawn;
            if (pawn != null)
            {
                pawn.Health = Math.Min(100, pawn.Health + 50);
                ctx.Player.SendChat($"[purple][VIP][/] Canınız yenilendi: [lime]{pawn.Health} HP[/]");
            }
        }, closeOnSelect: false)
        .AddSubmenu("Özel Silahlar", ctx =>
        {
            return MenuApi.CreateBuilder("vip.weapons", "VIP SİLAHLAR")
                .SetTheme(MenuTheme.Purple)
                .AddItem("Altın Deagle", c => c.Player.ExecuteCommand("give weapon_deagle"))
                .AddItem("Özel AWP", c => c.Player.ExecuteCommand("give weapon_awp"))
                .Build();
        })
        .Build();

    menu.Open(player);
}
```

### Recipe 2: Paginated Player Action Menu (Kick / Slay)
```csharp
public void OpenPlayerManagerMenu(IPlayer admin, ISwiftlyCore core)
{
    if (MenuApi == null) return;

    var builder = MenuApi.CreateBuilder("admin.kick_menu", "OYUNCU SEÇİN")
        .SetSubtitle("Yönetmek istediğiniz oyuncuya tıklayın")
        .SetItemsPerPage(7)
        .SetTheme(MenuTheme.Red);

    foreach (var target in core.PlayerManager.GetAllPlayers())
    {
        if (target == null || !target.IsValid || target.IsBot) continue;

        string targetName = target.Name;
        int targetId = target.PlayerID;

        builder.AddItem(targetName, ctx =>
        {
            // Submenu for target actions
            var actionMenu = MenuApi.CreateBuilder($"admin.actions.{targetId}", targetName)
                .SetTheme(MenuTheme.Red)
                .AddItem("Slay (Öldür)", c => c.Player.ExecuteCommand($"sw_slay #{targetId}"))
                .AddItem("Kick (Sunucudan At)", c => c.Player.ExecuteCommand($"sw_kick #{targetId}"))
                .Build();

            actionMenu.Open(ctx.Player);
        });
    }

    builder.Build().Open(admin);
}
```

---

## 7. Best Practices & Troubleshooting

1. **Always Use `IInterfaceManager` in `UseSharedInterface`:**
   Do not resolve `MenuApi` inside the constructor or before `UseSharedInterface` is invoked by SwiftlyS2.
2. **Never Block the Game Thread:**
   For async work (e.g. database transactions or HTTP requests), use `AddItemAsync` or fire a task with exception logging.
3. **Multi-Page Menus:**
   When adding more than 7 items, `MenuApi` automatically creates pages (`Sayfa 1/2`), populates slot `9. İleri` and slot `8. Geri`, and transitions pages seamlessly on key presses.
4. **Closing Menus on Event Handlers:**
   `MenuApi` already handles player death (`EventPlayerDeath`). For custom game modes (e.g. round start, map transition), call `MenuApi.CloseMenu(player)` or `MenuApi.CloseAll()`.

