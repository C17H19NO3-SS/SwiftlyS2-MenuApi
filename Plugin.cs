using MenuApi.API;
using MenuApi.Core;
using MenuApi.Items;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Commands;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Plugins;

namespace MenuApi;

[PluginMetadata(
    Id = "MenuApi",
    Version = "1.3.1",
    Name = "MenuApi",
    Author = "SoulSnatcher",
    Description = "CS2 icin dinamik temali, nesne tabanli (OOP) element destekli ve 1-9 silah tuslari ile calisan Menu API eklentisi."
)]
public sealed class Plugin(ISwiftlyCore core) : BasePlugin(core)
{
    public static Plugin? Instance { get; private set; }

    public MenuManager MenuManager { get; private set; } = null!;
    public IMenuApi MenuApi { get; private set; } = null!;

    private Guid _clientCommandHookGuid;
    private Guid _clientChatHookGuid;
    private readonly List<Guid> _commandGuids = new();

    public override void ConfigureSharedInterface(IInterfaceManager interfaceManager)
    {
        MenuManager = new MenuManager(Core);
        MenuApi = new MenuApiImpl(MenuManager);

        interfaceManager.AddSharedInterface<IMenuApi, MenuApiImpl>("MenuApi", (MenuApiImpl)MenuApi);
        Core.Logger.LogInformation("[MenuApi] IMenuApi paylasimli arayuzu (Shared API) basariyla kaydedildi.");
    }

    public override void Load(bool hotReload)
    {
        Instance = this;

        if (MenuManager == null)
        {
            MenuManager = new MenuManager(Core);
            MenuApi = new MenuApiImpl(MenuManager);
        }

        // 1. Client komut hook'u (slot1 - slot10 silah tuslari yakalama ve console komutlari)
        _clientCommandHookGuid = Core.Command.HookClientCommand(OnClientCommand);

        // 2. Client sohbet hook'u (!menu, /menu, 1, 2.. chat secimleri)
        _clientChatHookGuid = Core.Command.HookClientChat(OnClientChat);

        // 3. Tick dongusu
        Core.Event.OnTick += OnTick;

        // 4. Konsol Komutlari
        RegisterCommand("menuhud", OnMenuHudCommand, "Custom Panorama HUD layoutu yukler: sw_menuhud <layout_xml_path>");
        RegisterCommand("select", OnSelectCommand, "Menude secim yapar: sw_select <1-10>");

        // 5. Oyuncu olumunde menuyu temizle
        Core.GameEvent.HookPost<EventPlayerDeath>(OnPlayerDeath);

        Core.Logger.LogInformation(
            "[MenuApi] v1.3.1 basariyla yuklendi ve Shared API servisi aktif! (HotReload: {HotReload})", hotReload);
    }

    public override void Unload()
    {
        if (_clientCommandHookGuid != Guid.Empty)
        {
            try { Core.Command.UnhookClientCommand(_clientCommandHookGuid); } catch { }
            _clientCommandHookGuid = Guid.Empty;
        }

        if (_clientChatHookGuid != Guid.Empty)
        {
            try { Core.Command.UnhookClientChat(_clientChatHookGuid); } catch { }
            _clientChatHookGuid = Guid.Empty;
        }

        Core.Event.OnTick -= OnTick;

        foreach (var guid in _commandGuids)
        {
            try { Core.Command.UnregisterCommand(guid); } catch { }
        }
        _commandGuids.Clear();

        MenuManager?.CloseAll();
        Core.Logger.LogInformation("[MenuApi] Eklenti durduruldu ve tum menuler kapatildi.");
    }

    private void OnTick()
    {
        // 1. MenuFlickeringFix: Panorama HUD titremesini onleme
        try
        {
            var gameRules = Core.EntitySystem.GetGameRules();
            if (gameRules is { IsValid: true, WarmupPeriod: false })
            {
                gameRules.GameRestart = gameRules.RestartRoundTime.Value < Core.Engine.GlobalVars.CurrentTime;
            }
        }
        catch { }

        // 2. Menu oturumlari tick kontrolu
        MenuManager?.OnTick();
    }

