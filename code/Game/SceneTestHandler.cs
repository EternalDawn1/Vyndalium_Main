namespace GeneralGame;

public enum GeneralScenetest
{
	Creation,
	Game,
	MainMenu,
	Starting
}

public static class SceneHandlertest
{
	public static  void ChangeScenetest( GeneralScene scene, ulong? lobby = null, bool stopSound = true )
	{
		var path = scene switch
		{
			GeneralScene.Creation => "scenes/creation.scene",
			GeneralScene.Game => "scenes/dungeon_1.scene",
			GeneralScene.MainMenu => "scenes/lobby.scene",
			GeneralScene.Starting => "scenes/startlobby.scene",
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
			Networking.Connect( lobby.Value );
			
		}





		Game.ActiveScene.Load( resource );
		return;

	}
}
