using System;
using System.Collections.Generic;
using Sandbox;
using Editor;
using System.Threading;

namespace GeneralGame;

/// <summary>
/// Eine Sequenz von Animationen, die nacheinander abgespielt werden
/// </summary>
[Serializable]
public class AnimationSequence
{
    [Property] public string Name { get; set; } = "Sequenz";
    
    [Property, Title("Animationen in der Sequenz")] 
    public List<SequenceStep> Steps { get; set; } = new();
    
    [Property, Title("Im Loop abspielen")] 
    public bool Loop { get; set; } = false;
    
    [Property, Title("Verzögerung zwischen Wiederholungen (Sekunden)")] 
    public float LoopDelay { get; set; } = 0.5f;
    
    private CancellationTokenSource cancellationToken;
    
    [Button("Vorschau")]
    public void PlaySequence()
    {
        Log.Info($"Animationssequenz '{Name}' abspielen");
        var player = Player.Local;
        if (player == null)
        {
            Log.Warning("Kein lokaler Spieler gefunden");
            return;
        }

        var controller = player.GetComponent<BoneAnimationController>();
        if (controller != null)
        {
            controller.PlaySequence(Name);
        }
        else
        {
            Log.Warning("BoneAnimationController nicht gefunden am Spieler");
        }
    }
    
    public void Cancel()
    {
        cancellationToken?.Cancel();
    }
    
    public CancellationTokenSource GetNewCancellationToken()
    {
        Cancel();
        cancellationToken = new CancellationTokenSource();
        return cancellationToken;
    }
}

/// <summary>
/// Ein einzelner Schritt in einer Animations-Sequenz
/// </summary>
[Serializable]
public class AnimationStep
{
    [Property] public string BoneName { get; set; } = "clavicle_r";
    [Property] public Angles TargetRotation { get; set; } = new Angles( 0, 45, 0 );
    [Property] public float Duration { get; set; } = 0.25f;
    [Property] public float ReturnDuration { get; set; } = 0.5f;
    [Property] public EaseType EaseType { get; set; } = EaseType.EaseInOut;
    
    /// <summary>
    /// Wartezeit nach dieser Animation, bevor die nächste startet
    /// </summary>
    [Property] public float DelayAfter { get; set; } = 0f;
    
    /// <summary>
    /// Wenn true, wird die nächste Animation parallel gestartet
    /// </summary>
    [Property] public bool PlayNextInParallel { get; set; } = false;
}
[Serializable]
public class SequenceStep
{
    [Property] public string AnimationName { get; set; } = "";
    
    [Property, Title("Verzögerung vor dieser Animation (Sekunden)")]
    public float Delay { get; set; } = 0.0f;
    
    [Property, Title("Warten bis Animation abgeschlossen ist")]
    public bool WaitForCompletion { get; set; } = true;
}