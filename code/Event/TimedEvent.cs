using Sandbox;
using System;

namespace GeneralGame
{
	public sealed class TimedEvent : Component
	{
		[Property] public float DetectionRadius { get; set; } = 5.0f;
		[Property] TextRenderer TextRenderer { get; set; }
		[Property] SpriteRenderer SpriteRenderer { get; set; }
		[Property] public float FadeDuration { get; set; } = 1.0f;
		[Property] public bool DrawProximityRangeGizmo { get; set; } = true;

		private float fadeProgress = 0f;
		private bool isFadingIn = false;

		protected override void OnStart()
		{
			// Set initial alpha to 0
			if ( TextRenderer != null )
			{
				var textColor = TextRenderer.Color;
				textColor.a = 0f;
				TextRenderer.Color = textColor;
			}

			if ( SpriteRenderer != null )
			{
				var spriteColor = SpriteRenderer.Color;
				spriteColor.a = 0f;
				SpriteRenderer.Color = spriteColor;
			}
		}

		protected override void OnUpdate()
		{
			var player = FindPlayer();
			if ( player != null )
			{
				isFadingIn = true;
			}
			else
			{
				isFadingIn = false;
			}

			UpdateFade();
		}

		private void UpdateFade()
		{
			float previousFadeProgress = fadeProgress;

			if ( isFadingIn )
			{
				fadeProgress += Time.Delta / FadeDuration;
			}
			else
			{
				fadeProgress -= Time.Delta / FadeDuration;
			}

			fadeProgress = Math.Clamp( fadeProgress, 0f, 1f );

			if ( Math.Abs( fadeProgress - previousFadeProgress ) > 0.01f )
			{
				// Update TextRenderer color
				if ( TextRenderer != null )
				{
					var textColor = TextRenderer.Color;
					textColor.a = fadeProgress;
					TextRenderer.Color = textColor;
				}

				// Update SpriteRenderer color
				if ( SpriteRenderer != null )
				{
					var spriteColor = SpriteRenderer.Color;
					spriteColor.a = fadeProgress;
					SpriteRenderer.Color = spriteColor;
				}
			}
		}

		private Player FindPlayer()
		{
			if ( Network.IsProxy )
			{
				return null;
			}

			var players = Scene.GetAllComponents<Player>();

			foreach ( var player in players )
			{
				float distance = (player.WorldPosition - this.WorldPosition).Length;

				if ( distance < DetectionRadius )
				{
					return player;
				}
			}

			return null;
		}

		protected override void DrawGizmos()
		{
			const float boxSize = 4f;
			var bounds = new BBox( Vector3.One * -boxSize, Vector3.One * boxSize );

			Gizmo.Hitbox.BBox( bounds );

			Gizmo.Draw.Color = Color.Cyan.WithAlpha( (Gizmo.IsHovered || Gizmo.IsSelected) ? 0.5f : 0.2f );
			Gizmo.Draw.LineBBox( bounds );
			Gizmo.Draw.SolidBox( bounds );

			Gizmo.Draw.Color = Color.Cyan.WithAlpha( (Gizmo.IsHovered || Gizmo.IsSelected) ? 0.8f : 0.6f );

			// Zeichne den PlayerProximityDistance-Gizmo, wenn aktiviert
			if ( DrawProximityRangeGizmo )
			{
				Gizmo.Draw.Color = Color.Green.WithAlpha( 0.3f );
				Gizmo.Draw.LineSphere( Vector3.Zero, DetectionRadius );
			}
		}
	}
}