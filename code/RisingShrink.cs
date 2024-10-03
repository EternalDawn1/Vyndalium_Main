using Sandbox;

public sealed class RisingShrink : Component
{
	[Property] public float ShrinkSpeed { get; set; } = 0.5f;
	[Property] public float RiseSpeed { get; set; } = 0.5f;

	protected override void OnUpdate()
	{
		WorldPosition += Vector3.Up * Time.Delta * RiseSpeed;
		WorldScale -= Vector3.One * Time.Delta * ShrinkSpeed;
		if(WorldScale.x < 0.1f)
		{
			GameObject.Destroy();
		}
			
	}
	/// <summary>
	/// Made from TrollFaceReallife47 thanks <3
	/// </summary>

}
