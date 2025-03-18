namespace GeneralGame;

public sealed class RotationObject : Component
{
	[Property] public float RotationSpeed { get; set; } = 1.0f; // Geschwindigkeit der Rotation
	[Property] public bool RotateClockwise { get; set; } = true; // Richtung der Rotation
	[Property] public float RotationDuration { get; set; } = 10.0f; // Dauer der Rotation in Sekunden
	[Property] public GameObject TargetObject { get; set; } // Das zu rotierende GameObject
	[Property] public Vector3 RotationCenter { get; set; } = Vector3.Zero; // Mittelpunkt der Rotation
	[Property] public bool ChangeDirection { get; set; } = false; // Richtung nach einer gewissen Zeit ändern
	[Property] public float MoveSpeed { get; set; } = 1.0f; // Bewegungsgeschwindigkeit
	[Property] public bool MoveForward { get; set; } = true; // Bewegung nach vorne oder hinten
	[Property] public float MoveRange { get; set; } = 5.0f; // Reichweite der Bewegung
	[Property] public bool MoveUp { get; set; } = false; // Bewegung nach oben oder unten
	[Property] public bool RotateAroundX { get; set; } = true; // Rotation um die X-Achse
	[Property] public bool RotateAroundY { get; set; } = true; // Rotation um die Y-Achse
	[Property] public bool RotateAroundZ { get; set; } = true; // Rotation um die Z-Achse

	private float elapsedTime = 0.0f;
	private bool directionChanged = false;
	private Vector3 originalPosition;
	private bool movingForward = true;

	protected override void OnStart()
	{
		originalPosition = TargetObject.LocalPosition;
	}

	protected override void OnUpdate()
	{
		if ( TargetObject == null ) return;

		if ( elapsedTime < RotationDuration )
		{
			float rotationAmount = RotationSpeed * Time.Delta * (RotateClockwise ? 1 : -1);

			// Verschieben zum Rotationszentrum
			TargetObject.LocalPosition -= RotationCenter;

			if ( RotateAroundY )
			{
				TargetObject.WorldRotation *= Rotation.FromAxis( Vector3.Up, rotationAmount ); // Rotation um die Y-Achse
			}
			if ( RotateAroundX )
			{
				TargetObject.WorldRotation *= Rotation.FromAxis( Vector3.Right, rotationAmount ); // Rotation um die X-Achse
			}
			if ( RotateAroundZ )
			{
				TargetObject.WorldRotation *= Rotation.FromAxis( Vector3.Forward, rotationAmount ); // Rotation um die Z-Achse
			}

			// Zurückverschieben vom Rotationszentrum
			TargetObject.LocalPosition += RotationCenter;

			elapsedTime += Time.Delta;

			if ( ChangeDirection && !directionChanged && elapsedTime >= RotationDuration / 2 )
			{
				RotateClockwise = !RotateClockwise;
				directionChanged = true;
			}
		}

		Vector3 moveDirection = Vector3.Zero;
		if ( MoveForward )
		{
			if ( movingForward )
			{
				moveDirection += TargetObject.WorldRotation.Forward * MoveSpeed * Time.Delta;
				if ( (TargetObject.LocalPosition - originalPosition).Length >= MoveRange )
				{
					movingForward = false;
				}
			}
			else
			{
				moveDirection -= TargetObject.WorldRotation.Forward * MoveSpeed * Time.Delta;
				if ( (TargetObject.LocalPosition - originalPosition).Length <= 0.1f )
				{
					movingForward = true;
				}
			}
		}

		if ( MoveUp )
		{
			if ( movingForward )
			{
				moveDirection += Vector3.Up * MoveSpeed * Time.Delta;
				if ( (TargetObject.LocalPosition - originalPosition).Length >= MoveRange )
				{
					movingForward = false;
				}
			}
			else
			{
				moveDirection -= Vector3.Up * MoveSpeed * Time.Delta;
				if ( (TargetObject.LocalPosition - originalPosition).Length <= 0.1f )
				{
					movingForward = true;
				}
			}
		}

		TargetObject.LocalPosition += moveDirection;
	}
}