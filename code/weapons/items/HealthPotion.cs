using Sandbox;
using GeneralGame.HUD;
namespace GeneralGame;

public sealed class HealthPotion : ItemComponent
{
    [Property] public float HealthAmount { get; set; }
    
   // Neues SoundEvent für die Verwendung

    // Parameterloser Konstruktor
    public HealthPotion()
    {
        Tier = Tier.C; // Standard-Tier
        HealthAmount = CalculateHealthAmount( Tier ); // Berechne die Heilmenge basierend auf dem Tier
    }

    // Konstruktor mit Parameter
    public HealthPotion( float healthAmount, Tier tier )
    {
        HealthAmount = healthAmount;
        Tier = tier;
    }

    public void Use( Player player )
    {
        player.Health = Math.Min( player.MaxHealth, player.Health + HealthAmount );
        Log.Info( $"Player healed by {HealthAmount}. Current health: {player.Health}" );

        
        
    }

    private float CalculateHealthAmount( Tier tier )
    {
        return tier switch
        {
            Tier.C => 50.0f,
            Tier.B => 75.0f,
            Tier.A => 100.0f,
            Tier.S => 125.0f,
            Tier.SS => 150.0f,
            Tier.SSS => 200.0f,
            _ => 50.0f,
        };
    }
}