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
            Tier.Ultimate => 250.0f,
            _ => 50.0f,
        };
    }
    public void GeneratePotionStats()
    {
        int baseMinHeal = 0, baseMaxHeal = 0;
        double tierMultiplier = 1.0;

        // Definieren Sie die Basiswerte und den Multiplikator je nach Tier
        switch ( Tier )
        {
            case Tier.C:
                baseMinHeal = 5;
                baseMaxHeal = 25;
                tierMultiplier = 1.0;
                break;
            case Tier.B:
                baseMinHeal = 5;
                baseMaxHeal = 200;
                tierMultiplier = 1.0 + (1.0 * RequiredLevel / 100);
                break;
            case Tier.A:
                baseMinHeal = 5;
                baseMaxHeal = 350;
                tierMultiplier = 1.0 + (2.0 * RequiredLevel / 100);
                break;
            case Tier.S:
                baseMinHeal = 5;
                baseMaxHeal = 400;
                tierMultiplier = 1.0 + (3.0 * RequiredLevel / 100);
                break;
            case Tier.SS:
                baseMinHeal = 5;
                baseMaxHeal = 650;
                tierMultiplier = 1.0 + (4.0 * RequiredLevel / 100);
                break;
            case Tier.SSS:
                baseMinHeal = 5;
                baseMaxHeal = 1000;
                tierMultiplier = 1.0 + (8.0 * RequiredLevel / 100);
                break;
            default:
                // Keine Erhöhung für unbekannte Tiers
                break;
        }

        // Skalieren Sie die Basiswerte basierend auf dem Tier-Multiplikator
        int minHeal = (int)(baseMinHeal * tierMultiplier);
        int maxHeal = (int)(baseMaxHeal * tierMultiplier);

        // Setzen Sie die HealthAmount basierend auf den berechneten Werten
        HealthAmount = (minHeal + maxHeal) / 2; // Durchschnittswert
    }
}