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
	[Property] private Vignette Poison { get; set; }

	public HealthEffects()
	{
		Adjustments = new ColorAdjustments();
		FreezeAdjustments = new ColorAdjustments();
	}

	protected override void OnStart()
	{
		var vignettes = Components.GetAll<Vignette>().ToList();
		if ( vignettes.Count > 3 )
		{
			Vignette = vignettes.ElementAt( 1 );
			Freeze = vignettes.ElementAt( 2 );
			Poison = vignettes.ElementAt( 3 );

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

		if ( !LocalPlayer.IsValid() || !Vignette.IsValid() || Adjustments == null || FreezeAdjustments == null )
		{
			if ( Adjustments == null )
			{
				Log.Error( "Adjustments ist null fixed." );
			}

			if ( FreezeAdjustments == null )
			{
				Log.Error( "FreezeAdjustments ist null fixed." );
			}

			return;
		}

		var health = LocalPlayer.Health / LocalPlayer.MaxHealth;

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
		{
			
			return;
		}

		if ( !Freeze.IsValid() )
		{
		
			return;
		}

		

		FreezeAdjustments.Saturation = 0.1f;
		Freeze.Intensity = 1.1f;
		Freeze.Color = Color.Lerp( Color.White, Color.Blue, 1f );

		// Aktivieren Sie die Vignette
		Freeze.Enabled = true;

		
	}
	public void PoisonEffect()
	{
		

		if ( !LocalPlayer.IsValid() )
		{
			
			LocalPlayer = Scene.GetAllComponents<Player>()
				.FirstOrDefault( p => p.Network.IsOwner );
		}

		if ( !LocalPlayer.IsValid() )
		{
			
			return;
		}

		if ( !Freeze.IsValid() )
		{
		
			return;
		}

		

		FreezeAdjustments.Saturation = 0.1f;
		Freeze.Intensity = 1.1f;
		Freeze.Color = Color.Lerp( Color.White, Color.Green, 1f );

		// Aktivieren Sie die Vignette
		Freeze.Enabled = true;

		
	}

	public void DestroyFreeze()
	{
		if ( Freeze != null )
		{
			// Starte eine Coroutine, um den Freeze-Effekt langsam zu entfernen
			_ = FadeOutFreezeEffect();
		}
	}

	public void DestroyPoison()
	{
		if ( Freeze != null )
		{
			// Starte eine Coroutine, um den Freeze-Effekt langsam zu entfernen
			_ = FadeutPoisonEffect();
		}
	}
	private async Task FadeutPoisonEffect()
	{
		float duration = 2.0f; // Dauer des Fade-Out-Effekts in Sekunden
		float elapsed = 0.0f;

		Color initialColor = Freeze.Color;
		float initialIntensity = Freeze.Intensity;
		float initialSaturation = FreezeAdjustments.Saturation;

		while ( elapsed < duration )
		{
			float t = elapsed / duration;

			Freeze.Color = Color.Lerp( initialColor, Color.White, t );
			Freeze.Intensity = MathHelper.Lerp( initialIntensity, 0.0f, t );
			FreezeAdjustments.Saturation = MathHelper.Lerp( initialSaturation, 1.0f, t );

			elapsed += Time.Delta;
			await Task.Delay( (int)(Time.Delta * 1000) );
		}

		// Deaktiviere die Vignette nach dem Fade-Out
		Freeze.Enabled = false;
	}

	private async Task FadeOutFreezeEffect()
	{
		float duration = 2.0f; // Dauer des Fade-Out-Effekts in Sekunden
		float elapsed = 0.0f;

		Color initialColor = Freeze.Color;
		float initialIntensity = Freeze.Intensity;
		float initialSaturation = FreezeAdjustments.Saturation;

		while ( elapsed < duration )
		{
			float t = elapsed / duration;

			Freeze.Color = Color.Lerp( initialColor, Color.White, t );
			Freeze.Intensity = MathHelper.Lerp( initialIntensity, 0.0f, t );
			FreezeAdjustments.Saturation = MathHelper.Lerp( initialSaturation, 1.0f, t );

			elapsed += Time.Delta;
			await Task.Delay( (int)(Time.Delta * 1000) );
		}

		// Deaktiviere die Vignette nach dem Fade-Out
		Freeze.Enabled = false;
	}
}
public static class MathHelper
{
	public static float Lerp( float a, float b, float t )
	{
		return a + (b - a) * t;
	}
}