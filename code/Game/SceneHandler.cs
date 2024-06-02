namespace GeneralGame;

public enum GeneralScene
{
	Creation,
	Game,
	MainMenu,
    Starting
}

public static class SceneHandler
{
	public static async void ChangeScene( GeneralScene scene, ulong? lobby = null, bool stopSound = true )
	{
		var path = scene switch
		{
			GeneralScene.Creation => "scenes/creation.scene",
			GeneralScene.Game => "scenes/dom.scene",
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
			var connected = await GameNetworkSystem.TryConnectSteamId(lobby.Value);
			if (!connected)
				return; // Return if connection fails.
		}
        
		
          
		

		Game.ActiveScene.Load( resource );
		return;
        
	}
	
}

