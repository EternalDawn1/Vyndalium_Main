namespace GeneralGame;

using GeneralGame.HUD;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
public partial class Player
{
    /// <summary>
    /// Character Stats
    /// </summary>
    [Sync, Property, Group( "CharacterStats" )] public float Health { get; set; } = 100f;
    [Sync, Property, Group( "CharacterStats" )] public float Vyndalium { get; set; } = 25f;
    [Sync, Property, Group( "CharacterStats" )] public float StatsPoints { get; set; } = 0f;

    [Sync, Property, Group( "CharacterStats" )] public float MaxHealth { get; set; }
    [Sync, Property, Group( "CharacterStats" )] public float HealthRegenPerSecond { get; set; } = 10f;

    [Sync, Property, Group( "Movement" )] public float MoveSpeed { get; set; } = 150f; // Normale Laufgeschwindigkeit


    [Sync, Property, Group( "CharacterStats" )] public float Mana { get; set; } = 100f;
    [Sync, Property, Group( "CharacterStats" )] public float MaxMana { get; set; } = 100f;
    [Sync, Property, Group( "CharacterStats" )] public float ManaRegenPerSecond { get; set; } = 10f;

    [Sync, Property, Group( "CharacterStats" )] public float Armor { get; set; } = 0f;

    [Sync, Property, Group( "CharacterStats" )] public double AttackSpeed { get; set; } = 1.1f;
    [Sync, Property, Group( "CharacterStats" )] public float AttackPower { get; set; } = 0f;
    [Sync, Property, Group( "CharacterStats" )] public float MagicPower { get; set; } = 0f;
    [Sync, Property, Group( "CharacterStats" )] public double Evasion { get; set; } = 0.1f;
    [Sync, Property, Group( "CharacterStats" )] public double Block { get; set; } = 0f;
    [Sync, Property, Group( "CharacterStats" )] public float MagicDefense { get; set; } = 0f;
    [Sync, Property, Group( "CharacterStats" )] public float CritHitChance { get; set; } = 0f;
    [Sync, Property, Group( "CharacterStats" )] public float CritHitDamage { get; set; } = 0f;
    [Sync, Property, Group( "CharacterStats" )] public double BonusEXPGain { get; set; } = 0.1f;


