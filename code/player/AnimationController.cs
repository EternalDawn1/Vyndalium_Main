using Sandbox;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GeneralGame;

/// <summary>
/// Steuert temporäre Animationen für bestimmte Knochen eines Skelettmodells
/// </summary>
[Title( "Knochen-Animations-Controller" ), Category( "Animation" ), Icon( "skeleton" )]
public sealed class BoneAnimationController : Component
{
    [Property, Category( "Komponenten" )]
    public SkinnedModelRenderer ModelRenderer { get; set; }

    /// <summary>
    /// Liste von vordefinierten Animationen
    /// </summary>
    [Property, Category( "Animationen" )]
    public List<BoneAnimation> Animations { get; set; } = new();

    private Dictionary<string, Rotation> originalRotations = new();
    private Dictionary<string, AnimationState> activeAnimations = new();

    protected override void OnStart()
    {
        if ( ModelRenderer == null )
        {
            ModelRenderer = GameObject.Components.Get<SkinnedModelRenderer>();
        }

        // Originale Rotationen speichern
        if ( ModelRenderer?.Model != null )
        {
            foreach ( var anim in Animations )
            {
                if ( !string.IsNullOrEmpty( anim.BoneName ) && !originalRotations.ContainsKey( anim.BoneName ) )
                {
                    try
                    {
                        // Knochen-Index finden
                        int boneIndex = GetBoneIndex( anim.BoneName );
                        if ( boneIndex >= 0 )
                        {
                            // Aktuelle Rotation speichern
                            var currentTransform = GetBoneTransform( boneIndex );
                            originalRotations[anim.BoneName] = currentTransform.Rotation;
                        }
                    }
                    catch ( Exception ex )
                    {
                        Log.Warning( $"Fehler beim Abrufen des Knochens '{anim.BoneName}': {ex.Message}" );
                    }
                }
            }
        }
    }

    /// <summary>
    /// Spielt eine Animation für einen bestimmten Knochen ab
    /// </summary>
    /// <param name="animationName">Name der Animation</param>
    public async void PlayAnimation( string animationName )
    {
        var animation = Animations.Find( a => a.Name == animationName );
        if ( animation == null || ModelRenderer == null ) return;

        // Animation abbrechen, falls bereits aktiv
        if ( activeAnimations.TryGetValue( animation.BoneName, out var state ) )
        {
            state.CancellationToken.Cancel();
            activeAnimations.Remove( animation.BoneName );
        }

        // Knochen-Index finden
        int boneIndex = GetBoneIndex( animation.BoneName );
        if ( boneIndex < 0 )
        {
            Log.Warning( $"Knochen '{animation.BoneName}' nicht gefunden im Modell." );
            return;
        }

        // Original-Rotation abrufen oder speichern
        if ( !originalRotations.TryGetValue( animation.BoneName, out var originalRotation ) )
        {
            Transform currentTransform = GetBoneTransform( boneIndex );
            originalRotation = currentTransform.Rotation;
            originalRotations[animation.BoneName] = originalRotation;
        }

        // Neue Animation starten
        var token = new CancellationTokenSource();
        activeAnimations[animation.BoneName] = new AnimationState { CancellationToken = token };

        try
        {
            // Animation abspielen
            float elapsed = 0f;
            float duration = animation.Duration;

            while ( elapsed < duration && !token.IsCancellationRequested )
            {
                float progress = Math.Min( elapsed / duration, 1f );
                float easedProgress = EaseFunction( progress, animation.EaseType );

                // Rotation interpolieren
                Angles angles = animation.TargetRotation;
                var targetRotation = Rotation.From( angles );
                var currentRotation = Rotation.Slerp( originalRotation, targetRotation, easedProgress );

                // Rotation anwenden
                SetBoneTransform( boneIndex, new Transform( Vector3.Zero, currentRotation ) );

                await GameTask.DelaySeconds( Time.Delta );
                elapsed += Time.Delta;
            }

            // Rückkehr zur normalen Rotation, wenn nicht abgebrochen
            if ( !token.IsCancellationRequested )
            {
                await ReturnToOriginalRotation( animation.BoneName, animation.ReturnDuration );
            }
        }
        finally
        {
            activeAnimations.Remove( animation.BoneName );
        }
    }

    /// <summary>
    /// Setzt die Rotation eines Knochens zurück zur ursprünglichen Position
    /// </summary>
    private async Task ReturnToOriginalRotation( string boneName, float duration )
    {
        if ( !originalRotations.TryGetValue( boneName, out var originalRotation ) )
            return;

        // Knochen-Index finden
        int boneIndex = GetBoneIndex( boneName );
        if ( boneIndex < 0 ) return;

        var token = new CancellationTokenSource();
        activeAnimations[boneName] = new AnimationState { CancellationToken = token };

        try
        {
            Transform currentTransform = GetBoneTransform( boneIndex );
            var currentRotation = currentTransform.Rotation;
            float elapsed = 0f;

            while ( elapsed < duration && !token.IsCancellationRequested )
            {
                float progress = Math.Min( elapsed / duration, 1f );
                float easedProgress = EaseFunction( progress, EaseType.EaseInOut );

                var rotation = Rotation.Slerp( currentRotation, originalRotation, easedProgress );
                SetBoneTransform( boneIndex, new Transform( Vector3.Zero, rotation ) );

                await GameTask.DelaySeconds( Time.Delta );
                elapsed += Time.Delta;
            }

            // Finale Position setzen
            if ( !token.IsCancellationRequested )
            {
                SetBoneTransform( boneIndex, new Transform( Vector3.Zero, originalRotation ) );
            }
        }
        finally
        {
            activeAnimations.Remove( boneName );
        }
    }

