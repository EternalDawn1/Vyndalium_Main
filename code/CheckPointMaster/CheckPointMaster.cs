namespace GeneralGame;

public class Checkpoint : Component
{
	[Property] public GameObject GameObjectToUnlock { get; set; }
	[Property] public Vector3 Position { get; set; }
	[Property] public bool IsPermanentlyDisabled { get; set; } = false;
	[Property] public bool CanBeReactivated { get; set; } = true;
	[Property] public float DetectionRadius { get; set; } = 5.0f;
	[Property] private bool isCompleted;
	[Property] public int Index { get; set; }

	[Property]
	public bool IsCompleted
	{
		get { return isCompleted; }
		set
		{
			isCompleted = value;
			if ( isCompleted )
			{
				CheckPointMaster.Instance.OnCheckpointCompleted( Index );
			}
		}
	}

	[Property] public SpriteRenderer SpriteRenderer { get; set; }

	// Parameterloser Konstruktor
	public Checkpoint() { }


	protected override void OnAwake()
	{
		Position = LocalPosition;
	}

	public void CheckPlayerProximity( Player player )
	{
		if ( IsPermanentlyDisabled && !CanBeReactivated )
		{
			return;
		}

		float distance = Vector3.DistanceBetween( Position, player.Position );

		if ( distance <= DetectionRadius )
		{
			GameObjectToUnlock.Enabled = false;
			DisableSpriteRenderer();

			if ( !CanBeReactivated )
			{
				IsPermanentlyDisabled = true;
			}

			// Aktiviere den nächsten Checkpoint
			CheckPointMaster.Instance.ActivateNextCheckpoint();
		}
		else
		{
			GameObjectToUnlock.Enabled = true;
		}
	}

	public void DisableSpriteRenderer()
	{
		var spriteRenderer = this.Components.Get<SpriteRenderer>( FindMode.EnabledInSelfAndChildren );
		if ( spriteRenderer != null )
		{
			spriteRenderer.Enabled = false;
			IsCompleted = true;
		
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
			// Debugging-Log hinzufügen
			Log.Info( $"Anzahl der SpriteRenderer-Komponenten: {spriteRenderers?.Count ?? 0}" );
			foreach ( var sr in spriteRenderers )
			{
				Log.Info( $"SpriteRenderer:, Enabled: {sr.Enabled}" );
			}
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

	public void OnCheckpointCompleted( int index )
	{
		if ( index < checkpoints.Count - 1 )
		{
			checkpoints[index + 1].EnableSpriteRenderer();
			checkpoints[index + 1].GameObjectToUnlock.Enabled = true;
		}
	}

	public CheckPointMaster()
	{
		Instance = this;
	}



	protected override void OnAwake()
	{
		// Aktiviere den ersten Checkpoint
		if ( checkpoints.Count > 0 )
		{
			checkpoints[0].EnableSpriteRenderer();
			checkpoints[0].GameObjectToUnlock.Enabled = true;
		}

		// Deaktiviere alle anderen Checkpoints
		for ( int i = 1; i < checkpoints.Count; i++ )
		{
			checkpoints[i].DisableSpriteRenderer();
			checkpoints[i].GameObjectToUnlock.Enabled = false;
		}
	}


	protected override void OnUpdate()
	{
		Player player = FindPlayer();
		if ( player != null )
		{
			foreach ( var checkpoint in checkpoints )
			{
				checkpoint.CheckPlayerProximity( player );
			}
		}
	}

	private Player FindPlayer()
	{
		if ( Network.IsProxy )
			return null;

		var players = Scene.GetAllComponents<Player>();
		foreach ( var player in players )
		{
			// Überprüfe, ob der Spieler in der Nähe ist
			if ( (player.WorldPosition - this.WorldPosition).Length < PlayerProximityDistance )
				return player;
		}
		return null;
	}

	public void ActivateNextCheckpoint()
	{
		if ( currentCheckpointIndex < checkpoints.Count - 1 )
		{
			// Deaktiviere den aktuellen Checkpoint
			checkpoints[currentCheckpointIndex].DisableSpriteRenderer();
			checkpoints[currentCheckpointIndex].GameObjectToUnlock.Enabled = false;

			// Aktiviere den nächsten Checkpoint
			currentCheckpointIndex++;
			checkpoints[currentCheckpointIndex].EnableSpriteRenderer();
			checkpoints[currentCheckpointIndex].GameObjectToUnlock.Enabled = true;
		}
	}
}