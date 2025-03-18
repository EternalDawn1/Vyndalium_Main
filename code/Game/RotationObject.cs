namespace GeneralGame;

public sealed class MoveObject : Component
{
	private Vector3 direction;
	private Vector3 counterDirection;

	[Property,Feature("Move")] public GameObject Target { get; set; }
	[Property, Feature( "Move" )]
	public Vector3 Direction
	{
		get => direction;
		set
		{
			direction = value;
			CounterDirection = -value; // Setze das Gegenteil
		}
	}
	[Property, Feature( "Move" )]
	public Vector3 CounterDirection
	{
		get => counterDirection;
		private set => counterDirection = value;
	} // Neuer Vektor
	[Property, Feature( "Move" )] public float Speed { get; set; }
	[Property, Feature( "Move" )] public bool IsMove { get; set; }
	[Property, Feature( "Move" )] public bool IsReturning { get; set; } // Neue Eigenschaft
	[Property, Feature( "Move" )] private bool movingToCounterDirection = false; // Neue Eigenschaft
	[Property, Feature( "Move" )] private float distanceTraveled = 0f; // Neue Eigenschaft
	[Property, Feature( "Move" )] private float totalDistance; // Neue Eigenschaft

	[Property, FeatureEnabled( "Move" )] public bool IsMoveEnabled { get; set; }

	protected override void OnStart()
	{
		IsMove = true;
		totalDistance = Vector3.DistanceBetween( Direction, CounterDirection ); // Gesamtdistanz berechnen
	}

	protected override void OnUpdate()
	{
		if ( IsMove )
		{
			Vector3 currentDirection = movingToCounterDirection ? CounterDirection : Direction;
			float distanceThisFrame = Speed * Time.Delta;
			Target.LocalPosition += currentDirection * distanceThisFrame;
			distanceTraveled += distanceThisFrame;

			// Check if the target has reached the total distance
			if ( distanceTraveled >= totalDistance )
			{
				movingToCounterDirection = !movingToCounterDirection; // Richtung wechseln
				distanceTraveled = 0f; // Zurückgelegte Distanz zurücksetzen
			}
		}
	}
}