using Sandbox;
using Sandbox.Citizen;
namespace GeneralGame;
public sealed class Water : Component, Component.ITriggerListener
{
	[Property] public BoxCollider Collider { get; set; }

	[Property] private bool isPlayerInside = false;

	[Property] private Player playerInside;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( isPlayerInside && playerInside != null )
		{
			Log.Info( "Player is inside the water" );
			// Doppelte Überprüfung, ob der Spieler tatsächlich innerhalb des BoxCollider ist
			playerInside?.SetSwimming( true );
			playerInside.AnimationHelper.IsNoclipping = true;

			// Physik anpassen, um den Spieler im Wasser schweben zu lassen
			var playerPosition = playerInside.Position;
			var waterSurfaceY = Collider.WorldPosition.y + Collider.WorldPosition.y / 2;

			if ( playerInside.Components.TryGet<MoveHelper>( out MoveHelper moveHelper ) )
			{
				if ( playerPosition.y < waterSurfaceY )
				{
					// Spieler ist unter der Wasseroberfläche, Auftriebskraft anwenden
					moveHelper.Velocity += new Vector3( 0, 1, 0 ) * 0.1f; // Auftriebskraft
				}
				else
				{
					// Spieler ist über der Wasseroberfläche, Schwerkraft anwenden
					moveHelper.Velocity += new Vector3( 0, -1, 0 ) * 0.1f; // Schwerkraft
				}

				// Spieler kann springen, um an die Oberfläche zu kommen
				if ( Input.Down( "jump" ) )
				{
					moveHelper.Velocity += new Vector3( 0, 1, 0 ) * 0.5f; // Sprungkraft
				}
			}
		}
		else
		{
			playerInside?.SetSwimming( false );
		}
	}

	public void OnTriggerEnter( Collider other )
	{
		var player = other.Components.Get<Player>();
		if ( player != null )
		{
			isPlayerInside = true;
			playerInside = player;
		}
	}

	public void OnTriggerExit( Collider other )
	{
		var player = other.Components.Get<Player>();
		if ( player != null )
		{
			isPlayerInside = false;
			playerInside = null;
			playerInside?.SetSwimming( false );
		}
	}
}