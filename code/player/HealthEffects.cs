using Sandbox;
using System;
using System.Linq;

namespace GeneralGame;


public sealed partial class HealthEffects : Component
{
	[Property] public ColorAdjustments Adjustments { get; set; }
	[Property] public ColorAdjustments FreezeAdjustments { get; set; }
	[Property] public ColorAdjustments PoisonAdjustments { get; set; }
	[Property] public ColorAdjustments ShadowAdjustments { get; set; }

	private Player LocalPlayer { get; set; }
	[Property] private Vignette Vignette { get; set; }
	[Property] private Vignette Freeze { get; set; }
	[Property] private Vignette Poison { get; set; }

	[Property] public Vignette Shadow { get; set; }

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
			Shadow = vignettes.ElementAt( 4 );

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

		Adjustments.Saturation = 1.125f - (1f - health) * 0.9f;
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

		if ( !Poison.IsValid() )
		{
		
			return;
		}

		

		PoisonAdjustments.Saturation = 0.1f;
		Poison.Intensity = 1.1f;
		Poison.Color = Color.Lerp( Color.White, Color.Green, 1f );

		// Aktivieren Sie die Vignette
		Poison.Enabled = true;

		
	}
	public void ShadowEffect()
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

		

		ShadowAdjustments.Saturation = 0.1f;
		Shadow.Intensity = 1.1f;
		Shadow.Color = Color.Lerp( Color.White, Color.Black, 1f );

		// Aktivieren Sie die Vignette
		Shadow.Enabled = true;

		
	}

	public void DestroyShadow()
	{
		if ( Shadow != null )
		{
			// Starte eine Coroutine, um den Freeze-Effekt langsam zu entfernen
			_ = FadeOutShadowEffect();
		}
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
		if ( Poison != null )
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
	private async Task FadeOutShadowEffect()
	{
		float duration = 2.0f; // Dauer des Fade-Out-Effekts in Sekunden
		float elapsed = 0.0f;

		Color initialColor = Shadow.Color;
		float initialIntensity = Shadow.Intensity;
		float initialSaturation = ShadowAdjustments.Saturation;

		while ( elapsed < duration )
		{
			float t = elapsed / duration;

			Shadow.Color = Color.Lerp( initialColor, Color.White, t );
			Shadow.Intensity = MathHelper.Lerp( initialIntensity, 0.0f, t );
			ShadowAdjustments.Saturation = MathHelper.Lerp( initialSaturation, 1.0f, t );

			elapsed += Time.Delta;
			await Task.Delay( (int)(Time.Delta * 1000) );
		}

		// Deaktiviere die Vignette nach dem Fade-Out
		Shadow.Enabled = false;
	}
}
public static class MathHelper
{
	public static float Lerp( float a, float b, float t )
	{
		return a + (b - a) * t;
	}
}