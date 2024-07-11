using Sandbox;
using GeneralGame;

namespace GeneralGame;

public partial class Player : Component
{
	[Property, Sync , Group( "Movement" )]public float PlayerRunSpeed { get; set; } = 150f;
	[Sync,Property, Group( "Movement" )]public float PlayerWalkSpeed { get; private set; } = 150f; // Maximale Laufgeschwindigkeit
    [Sync,Property, Group( "Movement" )]public float MaxStamina { get;  set; } = 100f; // Maximale Ausdauer
    [Sync,Property, Group( "Movement" )]public float Stamina { get; private set; } = 100f; // Aktuelle Ausdauer
    [Sync,Property, Group( "Movement" )] public float StaminaPerSecond { get; private set; } = 10f;
	private RealTimeSince TimeSinceStoppedRunning { get; set; }
	private bool wasRunning = false;
	private bool RegenDelayed { get; set; }
	private const float RegenDelayDuration = 2.5f;
	

	
    private void RegenerateStamina()
    {
        if (IsRunning)
    {
        if (Stamina > 0)
        {
            MoveSpeed = PlayerRunSpeed + Stamina / MaxStamina * 200f;
            Stamina -= Time.Delta * 5f;
            StaminaPerSecond = 0f; // Set StaminaPerSecond to 0 while running
           
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

        if (wasRunning)
        {
            RegenDelayed = true;
            TimeSinceStoppedRunning = 0f;
           
        }

        if (RegenDelayed && TimeSinceStoppedRunning > RegenDelayDuration)
        {
            StaminaPerSecond = 10f; // Set StaminaPerSecond to 10 while not running
            Stamina += StaminaPerSecond * Time.Delta;
            if (Stamina > MaxStamina)
            {
                Stamina = MaxStamina;
            }
        }

        // Add this condition to start regenerating stamina when it's 0 or less
        if (Stamina <= 0)
        {
            StaminaPerSecond = 10f; // Set StaminaPerSecond to 10 while not running
            Stamina += StaminaPerSecond * Time.Delta;
            if (Stamina > MaxStamina)
            {
                Stamina = MaxStamina;
            }
        }
    }

    wasRunning = IsRunning;
    
    

    // Versuche, erneut zu sprinten, wenn genügend Zeit vergangen ist und die Ausdauer ausreichend ist
    
}
    public bool TryJump()
    {
        const float staminaCostForJump = 5f;
        if ( Stamina >= staminaCostForJump )
        {
            Stamina -= staminaCostForJump;
            // Optional: Fügen Sie hier Logik für den Sprung hinzu, z.B. Animation, Bewegung, etc.
            return true;
        }
        else
        {
            // Optional: Benachrichtigung, dass nicht genug Ausdauer zum Springen vorhanden ist
            Log.Info( "Nicht genügend Ausdauer zum Springen." );
            return false;
        }
    }
}