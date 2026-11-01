using SwiftlyS2.Shared.Players;

namespace MenuApi.API;

public interface IMenuApi
{
    IMenuBuilder CreateBuilder(string id);
    IMenuBuilder CreateBuilder(string id, string title);

    bool HasActiveMenu(IPlayer player);
    IMenu? GetActiveMenu(IPlayer player);
    void CloseMenu(IPlayer player, MenuCloseReason reason = MenuCloseReason.UserExit);
    void CloseAll();
}

