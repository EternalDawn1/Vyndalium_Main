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
            Log.Warning( $"Keine Sequenz mit dem Namen '{sequenceName}' gefunden. Verfügbare Sequenzen: {string.Join( ", ", Sequences.Select( s => s.Name ) )}" );
            return;
        }

        // Sequenz abbrechen, falls bereits aktiv
        if ( activeSequences.TryGetValue( sequenceName, out var activeSequence ) )
        {
            Log.Info( $"Abbruch der bereits laufenden Sequenz: {sequenceName}" );
            activeSequence.Cancel();
            activeSequences.Remove( sequenceName );
        }

        // Neue Sequenz starten
        var token = sequence.GetNewCancellationToken();
        activeSequences[sequenceName] = sequence;
        Log.Info( $"Neue Sequenz gestartet: {sequenceName}, Anzahl der Schritte: {sequence.Steps?.Count ?? 0}, Ausführungsmodus: {sequence.ExecutionMode}" );

        // Liste der verwendeten Knochen für Cleanup
        HashSet<string> usedBones = new HashSet<string>();

        try
        {
            int loopCount = 0;
            do
            {
                loopCount++;
                Log.Info( $"Starte Loop #{loopCount} für Sequenz: {sequenceName}" );

                if ( sequence.ExecutionMode == SequenceExecutionMode.Sequential )
                {
                    // Sequentieller Modus: Jeden Schritt nacheinander ausführen
                    foreach ( var step in sequence.Steps )
                    {
                        if ( token.IsCancellationRequested )
                        {
                            Log.Info( $"Abbruch: Sequenz {sequenceName} wurde abgebrochen" );
                            break;
                        }

                        await ExecuteAnimationStep( step, token, usedBones );
                    }
                }
                // ...existing code...
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
                // ...existing code...
                // Pause zwischen den Loops
                if ( sequence.Loop && !token.IsCancellationRequested && sequence.LoopDelay > 0 )
                {
                    Log.Info( $"Warte {sequence.LoopDelay} Sekunden zwischen Loops" );
                    await GameTask.DelaySeconds( sequence.LoopDelay );
                }

            } while ( sequence.Loop && !token.IsCancellationRequested );

            Log.Info( $"Sequenz-Schleife beendet für: {sequenceName}" );
        }
        finally
        {
            Log.Info( $"Finally-Block erreicht für Sequenz: {sequenceName}" );
            activeSequences.Remove( sequenceName );
            Log.Info( $"Sequenz aus activeSequences entfernt: {sequenceName}" );

            // Stelle sicher, dass alle verwendeten Knochen zurückgesetzt werden
            foreach ( string boneName in usedBones )
            {
                Log.Info( $"Prüfe Knochen für Cleanup: {boneName}" );
                if ( !activeAnimations.Any( a =>
                {
                    var anim = Animations.Find( anim => anim.Name == a.Key );
                    return anim?.BoneName.Equals( boneName, StringComparison.OrdinalIgnoreCase ) == true;
                } ) )
                {
                    Log.Info( $"Setze Knochen zurück: {boneName}" );
                    RemoveProceduralBone( boneName );
                }
                else
                {
                    Log.Info( $"Knochen wird noch verwendet, kein Reset: {boneName}" );
                }
            }

            Log.Info( $"Sequenz vollständig beendet: {sequenceName}" );
        }
    }

    // Hilfsmethode zum Ausführen eines einzelnen Animationsschritts
    // ...existing code...
    private async Task ExecuteAnimationStep( AnimationStep step, CancellationToken token, HashSet<string> usedBones )
    {
        Log.Info( $"Führe Schritt aus: Animation={step.AnimationName}, Delay={step.Delay}, WaitForCompletion={step.WaitForCompletion}" );

        // Verzögerung vor der Animation abwarten
        if ( step.Delay > 0 )
        {
            Log.Info( $"Warte {step.Delay} Sekunden vor der Animation" );
            await GameTask.DelaySeconds( step.Delay );
        }

        if ( token.IsCancellationRequested )
        {
            Log.Info( $"Abbruch: Sequenzschritt wurde während der Verzögerung abgebrochen" );
            return;
        }

        // Animation starten
        var animation = Animations.Find( a => a.Name == step.AnimationName );
        if ( animation != null )
        {
            Log.Info( $"Animation gefunden: {step.AnimationName}, Knochen: {animation.BoneName}" );

            // Knochen zur Liste der verwendeten Knochen hinzufügen
            if ( !string.IsNullOrEmpty( animation.BoneName ) )
            {
                lock ( usedBones ) // Thread-sicher für parallele Ausführung
                {
                    usedBones.Add( animation.BoneName.ToLower() ); // Konsistente Kleinschreibung verwenden
                }
                Log.Info( $"Knochen hinzugefügt für Cleanup: {animation.BoneName}" );
            }

            var taskCompletionSource = new TaskCompletionSource<bool>();
            Log.Info( $"TaskCompletionSource erstellt für Animation: {step.AnimationName}" );

            // Starte die Animation mit einem expliziten Callback
            PlayAnimation( step.AnimationName, () =>
            {
                Log.Info( $"Animation Callback ausgeführt für: {step.AnimationName}" );
                taskCompletionSource.TrySetResult( true ); // Verwende TrySetResult statt SetResult
            } );

            // Warte optional auf den Abschluss
            if ( step.WaitForCompletion )
            {
                Log.Info( $"Warte auf Abschluss der Animation: {step.AnimationName}" );

                try
                {
                    await taskCompletionSource.Task;
                    Log.Info( $"Animation abgeschlossen: {step.AnimationName}" );
                }
                catch ( Exception ex )
                {
                    Log.Warning( $"Fehler beim Warten auf Animation: {ex.Message}" );
                }
            }
            else
            {
                Log.Info( $"Keine Wartezeit für Animation: {step.AnimationName}" );
            }
        }
        else
        {
            Log.Warning( $"Animation '{step.AnimationName}' nicht gefunden" );
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
                Log.Warning( "Kein SkinnedModelRenderer gefunden!" );
                return;
            }
        }

        // Alternativ: ModelRenderer.BoneCount und ModelRenderer.GetBoneWorldTransform abrufen
        // Da GetBones() nicht existiert, verwenden wir die vorhandenen API-Methoden

        if ( ModelRenderer.Model != null )
        {
            Log.Info( $"Modell geladen: {ModelRenderer.Model.ResourcePath}" );

            // Rekursiv alle GameObjects durchsuchen, die vermutlich Knochen sein könnten
            FindBonesRecursive( GameObject );

            // Zusätzlich: Versuche, über den Transform-Baum die Bones zu finden
            var rootTransform = ModelRenderer.GameObject;
            Log.Info( $"Suche Knochen im Transform-Baum von: {rootTransform.Name}" );
            FindBonesInTransformHierarchy( rootTransform );
        }
        else
        {
            Log.Warning( "Kein Modell im SkinnedModelRenderer gefunden!" );
        }

        Log.Info( $"Insgesamt gefunden: {boneObjects.Count} Bones" );
    }

    // Neue Methode zum Durchsuchen der Transform-Hierarchie nach Bones
    private void FindBonesInTransformHierarchy( GameObject obj )
    {
        // Überprüfe den Namen auf typische Bone-Namen
        string name = obj.Name;
        if ( IsPotentialBoneName( name ) && !boneObjects.ContainsKey( name ) )
        {
            boneObjects[name] = obj;
            Log.Info( $"Potenziellen Knochen im Transform-Baum gefunden: {name}" );
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

    // Füge diese Methode zu BoneAnimationController hinzu:

    public void LogAnimationDetails()
    {
        Log.Info( $"Alle Animationen im Controller ({Animations.Count}):" );
        foreach ( var anim in Animations )
        {
            Log.Info( $"  - Animation: {anim.Name}, Knochen: {anim.BoneName}, Dauer: {anim.Duration}s" );
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

        foreach ( var kvp in boneObjects )
        {
            if ( string.Equals( kvp.Key, normalizedName, StringComparison.OrdinalIgnoreCase ) )
            {
                return kvp.Value;
            }
        }

        // Wenn nicht im Cache, erneut alle Knochen finden
        FindAllBoneObjects();

        // Nach dem Aktualisieren erneut mit Case-insensitive Suche versuchen
        foreach ( var kvp in boneObjects )
        {
            if ( string.Equals( kvp.Key, normalizedName, StringComparison.OrdinalIgnoreCase ) )
            {
                Log.Info( $"Knochen gefunden nach Aktualisierung: '{kvp.Key}' statt '{boneName}'" );
                return kvp.Value;
            }
        }

        Log.Warning( $"Knochen '{boneName}' nicht gefunden. Verfügbare Knochen: {string.Join( ", ", boneObjects.Keys.Take( 5 ) )}..." );
        return null;
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
            Log.Warning( $"Knochen '{animation.BoneName}' nicht gefunden im Modell." );
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
            Log.Info( $"Animation '{animationName}' abgeschlossen, rufe Callback auf" );
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
        Log.Info( $"Animation '{Name}' abspielen" );
        var player = Player.Local;
        if ( player == null )
        {
            Log.Warning( "Kein lokaler Spieler gefunden" );
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
                Log.Info( "Animation temporär zum Controller hinzugefügt" );
            }

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