namespace GeneralGame
{
	public sealed class MoveObject : Component
	{
		private Vector3 direction;
		private Vector3 counterDirection;

		[Property, Feature( "Move" ), ShowIf( "IsMoveEnabled", true )] public GameObject Target { get; set; }
		[Property, Feature( "Move" ), ShowIf( "IsMoveEnabled", true )]
		public Vector3 Direction
		{
			get => direction;
			set
			{
				direction = value;
				CounterDirection = -value; // Setze das Gegenteil
			}
		}
		[Property, Feature( "Move" ), ShowIf( "IsMoveEnabled", true )]
		public Vector3 CounterDirection
		{
			get => counterDirection;
			private set => counterDirection = value;
		} // Neuer Vektor
		[Property, Feature( "Move" ), ShowIf( "IsMoveEnabled", true )] public float Speed { get; set; }
		[Property, Feature( "Move" ), ShowIf( "IsMoveEnabled", true )] public bool IsMove { get; set; }
		[Property, Feature( "Move" ), ShowIf( "IsMoveEnabled", true )] public bool IsReturning { get; set; } // Neue Eigenschaft
		[Property, Feature( "Move" ), ShowIf( "IsMoveEnabled", true )] private bool movingToCounterDirection = false; // Neue Eigenschaft
		[Property, Feature( "Move" ), ShowIf( "IsMoveEnabled", true )] private float distanceTraveled = 0f; // Neue Eigenschaft
		[Property, Feature( "Move" ), ShowIf( "IsMoveEnabled", true )] private float totalDistance; // Neue Eigenschaft

		[Property] public bool IsMoveEnabled { get; set; }

		// Neue Eigenschaften für Rotation

		[Property, Feature( "Rotate" ), ShowIf( "IsRotateEnabled", true )] public float RotationSpeed { get; set; }
		[Property, Feature( "Rotate" ), ShowIf( "IsRotateEnabled", true )] public Vector3 RotationDirection { get; set; }
		[Property, Feature( "Rotate" ), ShowIf( "IsRotateEnabled", true )] public Vector3 RotationCenter { get; set; }
		[Property, Feature( "Rotate" ), ShowIf( "IsRotateEnabled", true )] public bool RotateAroundX { get; set; }
		[Property, Feature( "Rotate" ), ShowIf( "IsRotateEnabled", true )] public bool RotateAroundY { get; set; }
		[Property, Feature( "Rotate" ), ShowIf( "IsRotateEnabled", true )] public bool RotateAroundZ { get; set; }
		[Property, Feature( "Rotate" ), ShowIf( "IsRotateEnabled", true )] public GameObject RotationObject { get; set; }
		[Property] public bool IsRotateEnabled { get; set; }

		protected override void OnStart()
		{
			if ( IsMoveEnabled )
			{
				if ( Target == null )
				{
					Log.Error( "Target is null" );
					return;
				}
				IsMove = true;
				totalDistance = Vector3.DistanceBetween( Direction, CounterDirection ); // Gesamtdistanz berechnen
			}
		}

		protected override void OnUpdate()
		{
			if ( IsMoveEnabled && IsMove )
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

			if ( IsRotateEnabled && RotationObject != null )
			{
				float rotationThisFrame = RotationSpeed * Time.Delta;
				Vector3 rotationAmount = RotationDirection * rotationThisFrame;

				// Verschieben zum Rotationszentrum
				RotationObject.LocalPosition -= RotationCenter;

				if ( RotateAroundY )
				{
					RotationObject.WorldRotation *= Rotation.FromAxis( Vector3.Up, rotationAmount.y ); // Rotation um die Y-Achse
				}
				if ( RotateAroundX )
				{
					RotationObject.WorldRotation *= Rotation.FromAxis( Vector3.Right, rotationAmount.x ); // Rotation um die X-Achse
				}
				if ( RotateAroundZ )
				{
					RotationObject.WorldRotation *= Rotation.FromAxis( Vector3.Forward, rotationAmount.z ); // Rotation um die Z-Achse
				}

				// Zurückverschieben vom Rotationszentrum
				RotationObject.LocalPosition += RotationCenter;
			}
		}
	}
}