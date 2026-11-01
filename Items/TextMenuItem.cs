using MenuApi.API;
using SwiftlyS2.Shared.Players;

namespace MenuApi.Items;

public class TextMenuItem : IMenuItem
{
    public string Text { get; set; }
    public bool IsEnabled { get; set; } = false;
    public bool IsSelectable => false;
    public bool CloseOnSelect { get; set; } = false;
    public string? CustomColor { get; set; }
    public Func<MenuContext, ValueTask>? OnSelectAsync { get; set; } = null;

    public TextMenuItem(string text, string? customColor = null)
    {
        Text = text;
        CustomColor = customColor;
    }

    public ValueTask ExecuteAsync(MenuContext context)
    {
        return ValueTask.CompletedTask;
    }

    public string GetDisplayText(IPlayer player, int slotNumber, bool isHovered)
    {
        return Text;
    }
}
