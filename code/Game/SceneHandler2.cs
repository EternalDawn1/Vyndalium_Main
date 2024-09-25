namespace GeneralGame;
using System;
using System.Linq;
using System.Collections.Generic;
public enum GeneralScene2
{
    Creation,
    Game,
    MainMenu,
    Starting,
    Forest,
    StartBase,

    One,
    One2,
    One3,
    
}

public static class SceneHandler2
{
    public static GeneralScene2 CurrentScene { get; private set; }

    public static void ChangeScene2( GeneralScene2 scene, ulong? lobby = null, bool stopSound = true )
    {
        if ( !HasRequiredLevel2( scene ) )
        {
            // Handle insufficient level
            Log.Info( "Level zu niedrig, um diese Szene zu wechseln." );
            return;
        }
       
        // Lösche die aktuelle Szene
        DeleteCurrentScene();

        var path = scene switch
        {
            GeneralScene2.Creation => "scenes/creation.scene",
            GeneralScene2.Game => "scenes/dungeon_1.scene",
            GeneralScene2.MainMenu => "scenes/lobby.scene",
            GeneralScene2.Starting => "scenes/startlobby.scene",
            GeneralScene2.Forest => "scenes/forest.scene",
            GeneralScene2.StartBase => "scenes/startlobbynew.scene",
            GeneralScene2.One => "scenes/One/map1.scene",
            GeneralScene2.One2 => "scenes/One/map1.2.scene",
            GeneralScene2.One3 => "scenes/One/map1.3.scene",

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
            Networking.Connect( lobby.Value );
            // Return if connection fails.
        }
       


        Player.Setup();
        Log.Info( "loading +" + resource );

     


        // Definieren und Initialisieren der neuen Szene
        LoadNewScene( resource,scene );


        // Zerstören der alten Szene
        DeleteCurrentScene();

        

    }
    
    public static void LoadNewScene( GameResource resource, GeneralScene2 scene )
    {
        // Logik zum Laden der neuen Szene
        Game.ActiveScene.Load( resource );
        CurrentScene = scene; // Aktualisieren der aktuellen Szene
    }

    public static void DeleteCurrentScene()
    {
        // Logik zum Löschen der aktuellen Szene
        if ( CurrentScene != GeneralScene2.MainMenu ) // Beispiel: MainMenu als Standardwert
        {
            CurrentScene.Reset2();
            CurrentScene = GeneralScene2.MainMenu;
        }
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
            GeneralScene2.Forest => 10,
            GeneralScene2.One => 0,
            GeneralScene2.StartBase => 0,
            GeneralScene2.One2 => 0,
            GeneralScene2.One3 => 0,
            _ => 0
        };
    }
    public static void Reset2( this GeneralScene2 scene )
    {
       
        Log.Info( "Resetting scene: " + scene );
    }
}
