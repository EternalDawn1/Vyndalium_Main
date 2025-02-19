
using System.Threading.Tasks;
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

		[Property] public SpriteRenderer SpriteRenderer { get; set; } = new SpriteRenderer();

		[Property] public LineRenderer LineRenderer { get; set; } = new LineRenderer();

		[Property] public Action action { get; set; }



		// Parameterloser Konstruktor
		public Checkpoint() { }

		protected override void OnAwake()
		{
			Position = LocalPosition;
			CheckPointId = Guid.NewGuid();
			

		}

		// ...existing code...
		public void CheckPlayerProximity( Player player )
		{
			float distance = Vector3.DistanceBetween( Position, player.Position );

			if ( distance <= DetectionRadius )
			{
				CheckPointMaster.Instance.checkpointActivationStatus[CheckPointId] = false;
				action?.Invoke();
				// Aktiviere den nächsten Checkpoint
			

				Task.Delay( 2000 ).ContinueWith( _ => CheckPointMaster.Instance.ActivateNextCheckpoint() );

				// Führe die Aktion aus, wenn sie gesetzt ist

			}
		}
		
		// ...existing code...

		public void DisableSpriteRenderer()
		{
			var spriteRenderer = this.Components.Get<SpriteRenderer>( FindMode.EnabledInSelfAndChildren );
			if ( spriteRenderer != null )
			{
				spriteRenderer.Enabled = false;
			}
			else
			{
				
			}
		}
		public void EnableSpriteRenderer( bool firstCheckpointReached )
		{
			if ( SpriteRenderer != null )
			{
				SpriteRenderer.Enabled = true;
			
			}
			else
			{
			
			}
		}
		public void DisableLineRenderer()
		{
			var lineRenderer = this.Components.Get<LineRenderer>( FindMode.EnabledInSelfAndChildren );
			if ( lineRenderer != null )
			{
				lineRenderer.Enabled = false;
			}
			else
			{
				
			}
		}
		public void EnableLineRenderer( bool firstCheckpointReached )
		{
			if ( LineRenderer != null )
			{
				LineRenderer.Enabled = true;
			}
			else
			{
				
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
				Gizmo.Draw.Color = Color.Red.WithAlpha( 1f );
				Gizmo.Draw.LineBBox( new BBox( Position - Vector3.One * DetectionRadius, Position + Vector3.One * DetectionRadius ) );
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

		[Property] public int currentCheckpointIndex = 0;

		[Property] public Dictionary<Guid, bool> checkpointActivationStatus = new Dictionary<Guid, bool>();
		

		public CheckPointMaster()
		{
			Instance = this;
		}
		protected override void OnStart()
		{
		

			if ( checkpoints.Count > 0 )
			{
				// Weisen Sie die SpriteRenderer den Checkpoints zu
				for ( int i = 0; i < checkpoints.Count; i++ )
				{
					var spriteRenderer = checkpoints[i].GetComponent<SpriteRenderer>();
					if ( spriteRenderer != null )
					{
					
						
					}
					else
					{
					
					}
				}

				// Aktiviere nur den ersten Checkpoint
				checkpoints[0].EnableSpriteRenderer( true );
				checkpoints[0].EnableLineRenderer(true);
				checkpoints[0].GameObjectToUnlock.Enabled = true;
				checkpointActivationStatus[checkpoints[0].CheckPointId] = true;
			

				// Deaktiviere alle anderen Checkpoints
				for ( int i = 1; i < checkpoints.Count; i++ )
				{
					checkpoints[i].DisableSpriteRenderer();
					checkpoints[i].DisableLineRenderer();
					checkpointActivationStatus[checkpoints[i].CheckPointId] = false;
				
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
				checkpoints[currentCheckpointIndex].DisableLineRenderer();
				checkpoints[currentCheckpointIndex].GameObjectToUnlock.Enabled = false;
				checkpointActivationStatus[checkpoints[currentCheckpointIndex].CheckPointId] = false;

				currentCheckpointIndex++;
				if ( currentCheckpointIndex < checkpoints.Count )
				{
					// Aktiviere den nächsten Checkpoint
					checkpoints[currentCheckpointIndex].EnableSpriteRenderer( false );
					checkpoints[currentCheckpointIndex].EnableLineRenderer( false );
					checkpoints[currentCheckpointIndex].GameObjectToUnlock.Enabled = true;
					checkpointActivationStatus[checkpoints[currentCheckpointIndex].CheckPointId] = true;
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

		
	}
}

