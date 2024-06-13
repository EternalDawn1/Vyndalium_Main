using Sandbox;

public sealed class FaceThing : Component
{	
	public GameObject Thing { get; set; }
	protected override void OnUpdate()
	{
		Transform.Rotation = Rotation.LookAt( Transform.Position - Thing.Transform.Position );
	}
	/// <summary>
	/// Made from TrollFaceReallife47 thanks <3
	/// </summary>
}
