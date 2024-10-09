using Sandbox;
using GeneralGame.HUD;

namespace GeneralGame;

public sealed class Backpack : ItemEquipment
{
   
    [Property] public float SlotAmount { get; set; }

    // Neues SoundEvent für die Verwendung

    // Parameterloser Konstruktor
    public Backpack()
    {
        Tier = Tier.C; // Standard-Tier
        SlotAmount = CalculateSlotAmount( Tier ); // Berechne die SlotAmount basierend auf dem Tier
    }

    // Konstruktor mit Parameter
    public Backpack( float slotAmount, Tier tier )
    {
        SlotAmount = slotAmount;
        Tier = tier;
    }

    public void Use( Player player )
    {
        player.Health = Math.Min( player.MaxHealth, player.Health + SlotAmount );
        Log.Info( $"Player healed by {SlotAmount}. Current health: {player.Health}" );
    }

    private float CalculateSlotAmount( Tier tier )
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
}