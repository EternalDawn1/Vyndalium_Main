namespace GeneralGame
{
	public class Checkpoint : Component
	{
		[Property] public GameObject GameObjectToUnlock { get; set; }
		[Property] public Vector3 Position { get; set; }
		[Property] public bool IsPermanentlyDisabled { get; set; } = false;
		[Property] public bool CanBeReactivated { get; set; } = true;
		[Property] public float DetectionRadius { get; set; } = 5.0f;
		[Property] public Guid CheckPointId { get; set; }

		// Parameterloser Konstruktor
		public Checkpoint() { }

		protected override void OnAwake()
		{
			Position = LocalPosition;
			CheckPointId = Guid.NewGuid();
		}

		public void CheckPlayerProximity( Player player )
		{
			float distance = Vector3.DistanceBetween( Position, player.Position );
			Log.Info( $"Überprüfe Spielerproximity: Spieler {player.Name}, Checkpoint {CheckPointId}, Distanz {distance}" );
			if ( distance <= DetectionRadius )
			{
				Log.Info( $"Spieler {player.Name} ist innerhalb des Erkennungsradius von Checkpoint {CheckPointId}" );
				//GameObjectToUnlock.Enabled = true;
				//DisableSpriteRenderer();
				CheckPointMaster.Instance.checkpointActivationStatus[CheckPointId] = false;

				// Aktiviere den nächsten Checkpoint
				CheckPointMaster.Instance.ActivateNextCheckpoint();
			}
			else
			{
				Log.Info( $"Spieler {player.Name} ist außerhalb des Erkennungsradius von Checkpoint {CheckPointId}" );
			}
		}

		public void DisableSpriteRenderer()
		{
			var spriteRenderer = this.Components.Get<SpriteRenderer>( FindMode.EnabledInSelfAndChildren );
			if ( spriteRenderer != null )
			{
				spriteRenderer.Enabled = false;
			}
			else
			{
				Log.Warning( "SpriteRenderer nicht gefunden." );
			}
		}

		public void EnableSpriteRenderer()
		{
			var spriteRenderers = this.Components.GetAll<SpriteRenderer>( FindMode.EnabledInSelfAndChildren ).ToList();
			if ( spriteRenderers != null && spriteRenderers.Count > 1 )
			{
				spriteRenderers[1].Enabled = true;
				Log.Info( "SpriteRenderer des zweiten Elements wurde aktiviert." );
			}
			else if ( spriteRenderers != null && spriteRenderers.Count == 1 )
			{
				spriteRenderers[0].Enabled = true;
				Log.Info( "Nur ein SpriteRenderer gefunden und aktiviert." );
			}
			else
			{
				Log.Warning( "Nicht genügend SpriteRenderer gefunden." );
				Log.Info( $"Anzahl der SpriteRenderer-Komponenten: {spriteRenderers?.Count ?? 0}" );
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
				Gizmo.Draw.Color = Color.Red.WithAlpha( 0.3f );
				Gizmo.Draw.LineSphere( Vector3.Zero, DetectionRadius );
			}
		}
	

		[Property]
		public bool DrawProximityRangeGizmo { get; set; } = true;
	}

	public sealed class CheckPointMaster : Component
	{
		public static CheckPointMaster Instance { get; private set; }

		[Property]
		private List<Checkpoint> checkpoints = new List<Checkpoint>();

		[Property] private float PlayerProximityDistance = 100.0f; // Beispielwert

		[Property] private int currentCheckpointIndex = 0;

		[Property]public Dictionary<Guid, bool> checkpointActivationStatus = new Dictionary<Guid, bool>();

		public CheckPointMaster()
		{
			Instance = this;
		}
		protected override void OnStart()
		{
			Log.Info( "OnStart aufgerufen" );

			if ( checkpoints.Count > 0 )
			{
				// Aktiviere nur den ersten Checkpoint
				checkpoints[0].EnableSpriteRenderer();
				checkpoints[0].GameObjectToUnlock.Enabled = true;
				checkpointActivationStatus[checkpoints[0].CheckPointId] = true;
				Log.Info( $"Erster Checkpoint {checkpoints[0].CheckPointId} aktiviert" );

				// Deaktiviere alle anderen Checkpoints
				for ( int i = 1; i < checkpoints.Count; i++ )
				{
					checkpoints[i].DisableSpriteRenderer();
					checkpoints[i].GameObjectToUnlock.Enabled = false;
					checkpointActivationStatus[checkpoints[i].CheckPointId] = false;
					Log.Info( $"Checkpoint {checkpoints[i].CheckPointId} deaktiviert" );
				}
			}
		}

		protected override void OnUpdate()
		{
			
			Player player = FindPlayer();
			if ( player != null )
			{
			
				foreach ( var checkpoint in checkpoints )
				{
					if ( checkpointActivationStatus.ContainsKey( checkpoint.CheckPointId ) )
					{
						
						if ( checkpointActivationStatus[checkpoint.CheckPointId] )
						{
						
							checkpoint.CheckPlayerProximity( player );
						}
						else
						{
						
						}
					}
					else
					{
					
				}
			}
			}
			else
			{
				
			}
		}

		public void ActivateNextCheckpoint()
		{
			if ( currentCheckpointIndex < checkpoints.Count )
			{
				// Deaktiviere den aktuellen Checkpoint
				checkpoints[currentCheckpointIndex].DisableSpriteRenderer();
				checkpoints[currentCheckpointIndex].GameObjectToUnlock.Enabled = false;
				checkpointActivationStatus[checkpoints[currentCheckpointIndex].CheckPointId] = false;
				Log.Info( $"Checkpoint {checkpoints[currentCheckpointIndex].CheckPointId} deaktiviert" );
				

				// Aktiviere den nächsten Checkpoint
				currentCheckpointIndex++;
				if ( currentCheckpointIndex < checkpoints.Count )
				{
					checkpoints[currentCheckpointIndex].EnableSpriteRenderer();
					checkpoints[currentCheckpointIndex].GameObjectToUnlock.Enabled = true;
					checkpointActivationStatus[checkpoints[currentCheckpointIndex].CheckPointId] = true;
					Log.Info( $"Nächster Checkpoint {checkpoints[currentCheckpointIndex].CheckPointId} aktiviert" );
				}
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
			

				if ( distance < PlayerProximityDistance )
				{
			
					return player;
				}
			}

		
			return null;
		}

		public void OnCheckpointCompleted( int index )
		{
		
			if ( index < checkpoints.Count - 1 )
			{
			
				var nextCheckpoint = checkpoints[index + 1];
				if ( nextCheckpoint != null )
				{
					nextCheckpoint.EnableSpriteRenderer();
					if ( nextCheckpoint.GameObjectToUnlock != null )
					{
						nextCheckpoint.GameObjectToUnlock.Enabled = true;
					}
					else
					{
					
					}
				}
				else
				{
					
				}
			}
		}
	}
}

