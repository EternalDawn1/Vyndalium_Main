using Sandbox;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using Editor;

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

    [Property, Category("Animations-Sequenzen")]
    public List<AnimationSequence> Sequences { get; set; } = new();

    private Dictionary<string, Rotation> originalRotations = new();
    [Property]private Dictionary<string, AnimationState> activeAnimations = new();
    [Property]private Dictionary<string, AnimationSequence> activeSequences = new();

    // Cache der bone-GameObjects
    private Dictionary<string, GameObject> boneObjects = new();

    protected override void OnStart()
    {
        if ( ModelRenderer == null )
        {
            ModelRenderer = GameObject.Components.Get<SkinnedModelRenderer>();
        }

        // Alle Bone-GameObjects finden und cachen
        FindAllBoneObjects();
        
        
        // Originale Rotationen speichern
        foreach ( var anim in Animations )
        {
            if ( !string.IsNullOrEmpty( anim.BoneName ) && !originalRotations.ContainsKey( anim.BoneName ) )
            {
                var boneObject = GetBoneObject( anim.BoneName );
                if ( boneObject != null )
                {
                    originalRotations[anim.BoneName] = boneObject.LocalRotation;
                }
            }
        }
    }
    /// <summary>
    /// Markiert einen Knochen als prozedural, damit dieser vom Code gesteuert werden kann
    /// </summary>
    /// <param name="boneName">Name des Knochens</param>
    public void MakeBoneProcedural( string boneName )
    {
        var boneObject = GetBoneObject( boneName );
        if ( boneObject == null )
        {
            Log.Warning( $"Knochen '{boneName}' nicht gefunden im Modell." );
            return;
        }

        // Knochen als prozedural markieren
        boneObject.Flags |= GameObjectFlags.ProceduralBone;

        // Original-Rotation speichern, falls noch nicht geschehen
        if ( !originalRotations.ContainsKey( boneName ) )
        {
            originalRotations[boneName] = boneObject.LocalRotation;
        }

        Log.Info( $"Knochen '{boneName}' wird jetzt prozedural gesteuert." );
    }

    /// <summary>
    /// Entfernt die prozedurale Steuerung eines Knochens
    /// </summary>
    /// <param name="boneName">Name des Knochens</param>
    public void RemoveProceduralBone( string boneName )
    {
        var boneObject = GetBoneObject( boneName );
        if ( boneObject == null )
        {
            return;
        }

        // Prozedural-Flag entfernen
        boneObject.Flags &= ~GameObjectFlags.ProceduralBone;

        // Knochen zur Original-Rotation zurücksetzen
        if ( originalRotations.TryGetValue( boneName, out var originalRotation ) )
        {
            boneObject.LocalRotation = originalRotation;
        }
    }
    public async void PlaySequence( string sequenceName )
    {
        var sequence = Sequences.Find( s => s.Name == sequenceName );
        if ( sequence == null )
        {
            Log.Warning( $"Animationssequenz '{sequenceName}' nicht gefunden" );
            return;
        }

        // Sequenz abbrechen, falls bereits aktiv
        if ( activeSequences.TryGetValue( sequenceName, out var activeSequence ) )
        {
            activeSequence.Cancel();
            activeSequences.Remove( sequenceName );
        }

        // Neue Sequenz starten
        var token = sequence.GetNewCancellationToken();
        activeSequences[sequenceName] = sequence;

        // Liste der verwendeten Knochen für Cleanup
        HashSet<string> usedBones = new HashSet<string>();

        try
        {
            do
            {
                // Jeden Schritt der Sequenz ausführen
                foreach ( var step in sequence.Steps )
                {
                    if ( token.IsCancellationRequested )
                        break;

                    // Verzögerung vor der Animation abwarten
                    if ( step.Delay > 0 )
                    {
                        await GameTask.DelaySeconds( step.Delay );
                    }

                    if ( token.IsCancellationRequested )
                        break;

                    // Animation starten
                    var animation = Animations.Find( a => a.Name == step.AnimationName );
                    if ( animation != null )
                    {
                        // Knochen zur Liste der verwendeten Knochen hinzufügen
                        if ( !string.IsNullOrEmpty( animation.BoneName ) )
                        {
                            usedBones.Add( animation.BoneName );
                        }

                        var taskCompletionSource = new TaskCompletionSource<bool>();

                        // Starte die Animation
                        PlayAnimation( step.AnimationName, () =>
                        {
                            taskCompletionSource.SetResult( true );
                        } );

                        // Warte optional auf den Abschluss
                        if ( step.WaitForCompletion )
                        {
                            await taskCompletionSource.Task;
                        }
                    }
                    else
                    {
                        Log.Warning( $"Animation '{step.AnimationName}' nicht gefunden" );
                    }
                }

                // Pause zwischen den Loops
                if ( sequence.Loop && !token.IsCancellationRequested && sequence.LoopDelay > 0 )
                {
                    await GameTask.DelaySeconds( sequence.LoopDelay );
                }

            } while ( sequence.Loop && !token.IsCancellationRequested );
        }
        finally
        {
            activeSequences.Remove( sequenceName );

            // Stelle sicher, dass alle verwendeten Knochen zurückgesetzt werden,
            // falls sie nicht mehr von anderen aktiven Animationen verwendet werden
            foreach ( string boneName in usedBones )
            {
                if ( !activeAnimations.Any( a => Animations.Find( anim => anim.Name == a.Key )?.BoneName == boneName ) )
                {
                    RemoveProceduralBone( boneName );
                }
            }
        }
    }

    /// <summary>
    /// Findet alle Bone-GameObjects und speichert sie im Cache
    /// </summary>
    private void FindAllBoneObjects()
    {
        // Leere den Cache
        boneObjects.Clear();

        // Suche rekursiv nach allen Bone-GameObjects
        FindBonesRecursive( GameObject );

        // Zur Info ausgeben
        Log.Info( "Gefundene Knochen im Modell:" );
        foreach ( var pair in boneObjects )
        {
            Log.Info( $"  {pair.Key}" );
        }
    }

    /// <summary>
    /// Sucht rekursiv nach allen Bone-GameObjects
    /// </summary>
    private void FindBonesRecursive( GameObject obj )
    {
        // Füge dieses GameObject als möglichen Knochen hinzu
        if ( !boneObjects.ContainsKey( obj.Name ) )
        {
            boneObjects[obj.Name] = obj;
        }

        // Suche in allen Kindern
        foreach ( var child in obj.Children )
        {
            FindBonesRecursive( child );
        }
    }

    /// <summary>
    /// Liefert das GameObject für einen Knochen anhand seines Namens
    /// </summary>
    private GameObject GetBoneObject( string boneName )
    {
        if ( boneObjects.TryGetValue( boneName, out var boneObject ) )
        {
            return boneObject;
        }

        // Wenn nicht im Cache, erneut versuchen alle Knochen zu finden
        FindAllBoneObjects();

        if ( boneObjects.TryGetValue( boneName, out boneObject ) )
        {
            return boneObject;
        }

        return null;
    }


    public async void PlayAnimation(string animationName, Action onComplete = null)
    {
        var animation = Animations.Find( a => a.Name == animationName );
        if ( animation == null || ModelRenderer == null ) return;

        // Animation abbrechen, falls bereits aktiv
        if ( activeAnimations.TryGetValue( animation.BoneName, out var state ) )
        {
            state.CancellationToken.Cancel();
            activeAnimations.Remove( animation.BoneName );
        }

        // Bone GameObject finden
        var boneObject = GetBoneObject( animation.BoneName );
        if ( boneObject == null )
        {
            Log.Warning( $"Knochen '{animation.BoneName}' nicht gefunden im Modell." );
            return;
        }

        // Original-Rotation abrufen oder speichern
        if ( !originalRotations.TryGetValue( animation.BoneName, out var originalRotation ) )
        {
            originalRotation = boneObject.LocalRotation;
            originalRotations[animation.BoneName] = originalRotation;
        }
        MakeBoneProcedural( animation.BoneName );
        // Neue Animation starten
        var token = new CancellationTokenSource();
        activeAnimations[animation.BoneName] = new AnimationState { CancellationToken = token };

        try
        {
            do // Loop-Schleife hinzugefügt
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
                    boneObject.LocalRotation = currentRotation;

                    await GameTask.DelaySeconds( Time.Delta );
                    elapsed += Time.Delta;
                }

                if ( !animation.Loop )
                {
                    // Rückkehr zur normalen Rotation nur wenn nicht im Loop-Modus
                    if ( !token.IsCancellationRequested )
                    {
                        await ReturnToOriginalRotation( animation.BoneName, animation.ReturnDuration );
                    }
                    break; // Schleife beenden
                }
                else if ( !token.IsCancellationRequested )
                {
                    // Bei Loop: Kurze Pause an der Zielposition, dann wieder zurück
                    await GameTask.DelaySeconds( 0.2f );

                    // Zurück zur Ausgangsposition
                    await ReturnToOriginalRotation( animation.BoneName, animation.ReturnDuration );

                    // Kurze Pause an der Ausgangsposition
                    await GameTask.DelaySeconds( 0.2f );
                }

            } while ( animation.Loop && !token.IsCancellationRequested );
        }
        finally
        {
            activeAnimations.Remove( animation.BoneName );
            RemoveProceduralBone( animation.BoneName );
        }
    }
    /// <summary>
    /// Setzt die Rotation eines Knochens zurück zur ursprünglichen Position
    /// </summary>
    private async Task ReturnToOriginalRotation( string boneName, float duration )
    {
        if ( !originalRotations.TryGetValue( boneName, out var originalRotation ) )
            return;

        // Bone GameObject finden
        var boneObject = GetBoneObject( boneName );
        if ( boneObject == null ) return;

        var token = new CancellationTokenSource();
        activeAnimations[boneName] = new AnimationState { CancellationToken = token };

        try
        {
            var currentRotation = boneObject.LocalRotation;
            float elapsed = 0f;

            while ( elapsed < duration && !token.IsCancellationRequested )
            {
                float progress = Math.Min( elapsed / duration, 1f );
                float easedProgress = EaseFunction( progress, EaseType.EaseInOut );

                var rotation = Rotation.Slerp( currentRotation, originalRotation, easedProgress );
                boneObject.LocalRotation = rotation;

                await GameTask.DelaySeconds( Time.Delta );
                elapsed += Time.Delta;
            }

            // Finale Position setzen
            if ( !token.IsCancellationRequested )
            {
                boneObject.LocalRotation = originalRotation;
            }
        }
        finally
        {
            activeAnimations.Remove( boneName );
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
    [Property, Title( "Im Loop abspielen" )] public bool Loop { get; set; } = false;


    [Button( "Vorschau" )]
    public void PlayAnimation()
    {
        Log.Info( $"Animation '{Name}' abspielen" );
        var player = Player.Local;
        if ( player == null )
        {
            Log.Warning( "Kein lokaler Spieler gefunden" );
            return;
        }

        // Hole den einzelnen BoneAnimationController (nicht eine Liste)
        var controller = player.GetComponent<BoneAnimationController>();

        if ( controller != null )
        {
            // Prüfe, ob dieser Controller unsere Animation enthält
            if ( controller.Animations.Contains( this ) )
            {
                // Animation über den Controller abspielen
                if ( !string.IsNullOrEmpty( Name ) )
                {
                    controller.PlayAnimation( Name );
                }
                else
                {
                    Log.Warning( "Animation hat keinen Namen" );
                }
            }
            else
            {
                Log.Warning( "Diese Animation ist nicht im Controller registriert" );
            }
        }
        else
        {
            Log.Warning( "BoneAnimationController nicht gefunden am Spieler" );
        }
    }
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