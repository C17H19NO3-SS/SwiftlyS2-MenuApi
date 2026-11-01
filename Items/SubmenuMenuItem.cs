using MenuApi.API;
using SwiftlyS2.Shared.Players;

namespace MenuApi.Items;

public class SubmenuMenuItem : IMenuItem
{
    public string Text { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsSelectable => true;
    public bool CloseOnSelect { get; set; } = false;
    public string? CustomColor { get; set; }
    public Func<MenuContext, ValueTask>? OnSelectAsync { get; set; }

    public IMenu? Submenu { get; set; }
    public Func<MenuContext, IMenu>? SubmenuFactory { get; set; }

    public SubmenuMenuItem(string text, IMenu submenu, bool isEnabled = true, string? customColor = null)
    {
        Text = text;
        Submenu = submenu;
        IsEnabled = isEnabled;
        CustomColor = customColor;
        InitHandler();
    }

    public SubmenuMenuItem(string text, Func<MenuContext, IMenu> submenuFactory, bool isEnabled = true, string? customColor = null)
    {
        Text = text;
        SubmenuFactory = submenuFactory;
        IsEnabled = isEnabled;
        CustomColor = customColor;
        InitHandler();
    }

    private void InitHandler()
    {
        OnSelectAsync = ctx =>
        {
            var targetMenu = Submenu ?? SubmenuFactory?.Invoke(ctx);
            if (targetMenu != null)
            {
                ctx.CloseMenu = false;
            }
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
        return $"{Text} »";
    }
}