    [Sync, Property, Group( "CharacterStats" )] public float SlowResist { get; set; } = 0f;
    [Sync, Property, Group( "CharacterStats" )] public double Tenacity { get; set; } = 0f;
    [Sync, Property, Group( "CharacterStats" )] public double StunResist { get; set; } = 0f;
    [Sync, Property, Group( "CharacterStats" )] public float BlindResist { get; set; } = 0f;
    [Sync, Property, Group( "CharacterStats" )] public float FireResist { get; set; } = 0f;
    [Sync, Property, Group( "CharacterStats" )] public float PoisonResist { get; set; } = 0f;
    [Sync, Property, Group( "CharacterStats" )] public float BleedResist { get; set; } = 0f;
    [Sync, Property, Group( "CharacterStats" )] public float FreezeResist { get; set; } = 0f;
    [Sync, Property, Group( "CharacterStats" )] public float IceResist { get; set; } = 0f;
    [Sync, Property, Group( "CharacterStats" )] public float LightningResist { get; set; } = 0f;
    [Sync, Property, Group( "CharacterStats" )] public float LightResist { get; set; } = 0f;
    [Sync, Property, Group( "CharacterStats" )] public float ShadowResist { get; set; } = 0f;
    [Sync, Property, Group( "CharacterStats" )] public float BonusVyndalium { get; set; } = 0f;
    [Sync, Property, Group( "CharacterStats" )] public float BonusScore { get; set; } = 0f;
    [Sync, Property, Group( "CharacterStats" )] public float AttackValue { get; set; } = 0f;
    [Sync, Property, Group( "CharacterStats" )] public float MinArmorValue { get; set; } = 0f;
    [Sync, Property, Group( "CharacterStats" )] public float MaxArmorValue { get; set; } = 0f;
    [Sync, Property, Group( "CharacterStats" )] public float MinAttackValue { get; set; } = 0f;
    [Sync, Property, Group( "CharacterStats" )] public float MaxAttackValue { get; set; } = 0f;
    [Sync, Property, Group( "CharacterStats" )] public float STG { get; set; }
    [Sync, Property, Group( "CharacterStats" )] public float DEX { get; set; }
    [Sync, Property, Group( "CharacterStats" )] public float INT { get; set; }
    [Sync, Property, Group( "CharacterStats" )] public float PER { get; set; }
    [Sync, Property, Group( "CharacterStats" )] public float HE { get; set; } // Fügen Sie diese Zeile hinzu
    [Sync, Property, Group( "CharacterStats" )] public float AbilityHaste { get; set; } // Fügen Sie diese Zeile hinzu
    [Sync, Property, Group( "CharacterStats" )] public float AttackRange { get; set; } // Fügen Sie diese Zeile hinzu
    [Sync, Property, Group( "CharacterStats" )] public float ArmorPenetration { get; set; }
    [Sync, Property, Group( "CharacterStats" )] public float MagicPenetration { get; set; }// Fügen Sie diese Zeile hinzu
    [Sync, Property, Group( "CharacterStats" )] public float FireElementalDamage { get; set; } // Fügen Sie diese Zeile hinzu
    [Sync, Property, Group( "CharacterStats" )] public float IceElementalDamage { get; set; } // Fügen Sie diese Zeile hinzu
    [Sync, Property, Group( "CharacterStats" )] public float LightningElementalDamage { get; set; } // Fügen Sie diese Zeile hinzu
    [Sync, Property, Group( "CharacterStats" )] public float LightElementalDamage { get; set; } // Fügen Sie diese Zeile hinzu
    [Sync, Property, Group( "CharacterStats" )] public float ShadowElementalDamage { get; set; } // Fügen Sie diese Zeile hinzu
    [Sync, Property, Group( "CharacterStats" )] public float PoisonElementalDamage { get; set; } // Fügen Sie diese Zeile hinzu
    [Sync, Property, Group( "CharacterStats" )] public float BleedElementalDamage { get; set; } // Fügen Sie diese Zeile hinzu
    [Sync, Property, Group( "CharacterStats" )] public float FreezeElementalDamage { get; set; } // Fügen Sie diese Zeile hinzu
    [Sync, Property, Group( "CharacterStats" )] public float WaterElementalDamage { get; set; } // Fügen Sie diese Zeile hinzu
    [Sync, Property, Group( "CharacterStats" )] public float EarthElementalDamage { get; set; } // Fügen Sie diese Zeile hinzu
    [Sync, Property, Group( "CharacterStats" )] public float WindElementalDamage { get; set; } // Fügen Sie diese Zeile hinzu
    [Sync, Property, Group( "CharacterStats" )] public float HolyElementalDamage { get; set; }

    [Sync, Property, Group( "CharacterStats" )]
    public bool isFrozen{ get; set; } = false;
    [Sync] public int StrengthCost { get; set; } = 1;
    [Sync] public int AttackPowerCost { get; set; } = 1;
    [Sync] public int ArmorPenetrationCost { get; set; } = 2;
    [Sync] public int AttackRangeCost { get; set; } = 2;
    [Sync] public int AttackSpeedCost { get; set; } = 1;
    [Sync] public int CriticalChanceCost { get; set; } = 3;
    [Sync] public int CriticalDamageCost { get; set; } = 3;
    [Sync] public int EnduranceCost { get; set; } = 1;
    [Sync] public int SlowResistanceCost { get; set; } = 1;
    [Sync] public int BlindResistanceCost { get; set; } = 1;
    [Sync] public int StunResistanceCost { get; set; } = 1;
    [Sync] public int TenacityCost { get; set; } = 1;
    [Sync] public int CoverCost { get; set; } = 1;
    [Sync] public int MagicDefenseCost { get; set; } = 1;
    [Sync] public int ArmorCost { get; set; } = 1;
    [Sync] public int HealthCost { get; set; } = 1;
    [Sync] public int DexterityCost { get; set; } = 3;
    [Sync] public int EvasionCost { get; set; } = 3;
    [Sync] public int AbilityHasteCost { get; set; } = 2;
    [Sync] public int StaminaCost { get; set; } = 2;
    [Sync] public int PlayerWalkSpeedCost { get; set; } = 1;
    [Sync] public int PlayerRunSpeedCost { get; set; } = 1;
    [Sync] public int ManaCost { get; set; } = 2;
    [Sync] public int IntelligenceCost { get; set; } = 1; // Beispielwert
    [Sync] public int MagicPowerCost { get; set; } = 2; // Beispielwert
    [Sync] public int MagicPenetrationCost { get; set; } = 1; // Beispielwert
    [Sync] public int BonusEXPGainCost { get; set; } = 15; // Beispielwert
    [Sync] public int BonusVyndaliumGainCost { get; set; } = 25;
    private int burnDamagePerSecond = 5;
    public string Name { get; set; }

