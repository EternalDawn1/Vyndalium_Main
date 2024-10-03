using Sandbox;

public sealed class FaceThing : Component
{	
	public GameObject Thing { get; set; }
	protected override void OnUpdate()
	{
		WorldRotation = Rotation.LookAt( WorldPosition - Thing.WorldPosition );
	}
	/// <summary>
	/// Made from TrollFaceReallife47 thanks <3
	/// </summary>
}
