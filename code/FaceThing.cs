using Sandbox;

public sealed class FaceThing : Component
{
	public GameObject Thing { get; set; }

	protected override void OnUpdate()
	{
		if ( Thing == null )
		{
			Log.Error( "Thing is null." );
			return;
		}
		

		WorldRotation = Rotation.LookAt( WorldPosition - Thing.WorldPosition );
	}

	/// <summary>
	/// Made from TrollFaceReallife47 thanks <3
	/// </summary>
}