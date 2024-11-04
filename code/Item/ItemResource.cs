using System.Text.RegularExpressions;

namespace GeneralGame;



[GameResource( "Item Definition", "item", "Defines an item and its properties." )]
public partial class ItemResource : GameResource
{
    [Group( "Information"), Order( -100) ]
    public string Name { get; set; }

    [Group( "Information" ), Order( -100 )]
    public string Description { get; set; }

    [Group( "Information" ), Order( -100 )]
    public string Category { get; set; }
    [Group( "References" ), Order( -100 )]
    public PrefabFile Prefab { get; set; }

    [Group( "References" ), Order( -100 )]
    public Component Component { get; set; }

   

    [FeatureEnabled( "Item", Icon = "🪙" )] public bool IsItem { get; set; } = false;

    [Feature( "Item" )]
    [Group( "General" )]
    public Tier Tier { get; set; }

    [Feature( "Item" )]
    [Group( "General" )]
    public IconSettings Icon { get; set; }



    [Feature( "Item" )]
    public int RequiredLevel { get; set; }

    [Feature( "Item" )]
    [Group( "General" ), Range( 0, 27 )]
    public int ItemLevel { get; set; }

    [Feature( "Item" )]
    [Group( "General" )]
    public int BuyPrice { get; set; }

    [Feature( "Item" )]
    [Group( "General" )]
    public int SellPrice { get; set; }

    [Feature( "Item" )]
    [Group( "General" )]
    public int MaxStack { get; set; } = 1;

    [Feature( "Item" )]
    [Group( "General" )]
    public int WeightInGrams { get; set; }

  
    [Feature( "Item" )]
    [Group( "IsProperty" )]
  
    public bool IsFavorite { get; set; }

    [Feature( "Item" )]
    [Group( "IsProperty" )]
   
    public bool IsMaterial { get; set; }

    [Feature( "Item" )]
    [Group( "IsProperty" )]
  
    public bool IsPotion { get; set; }

    [Feature( "Item" )]
    [Group( "IsProperty" )]
  
    public bool IsWeapon { get; set; }

    [Feature( "Item" )]
    [Group( "IsProperty" )]
  
    public bool IsArmor { get; set; }

    [Feature( "Item" )]
    [Group( "IsProperty" )]
   
    public bool IsAccessory { get; set; }

    [Feature( "Item" )]
    [Group( "IsProperty" )]
   
    public bool IsConsumable { get; set; }

    [Feature( "Item" )]
    [Group( "IsProperty" )]
   
    public bool IsAspect { get; set; }

    [Feature( "Item" )]
    [Group( "IsProperty" )]
   
    public bool IsBackpack { get; set; }

    [Feature( "Item" )]
    [Group( "IsProperty" )]
    public bool IsWorld { get; set; }

    [Feature( "Item" ), Group( "Stats" ), Range( 0, 1800 )]
    public int DMG { get; set; }

    [Feature( "Item" ), Group( "Stats" ), Range( 0, 100 )]
    public int STG { get; set; }

    [Feature( "Item" ), Group( "Stats" ), Range( 0, 100 )]
    public int HE { get; set; }

    [Feature( "Item" ), Group( "Stats" ), Range( 0, 100 )]
    public int DEX { get; set; }

    [Feature( "Item" ), Group( "Stats" ), Range( 0, 100 )]
    public int PER { get; set; }

    [Feature( "Item" ), Group( "Stats" ), Range( 0, 100 )]
    public float INT { get; set; }

    [Feature( "Item" ), Group( "Stats" ), Range( 0, 100 )]
    public float Mana { get; set; }

    [Feature( "Item" ), Group( "Stats" ), Range( 0, 100 )]
    public float Health { get; set; }

  

    [Feature( "Item" ), Group( "Stats" ), Range( 0, 175 )]
    public float CritHitDamage { get; set; }

    [Feature( "Item" ), Group( "Stats" ), Range( 0, 150 )]
    public float CritHitChance { get; set; }

    [Feature( "Item" ), Group( "Stats" ), Range( 0, 46 )]
    public float AbilityHaste { get; set; }

    [Feature( "Item" ), Group( "Stats" ), Range( 0, 1000 )]
    public float AttackPower { get; set; }

    [Feature( "Item" ), Group( "Stats" ), Range( 0, 1000 )]
    public float MagicPower { get; set; }

    [Feature( "Item" ), Group( "Stats" ), Range( 0, 1000 )]
    public float MinAttackValue { get; set; }

    [Feature( "Item" ), Group( "Stats" ), Range( 0, 1000 )]
    public float MaxAttackValue { get; set; }

    [Feature( "Item" ), Group( "Stats" ), Range( 0, 1000 )]
    public float MinArmorValue { get; set; }

    [Feature( "Item" ), Group( "Stats" ), Range( 0, 1000 )]
    public float MaxArmorValue { get; set; }

    [Feature( "Item" ), Group( "Stats" ), Range( 0, 60 )]
    public float AttackSpeed { get; set; }

    [Feature( "Item" ), Group( "Stats" ), Range( 0, 1000 )]
    public float MoveSpeed { get; set; }

    [Feature( "Item" ), Group( "Stats" ), Range( 0, 1000 )]
    public float Armor { get; set; }

    [Feature( "Item" ), Group( "Stats" ), Range( 0, 500 )]
    public float MagicDefense { get; set; }

    [Feature( "Item" ), Group( "Stats" ), Range( 0, 100 )]
    public float Evasion { get; set; }

    [Feature( "Item" ), Group( "Stats" ), Range( 0, 100 )]
    public float Cover { get; set; }

    [Feature( "Item" ), Group( "Stats" ), Range( 0, 300 )]
    public float BonusEXP { get; set; }

    [Feature( "Item" ), Group( "Stats" ), Range( 0, 500 )]
    public float BonusScore { get; set; }

    [Feature( "Item" ), Group( "Stats" ), Range( 0, 200 )]
    public float BonusVyndalium { get; set; }

    [Feature( "Item" ), Group( "Stats" ), Range( 0, 150 )]
    public float Tenacity { get; set; }

    [Feature( "Item" ), Group( "Resistance" ), Range( 0, 100 )]
    public float StunResistance { get; set; }

    [Feature( "Item" ), Group( "Resistance" ), Range( 0, 100 )]
    public float BlindResistance { get; set; }

    [Feature( "Item" ), Group( "Resistance" ), Range( 0, 100 )]
    public float SlowResistence { get; set; }

    [Feature( "Item" ), Group( "Resistance" ), Range( 0, 100 )]
    public float FireResistence { get; set; }

    [Feature( "Item" ), Group( "Resistance" ), Range( 0, 100 )]
    public float BleedResistance { get; set; }

    [Feature( "Item" ), Group( "Resistance" ), Range( 0, 100 )]
    public float PoisonResistence { get; set; }

    [Feature( "Item" ), Group( "Resistance" ), Range( 0, 100 )]
    public float IceResistence { get; set; }

    [Feature( "Item" ), Group( "Resistance" ), Range( 0, 100 )]
    public float LightningResistence { get; set; }

    [Feature( "Item" ), Group( "Resistance" ), Range( 0, 100 )]
    public float HolyResistence { get; set; }

    [Feature( "Item" ), Group( "Resistance" ), Range( 0, 100 )]
    public float ShadowResistence { get; set; }

   

    
}