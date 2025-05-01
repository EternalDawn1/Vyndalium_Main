using Sandbox;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GeneralGame;

/// <summary>
/// Sequenz-Ausführungsmodus - bestimmt wie Schritte ausgeführt werden
/// </summary>
public enum SequenceExecutionMode
{
    /// <summary>Führt alle Schritte nacheinander aus</summary>
    Sequential,

    /// <summary>Führt alle Schritte gleichzeitig aus</summary>
    Parallel
}

/// <summary>
/// Definiert eine Sequenz von Animationsschritten
/// </summary>
[Serializable]
public class AnimationSequence
{
    [Property] public string Name { get; set; } = "Sequenz";

    /// <summary>
    /// Bestimmt, ob die Schritte nacheinander oder parallel ausgeführt werden
    /// </summary>
    [Property] public SequenceExecutionMode ExecutionMode { get; set; } = SequenceExecutionMode.Sequential;

    [Property] public List<AnimationStep> Steps { get; set; } = new();

    [Property] public bool Loop { get; set; } = false;
    [Property] public float LoopDelay { get; set; } = 0.5f;

    private CancellationTokenSource cancellationToken;

    public void Cancel()
    {
        Log.Info( $"Abbruch der Sequenz '{Name}'" );
        cancellationToken?.Cancel();
    }

    public CancellationToken GetNewCancellationToken()
    {
        cancellationToken?.Cancel();
        cancellationToken = new CancellationTokenSource();
        return cancellationToken.Token;
    }
}

/// <summary>
/// Ein einzelner Schritt in einer Animationssequenz
/// </summary>
[Serializable]
public class AnimationStep
{
    [Property] public string AnimationName { get; set; } = "";
    [Property] public float Delay { get; set; } = 0f;
    [Property] public bool WaitForCompletion { get; set; } = true;
}