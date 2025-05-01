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
  
    public bool HasSequence( string sequenceName )
    {
        return Sequences.Any( s => s.Name == sequenceName );
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
    // Diese Methode ersetzt die bestehende PlaySequence-Methode in BoneAnimationController




    public async void PlaySequence( string sequenceName )
    {
        var sequence = Sequences.Find( s => s.Name == sequenceName );
        if ( sequence == null )
        {
          
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
            int loopCount = 0;
            do
            {
                loopCount++;
                

                if ( sequence.ExecutionMode == SequenceExecutionMode.Sequential )
                {
                    // Sequentieller Modus: Jeden Schritt nacheinander ausführen
                    foreach ( var step in sequence.Steps )
                    {
                        if ( token.IsCancellationRequested )
                        {
                         
                            break;
                        }

                        await ExecuteAnimationStep( step, token, usedBones );
                    }
                }
                else if ( sequence.ExecutionMode == SequenceExecutionMode.Random )
                {
                    // Random Modus: Zufällig einen Schritt auswählen und ausführen
                    if ( sequence.Steps.Count > 0 )
                    {
                        var randomIndex = Random.Shared.Int( 0, sequence.Steps.Count - 1 );
                        var randomStep = sequence.Steps[randomIndex];

                        

                        if ( !token.IsCancellationRequested )
                        {
                            await ExecuteAnimationStep( randomStep, token, usedBones );
                        }
                    }
                   
                }
                else // Parallel
                {
                    var tasks = new List<Task>();

                    foreach ( var step in sequence.Steps )
                    {
                        if ( token.IsCancellationRequested ) break;

                        // Starte die Methode direkt als Task
                        var task = ExecuteAnimationStep( step, token, usedBones );
                        tasks.Add( task );
                    }

                    // Warte auf alle Tasks, wenn nötig
                    if ( tasks.Count > 0 )
                    {
                        await Task.WhenAll( tasks );
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
           

            // Stelle sicher, dass alle verwendeten Knochen zurückgesetzt werden
            foreach ( string boneName in usedBones )
            {
              
                if ( !activeAnimations.Any( a =>
                {
                    var anim = Animations.Find( anim => anim.Name == a.Key );
                    return anim?.BoneName.Equals( boneName, StringComparison.OrdinalIgnoreCase ) == true;
                } ) )
                {
                    
                    RemoveProceduralBone( boneName );
                }
               
            }

           
        }
    }

    // Hilfsmethode zum Ausführen eines einzelnen Animationsschritts
    // ...existing code...
    private async Task ExecuteAnimationStep( AnimationStep step, CancellationToken token, HashSet<string> usedBones )
    {
        

        // Verzögerung vor der Animation abwarten
        if ( step.Delay > 0 )
        {
          
            await GameTask.DelaySeconds( step.Delay );
        }

        if ( token.IsCancellationRequested )
        {
            
            return;
        }

        // Animation starten
        var animation = Animations.Find( a => a.Name == step.AnimationName );
        if ( animation != null )
        {
            

            // Knochen zur Liste der verwendeten Knochen hinzufügen
            if ( !string.IsNullOrEmpty( animation.BoneName ) )
            {
                lock ( usedBones ) // Thread-sicher für parallele Ausführung
                {
                    usedBones.Add( animation.BoneName.ToLower() ); // Konsistente Kleinschreibung verwenden
                }
                
            }

            var taskCompletionSource = new TaskCompletionSource<bool>();
            

            // Starte die Animation mit einem expliziten Callback
            PlayAnimation( step.AnimationName, () =>
            {
               
                taskCompletionSource.TrySetResult( true ); // Verwende TrySetResult statt SetResult
            } );

            // Warte optional auf den Abschluss
            if ( step.WaitForCompletion )
            {
                

                try
                {
                    await taskCompletionSource.Task;
                    
                }
                
                
            }
            
        }
        
    }
    // ...existing code...

    /// <summary>
    /// Findet alle Bone-GameObjects und speichert sie im Cache
    /// </summary>
    // Ersetze die bestehende FindAllBoneObjects-Methode:

    private void FindAllBoneObjects()
    {
        // Leere den Cache
        boneObjects.Clear();

        if ( ModelRenderer == null )
        {
            ModelRenderer = GameObject.Components.Get<SkinnedModelRenderer>();
            if ( ModelRenderer == null )
            {
               
                return;
            }
        }

        // Alternativ: ModelRenderer.BoneCount und ModelRenderer.GetBoneWorldTransform abrufen
        // Da GetBones() nicht existiert, verwenden wir die vorhandenen API-Methoden

        if ( ModelRenderer.Model != null )
        {
     
            // Rekursiv alle GameObjects durchsuchen, die vermutlich Knochen sein könnten
            FindBonesRecursive( GameObject );

            // Zusätzlich: Versuche, über den Transform-Baum die Bones zu finden
            var rootTransform = ModelRenderer.GameObject;
          
            FindBonesInTransformHierarchy( rootTransform );
        }
       
    }

    // Neue Methode zum Durchsuchen der Transform-Hierarchie nach Bones
    private void FindBonesInTransformHierarchy( GameObject obj )
    {
        // Überprüfe den Namen auf typische Bone-Namen
        string name = obj.Name;
        if ( IsPotentialBoneName( name ) && !boneObjects.ContainsKey( name ) )
        {
            boneObjects[name] = obj;
           
        }

        // Rekursiv alle Kinder durchgehen
        foreach ( var child in obj.Children )
        {
            FindBonesInTransformHierarchy( child );
        }
    }

    // Hilfsmethode zum Identifizieren potenzieller Knochen-Namen
    private bool IsPotentialBoneName( string name )
    {
        // Typische Knochennamen enthalten oft diese Begriffe
        string[] boneKeywords = new[] {
        "bone", "clavicle", "spine", "neck", "head", "arm", "hand", "finger", "leg", "foot", "toe",
        "pelvis", "hip", "thigh", "shin", "shoulder", "elbow", "wrist", "knee", "ankle",
        "thumb", "index", "middle", "ring", "pinky", "jaw", "eyeball", "brow"
    };

        name = name.ToLower();

        // Prüfe auf typische Knochennamen-Muster
        foreach ( var keyword in boneKeywords )
        {
            if ( name.Contains( keyword ) )
                return true;
        }

        // Prüfe auf typische Namenskonventionen wie "bip01_spine"
        if ( name.StartsWith( "bip" ) || name.StartsWith( "bone" ) || name.StartsWith( "b_" ) )
            return true;

        return false;
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
    /// Liefert das GameObject für einen Knochen anhand seines Namens, ignoriert Groß- und Kleinschreibung
    /// </summary>
    // Verbesserte GetBoneObject-Methode
    public GameObject GetBoneObject( string boneName )
    {
        if ( string.IsNullOrEmpty( boneName ) ) return null;

        // Verwende eine Case-insensitive Suche von Anfang an
        string normalizedName = boneName.ToLower();



        // Wenn nicht im Cache, erneut alle Knochen finden
        FindAllBoneObjects();

        // Nach dem Aktualisieren erneut mit Case-insensitive Suche versuchen
        foreach ( var kvp in boneObjects )
        {
            if ( string.Equals( kvp.Key, normalizedName, StringComparison.OrdinalIgnoreCase ) )
            {

                return kvp.Value;
            }
        }
        return null; // Wenn kein passendes GameObject gefunden wurde

        
    }


    // ...existing code...
    public async void PlayAnimation( string animationName, Action onComplete = null )
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
          
            onComplete?.Invoke(); // Callback aufrufen, wenn Knochen nicht gefunden wird
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

            // WICHTIG: Callback nach Abschluss der Animation aufrufen
            
            onComplete?.Invoke();
        }
    }
    // ...existing code...
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
    [Property] public string BoneName { get; set; } = "clavicle_R";
    [Property] public Angles TargetRotation { get; set; } = new Angles( 0, 45, 0 );
    [Property] public float Duration { get; set; } = 0.25f;
    [Property] public float ReturnDuration { get; set; } = 0.5f;
    [Property] public EaseType EaseType { get; set; } = EaseType.EaseInOut;
    [Property, Title( "Im Loop abspielen" )] public bool Loop { get; set; } = false;


    [Button( "Vorschau" )]
    public void PlayAnimation()
    {
      
        var player = Player.Local;
        if ( player == null )
        {
           
            return;
        }

        var controller = player.GetComponent<BoneAnimationController>();

        if ( controller != null )
        {
            // Prüfe, ob dieser Controller unsere Animation enthält
            if ( !controller.Animations.Contains( this ) )
            {
                // Animation temporär zum Controller hinzufügen
                controller.Animations.Add( this );
                
            }

            // Animation über den Controller abspielen
            if ( !string.IsNullOrEmpty( Name ) )
            {
                controller.PlayAnimation( Name );
            }
           
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