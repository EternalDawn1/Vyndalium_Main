namespace GeneralGame;

public sealed class RotationObject : Component
{
	[Property] public float RotationSpeed { get; set; } = 1.0f; // Geschwindigkeit der Rotation
	[Property] public bool RotateClockwise { get; set; } = true; // Richtung der Rotation
	[Property] public float RotationDuration { get; set; } = 10.0f; // Dauer der Rotation in Sekunden
	[Property] public GameObject TargetObject { get; set; } // Das zu rotierende GameObject

	private float elapsedTime = 0.0f;

	protected override void OnUpdate()
	{
		if ( elapsedTime < RotationDuration && TargetObject != null )
		{
			float rotationAmount = RotationSpeed * Time.Delta * (RotateClockwise ? 1 : -1);
			TargetObject.WorldRotation *= Rotation.FromYaw( rotationAmount );
			elapsedTime += Time.Delta;
		}
	}
}