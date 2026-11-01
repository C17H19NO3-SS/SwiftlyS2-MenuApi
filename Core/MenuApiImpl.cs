using MenuApi.API;
using SwiftlyS2.Shared.Players;

namespace MenuApi.Core;

public class MenuApiImpl : IMenuApi
{
    private readonly MenuManager _manager;

    public MenuApiImpl(MenuManager manager)
    {
        _manager = manager;
    }

    public IMenuBuilder CreateBuilder(string id)
    {
        return new MenuBuilder(id);
    }

    public IMenuBuilder CreateBuilder(string id, string title)
    {
        return new MenuBuilder(id, title);
    }

    public bool HasActiveMenu(IPlayer player)
    {
        return _manager.HasActiveMenu(player);
    }

    public IMenu? GetActiveMenu(IPlayer player)
    {
        return _manager.GetActiveMenu(player);
    }

    public void CloseMenu(IPlayer player, MenuCloseReason reason = MenuCloseReason.UserExit)
    {
        _manager.CloseMenu(player, reason);
    }

    public void CloseAll()
    {
        _manager.CloseAll();
    }
}