using MenuApi.API;
using SwiftlyS2.Shared.Players;

namespace MenuApi.Items;

public class MenuItem : IMenuItem
{
    public string Text { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsSelectable { get; set; } = true;
    public bool CloseOnSelect { get; set; } = true;
    public string? CustomColor { get; set; }
    public Func<MenuContext, ValueTask>? OnSelectAsync { get; set; }

    public MenuItem(string text, Action<MenuContext>? onSelect = null, bool isEnabled = true, bool closeOnSelect = true, string? customColor = null)
    {
        Text = text;
        IsEnabled = isEnabled;
        CloseOnSelect = closeOnSelect;
        CustomColor = customColor;

        if (onSelect != null)
        {
            OnSelectAsync = ctx =>
            {
                onSelect(ctx);
                return ValueTask.CompletedTask;
            };
        }
    }

    public MenuItem(string text, Func<MenuContext, ValueTask>? onSelectAsync, bool isEnabled = true, bool closeOnSelect = true, string? customColor = null)
    {
        Text = text;
        IsEnabled = isEnabled;
        CloseOnSelect = closeOnSelect;
        CustomColor = customColor;
        OnSelectAsync = onSelectAsync;
    }

    public virtual async ValueTask ExecuteAsync(MenuContext context)
    {
        if (!IsEnabled || !IsSelectable) return;
        if (OnSelectAsync != null)
        {
            await OnSelectAsync(context);
        }
    }

    public virtual string GetDisplayText(IPlayer player, int slotNumber, bool isHovered)
    {
        return Text;
    }
}

