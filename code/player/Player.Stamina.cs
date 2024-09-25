using Sandbox;
using GeneralGame;
using Sandbox.Citizen;

namespace GeneralGame;

public partial class Player : Component
{
	[Property, Sync , Group( "Movement" )]public float PlayerRunSpeed { get; set; } = 100f;
	[Sync,Property, Group( "Movement" )]public float PlayerWalkSpeed { get;  set; } = 100f; // Maximale Laufgeschwindigkeit
    [Sync,Property, Group( "Movement" )]public float MaxStamina { get;  set; } = 100f; // Maximale Ausdauer
    [Sync,Property, Group( "Movement" )]public float Stamina { get; private set; } = 100f; // Aktuelle Ausdauer
    [Sync,Property, Group( "Movement" )] public float StaminaPerSecond { get; private set; } = 10f;
	private RealTimeSince TimeSinceStoppedRunning { get; set; }
	private bool wasRunning = false;
    private bool wasJumping = false;
    private bool RegenDelayed { get; set; }
	private const float RegenDelayDuration = 2.5f;
    public Vector3 Position => Transform.Position;
    public bool IsSwimming { get; private set; }

    public void SetSwimming( bool isSwimming )
    {
        var citizen = Components.Get<CitizenAnimationHelper>();
        
        

        IsSwimming = isSwimming;

        if ( isSwimming )
        {
            // Logik zum Wechseln in den Schwimm-Modus
            citizen.IsSwimming = true;
            // Weitere Logik zum Schwimmen
        }
        else
        {
            // Logik zum Wechseln in den normalen Geh-Modus
            citizen.IsGrounded = true;
            // Weitere Logik zum Gehen
        }
    }

    private void RegenerateStamina()
    {
        if ( IsRunning )
        {
            if ( Stamina > 0 )
            {
                MoveSpeed = PlayerRunSpeed + Stamina / MaxStamina * 200f;
                Stamina -= Time.Delta * 5f;
                StaminaPerSecond = 0f; // Setze StaminaPerSecond auf 0 während des Laufens
            }
            else
            {
                MoveSpeed = 40f;
                IsRunning = false;
            }
        }
        else
        {
            MoveSpeed = PlayerRunSpeed;
            if ( wasRunning )
            {
                RegenDelayed = true;
                TimeSinceStoppedRunning = 0f;
            }
            if ( RegenDelayed && TimeSinceStoppedRunning > RegenDelayDuration )
            {
                StaminaPerSecond = 10f; // Setze StaminaPerSecond auf 10 während des Nicht-Laufens
                Stamina = Math.Max( 0, Stamina ); // Stelle sicher, dass Stamina nicht unter 0 fällt
                Stamina += StaminaPerSecond * Time.Delta;
                if ( Stamina > MaxStamina )
                {
                    Stamina = MaxStamina;
                }
            }
            // Füge diese Bedingung hinzu, um die Regeneration der Ausdauer zu starten, wenn sie 0 oder weniger ist
            if ( Stamina <= 0 )
            {
                StaminaPerSecond = 10f; // Setze StaminaPerSecond auf 10 während des Nicht-Laufens
                Stamina = Math.Max( 0, Stamina ); // Stelle sicher, dass Stamina nicht unter 0 fällt
                Stamina += StaminaPerSecond * Time.Delta;
                if ( Stamina > MaxStamina )
                {
                    Stamina = MaxStamina;
                }
            }
            // Füge diese Bedingung hinzu, um die Regeneration der Ausdauer zu starten, wenn sie weniger als MaxStamina ist
            else if ( Stamina < MaxStamina && !isJumping )
            {
                Stamina += StaminaPerSecond * Time.Delta;
                if ( Stamina > MaxStamina )
                {
                    Stamina = MaxStamina;
                }
            }
        }
        wasRunning = IsRunning;
        wasJumping = isJumping;
    }
    public bool TryJump()
    {
        const float staminaCostForJump = 5f;
        if ( Stamina >= staminaCostForJump )
        {
            Stamina -= staminaCostForJump;
            isJumping = true;
            wasJumping = true;
            // Optional: Fügen Sie hier Logik für den Sprung hinzu, z.B. Animation, Bewegung, etc.
            return true;
        }
        else
        {
            // Optional: Benachrichtigung, dass nicht genug Ausdauer zum Springen vorhanden ist
            return false;
        }
    }
}