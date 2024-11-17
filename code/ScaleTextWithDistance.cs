using Sandbox;

public sealed class ScaleTextWithDistance : Component
{
	[Property] public GameObject Thing { get; set; }
	TextRenderer textRenderer;
	[Property] public float Scale { get; set; } = 1f;

	protected override void OnStart()
	{
		textRenderer = GameObject.Components.Get<TextRenderer>();
		if ( textRenderer != null )
		{
			textRenderer.BlendMode = BlendMode.Lighten;
			textRenderer.FontFamily = "Geneva";
		}
		else
		{
			Log.Error( "TextRenderer is null." );
		}
	}

	protected override void OnUpdate()
	{
		if ( Thing == null )
		{
			Log.Error( "Thing is null." );
			return;
		}

		if ( textRenderer == null )
		{
			Log.Error( "TextRenderer is null." );
			return;
		}

		float distance = (WorldPosition - Thing.WorldPosition).Length;
		textRenderer.Scale = distance * 0.005f * Scale;
	}
}