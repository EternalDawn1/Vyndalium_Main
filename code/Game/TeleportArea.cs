namespace GeneralGame
{
	
	public enum TeleportDirection
    {
        North,
        East,
        South,
        West
    }
	public sealed class TeleportArea : Component
	{
		[Property] public TeleportPositionPoint TeleportPositionPoint { get; set; }
		[Property] public float DetectionRadius { get; set; } = 5.0f;
		
		[Property] public bool DrawProximityRangeGizmo { get; set; } = true;
		

		[Property] public bool ResetHasTeleported { get; set; } = false;

		[Property] private bool hasTeleported = false;
		[Property] public TeleportDirection TeleportDirection { get; set; } = TeleportDirection.North;

		protected override void OnUpdate()
		{
			base.OnUpdate();
			if ( !hasTeleported )
			{
				CheckPlayerProximity();
			}

			if ( ResetHasTeleported )
			{
				hasTeleported = false;
				ResetHasTeleported = true; // Setze ResetHasTeleported auf false, um die Schleife zu vermeiden
			}
		}

		protected override void OnStart()
		{
			if ( TeleportPositionPoint != null )
			{
				// Setze die Position von TeleportPositionPoint relativ zur lokalen Position von TeleportArea
				TeleportPositionPoint.Position = ( TeleportPositionPoint.LocalPosition );
			}
		}
		private Player FindPlayer()
		{
			if ( Network.IsProxy )
			{
				return null;
			}

			var players = Scene.GetAllComponents<Player>();

			foreach ( var player in players )
			{
				// Überprüfe, ob der Spieler in der Nähe ist
				float distance = (player.WorldPosition - this.WorldPosition).Length;

				if ( distance < DetectionRadius )
				{
					return player;
				}
			}

			return null;
		}

		private void CheckPlayerProximity()
		{
			var player = FindPlayer();
			if ( player != null )
			{
				TeleportPlayer( player );
			}
			else
			{
				
			}
		}

		private void TeleportPlayer( Player player )
		{
			if ( player == null )
			{
			
				return;
			}

			// Überprüfen, ob der Spieler bereits an der Zielposition ist
			if ( player.WorldPosition == TeleportPositionPoint.Position )
			{
				
				return;
			}

			// Debug-Ausgabe der aktuellen TeleportPosition
			

			// Debug-Ausgabe der aktuellen Position des Spielers vor dem Teleportieren
			

			// Stelle sicher, dass die Position-Eigenschaft des Players schreibbar ist
			player.WorldPosition = TeleportPositionPoint.Position;

			switch ( TeleportDirection )
			{
				case TeleportDirection.North:
					player.WorldRotation = Rotation.FromYaw( 0 );
					break;
				case TeleportDirection.East:
					player.WorldRotation = Rotation.FromYaw( 90 );
					break;
				case TeleportDirection.South:
					player.WorldRotation = Rotation.FromYaw( 180 );
					break;
				case TeleportDirection.West:
					player.WorldRotation = Rotation.FromYaw( 270 );
					break;
			}
			Player.Local.PlaySuccessSoundFromPath( "/sounds/chargedattack.sound", 0.0125f );

			// Debug-Ausgabe der aktuellen Position des Spielers nach dem Teleportieren



			player.BlackScreen( 0f,0.1f, 1f );
			// Setze hasTeleported auf true
			hasTeleported = true;

			// Setze hasTeleported zurück, wenn ResetHasTeleported aktiviert ist
			if ( ResetHasTeleported )
			{
				//hasTeleported = false;
				// ResetHasTeleported wieder auf true setzen
				ResetHasTeleported = true;
			}
		}

		protected override void DrawGizmos()
		{
			const float boxSize = 4f;
			var bounds = new BBox( Vector3.One * -boxSize, Vector3.One * boxSize );

			Gizmo.Hitbox.BBox( bounds );

			Gizmo.Draw.Color = Color.Cyan.WithAlpha( (Gizmo.IsHovered || Gizmo.IsSelected) ? 0.5f : 0.2f );
			Gizmo.Draw.LineBBox( bounds );
			Gizmo.Draw.SolidBox( bounds );

			Gizmo.Draw.Color = Color.Cyan.WithAlpha( (Gizmo.IsHovered || Gizmo.IsSelected) ? 0.8f : 0.6f );

			// Zeichne den PlayerProximityDistance-Gizmo, wenn aktiviert
			if ( DrawProximityRangeGizmo )
			{
				Gizmo.Draw.Color = Color.Green.WithAlpha( 0.3f );
				Gizmo.Draw.LineSphere( Vector3.Zero, DetectionRadius );
			}

			
		}
	}
}
public sealed class TeleportPositionPoint : Component
{
	[Property] public Vector3 Position { get; set; }
}