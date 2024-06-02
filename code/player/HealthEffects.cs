using Sandbox;
using System;
using System.Linq;

namespace GeneralGame;

public sealed partial class HealthEffects : Component
{
	[Property] public ColorAdjustments Adjustments { get; set; }
	
	private Player LocalPlayer { get; set; }
	private Vignette Vignette { get; set; }

	protected override void OnStart()
	{
		Vignette = Components.GetAll<Vignette>().ElementAt( 1 );
		base.OnStart();
	}

	protected override void OnFixedUpdate()
	{
		if ( !LocalPlayer.IsValid() )
		{
			LocalPlayer = Scene.GetAllComponents<Player>()
				.FirstOrDefault( p => p.Network.IsOwner );
		}

		if ( !LocalPlayer.IsValid() )
			return;

		if ( !Vignette.IsValid() )
			return;

		var health = (1f / LocalPlayer.MaxHealth) * LocalPlayer.Health;

		Adjustments.Saturation = 1f - (1f - health) * 0.9f;
		Vignette.Intensity = 0.6f * (1f - health);
		Vignette.Color = Color.Lerp( Color.White, Color.Red, 1f - health );
		
        

        // Apply the adjustments to the local player's camera
    
		base.OnFixedUpdate();
	}
}