    private HookResult OnClientCommand(int playerId, string commandLine)
    {
        if (string.IsNullOrWhiteSpace(commandLine))
            return HookResult.Continue;

        string cmd = commandLine.Trim().Trim('"').Trim(';').Trim();

        // Silah tuslari ve secim komutlari:
        // - slot1, slot2 ... slot10, slot0
        // - select1, select2 ... select10, select0
        // - select 1..10, menuselect 1..10, sw_select 1..10
        int slotNum = -1;

        if (cmd.StartsWith("slot", StringComparison.OrdinalIgnoreCase))
        {
            string slotStr = cmd.Substring(4).Trim();
            int.TryParse(slotStr, out slotNum);
        }
        else if (cmd.StartsWith("select", StringComparison.OrdinalIgnoreCase))
        {
            string rem = cmd.Substring(6).Trim();
            int.TryParse(rem, out slotNum);
        }
        else if (cmd.StartsWith("menuselect", StringComparison.OrdinalIgnoreCase))
        {
            string rem = cmd.Substring(10).Trim();
            int.TryParse(rem, out slotNum);
        }
        else if (cmd.StartsWith("sw_select", StringComparison.OrdinalIgnoreCase))
        {
            string rem = cmd.Substring(9).Trim();
            int.TryParse(rem, out slotNum);
        }

        if (slotNum != -1)
        {
            Core.Logger.LogInformation("[MenuApi] Komut yakalandi: Cmd='{Cmd}', Player={PlayerId}, Slot={Slot}", cmd, playerId, slotNum);
            return MenuManager.HandleSlotCommand(playerId, slotNum);
        }

        return HookResult.Continue;
    }

    private HookResult OnClientChat(int playerId, string text, bool teamOnly)
    {
        if (string.IsNullOrWhiteSpace(text))
            return HookResult.Continue;

        string trimmed = text.Trim();

        // 1 .. 9, 0 veya !1 .. !9, !0 veya /1 .. /9, /0 chat secimleri:
        // Eger menu aciksa secimi yap ve sohbeti TAMAMEN gizle (chate gitmesin)!
        string numStr = trimmed;
        if (numStr.StartsWith("!") || numStr.StartsWith("/"))
        {
            numStr = numStr[1..].Trim();
        }

        if (int.TryParse(numStr, out int num) && num >= 0 && num <= 9)
        {
            int mappedSlot = (num == 0) ? 10 : num;
            
            var player = Core.PlayerManager.GetPlayer(playerId);
            if (player != null && player.IsValid && MenuManager.HasActiveMenu(player))
            {
                MenuManager.HandleSlotCommand(playerId, mappedSlot);
            }

            return HookResult.Handled;
        }

        return MenuManager.HandleChatInput(playerId, text);
    }

    private HookResult OnPlayerDeath(EventPlayerDeath @event)
    {
        var victim = @event.UserIdPlayer;
        if (victim != null && victim.IsValid)
        {
            MenuManager.CloseMenu(victim, MenuCloseReason.PlayerDeath);
        }
        return HookResult.Continue;
    }

    private void RegisterCommand(string name, ICommandService.CommandListener handler, string help)
    {
        try
        {
            var guid = Core.Command.RegisterCommand(name, handler, registerRaw: false, permission: string.Empty, helpText: help);
            _commandGuids.Add(guid);
        }
        catch (Exception ex)
        {
            Core.Logger.LogError(ex, "[MenuApi] Komut kaydederken hata: {Name}", name);
        }
    }

    private void OnSelectCommand(ICommandContext ctx)
    {
        if (!ctx.IsSentByPlayer || ctx.Sender == null)
        {
            ctx.Reply("[MenuApi] Bu komut sadece oyuncular tarafindan kullanilabilir.");
            return;
        }

        if (ctx.Args.Length == 0 || !int.TryParse(ctx.Args[0], out int slotNum))
        {
            ctx.Reply("[MenuApi] Kullanim: sw_select <1-10>");
            return;
        }

        MenuManager.HandleSlotCommand(ctx.Sender.PlayerID, slotNum);
    }

    private void OnMenuHudCommand(ICommandContext ctx)
    {
        if (ctx.Args.Length == 0)
        {
            ctx.Reply("[MenuApi] Kullanim: sw_menuhud <panorama_layout_xml_yolu> (Ornek: panorama/layout/custom_game/menu.xml)");
            return;
        }

        string layoutPath = ctx.Args[0];
        bool ok = MenuManager.CustomHud.Initialize(layoutPath);
        if (ok)
        {
            ctx.Reply($"[MenuApi] CCSCustomHudLayout basariyla aktif edildi: {layoutPath}");
        }
        else
        {
            ctx.Reply($"[MenuApi] CCSCustomHudLayout olusturulamadi: {layoutPath}");
        }
    }
}