    public void ApplyFreeze( float durationInSeconds )
    {
        
        // Deaktiviere die Bewegungslogik
       

        // Erstelle oder aktiviere die Vignette
        //GameObject.Components.Create<HealthEffects>().FreezeEffect();
        
        

        // Verwenden Sie einen Timer, um die Bewegungslogik nach der angegebenen Dauer wieder zu aktivieren
        
    }
    private BurningEffect burningEffect;

    public void AddBurningEffect()
    {
        if ( burningEffect == null )
        {
            burningEffect = new BurningEffect();
            
        }
    }

    public void RemoveBurningEffect()
    {
        if ( burningEffect != null )
        {
            burningEffect.RemoveFrom( this );
            burningEffect = null;
        }
    }


    private List<StatusEffect> activeStatusEffects = new List<StatusEffect>();


    public void ApplyStatusEffect( StatusEffect effect )
    {
        if ( effect is BurnEffect burnEffect )
        {
            activeStatusEffects.Add( burnEffect );
            AddBurningEffect();
        }
    }


}
public class BurnEffect : StatusEffect
{
  
    public BurnEffect( float duration )
    {
        Duration = duration;
    }
   

 
    public override void Apply( Player player )
    {
        int damagePerSecond = 5; // Schaden pro Sekunde
        int totalDuration = (int)Duration;  // Gesamtdauer des Brenneffekts in Sekunden

        // Starten Sie einen Timer, der jede Sekunde Schaden zufügt
        for ( int i = 0; i < totalDuration; i++ )
        {
            // Verzögerung um 1 Sekunde
            Task.Delay( 1000 ).ContinueWith( _ =>
            {
                // Überprüfen, ob der Spieler noch lebt
                if ( player.LifeState == LifeState.Alive )
                {
                    // Fügen Sie dem Spieler Schaden zu
                    player.TakeDamage( DamageType.fire, damagePerSecond, player.Position, Vector3.Zero, Guid.Empty, player.Id );
                    Log.Info( $"Burn effect applied to {player.Name}: {damagePerSecond} damage." );
                }
            } );
        }
    }
}
public class BurningEffect
{
    private Panel overlay;

    public BurningEffect()
    {
        overlay = new Panel();
        overlay.Style.BackgroundColor = Color.Red.WithAlpha( 0.5f ); // Rotes Overlay mit Transparenz
        overlay.Style.Width = Length.Percent( 100 );
        overlay.Style.Height = Length.Percent( 100 );
        overlay.Style.Position = PositionMode.Absolute;
        overlay.Style.Top = 0;
        overlay.Style.Left = 0;
        overlay.Style.ZIndex = 1000; // Sicherstellen, dass das Overlay oben angezeigt wird
    }
 

   

    public void RemoveFrom( Player player )
    {
        overlay.Delete();
    }
}

public abstract class StatusEffect
{
    public float Duration { get; set; }
    public abstract void Apply( Player player );
}