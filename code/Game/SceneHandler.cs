namespace GeneralGame;
using Sandbox.Services;

public enum GeneralScene
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

public static class SceneHandler
{
	public static GeneralScene CurrentScene { get; private set; }

	public static void ChangeScene( GeneralScene scene, ulong? lobby = null, bool stopSound = true )
	{
		if ( !HasRequiredLevel( scene ) )
		{
			// Handle insufficient level
			Log.Info( "Level zu niedrig, um diese Szene zu wechseln." );
			return;
		}
		

		// Lösche die aktuelle Szene
		DeleteCurrentScene();

		var path = scene switch
		{
			GeneralScene.Creation => "scenes/creation.scene",
			GeneralScene.Game => "scenes/dungeon_1.scene",
			GeneralScene.MainMenu => "scenes/lobby.scene",
			GeneralScene.Starting => "scenes/startlobby.scene",
			GeneralScene.Forest => "scenes/forest.scene",
			GeneralScene.StartBase => "scenes/startlobbynew.scene",
			GeneralScene.One => "scenes/One/map1.scene",
			GeneralScene.One2 => "scenes/One/map1.2.scene",
			GeneralScene.One3 => "scenes/One/map1.3.scene",

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
			var connected = GameNetworkSystem.TryConnectSteamId( lobby.Value );
			// Return if connection fails.
		}



		Player.Setup();
		Log.Info( "loading +" + resource );

		// Definieren und Initialisieren der neuen Szene
		var newScene = new Scene();
		newScene.Load( resource );

		// Speichern der aktuellen Szene
		var oldScene = Game.ActiveScene;

		// Aktivieren der neuen Szene
		Game.ActiveScene = newScene;
		oldScene?.Destroy();
	}

	public static void DeleteCurrentScene()
	{
		// Logik zum Löschen der aktuellen Szene
		if ( CurrentScene != GeneralScene.MainMenu ) // Beispiel: MainMenu als Standardwert
		{
			CurrentScene.Reset();
			CurrentScene = GeneralScene.MainMenu;
		}
	}

	public static bool HasRequiredLevel( GeneralScene scene )
	{
		int playerLevel = Player.Local.GetLevel(); // Annahme: Es gibt eine Methode, um das Spielerlevel zu bekommen
		return playerLevel >= scene.GetRequiredLevel();
	}
}




public static class GeneralSceneExtensions
{
	public static int GetRequiredLevel( this GeneralScene scene )
	{
		return scene switch
		{
			GeneralScene.Creation => 1,
			GeneralScene.Game => 0,
			GeneralScene.MainMenu => 0,
			GeneralScene.Starting => 5,
			GeneralScene.Forest => 10,
			GeneralScene.One => 0,
			GeneralScene.StartBase => 0,
			GeneralScene.One2 => 0,
			GeneralScene.One3 => 0,
			_ => 0
		};
	}
	public static void Reset( this GeneralScene scene )
	{

		Log.Info( "Resetting scene: " + scene );
	}

}