using SwiftlyS2.Shared.Players;

namespace MenuApi.Sound;

public static class MenuSounds
{
    public const string Scroll = "sounds/ui/menu_scroll.vsnd";
    public const string Select = "sounds/ui/menu_accept.vsnd";
    public const string Invalid = "sounds/ui/menu_invalid.vsnd";
    public const string Close = "sounds/ui/menu_back.vsnd";

    public static void Play(IPlayer player, string sound)
    {
        if (player == null || !player.IsValid) return;

        try
        {
            // CS2 istemcisinde anında UI sesi çalar
            player.ExecuteCommand($"play {sound}");
        }
        catch
        {
            // Sessizce yut
        }
    }
}

