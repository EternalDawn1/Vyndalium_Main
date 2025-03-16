
using Sandbox.Physics;
using Sandbox.Utility;
using System.Numerics;
using System.Linq;
using Editor;
namespace GeneralGame;

public enum ItemState
{
	None,
	Backpack,
	Equipped,
	StorageBox,
	Storage,
	Shop,
	Upgrade,
	Materials,
	Aspect,
	BackpackBag,

}
public enum Tier 
{
	C,
	B,
	A,
	S,
	SS,
	SSS,
	Ultimate,
}
public enum AspectType
{
	None,
	Fire,
	Water,
	Ice,
	Air,
	Earth,
	Shadow,
	Holy,
	Bleed,
	Poison,
	Lightning,
	Glitch,
}






public class ItemComponent : Component
{
	[Property, Group( "General" )]
	public bool IsMelee { get; set; }

	public bool IsFavorite { get; set; }
	[Property, Group( "Type" )]
	public bool IsMaterial { get; set; }
	[Property , Group( "Type" )]
	public bool IsPotion { get; set; }
	[Property , Group( "Type" )]
	public bool IsWeapon { get; set; }
	[Property , Group( "Type" )]
	public bool IsArmor { get; set; }
	[Property , Group( "Type" )]
	public bool IsAccessory { get; set; }
	[Property , Group( "Type" )]
	public bool IsConsumable { get; set; }

	[Property, Group( "Type" )] 
	public bool IsChest { get; set; }
	
	[Property, Group( "Type" )] public bool IsAspect { get; set; }
	[Property, Group( "Type" )] public bool IsBackpack { get; set; }
	[Property, Group( "Type" )] public bool IsWorld { get; set; }
	
	public bool CanEquip( int playerLevel )
	{
		return playerLevel >= RequiredLevel;
	}
	
	public void ApplyAspect( ItemComponent aspectItem )
	{
		this.Aspect = aspectItem.Aspect;
		Log.Info( $"Applied aspect {aspectItem.Aspect} to {Name}" );
	}

	[Property]public int BuyPrice { get; set; }

	[Property ,Group("Main"),Range(0,100)]public int RequiredLevel { get; set; }

	public string GetAspectAsString()
	{
		return Aspect.ToString();
	}
	public ItemResource ItemResource { get; set; }


	public ItemComponent()
	{
		InitializeStats();
		IsAspect = false;
		Aspect = AspectType.None;
		
	}
	private void InitializeStats()
	{
		if ( !_isDMGInitialized )
		{
			_dmg = GenerateRandomDMG( Tier );
			_isDMGInitialized = true;
		}
	}
	
	[Property,]public AspectType Aspect { get; set; }
	/// <summary>
	/// The name of the item.
	/// </summary>
	[Sync]
	[Property]
	public string Name { get; set; }

	/// <summary>
	/// The icon to display.
	/// </summary>
	[Property] public IconSettings Icon { get; set; }

	/// <summary>
	/// The description of the item.
	/// </summary>
	[Property] public string Description { get; set; }

