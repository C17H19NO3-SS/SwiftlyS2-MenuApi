using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace MenuApi.Core;

public sealed class CustomHudService
{
    private readonly ISwiftlyCore _core;
    private CCSCustomHudLayout? _hudEntity;

    public string LayoutXmlPath { get; set; } = string.Empty;
    public bool IsActive => _hudEntity != null && _hudEntity.IsValid;

    public CustomHudService(ISwiftlyCore core)
    {
        _core = core;
    }

    public bool Initialize(string layoutXmlPath)
    {
        if (string.IsNullOrWhiteSpace(layoutXmlPath))
        {
            return false;
        }

        try
        {
            LayoutXmlPath = layoutXmlPath.Trim();

            _hudEntity = _core.EntitySystem.CreateEntity<CCSCustomHudLayout>();
            _hudEntity.StrLayout = LayoutXmlPath;
            _hudEntity.StrLayoutUpdated();
            _hudEntity.DispatchSpawn();

            _core.Logger.LogInformation("[MenuApi] CCSCustomHudLayout ({Path}) olusturuldu (Index: {Index}).", LayoutXmlPath, _hudEntity.Index);
            return true;
        }
        catch (Exception ex)
        {
            _core.Logger.LogError(ex, "[MenuApi] CCSCustomHudLayout olusturulurken hata oluştu.");
            return false;
        }
    }

    public void UpdatePlayerMenu(IPlayer player, string title, string? subtitle, List<string> items, string pageInfo, bool hasBack, bool hasNext)
    {
        if (_hudEntity == null || !_hudEntity.IsValid || player == null || !player.IsValid) return;

        int playerId = player.PlayerID;
        const string panelId = "custom_menu_panel";

        try
        {
            _hudEntity.SetDialogVariableStringForPlayer(playerId, panelId, "menu_title", title);
            _hudEntity.SetDialogVariableStringForPlayer(playerId, panelId, "menu_subtitle", subtitle ?? string.Empty);
            _hudEntity.SetDialogVariableStringForPlayer(playerId, panelId, "menu_page", pageInfo);

            for (int i = 0; i < 7; i++)
            {
                string text = (i < items.Count) ? items[i] : string.Empty;
                _hudEntity.SetDialogVariableStringForPlayer(playerId, panelId, $"item_{i + 1}", text);
            }

            _hudEntity.SetDialogVariableStringForPlayer(playerId, panelId, "item_8", hasBack ? "8. Geri" : string.Empty);
            _hudEntity.SetDialogVariableStringForPlayer(playerId, panelId, "item_9", hasNext ? "9. İleri" : string.Empty);
            _hudEntity.SetDialogVariableStringForPlayer(playerId, panelId, "item_0", "0. Çıkış");

            _hudEntity.SetHasClassForPlayer(playerId, panelId, "visible", EHudPanelClassStatus_t.k_eHudPanelClassStatus_HasClass);
        }
        catch (Exception ex)
        {
            _core.Logger.LogError(ex, "[MenuApi] Custom HUD guncellenirken hata.");
        }
    }

    public void ClosePlayerMenu(IPlayer player)
    {
        if (_hudEntity == null || !_hudEntity.IsValid || player == null || !player.IsValid) return;

        int playerId = player.PlayerID;
        try
        {
            _hudEntity.SetHasClassForPlayer(playerId, "custom_menu_panel", "visible", EHudPanelClassStatus_t.k_eHudPanelClassStatus_DoesNotHaveClass);
        }
        catch { }
    }

    public void Cleanup()
    {
        if (_hudEntity != null && _hudEntity.IsValid)
        {
            try
            {
                _hudEntity.AcceptInput("Kill", string.Empty);
            }
            catch { }
            _hudEntity = null;
        }
    }
}
