using System;
using Sandbox;
using GeneralGame;


public static class ConnectionExtensions
{
    public static bool IsAdmin( this Connection conn )
        => conn?.IsHost == true || (Player.DevsAreAdmins && conn.IsDev());

    public static bool IsDev( this Connection conn )
        => conn?.SteamId == Player.ETERNAL_STEAM_ID;
}