    /// <summary>
    /// Ermittelt den Index eines Knochens anhand seines Namens
    /// </summary>/// <summary>
    /// Ermittelt den Index eines Knochens anhand seines Namens
    /// </summary>
    /// <summary>
    /// Ermittelt den Index eines Knochens anhand seines Namens
    /// </summary>
    /// <summary>
    /// Ermittelt den Index eines Knochens anhand seines Namens
    /// </summary>
    /// <summary>
    /// Ermittelt den Index eines Knochens anhand seines Namens
    /// </summary>
    /// <summary>
    /// Ermittelt den Index eines Knochens anhand seines Namens
    /// </summary>
    private int GetBoneIndex( string boneName )
    {
        if ( ModelRenderer?.Model == null ) return -1;

        // DirectlyAccessBones in der ModelRenderer-Klasse verwenden
        // Hier müssen wir ein anderes Verfahren verwenden, da Count nicht verfügbar ist
        var model = ModelRenderer.Model;
        int boneCount = model.BoneCount; // Verwende BoneCount statt Bones.Count

        for ( int i = 0; i < boneCount; i++ )
        {
            if ( model.GetBoneName( i ) == boneName )
                return i;
        }

        return -1;
    }
    /// <summary>
    /// Ermittelt die aktuelle Transformation eines Knochens
    /// </summary>
    /// <summary>
    /// Ermittelt die aktuelle Transformation eines Knochens
    /// </summary>
    /// <summary>
    /// Ermittelt die aktuelle Transformation eines Knochens
    /// </summary>
    private Transform GetBoneTransform( int boneIndex )
    {
        if ( boneIndex < 0 || ModelRenderer == null || ModelRenderer.Model == null )
            return new Transform();

        var model = ModelRenderer.Model;
        if ( boneIndex < model.BoneCount )
        {
            // Bone-Namen aus dem Modell holen
            string boneName = model.GetBoneName( boneIndex );

            // Alternativ: Mit der korrekten API auf die Bone-Transformation zugreifen
            // Wenn GetBoneTransform nicht existiert, gibt es wahrscheinlich eine andere Methode

            // Versuchen wir GetAttachment für die Position
            var transform = new Transform();
            try
            {
                // Versuche ModelRenderer.GetAttachment oder ähnliche Methode
                transform = ModelRenderer.GetAttachment( boneName ) ?? new Transform();
            }
            catch
            {
                // Wenn das nicht funktioniert, versuche, die Basis-Transformation zurückzugeben
                Log.Warning( $"Konnte Transformation für Knochen {boneName} nicht abrufen" );
            }

            return transform;
        }

        return new Transform();
    }

    /// <summary>
    /// Setzt die Transformation eines Knochens
    /// </summary>
    /// <summary>
    /// Setzt die Transformation eines Knochens
    /// </summary>
    private void SetBoneTransform( int boneIndex, Transform transform )
    {
        if ( boneIndex < 0 || ModelRenderer == null || ModelRenderer.Model == null )
            return;

        var model = ModelRenderer.Model;
        if ( boneIndex < model.BoneCount )
        {
            // Bone-Namen aus dem Modell holen
            string boneName = model.GetBoneName( boneIndex );

            // Versuche über das GameObject-System den Bone zu finden
            var boneObject = GameObject.Children.FirstOrDefault( x => x.Name == boneName );
            if ( boneObject != null )
            {
                boneObject.WorldRotation = transform.Rotation;
            }
            else
            {
                Log.Warning( $"Konnte GameObject für Knochen {boneName} nicht finden" );
            }
        }
    }
    /// <summary>
    /// Easing-Funktion für flüssige Animationen
    /// </summary>
    private float EaseFunction( float t, EaseType easeType )
    {
        return easeType switch
        {
            EaseType.Linear => t,
            EaseType.EaseIn => t * t,
            EaseType.EaseOut => t * (2 - t),
            EaseType.EaseInOut => t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t,
            EaseType.EaseInBack => t * t * ((1.70158f + 1) * t - 1.70158f),
            EaseType.EaseOutBack => (--t) * t * ((1.70158f + 1) * t + 1.70158f) + 1,
            _ => t,
        };
    }

    private class AnimationState
    {
        public CancellationTokenSource CancellationToken { get; set; }
    }
}

/// <summary>
/// Eine Animation für einen bestimmten Knochen
/// </summary>
[Serializable]
public class BoneAnimation
{
    [Property] public string Name { get; set; } = "Animation";
    [Property] public string BoneName { get; set; } = "clavicle_r";
    [Property] public Angles TargetRotation { get; set; } = new Angles( 0, 45, 0 );
    [Property] public float Duration { get; set; } = 0.25f;
    [Property] public float ReturnDuration { get; set; } = 0.5f;
    [Property] public EaseType EaseType { get; set; } = EaseType.EaseInOut;
}

/// <summary>
/// Verschiedene Easing-Funktionen für Animationen
/// </summary>
public enum EaseType
{
    Linear,
    EaseIn,
    EaseOut,
    EaseInOut,
    EaseInBack,
    EaseOutBack
}