using Sandbox;

public sealed class ScaleTextWithDistance : Component
{
	[Property] public GameObject Thing { get; set; }
	TextRenderer textRenderer;
	[Property] public float Scale { get; set; } = 1f;
	protected override void OnStart()
	{

		textRenderer = GameObject.Components.Get<TextRenderer>();
	}
	protected override void OnUpdate()
	{
		float distance = (WorldPosition - Thing.WorldPosition).Length;
		textRenderer.Scale = distance * 0.005f * Scale;
	}
	/// <summary>
	/// Made from TrollFaceReallife47 thanks <3
	/// </summary>
}
