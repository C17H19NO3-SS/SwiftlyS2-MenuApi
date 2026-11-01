using MenuApi.API;
using SwiftlyS2.Shared.Players;

namespace MenuApi.Items;

public class ToggleMenuItem : IMenuItem
{
    public string Text { get; set; }
    public bool Value { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsSelectable => true;
    public bool CloseOnSelect { get; set; }
    public string? CustomColor { get; set; }
    public Func<MenuContext, ValueTask>? OnSelectAsync { get; set; }
    public Action<MenuContext, bool>? OnToggle { get; set; }

    public ToggleMenuItem(string text, bool initialValue, Action<MenuContext, bool> onToggle, bool closeOnSelect = false, string? customColor = null)
    {
        Text = text;
        Value = initialValue;
        IsEnabled = true;
        CloseOnSelect = closeOnSelect;
        CustomColor = customColor;
        OnToggle = onToggle;

        OnSelectAsync = ctx =>
        {
            Value = !Value;
            OnToggle?.Invoke(ctx, Value);
            ctx.CloseMenu = CloseOnSelect;
            return ValueTask.CompletedTask;
        };
    }

    public async ValueTask ExecuteAsync(MenuContext context)
    {
        if (!IsEnabled) return;
        if (OnSelectAsync != null)
        {
            await OnSelectAsync(context);
        }
    }

    public string GetDisplayText(IPlayer player, int slotNumber, bool isHovered)
    {
        string status = Value ? "<span color=\"#55FF55\">[ACIK]</span>" : "<span color=\"#FF5555\">[KAPALI]</span>";
        return $"{Text} {status}";
    }
}
