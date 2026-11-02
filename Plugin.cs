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

        // 1. Tick dongusu (Flicker fix ve session timeout/render)
        Core.Event.OnTick += OnTick;

        // 2. Konsol secim komutu: sw_select <1-10> (Konsoldan bind tuslari icin: bind 1 "sw_select 1")
        RegisterCommand("select", OnSelectCommand, "Menude secim yapar: sw_select <1-10>");

        // 3. Hizli slot ve sohbet komutlari: sw_1 .. sw_9, sw_0 (Sohbetten !1 .. !9, !0 secimleri)
        RegisterSlotCommands();

        // 4. Custom Panorama HUD yukleme komutu: sw_menuhud <layout_xml_path>
        RegisterCommand("menuhud", OnMenuHudCommand, "Custom Panorama HUD layoutu yukler: sw_menuhud <layout_xml_path>");

        // 5. Oyuncu olumunde menuyu temizle
        Core.GameEvent.HookPost<EventPlayerDeath>(OnPlayerDeath);

        Core.Logger.LogInformation(
            "[MenuApi] v1.3.1 basariyla yuklendi ve optimize komut dinleyicileri aktif! (HotReload: {HotReload})", hotReload);
    }

    public override void Unload()
    {
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

    private void RegisterSlotCommands()
    {
        for (int i = 1; i <= 9; i++)
        {
            int slot = i;
            RegisterCommand(slot.ToString(), ctx => OnDirectSlotCommand(ctx, slot), $"Menude {slot}. secenegi secer: sw_{slot} veya !{slot}");
        }

        RegisterCommand("0", ctx => OnDirectSlotCommand(ctx, 10), "Menuden cikar: sw_0 veya !0");
        RegisterCommand("10", ctx => OnDirectSlotCommand(ctx, 10), "Menuden cikar: sw_10 veya !10");
    }

    private void OnDirectSlotCommand(ICommandContext ctx, int slotNum)
    {
        if (!ctx.IsSentByPlayer || ctx.Sender == null)
            return;

        if (MenuManager.HasActiveMenu(ctx.Sender))
        {
            MenuManager.HandleSlotCommand(ctx.Sender.PlayerID, slotNum);
        }
    }

    private void OnSelectCommand(ICommandContext ctx)
    {
        if (!ctx.IsSentByPlayer || ctx.Sender == null)
            return;

        if (ctx.Args.Length == 0 || !int.TryParse(ctx.Args[0], out int slotNum))
            return;

        int mappedSlot = (slotNum == 0) ? 10 : slotNum;
        MenuManager.HandleSlotCommand(ctx.Sender.PlayerID, mappedSlot);
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
}