	/// <summary>
	/// Weapon All things
	/// </summary>
	/// 
	public double SuccessChance { get; set; }
	private bool _isDMGInitialized = false;
	[Property, Group( "Weapon" ), Range( 0, 1800 )]
	public int DMG
	{
		get => _dmg;
		set
		{
			_dmg = value;
		}
	}
	public string GetAspectIcon()
	{
		return Aspect switch
		{
			AspectType.Fire => "ui/textures/fire.png",
			AspectType.Water => "ui/aspects/water-drop.png",
			AspectType.Ice => "ui/textures/snow.png",
			AspectType.Air => "ui/textures/storm.png",
			AspectType.Earth => "ui/hud/tree.png",
			AspectType.Shadow => "ui/aspects/purple-ribbon.png",
			AspectType.Holy => "ui/aspects/holy-star.png",
			AspectType.Bleed => "ui/textures/blood.png",
			AspectType.Poison => "ui/aspects/poison.png",
			AspectType.Lightning => "ui/aspects/flash.png",
			AspectType.Glitch => "ui/aspects/bath-salt-bomb.png",
			_ => "",
		};
	}
	private int _dmg;
	[Property, Group( "Weapon" ), Range( 0, 100 )] public int STG { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 100 )] public int HE { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 100 )] public int DEX { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 100 )] public int PER { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 100 )] public float INT { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 100 )] public float Mana { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 100 )] public float Health { get; set; }
	
	[Property, Range( 0, 27 )] public int ItemLevel { get; set; }
	[Property,Group("Weapon"),Range(1,20)]public float FireRate { get; set; }
	[Property,Group("Weapon"),Range(0,1000)]public float BulletSpeed { get; set; }
	[Property,Group("Weapon"),Range(0,1000)]public float BulletSpread { get; set; }
	[Property,Group("Weapon"),Range(0,1000)]public float Clipsize { get; set; }

	[Property, Group( "Weapon" ), Range( 0, 175 )] public float CritHitDamage { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 150 )] public float CritHitChance { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 46 )] public float AbilityHaste { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float AttackPower { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float MagicPower { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float MinAttackValue { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float MaxAttackValue { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float MinArmorValue { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float MaxArmorValue { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float ArmorPenetration { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float MaxHealthDMG { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float AttackRange { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float AttackSpeed { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float MagicPenetration { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float FireElementalDamage { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float IceElementalDamage { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float LightningElementalDamage { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float HolyElementalDamage { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float LightElementalDamage { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float ShadowElementalDamage { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float PoisonElementalDamage { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float BleedElementalDamage { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float FreezeElementalDamage { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float WaterElementalDamage { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float EarthElementalDamage { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float WindElementalDamage { get; set; }



	[Property] public Tier Tier { get; set; }
	[Property, Range( 100, 0 )] public int DamageBalance { get; set; }
	[Property, Range( 1000, 0 )] public int Durability { get; set; }
	
	
	

	[Property,Group ("Armor"),Range(0,1000)]public float HealthRegen { get; set; }
	[Property, Group( "Armor" ), Range( 0, 1000 )] public float ManaRegen { get; set; }
	[Property, Group( "Armor" ), Range( 0, 1000 )] public float MoveSpeed { get; set; }
	[Property, Group( "Armor" ), Range( 0, 1000 )] public float Armor { get; set; }
	[Property, Group( "Armor" ), Range( 0, 500 )] public float MagicDefense { get; set; }
	[Property, Group( "Armor" ), Range( 0, 100 )] public float Evasion { get; set; }
	[Property, Group( "Armor" ), Range( 0, 100 )] public float Cover { get; set; }
	[Property, Group( "Accessory" ), Range( 0, 300 )] public float BonusEXP { get; set; }
	[Property, Group( "Accessory" ), Range( 0, 500 )] public float BonusScore { get; set; }
	[Property, Group( "Accessory" ), Range( 0, 200 )] public float BonusVyndalium { get; set; }
	[Property, Group( "Accessory" ), Range( 0, 150 )] public float Tenacity { get; set; }
	[Property, Group( "Armor" ), Range( 0, 100 )] public float StunResistance { get; set; }
	[Property, Group( "Accessory" ), Range( 0, 100 )] public float BlindResistance { get; set; }
	[Property, Group( "Accessory" ), Range( 0, 100 )] public float SlowResistence { get; set; }
	[Property, Group( "Accessory" ), Range( 0, 100 )] public float FireResistence { get; set; }
	[Property, Group( "Accessory" ), Range( 0, 100 )] public float BleedResistance { get; set; }
	[Property, Group( "Accessory" ), Range( 0, 100 )] public float PoisonResistence { get; set; }
	[Property, Group( "Accessory" ), Range( 0, 100 )] public float IceResistence { get; set; }
	[Property, Group( "Accessory" ), Range( 0, 100 )] public float LightningResistence { get; set; }
	[Property, Group( "Accessory" ), Range( 0, 100 )] public float HolyResistence { get; set; }
	[Property, Group( "Accessory" ), Range( 0, 100 )] public float ShadowResistence { get; set; }
	[Property, Group( "Accessory" ), Range( 0, 100 )] public float StaminaSecond { get; set; }
	[Property, Group( "Accessory" ), Range( 0, 100 )] public float WalkSpeed { get; set; }
	[Property, Group( "Accessory" ), Range( 0, 100 )] public float RunSpeed { get; set; }
	[Property, Group( "Accessory" ), Range( 0, 100 )] public float Stamina { get; set; }
	[Property,Group("Accessory"),Range(0,100)]public float StunResist { get; set; }
	[Property,Group("Accessory"),Range(0,100)]public float BlindResist { get; set; }
	[Property,Group("Accessory"),Range(0,100)]public float SlowResist { get; set; }
	[Property,Group("Accessory"),Range(0,100)]public float FireResist { get; set; }
	[Property,Group("Accessory"),Range(0,100)]public float FreezeResist { get; set; }
	[Property,Group("Accessory"),Range(0,100)]public float BleedResist { get; set; }
	[Property,Group("Accessory"),Range(0,100)]public float PoisonResist { get; set; }
	[Property,Group("Accessory"),Range(0,100)]public float IceResist { get; set; }
	[Property,Group("Accessory"),Range(0,100)]public float LightningResist { get; set; }
	[Property,Group("Accessory"),Range(0,100)]public float HolyResist { get; set; }
	[Property,Group("Accessory"),Range(0,100)]public float ShadowResist { get; set; }
	[Property,Group("Accessory"),Range(0,100)]public float LightResist { get; set; }

	



	




	public class TierClass
	{
		public Tier _tier;

		public Tier Tier
		{

			get { return _tier; }
			set { _tier = value; }

		}

		public static implicit operator Tier( TierClass v )
		{
			throw new NotImplementedException();
		}
	}






	/// <summary>
	/// The sell price of an item in mk (-1 indicating it cannot be sold).
	/// </summary>
	[Property, Sync] public int SellPrice { get; set; } = 0;

	/// <summary>
	/// Maximum amount of items in this stack, default is 0 for not stackable.
	/// </summary>
	[Property]
	public int MaxStack { get; set; } =1;

	private int _count = 1;

	/// <summary>
	/// The count of items in the stack.
	/// </summary>
	[Property]
	public int Count
	{
		get => _count;
		set
		{
			if ( value > MaxStack )
			{
				_count = MaxStack;
			}
			else
			{
				_count = value;
			}
		}
	}
	[Sync] public string Prefab { get; private set; }

	public Texture IconTexture => Texture.Load( FileSystem.Mounted, Icon.Path );
	public static implicit operator ItemComponent( GameObject obj )
		=> obj.Components.Get<ItemComponent>();

	/// <summary>
	/// If the item is in the player's inventory (this includes backpack and equipped items).
	/// </summary>


	/// <summary>
	/// Whether the item can be sold.
	/// </summary>


	/// <summary>
	/// Whether the item can be sold.
	/// </summary>
	public bool IsStackable => MaxStack >= 1;

	/// <summary>
	/// The last player that had this item parented to them.
	/// </summary>
	[Property] public Player LastOwner { get; set; }

	private readonly SoundEvent _pickupSound = ResourceLibrary.Get<SoundEvent>( "sounds/misc/pickup.sound" );

	private ItemState _state;
	[Property] public bool IsItem { get; set; }
	[Property] public bool IsEquipment { get; set; }


	/// <summary>
	/// If the item is in the player's backpack (note not equipped!).
	/// </summary>
	[Sync]
	public ItemState State
	{
		get => _state;
		set
		{
			_state = value;
			UpdateState();
			
		}
	}

	public TierClass ItemTier { get; set; }
	


	public int GenerateRandomDMG( Tier tier )
	{
		Random random = new Random();
		double roll = random.NextDouble() * 100;
		int maxTierValue = tier switch
		{
			Tier.C => 200,
			Tier.B => 400,
			Tier.A => 600,
			Tier.S => 800,
			Tier.SS => 1000,
			Tier.SSS => 2000,
			Tier.Ultimate => 3000,
			_ => 0
		};
		int minimumTierValue = tier switch
		{
			Tier.C => 10,
			Tier.B => 100,
			Tier.A => 200,
			Tier.S => 300,
			Tier.SS => 400,
			Tier.SSS => 500,
			Tier.Ultimate => 1000,
			_ => 0
		};

		if ( roll < 55 ) // 55% Wahrscheinlichkeit
		{
			return random.Next( minimumTierValue, (int)(maxTierValue * 0.4) + 1 ); // minimumTierValue bis 40% des maxTierValue
		}
		else if ( roll < 80 ) // 25% Wahrscheinlichkeit
		{
			return random.Next( (int)(maxTierValue * 0.4), (int)(maxTierValue * 0.6) + 1 ); // 40% bis 60% des maxTierValue
		}
		else if ( roll < 95 ) // 15% Wahrscheinlichkeit
		{
			return random.Next( (int)(maxTierValue * 0.6), (int)(maxTierValue * 0.8) + 1 ); // 60% bis 80% des maxTierValue
		}
		else // 5% Wahrscheinlichkeit
		{
			return random.Next( (int)(maxTierValue * 0.8), maxTierValue + 1 ); // 80% bis maxTierValue
		}
	}
	public int Level { get; set; } = 1;
	public int GetPlayerLevel()
	{
		var player = Player.Local;
		if ( Player.Local != null )
		{
			return player.Level;
		}
		else if ( Player.Local == null )
		{
			return Level;
		}
		else
		{
			// Fallback-Wert, wenn Player.Local null ist
			return Player.Local.Level; // Beispielwert, kann angepasst werden
		}

	}
	public virtual int DetermineRequiredLevelForTier( string tier )
	{
		int playerLevel = GetPlayerLevel();
		int maxLevel = Player.Local?.GetMaxLevel() ?? 100;
		 // Verwenden Sie das maximale Level des Spielers oder 100 als Fallback
		var random = new Random();
		int requiredLevel;

		// Bestimme die Basislevel in 5er-Schritten
		int baseLevel = (playerLevel / 5) * 5;

		// Wahrscheinlichkeitsbasierte Berechnung
		int chance = random.Next( 100 );
		if ( chance < 60 ) // 70% Chance auf Level innerhalb von 5 Leveln höher oder gleich dem Basislevel
		{
			int lowerBound = Math.Max( baseLevel, 0 );
			int upperBound = Math.Min( baseLevel + 5, baseLevel + 5 );
			requiredLevel = random.Next( lowerBound, upperBound + 1 );
		}
		else if ( chance < 70 ) // 20% Chance auf Level innerhalb von 10 Leveln höher oder gleich dem Basislevel
		{
			int lowerBound = Math.Max( baseLevel, 0 );
			int upperBound = Math.Min( baseLevel + 10, maxLevel );
			requiredLevel = random.Next( lowerBound, upperBound + 1 );
		}
		else if ( chance < 75 ) // 10% Chance auf Level innerhalb von 15 Leveln höher oder gleich dem Basislevel
		{
			int lowerBound = Math.Max( baseLevel, 0 );
			int upperBound = Math.Min( baseLevel + 15, maxLevel );
			requiredLevel = random.Next( lowerBound, upperBound + 1 );
		}
		else if ( chance < 80 )
		{
			int lowerBound = Math.Max( baseLevel, 0 );
			int upperBound = Math.Min( baseLevel + 20, maxLevel );
			requiredLevel = random.Next( lowerBound, upperBound + 1 );
		}
		else if ( chance < 85 )
		{
			int lowerBound = Math.Max( baseLevel, 0 );
			int upperBound = Math.Min( baseLevel + 25, maxLevel );
			requiredLevel = random.Next( lowerBound, upperBound + 1 );
		}
		else if ( chance < 90 )
		{
			int lowerBound = Math.Max( baseLevel, 0 );
			int upperBound = Math.Min( baseLevel + 30, maxLevel );
			requiredLevel = random.Next( lowerBound, upperBound + 1 );
		}
		else if ( chance < 95 )
		{
			int lowerBound = Math.Max( baseLevel, 0 );
			int upperBound = Math.Min( baseLevel + 35, maxLevel );
			requiredLevel = random.Next( lowerBound, upperBound + 1 );
		}
		else if ( chance < 100 )
		{
			int lowerBound = Math.Max( baseLevel, 0 );
			int upperBound = Math.Min( baseLevel + 40, maxLevel );
			requiredLevel = random.Next( lowerBound, upperBound + 1 );
		}
		else if ( chance < 105 )
		{
			int lowerBound = Math.Max( baseLevel, 0 );
			int upperBound = Math.Min( baseLevel + 45, maxLevel );
			requiredLevel = random.Next( lowerBound, upperBound + 1 );
		}
		else if ( chance < 110 )
		{
			int lowerBound = Math.Max( baseLevel, 0 );
			int upperBound = Math.Min( baseLevel + 50, maxLevel );
			requiredLevel = random.Next( lowerBound, upperBound + 1 );
		}
		else if ( chance < 115 )
		{
			int lowerBound = Math.Max( baseLevel, 0 );
			int upperBound = Math.Min( baseLevel + 55, maxLevel );
			requiredLevel = random.Next( lowerBound, upperBound + 1 );
		}
		else if ( chance < 120 )
		{
			int lowerBound = Math.Max( baseLevel, 0 );
			int upperBound = Math.Min( baseLevel + 60, maxLevel );
			requiredLevel = random.Next( lowerBound, upperBound + 1 );
		}
		else if ( chance < 125 )
		{
			int lowerBound = Math.Max( baseLevel, 0 );
			int upperBound = Math.Min( baseLevel + 65, maxLevel );
			requiredLevel = random.Next( lowerBound, upperBound + 1 );
		}
		else if ( chance < 130 )
		{
			int lowerBound = Math.Max( baseLevel, 0 );
			int upperBound = Math.Min( baseLevel + 70, maxLevel );
			requiredLevel = random.Next( lowerBound, upperBound + 1 );
		}
		else if ( chance < 135 )
		{
			int lowerBound = Math.Max( baseLevel, 0 );
			int upperBound = Math.Min( baseLevel + 75, maxLevel );
			requiredLevel = random.Next( lowerBound, upperBound + 1 );
		}
		else if ( chance < 140 )
		{
			int lowerBound = Math.Max( baseLevel, 0 );
			int upperBound = Math.Min( baseLevel + 80, maxLevel );
			requiredLevel = random.Next( lowerBound, upperBound + 1 );
		}
		else if ( chance < 145 )
		{
			int lowerBound = Math.Max( baseLevel, 0 );
			int upperBound = Math.Min( baseLevel + 85, maxLevel );
			requiredLevel = random.Next( lowerBound, upperBound + 1 );
		}

		else
		{
			int lowerBound = Math.Max( baseLevel, 0 );
			int upperBound = Math.Min( baseLevel + 45, maxLevel );
			requiredLevel = random.Next( lowerBound, upperBound + 1 );
		}




		// Füge die Möglichkeit hinzu, dass Items auch 5 oder 10 Level unter dem Spielerlevel droppen können
		if ( random.Next( 100 ) < 20 ) // 20% Chance auf Level innerhalb von 5 Leveln unter dem Spielerlevel
		{
			int lowerBound = Math.Max( playerLevel - 5, 0 );
			requiredLevel = random.Next( lowerBound, playerLevel + 1 );
		}
		else if ( random.Next( 100 ) < 10 ) // 10% Chance auf Level innerhalb von 10 Leveln unter dem Spielerlevel
		{
			int lowerBound = Math.Max( playerLevel - 10, 0 );
			requiredLevel = random.Next( lowerBound, playerLevel + 1 );
		}

		return requiredLevel;
	}
	public int CalculateSellPrice()
	{
		int basePrice = 0;
		int additionalPricePerStat = 100;
		int numberOfStats = GetNumberOfStats();

		switch ( Tier )
		{
			case Tier.C:
				basePrice = 0;
				break;
			case Tier.B:
				basePrice = 150;
				break;
			case Tier.A:
				basePrice = 300;
				break;
			case Tier.S:
				basePrice = 600;
				break;
			case Tier.SS:
				basePrice = 900;
				break;
			case Tier.SSS:
				basePrice = 1500;
				break;
			case Tier.Ultimate:
				basePrice = 3000;
				break;
		}

		int levelPrice = CalculateLevelPrice( ItemLevel );
		int upgradeCost = CalculateUpgradeCost( ItemLevel, Tier );
		int additionalUpgradeCost = (int)(upgradeCost * 0.5);

		// Berücksichtige den requiredLevel des Items
		int requiredLevel = DetermineRequiredLevelForTier( Tier.ToString() );

		// Berechne den Verkaufspreis für jedes requiredLevel
		int sellPrice = basePrice + (numberOfStats * additionalPricePerStat) + levelPrice + additionalUpgradeCost;
		SellPrice = sellPrice * requiredLevel;

		return SellPrice;
	}
	private int CalculateUpgradeCost( int itemLevel, Tier tier )
	{
		int baseCost = itemLevel switch
		{
			1 => 200,
			2 => 500,
			3 => 2000,
			24 => 5000,
			25 => 15000,
			26 => 20000,
			27 => 25000,
			_ => (int)(500 * Math.Pow( 1.3, itemLevel - 1 )), // Exponentielle Berechnung für andere Level
		};

		double tierMultiplier = Math.Pow( 1.6, (double)tier - 1 );

		// Berücksichtige den requiredLevel des Items
		int requiredLevel = DetermineRequiredLevelForTier( tier.ToString() );
		double requiredLevelMultiplier = Math.Pow( 1.1, requiredLevel / 5 ); // Beispiel: Multipliziere die Kosten basierend auf dem requiredLevel in 5er-Schritten

		return (int)(baseCost * tierMultiplier * requiredLevelMultiplier);
	}
	private int CalculateLevelPrice( int level )
	{
		if ( level == 0 ) return 0;
		int price = 150; // Preis für Level 1
		for ( int i = 2; i <= level; i++ )
		{
			price += 100 + (i - 1) * 25;
		}
		return price;
	}

	private int GetNumberOfStats()
	{
		int count = 0;
		if ( DMG > 0 ) count++;
		if ( HE > 0 ) count++;
		if ( Armor > 0 ) count++;
		if ( STG > 0 ) count++;
		if ( DEX > 0 ) count++;
		if ( PER > 0 ) count++;
		if ( INT > 0 ) count++;
		if ( Mana > 0 ) count++;
		if ( Health > 0 ) count++;
		if ( CritHitDamage > 0 ) count++;
		if ( CritHitChance > 0 ) count++;
		if ( AbilityHaste > 0 ) count++;
		if ( AttackPower > 0 ) count++;
		if ( MagicPower > 0 ) count++;
		if ( AttackSpeed > 0 ) count++;
		if ( MoveSpeed > 0 ) count++;
		if ( MagicDefense > 0 ) count++;
		if ( Evasion > 0 ) count++;
		if ( Cover > 0 ) count++;
		if ( BonusEXP > 0 ) count++;
		if ( BonusScore > 0 ) count++;
		if ( BonusVyndalium > 0 ) count++;
		if ( Tenacity > 0 ) count++;
		if ( StunResistance > 0 ) count++;
		if ( BlindResistance > 0 ) count++;
		if ( BleedResistance > 0 ) count++;
		if ( SlowResistence > 0 ) count++;
		if ( FireResistence > 0 ) count++;
		if ( PoisonResistence > 0 ) count++;
		if ( IceResistence > 0 ) count++;
		if ( LightningResistence > 0 ) count++;
		if ( HolyResistence > 0 ) count++;
		if ( ShadowResistence > 0 ) count++;
		if ( StaminaSecond > 0 ) count++;
		if ( WalkSpeed > 0 ) count++;
		if ( RunSpeed > 0 ) count++;
		if ( Stamina > 0 ) count++;
		if ( StunResist > 0 ) count++;
		if ( BlindResist > 0 ) count++;
		if ( SlowResist > 0 ) count++;
		if ( FireResist > 0 ) count++;
		if ( FreezeResist > 0 ) count++;
		if ( BleedResist > 0 ) count++;
		if ( PoisonResist > 0 ) count++;
		if ( IceResist > 0 ) count++;
		if ( LightningResist > 0 ) count++;
		if ( HolyResist > 0 ) count++;
		if ( ShadowResist > 0 ) count++;
		if ( LightResist > 0 ) count++;

		return count;
	}
	private void SetDefaultStats()
	{
		Random random = new Random();

		switch ( Tier )
		{
			case Tier.C:
				DamageBalance = random.Next( 90, 101 ); // Bereich 90-100
				Durability = random.Next( 900, 1001 ); // Bereich 900-1000
				break;
			case Tier.B:
				DamageBalance = random.Next( 80, 91 ); // Bereich 80-90
				Durability = random.Next( 1800, 2001 ); // Bereich 1800-2000
				break;
			case Tier.A:
				DamageBalance = random.Next( 70, 81 ); // Bereich 70-80
				Durability = random.Next( 3600, 4001 ); // Bereich 3600-4000
				break;
			case Tier.S:
				DamageBalance = random.Next( 60, 71 ); // Bereich 60-70
				Durability = random.Next( 5400, 6001 ); // Bereich 5400-6000
				break;
			case Tier.SS:
				DamageBalance = random.Next( 50, 61 ); // Bereich 50-60
				Durability = random.Next( 7200, 8001 ); // Bereich 7200-8000
				break;
			case Tier.SSS:
				DamageBalance = random.Next( 30, 41 ); // Bereich 30-40
				Durability = random.Next( 8100, 9001 ); // Bereich 8100-9000
				break;
			case Tier.Ultimate:
				DamageBalance = random.Next( 0, 51 ); // Bereich 0-50
				Durability = random.Next( 9000, 10001 ); // Bereich 9000-10000
				break;
			default:
				DamageBalance = random.Next( 50, 101 ); // Bereich 50-100
				Durability = random.Next( 5000, 10001 ); // Bereich 5000-10000
				break;
		}
	}
	public void GenerateRandomStats()
	{
		if ( IsWeapon )
		{
			GenerateWeaponStats();
			SetDefaultStats();
		}
		else if ( IsArmor )
		{
			GenerateArmorStats();
			SetDefaultStats();
		}
		else if ( IsAccessory )
		{
			GenerateAccessoryStats();
			SetDefaultStats();
		}
		else if ( IsConsumable )
		{
		

		}
		else if ( IsAspect )
		{

		}
		else if ( IsBackpack )
		{

		}
		else if ( IsMaterial )
		{

		}
		else if ( IsPotion )
		{
			
		}
		
	}
	public (int min, int max) GetStatRange( int baseMin, int baseMax )
	{
		switch ( Tier )
		{
			case Tier.C:
				return (Math.Max( 0, baseMin ), Math.Min( 2, baseMax / 5 ));
			case Tier.B:
				return (Math.Max( 2, baseMin ), Math.Min( 4, baseMax * 2 / 5 ));
			case Tier.A:
				return (Math.Max( 4, baseMin ), Math.Min( 6, baseMax * 3 / 5 ));
			case Tier.S:
				return (Math.Max( 6, baseMin ), Math.Min( 8, baseMax * 4 / 5 ));
			case Tier.SS:
				return (Math.Max( 8, baseMin ), Math.Min( 10, baseMax ));
			case Tier.SSS:
				return (Math.Max( 10, baseMin ), Math.Min( 12, baseMax * 2 ));
			case Tier.Ultimate:
				return (Math.Max( 12, baseMin ), Math.Min( 15, baseMax * 3 ));
			default:
				return (baseMin, baseMax);
		}
	}




	private void GenerateWeaponStats()
	{
		Random random = new Random();

		// Definieren Sie die Basiswerte für Waffenstatistiken
		int baseMinSTG = 1, baseMaxSTG = 15;
		int baseMinInt = 1, baseMaxInt = 15;
		int baseMinMana = 1, baseMaxMana = 100;
		int baseMinCritHitDamage = 1, baseMaxCritHitDamage = 15;
		int baseMinCritHitChance = 1, baseMaxCritHitChance = 15;
		int baseMinAttackPower = 1, baseMaxAttackPower = 100;
		int baseMinMagicPower = 1, baseMaxMagicPower = 100;
		int baseMinAttackSpeed = 1, baseMaxAttackSpeed = 14;
		int baseMinMoveSpeed = 1, baseMaxMoveSpeed = 100;
		int baseMinBonusScore = 1, baseMaxBonusScore = 100;
		int baseMinBonusEXP = 1, baseMaxBonusEXP = 100;
		int baseMinBonusVyndalium = 1, baseMaxBonusVyndalium = 100;
		int baseMinArmorPenetration = 1, baseMaxArmorPenetration = 100;
		int baseMinMaxHealthDMG = 1, baseMaxMaxHealthDMG = 100;
		int baseMinAttackRange = 1, baseMaxAttackRange = 1000;
		int baseMinMagicPenetration = 1, baseMaxMagicPenetration = 100;
		int baseMinFireElementalDamage = 1, baseMaxFireElementalDamage = 100;
		int baseMinIceElementalDamage = 1, baseMaxIceElementalDamage = 100;
		int baseMinLightningElementalDamage = 1, baseMaxLightningElementalDamage = 100;
		int baseMinHolyElementalDamage = 1, baseMaxHolyElementalDamage = 100;
		int baseMinLightElementalDamage = 1, baseMaxLightElementalDamage = 100;
		int baseMinShadowElementalDamage = 1, baseMaxShadowElementalDamage = 100;
		int baseMinPoisonElementalDamage = 1, baseMaxPoisonElementalDamage = 100;
		int baseMinBleedElementalDamage = 1, baseMaxBleedElementalDamage = 100;
		int baseMinFreezeElementalDamage = 1, baseMaxFreezeElementalDamage = 100;
		int baseMinWaterElementalDamage = 1, baseMaxWaterElementalDamage = 100;
		int baseMinEarthElementalDamage = 1, baseMaxEarthElementalDamage = 100;
		int baseMinWindElementalDamage = 1, baseMaxWindElementalDamage = 100;
		



		

		// Bestimmen Sie die maximalen Werte basierend auf dem Tier
		var (minSTG, maxSTG) = GetStatRange( baseMinSTG, baseMaxSTG );
		var (minInt, maxInt) = GetStatRange( baseMinInt, baseMaxInt );
		var (minMana, maxMana) = GetStatRange( baseMinMana, baseMaxMana );
		var (minCritHitDamage, maxCritHitDamage) = GetStatRange( baseMinCritHitDamage, baseMaxCritHitDamage );
		var (minCritHitChance, maxCritHitChance) = GetStatRange( baseMinCritHitChance, baseMaxCritHitChance );
		var (minAttackPower, maxAttackPower) = GetStatRange( baseMinAttackPower, baseMaxAttackPower );
		var (minMagicPower, maxMagicPower) = GetStatRange( baseMinMagicPower, baseMaxMagicPower );
		var (minAttackSpeed, maxAttackSpeed) = GetStatRange( baseMinAttackSpeed, baseMaxAttackSpeed );
		var (minMoveSpeed, maxMoveSpeed) = GetStatRange( baseMinMoveSpeed, baseMaxMoveSpeed );
		var (minBonusScore, maxBonusScore) = GetStatRange( baseMinBonusScore, baseMaxBonusScore );
		var (minBonusEXP, maxBonusEXP) = GetStatRange( baseMinBonusEXP, baseMaxBonusEXP );
		var (minBonusVyndalium, maxBonusVyndalium) = GetStatRange( baseMinBonusVyndalium, baseMaxBonusVyndalium );
		var (minArmorPenetration, maxArmorPenetration) = GetStatRange( baseMinArmorPenetration, baseMaxArmorPenetration );
		var (minMaxHealthDMG, maxMaxHealthDMG) = GetStatRange( baseMinMaxHealthDMG, baseMaxMaxHealthDMG );
		var (minAttackRange, maxAttackRange) = GetStatRange( baseMinAttackRange, baseMaxAttackRange );
		var (minMagicPenetration, maxMagicPenetration) = GetStatRange( baseMinMagicPenetration, baseMaxMagicPenetration );
		var (minFireElementalDamage, maxFireElementalDamage) = GetStatRange( baseMinFireElementalDamage, baseMaxFireElementalDamage );
		var (minIceElementalDamage, maxIceElementalDamage) = GetStatRange( baseMinIceElementalDamage, baseMaxIceElementalDamage );
		var (minLightningElementalDamage, maxLightningElementalDamage) = GetStatRange( baseMinLightningElementalDamage, baseMaxLightningElementalDamage );
		var (minHolyElementalDamage, maxHolyElementalDamage) = GetStatRange( baseMinHolyElementalDamage, baseMaxHolyElementalDamage );
		var (minLightElementalDamage, maxLightElementalDamage) = GetStatRange( baseMinLightElementalDamage, baseMaxLightElementalDamage );
		var (minShadowElementalDamage, maxShadowElementalDamage) = GetStatRange( baseMinShadowElementalDamage, baseMaxShadowElementalDamage );
		var (minPoisonElementalDamage, maxPoisonElementalDamage) = GetStatRange( baseMinPoisonElementalDamage, baseMaxPoisonElementalDamage );
		var (minBleedElementalDamage, maxBleedElementalDamage) = GetStatRange( baseMinBleedElementalDamage, baseMaxBleedElementalDamage );
		var (minFreezeElementalDamage, maxFreezeElementalDamage) = GetStatRange( baseMinFreezeElementalDamage, baseMaxFreezeElementalDamage );
		var (minWaterElementalDamage, maxWaterElementalDamage) = GetStatRange( baseMinWaterElementalDamage, baseMaxWaterElementalDamage );
		var (minEarthElementalDamage, maxEarthElementalDamage) = GetStatRange( baseMinEarthElementalDamage, baseMaxEarthElementalDamage );
		var (minWindElementalDamage, maxWindElementalDamage) = GetStatRange( baseMinWindElementalDamage, baseMaxWindElementalDamage );
		var (minFireRate, maxFireRate) = GetFireRateRange( Tier );
		var (minBulletSpeed, maxBulletSpeed) = GetBulletSpeedRange( Tier );



		// Bestimmen Sie die maximale Anzahl von Statistiken basierend auf dem Tier
		double[] probabilities = { 0.6, 0.2, 0.05, 0.025, 0.0125, 0.01, 0.075, 0.05 };

		// Bestimmen Sie die maximale Anzahl von Statistiken basierend auf dem Tier
		int maxStats = 0;
		switch ( Tier )
		{
			case Tier.C:
				maxStats = Math.Min( GetRandomStatCount( probabilities, random ), 1 ); // Max 1
				break;
			case Tier.B:
				maxStats = Math.Min( GetRandomStatCount( probabilities, random ), 2 ); // Max 2
				break;
			case Tier.A:
				maxStats = Math.Min( GetRandomStatCount( probabilities, random ), 5 ); // Max 5
				break;
			case Tier.S:
				maxStats = Math.Min( GetRandomStatCount( probabilities, random ), 6 ); // Max 6
				break;
			case Tier.SS:
				maxStats = Math.Min( GetRandomStatCount( probabilities, random ), 7 ); // Max 7
				break;
			case Tier.SSS:
				maxStats = Math.Min( GetRandomStatCount( probabilities, random ), 8 ); // Max 8
				break;
			case Tier.Ultimate:
				maxStats = Math.Min( GetRandomStatCount( probabilities, random ), 9 ); // Max 9
				break;
		}

		// Generieren Sie zufällige Werte innerhalb der definierten Bereiche
		List<Action> statsGenerators = new List<Action>
	{
		() => STG = random.Next(minSTG, maxSTG + 1),
		() => INT = random.Next(minInt, maxInt + 1),
		() => Mana = random.Next(minMana, maxMana + 1),

		() => CritHitDamage = random.Next(minCritHitDamage, maxCritHitDamage + 1),
		() => CritHitChance = random.Next(minCritHitChance, maxCritHitChance + 1),

		() => AttackPower = random.Next(minAttackPower, maxAttackPower + 1),
		() => ArmorPenetration = random.Next(minArmorPenetration, maxArmorPenetration + 1),
		() => MaxHealthDMG = random.Next(minMaxHealthDMG, maxMaxHealthDMG + 1),
		() => AttackRange = random.Next(minAttackRange, maxAttackRange + 1),
		
		() => MagicPower = random.Next(minMagicPower, maxMagicPower + 1),
		() => MagicPenetration = random.Next(minMagicPenetration, maxMagicPenetration + 1),
		() => FireElementalDamage = random.Next(minFireElementalDamage, maxFireElementalDamage + 1),
		() => IceElementalDamage = random.Next(minIceElementalDamage, maxIceElementalDamage + 1),
		() => LightningElementalDamage = random.Next(minLightningElementalDamage, maxLightningElementalDamage + 1),
		() => HolyElementalDamage = random.Next(minHolyElementalDamage, maxHolyElementalDamage + 1),
		() => LightElementalDamage = random.Next(minLightElementalDamage, maxLightElementalDamage + 1),
		

		
		() => ShadowElementalDamage = random.Next(minShadowElementalDamage, maxShadowElementalDamage + 1),
		() => PoisonElementalDamage = random.Next(minPoisonElementalDamage, maxPoisonElementalDamage + 1),
		() => BleedElementalDamage = random.Next(minBleedElementalDamage, maxBleedElementalDamage + 1),
		() => FreezeElementalDamage = random.Next(minFreezeElementalDamage, maxFreezeElementalDamage + 1),
		() => WaterElementalDamage = random.Next(minWaterElementalDamage, maxWaterElementalDamage + 1),
		() => EarthElementalDamage = random.Next(minEarthElementalDamage, maxEarthElementalDamage + 1),
		() => WindElementalDamage = random.Next(minWindElementalDamage, maxWindElementalDamage + 1),
		() => AttackSpeed = random.Next(minAttackSpeed, maxAttackSpeed + 1),

	


		() => MoveSpeed = random.Next(minMoveSpeed, maxMoveSpeed + 1),
		() => BonusScore = random.Next(minBonusScore, maxBonusScore + 1),
		() => BonusEXP = random.Next(minBonusEXP, maxBonusEXP + 1),
		() => BonusVyndalium = random.Next(minBonusVyndalium, maxBonusVyndalium + 1),
		() => FireRate = random.Next(minFireRate, maxFireRate + 1),
		() => BulletSpeed = random.Next((int)minBulletSpeed, (int)maxBulletSpeed + 1),
		() => ItemLevel = GenerateRandomItemLevel(random),
	};
		
		// Wählen Sie zufällig eine bestimmte Anzahl von Statistiken aus
		statsGenerators.OrderBy( x => random.Next() ).Take( maxStats ).ToList().ForEach( action => action() );

		FireRate = random.Next( minFireRate, maxFireRate + 1 );
		BulletSpeed = random.Next( (int)minBulletSpeed, (int)maxBulletSpeed + 1 );
	}
	private (int min, int max) GetFireRateRange( Tier tier )
	{
		return tier switch
		{
			Tier.C => (2, 6),
			Tier.B => (6, 9),
			Tier.A => (7, 13),
			Tier.S => (8, 15),
			Tier.SS => (9, 17),
			Tier.SSS => (10, 22),
			Tier.Ultimate => (8, 30),
			_ => (0, 30)
		};
	}

	private (float min, float max) GetBulletSpeedRange( Tier tier )
	{
		return tier switch
		{
			Tier.C => (0.2f, 1),
			Tier.B => (0.20f, 1.5f),
			Tier.A => (0.25f, 2.0f),
			Tier.S => (0.3f, 2.5f),
			Tier.SS => (0.4f, 3.0f),
			Tier.SSS => (0.5f, 3.5f),
			Tier.Ultimate => (3.5f, 10.0f),
			_ => (0, 30)
		};
	}
	private int GetRandomStatCount( double[] probabilities, Random random )
	{
		double cumulative = 0.0;
		double roll = random.NextDouble();

		for ( int i = 0; i < probabilities.Length; i++ )
		{
			cumulative += probabilities[i];
			if ( roll < cumulative )
			{
				return i + 1;
			}
		}

		return probabilities.Length; // Falls keine Übereinstimmung gefunden wird, geben Sie die maximale Anzahl zurück
	}
	private void GenerateArmorStats()
	{
		Random random = new Random();

		// Definieren Sie die Basiswerte für Rüstungsstatistiken
		int baseMinPER = 1, baseMaxPER = 15;
		int baseMinDex = 1, baseMaxDex = 15;
		int baseMinTenacity = 1, baseMaxTenacity = 100;
		int baseMinMoveSpeed = 1, baseMaxMoveSpeed = 1000;
		int baseMinArmor = 1, baseMaxArmor = 500;
		int baseMinMagicDefense = 1, baseMaxMagicDefense = 500;
		int baseMinEvasion = 1, baseMaxEvasion = 100;
		int baseMinCover = 1, baseMaxCover = 100;
		int baseMinAbilityHaste = 1, baseMaxAbilityHaste = 100;
		int baseMinMana = 1, baseMaxMana = 250;
		int baseMinHealth = 1, baseMaxHealth = 250;
		int baseMinHealthRegen = 1, baseMaxHealthRegen = 100;
		int baseMinManaRegen = 1, baseMaxManaRegen = 100;

		int baseMinStunResistance = 1, baseMaxStunResistance = 10;
		int baseMinBlindResistance = 1, baseMaxBlindResistance = 10;
		int baseMinBleedResistance = 1, baseMaxBleedResistance = 10;
		int baseMinFreezeResistance = 1, baseMaxFreezeResistance = 10;
		int baseMinSlowResistence = 1, baseMaxSlowResistence = 10;
		int baseMinFireResistence = 1, baseMaxFireResistence = 10;
		int baseMinPoisonResistence = 1, baseMaxPoisonResistence = 10;
		int baseMinIceResistence = 1, baseMaxIceResistence = 10;
		int baseMinLightningResistence = 1, baseMaxLightningResistence = 10;
		int baseMinLightResistence = 1, baseMaxLightResistence = 10;
		int baseMinShadowResistence = 1, baseMaxShadowResistence = 10;
		int baseMinHolyResistence = 1, baseMaxHolyResistence = 10;
	



		// Bestimmen Sie die maximalen Werte basierend auf dem Tier
		var (minPER, maxPER) = GetStatRange( baseMinPER, baseMaxPER );
		var (minHealthRegen, maxHealthRegen) = GetStatRange( baseMinHealthRegen, baseMaxHealthRegen );
		var (minDex, maxDex) = GetStatRange( baseMinDex, baseMaxDex );
		var (minTenacity, maxTenacity) = GetStatRange( baseMinTenacity, baseMaxTenacity );
		var (minMoveSpeed, maxMoveSpeed) = GetStatRange( baseMinMoveSpeed, baseMaxMoveSpeed );
		var (minArmor, maxArmor) = GetStatRange( baseMinArmor, baseMaxArmor );
		var (minMagicDefense, maxMagicDefense) = GetStatRange( baseMinMagicDefense, baseMaxMagicDefense );
		var (minEvasion, maxEvasion) = GetStatRange( baseMinEvasion, baseMaxEvasion );
		var (minCover, maxCover) = GetStatRange( baseMinCover, baseMaxCover );
		var (minAbilityHaste, maxAbilityHaste) = GetStatRange( baseMinAbilityHaste, baseMaxAbilityHaste );
		var (minMana, maxMana) = GetStatRange( baseMinMana, baseMaxMana );
		var (minHealth, maxHealth) = GetStatRange( baseMinHealth, baseMaxHealth );

		var (minStunResistance, maxStunResistance) = GetStatRange( baseMinStunResistance, baseMaxStunResistance );
		var (minBlindResistance, maxBlindResistance) = GetStatRange( baseMinBlindResistance, baseMaxBlindResistance );
		var (minBleedResistance, maxBleedResistance) = GetStatRange( baseMinBleedResistance, baseMaxBleedResistance );
		var (minSlowResistence, maxSlowResistence) = GetStatRange( baseMinSlowResistence, baseMaxSlowResistence );
		var (minShadowResist, maxShadowResist) = GetStatRange( baseMinShadowResistence, baseMaxShadowResistence );
		var (minFireResistence, maxFireResistence) = GetStatRange( baseMinFireResistence, baseMaxFireResistence );
		var (minPoisonResistence, maxPoisonResistence) = GetStatRange( baseMinPoisonResistence, baseMaxPoisonResistence );
		var (minIceResistence, maxIceResistence) = GetStatRange( baseMinIceResistence, baseMaxIceResistence );
		var (minLightningResistence, maxLightningResistence) = GetStatRange( baseMinLightningResistence, baseMaxLightningResistence );
		var (minHolyResistence, maxHolyResistence) = GetStatRange( baseMinHolyResistence, baseMaxHolyResistence );
		var (minLightResist, maxLightResist) = GetStatRange( baseMinLightResistence, baseMaxLightResistence );

		var (minManaRegen, maxManaRegen) = GetStatRange( baseMinManaRegen, baseMaxManaRegen );


		// Bestimmen Sie die maximale Anzahl von Statistiken basierend auf dem Tier
		// Bestimmen Sie die maximale Anzahl von Statistiken basierend auf dem Tier
		double[] probabilities = { 0.7, 0.1, 0.05, 0.025, 0.0125, 0.01, 0.0075, 0.005 };

		// Bestimmen Sie die maximale Anzahl von Statistiken basierend auf dem Tier
		int maxStats = 0;
		switch ( Tier )
		{
			case Tier.C:
				maxStats = Math.Min( GetRandomStatCount( probabilities, random ), 1 ); // Max 1
				break;
			case Tier.B:
				maxStats = Math.Min( GetRandomStatCount( probabilities, random ), 2 ); // Max 2
				break;
			case Tier.A:
				maxStats = Math.Min( GetRandomStatCount( probabilities, random ), 5 ); // Max 5
				break;
			case Tier.S:
				maxStats = Math.Min( GetRandomStatCount( probabilities, random ), 6 ); // Max 6
				break;
			case Tier.SS:
				maxStats = Math.Min( GetRandomStatCount( probabilities, random ), 7 ); // Max 7
				break;
			case Tier.SSS:
				maxStats = Math.Min( GetRandomStatCount( probabilities, random ), 8 ); // Max 8
				break;
			case Tier.Ultimate:
				maxStats = Math.Min( GetRandomStatCount( probabilities, random ), 9 ); // Max 9
				break;
		}

		// Generieren Sie zufällige Werte innerhalb der definierten Bereiche
		List<Action> statsGenerators = new List<Action>
	{
		() => PER = random.Next(minPER, maxPER + 1),
		() => DEX = random.Next(minDex, maxDex + 1),
		() => Tenacity = random.Next(minTenacity, maxTenacity + 1),
		() => MoveSpeed = random.Next(minMoveSpeed, maxMoveSpeed + 1),
		() => Armor = random.Next(minArmor, maxArmor + 1),
		() => MagicDefense = random.Next(minMagicDefense, maxMagicDefense + 1),
		() => Evasion = random.Next(minEvasion, maxEvasion + 1),
		() => Cover = random.Next(minCover, maxCover + 1),
		() => AbilityHaste = random.Next(minAbilityHaste, maxAbilityHaste + 1),
		() => Mana = random.Next(minMana, maxMana + 1),
		() => Health = random.Next(minHealth, maxHealth + 1),
		() => HealthRegen = random.Next(minHealthRegen, maxHealthRegen + 1),
		() => ManaRegen = random.Next(minManaRegen, maxManaRegen + 1),

		() => FreezeResist = random.Next(baseMinFreezeResistance, baseMaxFreezeResistance + 1),

		() => SlowResistence = random.Next(minSlowResistence, maxSlowResistence + 1),
		() => FireResistence = random.Next(minFireResistence, maxFireResistence + 1),
		() => BleedResistance = random.Next(minBleedResistance, maxBleedResistance + 1),

		() => PoisonResistence = random.Next(minPoisonResistence, maxPoisonResistence + 1),
		() => IceResistence = random.Next(minIceResistence, maxIceResistence + 1),
		() => LightningResistence = random.Next(minLightningResistence, maxLightningResistence + 1),
		() => HolyResistence = random.Next(minHolyResistence, maxHolyResistence + 1),
		() => ShadowResist = random.Next(minShadowResist, maxShadowResist + 1),
		() => LightResist = random.Next(minLightResist, maxLightResist + 1),

		() => ItemLevel = GenerateRandomItemLevel(random),
	};

		// Wählen Sie zufällig eine bestimmte Anzahl von Statistiken aus
		statsGenerators.OrderBy( x => random.Next() ).Take( maxStats ).ToList().ForEach( action => action() );
	}
	private void GenerateAccessoryStats()
	{
		Random random = new Random();

		// Definieren Sie die Basiswerte für Accessoirestatistiken
		int baseMinBonusEXP = 10, baseMaxBonusEXP = 100;
		int baseMinBonusScore = 10, baseMaxBonusScore = 100;
		int baseMinBonusVyndalium = 10, baseMaxBonusVyndalium = 100;
		int baseMinTenacity = 1, baseMaxTenacity = 10;
		int baseMinStunResistance = 1, baseMaxStunResistance = 10;
		int baseMinBlindResistance = 1, baseMaxBlindResistance = 10;
		int baseMinBleedResistance = 1, baseMaxBleedResistance = 10;
		int baseMinFreezeResistance = 1, baseMaxFreezeResistance = 10;
		int baseMinSlowResistence = 1, baseMaxSlowResistence = 10;
		int baseMinFireResistence = 1, baseMaxFireResistence = 10;
		int baseMinPoisonResistence = 1, baseMaxPoisonResistence = 10;
		int baseMinIceResistence = 1, baseMaxIceResistence = 10;
		int baseMinLightningResistence = 1, baseMaxLightningResistence = 10;
		int baseMinLightResistence = 1, baseMaxLightResistence = 10;
		int baseMinShadowResistence = 1, baseMaxShadowResistence = 10;
		int baseMinHolyResistence = 1, baseMaxHolyResistence = 10;
		int basestaminapersecond = 1, basemaxStaminaSecond = 100;
		int baseminWalkSpeed = 1, basemaxWalkSpeed = 100;
		int baseminRunSpeed = 1, basemaxRunSpeed = 100;
		int baseMinStamina = 1, baseMaxStamina = 100;
		int baseMinAbilityHaste = 1, baseMaxAbilityHaste = 10;
		int baseMinCritHitChance = 1, baseMaxCritHitChance = 10;
		int baseMinCritHitDamage = 1, baseMaxCritHitDamage = 10;

		// Bestimmen Sie die maximalen Werte basierend auf dem Tier
		var (minBonusEXP, maxBonusEXP) = GetStatRange( baseMinBonusEXP, baseMaxBonusEXP );
		var (minBonusScore, maxBonusScore) = GetStatRange( baseMinBonusScore, baseMaxBonusScore );
		var (minBonusVyndalium, maxBonusVyndalium) = GetStatRange( baseMinBonusVyndalium, baseMaxBonusVyndalium );
		var (minTenacity, maxTenacity) = GetStatRange( baseMinTenacity, baseMaxTenacity );
		var (minStunResistance, maxStunResistance) = GetStatRange( baseMinStunResistance, baseMaxStunResistance );
		var (minBlindResistance, maxBlindResistance) = GetStatRange( baseMinBlindResistance, baseMaxBlindResistance );
		var (minBleedResistance, maxBleedResistance) = GetStatRange( baseMinBleedResistance, baseMaxBleedResistance );
		var (minSlowResistence, maxSlowResistence) = GetStatRange( baseMinSlowResistence, baseMaxSlowResistence );
		var (minShadowResist, maxShadowResist) = GetStatRange( baseMinShadowResistence, baseMaxShadowResistence );
		var (minFireResistence, maxFireResistence) = GetStatRange( baseMinFireResistence, baseMaxFireResistence );
		var (minPoisonResistence, maxPoisonResistence) = GetStatRange( baseMinPoisonResistence, baseMaxPoisonResistence );
		var (minIceResistence, maxIceResistence) = GetStatRange( baseMinIceResistence, baseMaxIceResistence );
		var (minLightningResistence, maxLightningResistence) = GetStatRange( baseMinLightningResistence, baseMaxLightningResistence );
		var (minHolyResistence, maxHolyResistence) = GetStatRange( baseMinHolyResistence, baseMaxHolyResistence );
		var (minLightResist, maxLightResist) = GetStatRange( baseMinLightResistence, baseMaxLightResistence );
		var (minStamina, maxStamina) = GetStatRange( baseMinStamina, baseMaxStamina );
		var (minStaminaSecond, maxStaminaSecond) = GetStatRange( basestaminapersecond, basemaxStaminaSecond );
		var (minWalkSpeed, maxWalkSpeed) = GetStatRange( baseminWalkSpeed, basemaxWalkSpeed );
		var (minRunSpeed, maxRunSpeed) = GetStatRange( baseminRunSpeed, basemaxRunSpeed );
		var (minAbilityHaste, maxAbilityHaste) = GetStatRange( baseMinAbilityHaste, baseMaxAbilityHaste );
		var (minCritHitChance, maxCritHitChance) = GetStatRange( baseMinCritHitChance, baseMaxCritHitChance );
		var (minCritHitDamage, maxCritHitDamage) = GetStatRange( baseMinCritHitDamage, baseMaxCritHitDamage );

		// Sicherstellen, dass minValue nicht größer als maxValue ist
		if ( minAbilityHaste > maxAbilityHaste ) (minAbilityHaste, maxAbilityHaste) = (maxAbilityHaste, minAbilityHaste);
		if ( minCritHitChance > maxCritHitChance ) (minCritHitChance, maxCritHitChance) = (maxCritHitChance, minCritHitChance);
		if ( minCritHitDamage > maxCritHitDamage ) (minCritHitDamage, maxCritHitDamage) = (maxCritHitDamage, minCritHitDamage);

		// Bestimmen Sie die maximale Anzahl von Statistiken basierend auf dem Tier
		double[] probabilities = { 0.7, 0.1, 0.05, 0.025, 0.0125, 0.01, 0.0075, 0.005 };
		int maxStats = 0;
		switch ( Tier )
		{
			case Tier.C:
				maxStats = Math.Min( GetRandomStatCount( probabilities, random ), 1 ); // Max 1
				break;
			case Tier.B:
				maxStats = Math.Min( GetRandomStatCount( probabilities, random ), 2 ); // Max 2
				break;
			case Tier.A:
				maxStats = Math.Min( GetRandomStatCount( probabilities, random ), 5 ); // Max 5
				break;
			case Tier.S:
				maxStats = Math.Min( GetRandomStatCount( probabilities, random ), 6 ); // Max 6
				break;
			case Tier.SS:
				maxStats = Math.Min( GetRandomStatCount( probabilities, random ), 7 ); // Max 7
				break;
			case Tier.SSS:
				maxStats = Math.Min( GetRandomStatCount( probabilities, random ), 8 ); // Max 8
				break;
			case Tier.Ultimate:
				maxStats = Math.Min( GetRandomStatCount( probabilities, random ), 9 ); // Max 9
				break;
		}

		// Generieren Sie zufällige Werte innerhalb der definierten Bereiche
		List<Action> statsGenerators = new List<Action>
	{
		() => BonusEXP = random.Next(minBonusEXP, maxBonusEXP + 1),
		() => StaminaSecond = random.Next(minStaminaSecond, maxStaminaSecond + 1),
		() => WalkSpeed = random.Next(minWalkSpeed, maxWalkSpeed + 1),
		() => RunSpeed = random.Next(minRunSpeed, maxRunSpeed + 1),
		() => Stamina = random.Next(minStamina, maxStamina + 1),
		() => AbilityHaste = random.Next(minAbilityHaste, maxAbilityHaste + 1),
		() => CritHitChance = random.Next(minCritHitChance, maxCritHitChance + 1),
		() => CritHitDamage = random.Next(minCritHitDamage, maxCritHitDamage + 1),
		() => BlindResistance = random.Next(minBlindResistance, maxBlindResistance + 1),
		() => FreezeResist = random.Next(baseMinFreezeResistance, baseMaxFreezeResistance + 1),
		() => SlowResistence = random.Next(minSlowResistence, maxSlowResistence + 1),
		() => FireResistence = random.Next(minFireResistence, maxFireResistence + 1),
		() => BleedResistance = random.Next(minBleedResistance, maxBleedResistance + 1),
		() => PoisonResistence = random.Next(minPoisonResistence, maxPoisonResistence + 1),
		() => IceResistence = random.Next(minIceResistence, maxIceResistence + 1),
		() => LightningResistence = random.Next(minLightningResistence, maxLightningResistence + 1),
		() => HolyResistence = random.Next(minHolyResistence, maxHolyResistence + 1),
		() => ShadowResist = random.Next(minShadowResist, maxShadowResist + 1),
		() => LightResist = random.Next(minLightResist, maxLightResist + 1),
		() => BonusScore = random.Next(minBonusScore, maxBonusScore + 1),
		() => BonusVyndalium = random.Next(minBonusVyndalium, maxBonusVyndalium + 1),
		() => Tenacity = random.Next(minTenacity, maxTenacity + 1),
		() => StunResistance = random.Next(minStunResistance, maxStunResistance + 1),
		
		() => ItemLevel = GenerateRandomItemLevel(random),
	};

		// Wählen Sie zufällig eine bestimmte Anzahl von Statistiken aus
		statsGenerators.OrderBy( x => random.Next() ).Take( maxStats ).ToList().ForEach( action => action() );
	}
	

	private int GenerateRandomItemLevel( Random random )
	{
		double roll = random.NextDouble() * 100;
		if ( roll < 70 ) // 70% Wahrscheinlichkeit
			return random.Next( 0, 6 ); // 0-5
		else if ( roll < 70 + 20 ) // 20% Wahrscheinlichkeit
			return random.Next( 5, 11 ); // 5-10
		else if ( roll < 70 + 20 + 5 ) // 5% Wahrscheinlichkeit
			return random.Next( 10, 16 ); // 10-15
		else if ( roll < 70 + 20 + 5 + 2.5 ) // 2.5% Wahrscheinlichkeit
			return random.Next( 15, 21 ); // 15-20
		else if ( roll < 70 + 20 + 5 + 2.5 + 1.25 ) // 1.25% Wahrscheinlichkeit
			return random.Next( 20, 25 ); // 20-24
		else if ( roll < 70 + 20 + 5 + 2.5 + 1.25 + 0.9 ) // 0.9% Wahrscheinlichkeit
			return 25; // 25
		else if ( roll < 70 + 20 + 5 + 2.5 + 1.25 + 0.9 + 0.4 ) // 0.4% Wahrscheinlichkeit
			return random.Next( 26, 28 ); // 26-27
		else // Falls keine der Bedingungen erfüllt ist
			return 0; // 0
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		Prefab = GameObject.PrefabInstanceSource;

		// Überprüfen Sie, ob ItemTier und Stats initialisiert sind
		if ( ItemTier == null )
		{
			ItemTier = new TierClass(); // oder eine geeignete Standardinitialisierung
		}
		var baseGun = GameObject.Components.Get<BaseGun>();
		if ( baseGun != null )
		{
			baseGun.InitializeFireRate( this );
		}



		//GenerateRandomStats();
	}
	private void UpdateState()
	{
		GameObject.Enabled = State != ItemState.Backpack;
		if ( this is ItemEquipment equipment )
			equipment.UpdateEquipped();
	}
	private bool isMovingItem = false;


	protected override void OnUpdate()
	{
		if (isMovingItem)
		{
			MoveItemToMousePosition();
		}
		
	}
	private PhysicsBody GrabbedBody;
	private GameObject GrabbedObject;

	private Vector3 GrabbedAimLocal;
	private Vector3 GrabbedObjectLocal;

	private PhysicsBody GrabBody;
	private Sandbox.Physics.FixedJoint GrabJoint;



	

	protected override void OnFixedUpdate()
	{
		if ( IsProxy )
			return;

		if ( !GrabbedBody.IsValid() )
			return;

		if ( !GrabBody.IsValid() )
			return;

		var aimTransform = Scene.Camera.WorldTransform;
		GrabBody.Position = aimTransform.PointToWorld( GrabbedAimLocal );
	}
	protected override void OnEnabled()
	{
		base.OnEnabled();

		Clear();

		GrabBody = new PhysicsBody( Scene.PhysicsWorld )
		{
			BodyType = PhysicsBodyType.Keyframed
		};
	}

	protected override void OnDisabled()
	{
		base.OnDisabled();

		Clear();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		Clear();
	}

	private void Clear()
	{
		GrabJoint?.Remove();
		GrabJoint = null;

		GrabBody?.Remove();
		GrabBody = null;

		GrabbedBody = null;
		GrabbedObject = null;
		GrabbedAimLocal = default;
	}

	protected override void OnPreRender()
	{
		base.OnPreRender();

		if ( Scene.Camera == null )
			return;

		if ( !GrabbedObject.IsValid() )
		{
			var tr = Scene.Trace.Ray( Scene.Camera.ScreenNormalToRay( 0.5f ), 1000.0f )
						.IgnoreGameObjectHierarchy( GameObject.Root )
						.Run();

			if ( tr.Hit )
			{
				// Hier können Sie zusätzliche Logik hinzufügen, falls erforderlich
			}
		}
		else
		{
			if ( GrabbedObject != null )
			{
				var position = GrabbedObject.WorldTransform.PointToWorld( GrabbedObjectLocal );
				// Hier können Sie zusätzliche Logik hinzufügen, falls erforderlich
			}
		}
	}
	private void MoveItemToMousePosition()
	{
		if ( IsProxy )
			return;

		if ( GrabbedBody != null && GrabbedBody.IsValid() )
		{
			if ( !Input.Down( "reload" ) )
			{
				GrabJoint?.Remove();
				GrabJoint = null;

				GrabbedBody = null;
				GrabbedObject = null;
				GrabbedAimLocal = default;
				isMovingItem = false; // Beenden des Bewegens des Items
			}
			else
			{
				return;
			}
		}

		var tr = Scene.Trace.Ray( Scene.Camera.WorldPosition, Scene.Camera.WorldPosition + Scene.Camera.WorldRotation.Forward * 1000 )
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.Run();

		if ( !tr.Hit || tr.Body is null )
			return;

		if ( tr.Body.BodyType == PhysicsBodyType.Static )
			return;

		if ( Input.Down( "reload" ) )
		{
			var aimTransform = Scene.Camera.WorldTransform;

			GrabbedBody = tr.Body;
			GrabbedObject = tr.GameObject;
			GrabbedObjectLocal = GrabbedObject.WorldTransform.PointToLocal( tr.HitPosition );

			var localOffset = GrabbedBody.Transform.PointToLocal( tr.HitPosition );

			GrabbedAimLocal = aimTransform.PointToLocal( tr.HitPosition );

			// Initialisiere GrabBody, falls es null ist
			if ( GrabBody == null )
			{
				GrabBody = new PhysicsBody( Scene.PhysicsWorld )
				{
					BodyType = PhysicsBodyType.Keyframed
				};
			}

			GrabBody.Position = tr.HitPosition;

			GrabJoint?.Remove();
			GrabJoint = PhysicsJoint.CreateFixed( new PhysicsPoint( GrabBody ), new PhysicsPoint( GrabbedBody ) );
			GrabJoint.Point1 = new PhysicsPoint( GrabBody );
			GrabJoint.Point2 = new PhysicsPoint( GrabbedBody, localOffset );

			var maxForce = 100.0f * tr.Body.Mass * Scene.PhysicsWorld.Gravity.Length;
			GrabJoint.SpringLinear = new PhysicsSpring( 15, 1, maxForce );
			GrabJoint.SpringAngular = new PhysicsSpring( 0, 0, 0 );
		}
	}

	protected override void OnStart()
	{
		GameObject.SetupNetworking();
		var interactions = Components.GetOrCreate<Interactions>();
		interactions.AddInteraction( new Interaction()
		{
			Identifier = $"item.move.{Name}",
			Action = ( Player interactor, GameObject obj ) =>
			{
				isMovingItem = !isMovingItem;
			},
			Keybind = "reload",
			Description = "Move",
			Disabled = () => !CanMoveItem(),
			ShowWhenDisabled = () => true,
			Accessibility = AccessibleFrom.World,
		} );
		if ( IsItem != true )
		{
			interactions.AddInteraction( new Interaction()
			{
				Identifier = "item.pickup",
				Action = ( Player interactor, GameObject obj ) => interactor.Inventory.GiveItem( this ),
				Keybind = "use",
				Description = "Take",
				Stats = "Take",
				Disabled = () => !Player.Local.Inventory.HasSpaceInBackpack(),
				ShowWhenDisabled = () => true,
				Accessibility = AccessibleFrom.All,

			} );
			
		}
		else
		{
			// Pickup
			
			interactions.AddInteraction( new Interaction()
			{
				Identifier = "item.pickup",
				Action = ( Player interactor, GameObject obj ) => interactor.Inventory.GiveItem( this ),
				Keybind = "use",
				Description = "Take",
				Stats = "Take",
				Disabled = () => !Player.Local.Inventory.HasSpaceInBackpack(),
				ShowWhenDisabled = () => true,
				Accessibility = AccessibleFrom.All,
				Sound = () => _pickupSound,
			} );
		}
		
	}
	private bool CanMoveItem()
	{
		return !isMovingItem;
	}


	
}
