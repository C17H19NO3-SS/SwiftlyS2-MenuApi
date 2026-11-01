using SwiftlyS2.Shared.Players;

namespace MenuApi.API;

public interface IMenuItem
{
    string Text { get; set; }
    bool IsEnabled { get; set; }
    bool IsSelectable { get; }
    bool CloseOnSelect { get; set; }
    string? CustomColor { get; set; }
    Func<MenuContext, ValueTask>? OnSelectAsync { get; set; }

    ValueTask ExecuteAsync(MenuContext context);
    string GetDisplayText(IPlayer player, int slotNumber, bool isHovered);
}

public interface IMenu
{
    string Id { get; }
    string Title { get; set; }
    string? Subtitle { get; set; }
    int ItemsPerPage { get; set; }
    TimeSpan Timeout { get; set; }
    bool PlaySounds { get; set; }
    bool AllowWasdNavigation { get; set; }
    bool AllowSlotSelection { get; set; }
    bool AllowChatSelection { get; set; }
    bool FreezePlayer { get; set; }
    MenuTheme Theme { get; set; }
    IReadOnlyList<IMenuItem> Items { get; }

    void AddItem(IMenuItem item);
    void RemoveItem(IMenuItem item);
    void ClearItems();

    void Open(IPlayer player, int initialPage = 0);
    void Close(IPlayer player, MenuCloseReason reason = MenuCloseReason.UserExit);
}

public interface IMenuBuilder
{
    IMenuBuilder SetTitle(string title);
    IMenuBuilder SetSubtitle(string subtitle);
    IMenuBuilder SetItemsPerPage(int itemsPerPage);
    IMenuBuilder SetTimeout(TimeSpan timeout);
    IMenuBuilder SetTheme(MenuTheme theme);
    IMenuBuilder ConfigureTheme(Action<MenuTheme> configure);
    IMenuBuilder SetFontSize(int px);
    IMenuBuilder SetFontSize(string sizeOrClass);
    IMenuBuilder SetTitleFontSize(int px);
    IMenuBuilder SetTitleFontSize(string sizeOrClass);
    IMenuBuilder SetFontSizePx(int px);
    IMenuBuilder SetTitleFontSizePx(int px);
    IMenuBuilder EnableSounds(bool enabled = true);
    IMenuBuilder EnableWasdNavigation(bool enabled = true);
    IMenuBuilder EnableSlotSelection(bool enabled = true);
    IMenuBuilder EnableChatSelection(bool enabled = true);
    IMenuBuilder SetFreezePlayer(bool freeze = true);

    // Nesne tabanlı öğe ekleme (Object-Oriented Element API)
    IMenuBuilder Add(IMenuItem item);

    // Fluent kısayollar
    IMenuBuilder AddItem(string text, Action<MenuContext> onSelect, bool enabled = true, bool closeOnSelect = true);
    IMenuBuilder AddItem(string text, Action<MenuContext> onSelect, string customColor, bool enabled = true, bool closeOnSelect = true);
    IMenuBuilder AddItemAsync(string text, Func<MenuContext, ValueTask> onSelectAsync, bool enabled = true, bool closeOnSelect = true);
    IMenuBuilder AddToggle(string text, bool initialValue, Action<MenuContext, bool> onToggle, bool closeOnSelect = false);
    IMenuBuilder AddSubmenu(string text, IMenu submenu, bool enabled = true);
    IMenuBuilder AddSubmenu(string text, Func<MenuContext, IMenu> submenuFactory, bool enabled = true);
    IMenuBuilder AddText(string text, string? color = null);
    IMenuBuilder AddSpacer();

    IMenu Build();
    void Open(IPlayer player, int initialPage = 0);
}
