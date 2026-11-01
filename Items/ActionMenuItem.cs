using MenuApi.API;

namespace MenuApi.Items;

public class ActionMenuItem : MenuItem
{
    public ActionMenuItem(string text, Action<MenuContext> onSelect, bool isEnabled = true, bool closeOnSelect = true, string? customColor = null)
        : base(text, onSelect, isEnabled, closeOnSelect, customColor)
    {
    }

    public ActionMenuItem(string text, Func<MenuContext, ValueTask> onSelectAsync, bool isEnabled = true, bool closeOnSelect = true, string? customColor = null)
        : base(text, onSelectAsync, isEnabled, closeOnSelect, customColor)
    {
    }
}
