using Sandbox;
using System;
using System.Linq;

namespace GeneralGame;


public sealed partial class HealthEffects : Component
{
	[Property] public ColorAdjustments Adjustments { get; set; }
	[Property] public ColorAdjustments FreezeAdjustments { get; set; }

	private Player LocalPlayer { get; set; }
	[Property] private Vignette Vignette { get; set; }
	[Property] private Vignette Freeze { get; set; }

	public HealthEffects()
	{
		Adjustments = new ColorAdjustments();
		FreezeAdjustments = new ColorAdjustments();
	}

	protected override void OnStart()
	{
		var vignettes = Components.GetAll<Vignette>().ToList();
		if ( vignettes.Count > 2 )
		{
			Vignette = vignettes.ElementAt( 1 );
			Freeze = vignettes.ElementAt( 2 );
		}
		else
		{
			// Fehlerbehandlung oder Standardwerte setzen
			Log.Error( "Nicht genügend Vignette-Komponenten gefunden." );
		}

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

		if ( Adjustments == null )
		{
			Log.Error( "Adjustments ist null fixed." );
			return;
		}

		if ( FreezeAdjustments == null )
		{
			Log.Error( "FreezeAdjustments ist null fixed." );
			return;
		}

		var health = (1f / LocalPlayer.MaxHealth) * LocalPlayer.Health;

		Adjustments.Saturation = 1f - (1f - health) * 0.9f;
		Vignette.Intensity = 0.6f * (1f - health);
		Vignette.Color = Color.Lerp( Color.White, Color.Red, 1f - health );

		// Apply the adjustments to the local player's camera

		base.OnFixedUpdate();
	}

	public void FreezeEffect()
	{
		if ( !LocalPlayer.IsValid() )
		{
			LocalPlayer = Scene.GetAllComponents<Player>()
				.FirstOrDefault( p => p.Network.IsOwner );
		}

		if ( !LocalPlayer.IsValid() )
			return;

		if ( !Freeze.IsValid() )
			return;

		FreezeAdjustments.Saturation = 0.1f;
		Freeze.Intensity = 1.6f;
		Freeze.Color = Color.Lerp( Color.White, Color.Blue, 1f );

		// Aktivieren Sie die Vignette
		Freeze.Enabled = true;
	}

	public void DestroyFreeze()
	{
		if ( Freeze != null )
		{
			Freeze.Enabled = false;
		}
	}
}