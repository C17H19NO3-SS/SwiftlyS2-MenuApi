using MenuApi.API;
using SwiftlyS2.Shared.Players;

namespace MenuApi.Core;

public class Menu : IMenu
{
    private readonly List<IMenuItem> _items = new();

    public string Id { get; }
    public string Title { get; set; }
    public string? Subtitle { get; set; }
    public int ItemsPerPage { get; set; } = 7;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public bool PlaySounds { get; set; } = true;
    public bool AllowWasdNavigation { get; set; } = false;
    public bool AllowSlotSelection { get; set; } = true;
    public bool AllowChatSelection { get; set; } = true;
    public bool FreezePlayer { get; set; } = false;
    public MenuTheme Theme { get; set; } = new();

    public IReadOnlyList<IMenuItem> Items => _items;

    public Menu(string id, string title = "Menu")
    {
        Id = id;
        Title = title;
    }

    public void AddItem(IMenuItem item)
    {
        _items.Add(item);
    }

    public void RemoveItem(IMenuItem item)
    {
        _items.Remove(item);
    }

    public void ClearItems()
    {
        _items.Clear();
    }

    public void Open(IPlayer player, int initialPage = 0)
    {
        MenuManager.Instance?.OpenMenu(player, this, initialPage);
    }

    public void Close(IPlayer player, MenuCloseReason reason = MenuCloseReason.UserExit)
    {
        MenuManager.Instance?.CloseMenu(player, reason);
    }
}
