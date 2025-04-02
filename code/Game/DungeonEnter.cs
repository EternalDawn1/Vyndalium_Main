using GeneralGame.HUD;
namespace GeneralGame;

public sealed class DungeonEnter : Component, Component.ITriggerListener
{
	[Property] public ShopStorage Storage { get; set; }

	Player player { get; set; }

	[Property] bool MissionPanel { get; set; }
	[Property] bool ShopPanel { get; set; }

	

	float PlayerProximityDistance = 300.0f;

	public void OnTriggerEnter( Collider other )
	{
		if ( IsProxy )
			return;

		// Überprüfe, ob der Collider ein Spieler ist
		var player = other.GetComponent<Player>();
		if ( player != null && player == Player.Local )
		{
		
			// Überprüfe, ob der Spieler in der Nähe ist
			if ( (player.WorldPosition - this.WorldPosition).Length < PlayerProximityDistance )
			{
				if ( MissionPanel )
				{
					FullScreenManager.Instance.Display( FullScreenManager.FullScreenPanel.MissonPanel );
				}
				else if ( ShopPanel )
				{
					FullScreenManager.Instance.Display( FullScreenManager.FullScreenPanel.ShopPanel );
				}
			}
		}
	}

	public void OnTriggerExit( Collider other )
	{
		var player = other.GetComponent<Player>();
		if ( player != null && player == Player.Local )
		{
			FullScreenManager.Instance.Display( FullScreenManager.FullScreenPanel.InGameHud );
		}
	}
}