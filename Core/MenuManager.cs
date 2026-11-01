using System.Collections.Concurrent;
using MenuApi.API;
using MenuApi.Items;
using MenuApi.Rendering;
using MenuApi.Sound;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Players;

namespace MenuApi.Core;

public class MenuManager
{
    public static MenuManager? Instance { get; private set; }

    private readonly ISwiftlyCore _core;
    private readonly ConcurrentDictionary<int, MenuSession> _sessions = new();

    public CustomHudService CustomHud { get; }

    public MenuManager(ISwiftlyCore core)
    {
        _core = core;
        CustomHud = new CustomHudService(core);
        Instance = this;
    }

    public bool HasActiveMenu(IPlayer player)
    {
        if (player == null || !player.IsValid) return false;
        return _sessions.ContainsKey(player.PlayerID);
    }

    public IMenu? GetActiveMenu(IPlayer player)
    {
        if (player == null || !player.IsValid) return null;
        return _sessions.TryGetValue(player.PlayerID, out var session) ? session.CurrentMenu : null;
    }

    public void OpenMenu(IPlayer player, IMenu menu, int initialPage = 0)
    {
        if (player == null || !player.IsValid) return;

        int playerId = player.PlayerID;

        // Varsa eski menüyü kapat
        _sessions.TryRemove(playerId, out _);

        var session = new MenuSession(player, menu, initialPage);
        _sessions[playerId] = session;

        if (menu.PlaySounds)
        {
            MenuSounds.Play(player, MenuSounds.Scroll);
        }

        RenderSession(session);
    }

    public void CloseMenu(IPlayer player, MenuCloseReason reason = MenuCloseReason.UserExit)
    {
        if (player == null || !player.IsValid) return;

        int playerId = player.PlayerID;
        if (_sessions.TryRemove(playerId, out var session))
        {
            try
            {
                // Ekrandaki CenterHTML'i temizle
                player.SendCenterHTML(" ", 1);

                // Custom HUD aktifse onu da kapat
                CustomHud.ClosePlayerMenu(player);

                if (session.CurrentMenu.PlaySounds && reason == MenuCloseReason.UserExit)
                {
                    MenuSounds.Play(player, MenuSounds.Close);
                }
            }
            catch
            {
                // Oyuncu çıkmış olabilir
            }
        }
    }

    public void CloseAll()
    {
        foreach (var kvp in _sessions)
        {
            try
            {
                var player = _core.PlayerManager.GetPlayer(kvp.Key);
                if (player != null && player.IsValid)
                {
                    player.SendCenterHTML(" ", 1);
                    CustomHud.ClosePlayerMenu(player);
                }
            }
            catch { }
        }
        _sessions.Clear();
        CustomHud.Cleanup();
    }

    public void OnTick()
    {
        if (_sessions.IsEmpty) return;

        foreach (var kvp in _sessions)
        {
            int playerId = kvp.Key;
            var session = kvp.Value;
            var player = session.Player;

            if (player == null || !player.IsValid)
            {
                _sessions.TryRemove(playerId, out _);
                continue;
            }

            // Süre aşımı kontrolü
            if (session.IsExpired())
            {
                CloseMenu(player, MenuCloseReason.Timeout);
                continue;
            }

            // DİKKAT: WASD kesinlikle dinlenmez! Oyuncu serbestçe yürür.

            // Ekran render yenileme:
            // SADECE menü içeriği/sayfası değiştiğinde (IsDirty) render edilir!
            // Tick başı gereksiz SendCenterHTML çağrısı Panorama HUD'ını resetleyip FLICKER (titreme) yapar.
            // Süre bitimine yaklaşırken CenterHTML'in kaybolmaması için sadece 1 saniyede bir (64 tick) tazelenir.
            session.RenderTickCount++;
            if (session.IsDirty || session.RenderTickCount >= 64)
            {
                session.RenderTickCount = 0;
                session.IsDirty = false;
                RenderSession(session);
            }
        }
    }

    public HookResult HandleSlotCommand(int playerId, int slotNumber)
    {
        if (!_sessions.TryGetValue(playerId, out var session))
        {
            return HookResult.Continue;
        }

        if (!session.CurrentMenu.AllowSlotSelection)
        {
            return HookResult.Continue;
        }

        var player = session.Player;
        if (player == null || !player.IsValid)
        {
            return HookResult.Continue;
        }

        session.ResetInteraction();

        // 1-7: Menü öğeleri doğrudan çalıştırılır
        if (slotNumber >= 1 && slotNumber <= 7)
        {
            ExecuteSlot(session, slotNumber);
            return HookResult.Stop; // Silah değişimini engelle!
        }
        // 8: Geri
        else if (slotNumber == 8)
        {
            if (session.HasPreviousPage())
            {
                session.PreviousPage();
                if (session.CurrentMenu.PlaySounds)
                    MenuSounds.Play(player, MenuSounds.Scroll);
                RenderSession(session);
            }
            else if (session.PopSubmenu())
            {
                if (session.CurrentMenu.PlaySounds)
                    MenuSounds.Play(player, MenuSounds.Scroll);
                RenderSession(session);
            }
            else
            {
                if (session.CurrentMenu.PlaySounds)
                    MenuSounds.Play(player, MenuSounds.Invalid);
            }
            return HookResult.Stop;
        }
        // 9: İleri
        else if (slotNumber == 9)
        {
            if (session.HasNextPage())
            {
                session.NextPage();
                if (session.CurrentMenu.PlaySounds)
                    MenuSounds.Play(player, MenuSounds.Scroll);
                RenderSession(session);
            }
            else
            {
                if (session.CurrentMenu.PlaySounds)
                    MenuSounds.Play(player, MenuSounds.Invalid);
            }
            return HookResult.Stop;
        }
        // 10 veya 0: Çıkış
        else if (slotNumber == 10 || slotNumber == 0)
        {
            CloseMenu(player, MenuCloseReason.UserExit);
            return HookResult.Stop;
        }

        return HookResult.Continue;
    }

