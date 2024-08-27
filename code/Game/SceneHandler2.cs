namespace GeneralGame;

public enum GeneralScene2
{
    Creation,
    Game,
    MainMenu,
    Starting,
    Forest,
    StartBase,
}

public static class SceneHandler2
{
    public static void ChangeScene2( GeneralScene2 scene, ulong? lobby = null, bool stopSound = true )
    {
        if ( !HasRequiredLevel2( scene ) )
        {
            // Handle insufficient level
            Log.Info( "Level zu niedrig, um diese Szene zu wechseln." );
            return;
        }

        var path = scene switch
        {
            GeneralScene2.Creation => "scenes/creation.scene",
            GeneralScene2.Game => "scenes/dungeon_1.scene",
            GeneralScene2.MainMenu => "scenes/lobby.scene",
            GeneralScene2.Starting => "scenes/startlobby.scene",
            GeneralScene2.Forest => "scenes/forest.scene",
            GeneralScene2.StartBase => "scenes/startlobbynew.scene",

            _ => null
        };

        if ( string.IsNullOrEmpty( path ) )
            return;

        if ( !ResourceLibrary.TryGet<GameResource>( path, out var resource ) )
            return;

        if ( stopSound )
        {
            Sound.StopAll( 5f );
            Log.Info( "Szene wird gewechselt." );
        }

        // If is game.
        if ( lobby.HasValue )
        {
            Log.Info( "Lobby" );
            var connected =  GameNetworkSystem.TryConnectSteamId( lobby.Value );
             // Return if connection fails.
        }

        Log.Info( "loading +" + resource );
        Game.ActiveScene.Load( resource );
        Player.Setup();
    }

    public static bool HasRequiredLevel2( GeneralScene2 scene )
    {
        int playerLevel = Player.Local.GetLevel(); // Annahme: Es gibt eine Methode, um das Spielerlevel zu bekommen
        return playerLevel >= scene.GetRequiredLevel2();
    }
}

public static class GeneralSceneExtensions2
{
    public static int GetRequiredLevel2( this GeneralScene2 scene )
    {
        return scene switch
        {
            GeneralScene2.Creation => 1,
            GeneralScene2.Game => 0,
            GeneralScene2.MainMenu => 0,
            GeneralScene2.Starting => 5,
            GeneralScene2.Forest => 25,
            GeneralScene2.StartBase => 0,
            _ => 0
        };
    }
}