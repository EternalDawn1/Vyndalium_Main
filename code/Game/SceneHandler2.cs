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
    Forest2,
    StartBase,

    One,
    One2,
    One3,
    Forest3,
    Forest4,
    untitled3
    
}

public static class SceneHandler2
{
    public static GeneralScene2 CurrentScene { get; set; }

    public static void ChangeScene2( GeneralScene2 scene, ulong? lobby = null, bool stopSound = true )
    {
        if ( !HasRequiredLevel2( scene ) )
        {
            // Handle insufficient level
         
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
            GeneralScene2.Forest2 => "scenes/forest2.scene",
            GeneralScene2.Forest3 => "scenes/forest3.scene",
            GeneralScene2.Forest4 => "scenes/forest4.scene",
            GeneralScene2.untitled3 => "scenes/untitled3.scene",

            _ => null
        };

        if ( string.IsNullOrEmpty( path ) )
            return;

        if ( !ResourceLibrary.TryGet<GameResource>( path, out var resource ) )
            return;

        if ( stopSound )
        {
            Sound.StopAll( 5f );
          
        }

        // If is game.
        if ( lobby.HasValue )
        {
           
            Networking.Connect( lobby.Value );
            // Return if connection fails.
        }
       


        Player.Setup();
       

     


        // Definieren und Initialisieren der neuen Szene
        LoadNewScene( resource,scene );


        // Zerstören der alten Szene
        DeleteCurrentScene();

        CurrentScene = scene;

        

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
        }
        CurrentScene = GeneralScene2.MainMenu;
    }

    public static bool HasRequiredLevel2( GeneralScene2 scene )
    {
        int playerLevel = Player.Local.GetLevel(); // Annahme: Es gibt eine Methode, um das Spielerlevel zu bekommen
        return playerLevel >= scene.GetRequiredLevel2();
    }
    public static string GetSceneName( GeneralScene2 scene )
    {
      
        return scene switch
        {
            GeneralScene2.Creation => "Creation",
            GeneralScene2.Game => "Game",
            GeneralScene2.MainMenu => "Main Menu",
            GeneralScene2.Starting => "Starting",
            GeneralScene2.Forest => "Forest 2-1",
            GeneralScene2.Forest2 => "Forest 2-2",
            GeneralScene2.One => "1-1",
            GeneralScene2.StartBase => "Lobby",
            GeneralScene2.One2 => "1-2",
            GeneralScene2.One3 => "1-3",
            GeneralScene2.Forest3 => "Forest 2-3",
            GeneralScene2.Forest4 => "Forest 2-4",
            GeneralScene2.untitled3 => "Untitled 3",
            _ => "Unknown"
        };
    }
}




public static class GeneralSceneExtensions2
{
    public static int GetRequiredLevel2( this GeneralScene2 scene )
    {
        return scene switch
        {
            GeneralScene2.Creation => 0,
            GeneralScene2.Game => 0,
            GeneralScene2.MainMenu => 0,
            GeneralScene2.Starting => 45,
            GeneralScene2.Forest => 10,
            GeneralScene2.Forest2 => 10,
            GeneralScene2.One => 0,
            GeneralScene2.StartBase => 0,
            GeneralScene2.One2 => 0,
            GeneralScene2.One3 => 0,
            GeneralScene2.Forest3 => 25,
            GeneralScene2.Forest4 => 25,
            GeneralScene2.untitled3 => 0,
            _ => 0
        };
    }
    public static void Reset2( this GeneralScene2 scene )
    {
       
     
    }
}
