namespace GeneralGame;

public enum GeneralScene
{
	Creation,
	Game,
	MainMenu,
	Starting,
	Forest,
	StartBase,

}

public static class SceneHandler
{
	public static async void ChangeScene( GeneralScene scene, ulong? lobby = null, bool stopSound = true )
	{
		if ( !HasRequiredLevel( scene ) )
		{
			// Handle insufficient level
			Log.Info( "Level zu niedrig, um diese Szene zu wechseln." );
			return;
		}
		var path = scene switch
		{
			GeneralScene.Creation => "scenes/creation.scene",
			GeneralScene.Game => "scenes/dungeon_1.scene",
			GeneralScene.MainMenu => "scenes/lobby.scene",
			GeneralScene.Starting => "scenes/startlobby.scene",
			GeneralScene.Forest => "scenes/forest.scene",
			GeneralScene.StartBase => "scenes/startlobbynew.scene",
			
			_ => null
		};



		if ( string.IsNullOrEmpty( path ) )
			return;

		if ( !ResourceLibrary.TryGet<GameResource>( path, out var resource ) )
			return;

		if ( stopSound )
			Sound.StopAll( 5f );

		// If is game.
		if ( lobby.HasValue )
		{
			var connected = await GameNetworkSystem.TryConnectSteamId( lobby.Value );
			if ( !connected )
				return; // Return if connection fails.
		}





		Game.ActiveScene.Load( resource );
		Player.Setup();
		return;

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
			GeneralScene.Game => 5,
			GeneralScene.MainMenu => 0,
			GeneralScene.Starting => 2,
			GeneralScene.Forest => 10,
			_ => 0
		};
	}
}

