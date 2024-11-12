using Sandbox;

namespace GeneralGame;

public sealed partial class PlayerFootsteps : Component
{
	[Property] private SkinnedModelRenderer ModelRenderer { get; set; }
	

	private TimeSince TimeSinceLastStep;

	protected override void OnEnabled()
	{
		if ( ModelRenderer.IsValid() )
		{
			ModelRenderer.OnFootstepEvent += OnEvent;
		}
		
		

		
	}

	protected override void OnDisabled()
	{
		if ( ModelRenderer.IsValid() )
		{
			ModelRenderer.OnFootstepEvent -= OnEvent;
		}

		
	}

	private void OnEvent( SceneModel.FootstepEvent e )
	{
		if ( TimeSinceLastStep < 0.2f )
			return;

		var pos = WorldPosition;
		var trace = Scene.Trace.Ray( pos + Vector3.Up * 10f, pos + Vector3.Down * 10f )
			.Radius( 1 )
			.WithoutTags( "trigger" )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();

		if ( !trace.Hit )
			return;

		if ( trace.Surface is null )
			return;

		TimeSinceLastStep = 0f;

		var sound = e.FootId == 0 ? trace.Surface.Sounds.FootLeft : trace.Surface.Sounds.FootRight;
		if ( sound is null ) return;

		var handle = Sound.Play( sound, trace.HitPosition + trace.Normal * 1f );
		handle.Volume *= e.Volume;
	}
}