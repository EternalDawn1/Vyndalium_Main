using Sandbox;
namespace GeneralGame;
public sealed class Switch : Component
{
	[Property]
	public PointLight Light { get; set; }

	[Property] public bool DrawProximityRangeGizmo { get; set; }
	[Property] public float PlayerProximityDistance { get; set; } = 1000f;
	[Property] public float CheckInterval { get; set; } = 1.0f; // Intervall in Sekunden

	private float timeSinceLastCheck = 0f;
	private bool isPlayerNearby = false;

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
			Gizmo.Draw.LineSphere( Vector3.Zero, PlayerProximityDistance );
		}
	}

	protected override void OnUpdate()
	{
		if ( Light == null ) return;

		timeSinceLastCheck += Time.Delta;
		if ( timeSinceLastCheck >= CheckInterval )
		{
			isPlayerNearby = IsPlayerNearby();
			timeSinceLastCheck = 0f;
		}

		Light.Enabled = isPlayerNearby;
	}

	private bool IsPlayerNearby()
	{
		if ( Network.IsProxy )
			return false;

		var players = Scene.GetAllComponents<Player>();
		foreach ( var player in players )
		{
			// Überprüfe, ob der Spieler in der Nähe ist
			if ( (player.WorldPosition - this.WorldPosition).Length < PlayerProximityDistance )
				return true;
		}
		return false;
	}
}