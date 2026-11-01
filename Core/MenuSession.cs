using MenuApi.API;
using SwiftlyS2.Shared.Players;

namespace MenuApi.Core;

public class MenuSession
{
    public IPlayer Player { get; }
    public IMenu CurrentMenu { get; private set; }
    public int CurrentPage { get; set; }

    public Stack<(IMenu Menu, int Page)> History { get; } = new();

    public DateTime OpenedAt { get; } = DateTime.UtcNow;
    public DateTime LastInteractionAt { get; set; } = DateTime.UtcNow;

    public bool IsDirty { get; set; } = true;
    public int RenderTickCount { get; set; } = 0;
    public string? LastRenderedHtml { get; set; }

    public MenuSession(IPlayer player, IMenu menu, int initialPage = 0)
    {
        Player = player;
        CurrentMenu = menu;
        CurrentPage = Math.Max(0, initialPage);
    }

    public void PushSubmenu(IMenu submenu)
    {
        History.Push((CurrentMenu, CurrentPage));
        CurrentMenu = submenu;
        CurrentPage = 0;
        ResetInteraction();
    }

    public bool PopSubmenu()
    {
        if (History.Count > 0)
        {
            var (prevMenu, prevPage) = History.Pop();
            CurrentMenu = prevMenu;
            CurrentPage = prevPage;
            ResetInteraction();
            return true;
        }
        return false;
    }

    public void ResetInteraction()
    {
        LastInteractionAt = DateTime.UtcNow;
        IsDirty = true;
    }

    public bool IsExpired()
    {
        if (CurrentMenu.Timeout <= TimeSpan.Zero) return false;
        return (DateTime.UtcNow - LastInteractionAt) > CurrentMenu.Timeout;
    }

    public int GetTotalPages()
    {
        int perPage = Math.Clamp(CurrentMenu.ItemsPerPage, 1, 7);
        return Math.Max(1, (int)Math.Ceiling(CurrentMenu.Items.Count / (double)perPage));
    }

    public bool HasNextPage()
    {
        return CurrentPage + 1 < GetTotalPages();
    }

    public bool HasPreviousPage()
    {
        return CurrentPage > 0;
    }

    public bool NextPage()
    {
        if (HasNextPage())
        {
            CurrentPage++;
            ResetInteraction();
            return true;
        }
        return false;
    }

    public bool PreviousPage()
    {
        if (HasPreviousPage())
        {
            CurrentPage--;
            ResetInteraction();
            return true;
        }
        return false;
    }
}
