using MenuApi.API;
using MenuApi.Items;
using SwiftlyS2.Shared.Players;

namespace MenuApi.Core;

public class MenuBuilder : IMenuBuilder
{
    private readonly Menu _menu;

    public MenuBuilder(string id, string title = "Menu")
    {
        _menu = new Menu(id, title);
    }

    public IMenuBuilder SetTitle(string title)
    {
        _menu.Title = title;
        return this;
    }

    public IMenuBuilder SetSubtitle(string subtitle)
    {
        _menu.Subtitle = subtitle;
        return this;
    }

    public IMenuBuilder SetItemsPerPage(int itemsPerPage)
    {
        _menu.ItemsPerPage = Math.Clamp(itemsPerPage, 1, 7);
        return this;
    }

    public IMenuBuilder SetTimeout(TimeSpan timeout)
    {
        _menu.Timeout = timeout;
        return this;
    }

    public IMenuBuilder SetTheme(MenuTheme theme)
    {
        _menu.Theme = theme;
        return this;
    }

    public IMenuBuilder ConfigureTheme(Action<MenuTheme> configure)
    {
        configure?.Invoke(_menu.Theme);
        return this;
    }

    public IMenuBuilder SetFontSize(int px)
    {
        _menu.Theme.SetFontSize(px);
        return this;
    }

    public IMenuBuilder SetFontSize(string sizeOrClass)
    {
        _menu.Theme.SetFontSize(sizeOrClass);
        return this;
    }

    public IMenuBuilder SetTitleFontSize(int px)
    {
        _menu.Theme.SetTitleFontSize(px);
        return this;
    }

    public IMenuBuilder SetTitleFontSize(string sizeOrClass)
    {
        _menu.Theme.SetTitleFontSize(sizeOrClass);
        return this;
    }

    public IMenuBuilder SetFontSizePx(int px)
    {
        _menu.Theme.SetFontSizePx(px);
        return this;
    }

    public IMenuBuilder SetTitleFontSizePx(int px)
    {
        _menu.Theme.SetTitleFontSizePx(px);
        return this;
    }

    public IMenuBuilder EnableSounds(bool enabled = true)
    {
        _menu.PlaySounds = enabled;
        return this;
    }

    public IMenuBuilder EnableWasdNavigation(bool enabled = false)
    {
        _menu.AllowWasdNavigation = enabled;
        return this;
    }

    public IMenuBuilder EnableSlotSelection(bool enabled = true)
    {
        _menu.AllowSlotSelection = enabled;
        return this;
    }

    public IMenuBuilder EnableChatSelection(bool enabled = true)
    {
        _menu.AllowChatSelection = enabled;
        return this;
    }

    public IMenuBuilder SetFreezePlayer(bool freeze = true)
    {
        _menu.FreezePlayer = freeze;
        return this;
    }

    // Nesne tabanlı öğe ekleme (Object-Oriented Element API)
    public IMenuBuilder Add(IMenuItem item)
    {
        if (item != null)
        {
            _menu.AddItem(item);
        }
        return this;
    }

    public IMenuBuilder AddItem(string text, Action<MenuContext> onSelect, bool enabled = true, bool closeOnSelect = true)
    {
        _menu.AddItem(new ActionMenuItem(text, onSelect, enabled, closeOnSelect));
        return this;
    }

    public IMenuBuilder AddItem(string text, Action<MenuContext> onSelect, string customColor, bool enabled = true, bool closeOnSelect = true)
    {
        _menu.AddItem(new ActionMenuItem(text, onSelect, enabled, closeOnSelect, customColor));
        return this;
    }

    public IMenuBuilder AddItemAsync(string text, Func<MenuContext, ValueTask> onSelectAsync, bool enabled = true, bool closeOnSelect = true)
    {
        _menu.AddItem(new ActionMenuItem(text, onSelectAsync, enabled, closeOnSelect));
        return this;
    }

    public IMenuBuilder AddToggle(string text, bool initialValue, Action<MenuContext, bool> onToggle, bool closeOnSelect = false)
    {
        _menu.AddItem(new ToggleMenuItem(text, initialValue, onToggle, closeOnSelect));
        return this;
    }

    public IMenuBuilder AddSubmenu(string text, IMenu submenu, bool enabled = true)
    {
        _menu.AddItem(new SubmenuMenuItem(text, submenu, enabled));
        return this;
    }

    public IMenuBuilder AddSubmenu(string text, Func<MenuContext, IMenu> submenuFactory, bool enabled = true)
    {
        _menu.AddItem(new SubmenuMenuItem(text, submenuFactory, enabled));
        return this;
    }

    public IMenuBuilder AddText(string text, string? color = null)
    {
        _menu.AddItem(new TextMenuItem(text, color));
        return this;
    }

    public IMenuBuilder AddSpacer()
    {
        _menu.AddItem(new TextMenuItem(" "));
        return this;
    }

    public IMenu Build()
    {
        return _menu;
    }

    public void Open(IPlayer player, int initialPage = 0)
    {
        _menu.Open(player, initialPage);
    }
}