    public HookResult HandleChatInput(int playerId, string text)
    {
        if (!_sessions.TryGetValue(playerId, out var session))
        {
            return HookResult.Continue;
        }

        if (!session.CurrentMenu.AllowChatSelection)
        {
            return HookResult.Continue;
        }

        string trimmed = (text ?? string.Empty).Trim();
        if (trimmed.StartsWith("!") || trimmed.StartsWith("/"))
        {
            trimmed = trimmed[1..].Trim();
        }

        if (int.TryParse(trimmed, out int num))
        {
            if (num >= 0 && num <= 9)
            {
                int mappedSlot = (num == 0) ? 10 : num;
                HandleSlotCommand(playerId, mappedSlot);
                return HookResult.Handled; // Sohbet kutusuna yazılmasını tamamen engelle (gizle)!
            }
        }

        return HookResult.Continue;
    }

    private void ExecuteSlot(MenuSession session, int slotNumber)
    {
        var player = session.Player;
        var menu = session.CurrentMenu;
        int perPage = Math.Clamp(menu.ItemsPerPage, 1, 7);
        int itemIndex = (session.CurrentPage * perPage) + (slotNumber - 1);

        if (itemIndex < 0 || itemIndex >= menu.Items.Count)
        {
            if (menu.PlaySounds)
                MenuSounds.Play(player, MenuSounds.Invalid);
            return;
        }

        var item = menu.Items[itemIndex];
        if (!item.IsSelectable || !item.IsEnabled)
        {
            if (menu.PlaySounds)
                MenuSounds.Play(player, MenuSounds.Invalid);
            return;
        }

        // Submenu kontrolü
        if (item is SubmenuMenuItem submenuItem)
        {
            var targetMenu = submenuItem.Submenu;
            if (targetMenu == null && submenuItem.SubmenuFactory != null)
            {
                var factoryContext = new MenuContext
                {
                    Player = player,
                    Menu = menu,
                    Item = item,
                    SlotIndex = slotNumber,
                    PageIndex = session.CurrentPage,
                    CloseMenu = false
                };
                targetMenu = submenuItem.SubmenuFactory(factoryContext);
            }

            if (targetMenu != null)
            {
                if (menu.PlaySounds)
                    MenuSounds.Play(player, MenuSounds.Select);

                session.PushSubmenu(targetMenu);
                RenderSession(session);
                return;
            }
        }

        // Normal öğe çalıştırma
        var context = new MenuContext
        {
            Player = player,
            Menu = menu,
            Item = item,
            SlotIndex = slotNumber,
            PageIndex = session.CurrentPage,
            CloseMenu = item.CloseOnSelect
        };

        if (menu.PlaySounds)
            MenuSounds.Play(player, MenuSounds.Select);

        try
        {
            var task = item.ExecuteAsync(context);
            if (!task.IsCompletedSuccessfully)
            {
                task.AsTask().ContinueWith(t =>
                {
                    if (t.IsFaulted && t.Exception != null)
                    {
                        _core.Logger.LogError(t.Exception, "[MenuApi] Menü öğesi çalıştırılırken hata: {Item}", item.Text);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _core.Logger.LogError(ex, "[MenuApi] Menü öğesi hatası: {Item}", item.Text);
        }

        if (context.CloseMenu)
        {
            CloseMenu(player, MenuCloseReason.ItemSelected);
        }
        else
        {
            session.IsDirty = true;
            RenderSession(session);
        }
    }

    private void RenderSession(MenuSession session)
    {
        var player = session.Player;
        if (player == null || !player.IsValid) return;

        // Custom HUD desteği varsa değişkenleri aktar
        if (CustomHud.IsActive)
        {
            int perPage = Math.Clamp(session.CurrentMenu.ItemsPerPage, 1, 7);
            int startIndex = session.CurrentPage * perPage;
            int endIndex = Math.Min(startIndex + perPage, session.CurrentMenu.Items.Count);

            var itemsList = new List<string>();
            int slot = 1;
            for (int i = startIndex; i < endIndex; i++)
            {
                var it = session.CurrentMenu.Items[i];
                itemsList.Add($"{slot}. {it.GetDisplayText(player, slot, false)}");
                slot++;
            }

            int totalPages = session.GetTotalPages();
            string pageInfo = totalPages > 1 ? $"Sayfa {session.CurrentPage + 1}/{totalPages}" : string.Empty;

            CustomHud.UpdatePlayerMenu(
                player,
                session.CurrentMenu.Title,
                session.CurrentMenu.Subtitle,
                itemsList,
                pageInfo,
                session.HasPreviousPage() || session.History.Count > 0,
                session.HasNextPage()
            );
        }

        // Dinamik Tema & Nesne Tabanlı CenterHTML Render
        string html = MenuRenderer.Render(
            player,
            session.CurrentMenu,
            session.CurrentPage,
            session.HasPreviousPage(),
            session.HasNextPage(),
            session.History.Count > 0
        );

        // İçerik değişmediyse ve periyodik yenileme yapılıyorsa istemciye 3 saniyelik buffer ile gönder
        session.LastRenderedHtml = html;
        player.SendCenterHTML(html, 30);
    }
}
