using System.Threading;
using System;
using Sandbox;

namespace GeneralGame
{
    [Serializable]
    public class AnimationSequence
    {
        public string Name { get; set; } = "Sequenz";

        [Property]
        public List<AnimationStep> Steps { get; set; } = new();

        [Property]
        public SequenceExecutionMode ExecutionMode { get; set; } = SequenceExecutionMode.Sequential;

        [Property]
        public bool Loop { get; set; } = false;

        [Property, ShowIf( "Loop", true )]
        public float LoopDelay { get; set; } = 0.5f;

        // CancellationTokenSource für diese Sequenz
        private CancellationTokenSource tokenSource;

        /// <summary>
        /// Erstellt ein neues CancellationToken für diese Sequenz
        /// </summary>
        public CancellationToken GetNewCancellationToken()
        {
            // Beende vorheriges Token, falls vorhanden
            if ( tokenSource != null )
            {
                tokenSource.Cancel();
                tokenSource.Dispose();
            }

            // Erstelle neues Token
            tokenSource = new CancellationTokenSource();
            return tokenSource.Token;
        }

        /// <summary>
        /// Gibt das aktuelle CancellationToken zurück
        /// </summary>
        public CancellationToken GetCancellationToken()
        {
            if ( tokenSource == null )
            {
                tokenSource = new CancellationTokenSource();
            }

            return tokenSource.Token;
        }

        /// <summary>
        /// Bricht die Sequenz ab
        /// </summary>
        public void Cancel()
        {
            if ( tokenSource != null && !tokenSource.IsCancellationRequested )
            {
                tokenSource.Cancel();
            }
        }
    }

    /// <summary>
    /// Ausführungsmodus für Animations-Sequenzen
    /// </summary>
    public enum SequenceExecutionMode
    {
        /// <summary>Führt die Animationen nacheinander aus</summary>
        Sequential,

        /// <summary>Führt die Animationen parallel aus</summary>
        Parallel,

        /// <summary>Führt eine zufällige Animation aus</summary>
        Random
    }

    /// <summary>
    /// Ein Schritt in einer Animations-Sequenz
    /// </summary>
    [Serializable]
    public class AnimationStep
    {
        [Property]
        public string AnimationName { get; set; }

        [Property]
        public float Delay { get; set; } = 0;

        [Property]
        public bool WaitForCompletion { get; set; } = true;

        [Property]
        public float TimeScale { get; set; } = 1.0f;
    }
}