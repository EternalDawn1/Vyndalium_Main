

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
}






public class ItemComponent : Component
{

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
	private int _dmg;
	[Property, Group( "Weapon" ), Range( 0, 100 )] public int STG { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 100 )] public int HE { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 100 )] public int DEX { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 100 )] public int PER { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 100 )] public float INT { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 100 )] public float Mana { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 100 )] public float Health { get; set; }
	[Property, Range( 0, 27 )] public int ItemLevel { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 175 )] public float CritHitDamage { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 150 )] public float CritHitChance { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 46 )] public float AbilityHaste { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float AttackPower { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float MagicPower { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float MinAttackValue { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float MaxAttackValue { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float MinArmorValue { get; set; }
	[Property, Group( "Weapon" ), Range( 0, 1000 )] public float MaxArmorValue { get; set; }
	[Property] public Tier Tier { get; set; }
	[Property, Range( 100, 0 )] public int DamageBalance { get; set; }
	[Property, Range( 1000, 0 )] public int Durability { get; set; }
	
	
	[Property, Group( "Weapon" ), Range( 0, 60 )] public float AttackSpeed { get; set; }
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
	/// The weight (in grams) of the item.
	/// </summary>
	[Property, Sync] public int WeightInGrams { get; set; }

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

		SellPrice = basePrice + (numberOfStats * additionalPricePerStat) + levelPrice + additionalUpgradeCost;
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
		return (int)(baseCost * tierMultiplier);
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
		return count;
	}
	public void GenerateRandomStats()
	{
		if ( IsWeapon )
		{
			GenerateWeaponStats();
		}
		else if ( IsArmor )
		{
			GenerateArmorStats();
		}
		else if ( IsAccessory )
		{
			GenerateAccessoryStats();
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
				return (0, baseMax / 5);
			case Tier.B:
				return (baseMax / 5, baseMax * 2 / 5);
			case Tier.A:
				return (baseMax * 2 / 5, baseMax * 3 / 5);
			case Tier.S:
				return (baseMax * 3 / 5, baseMax * 4 / 5);
			case Tier.SS:
				return (baseMax * 4 / 5, baseMax);
			case Tier.SSS:
				return (baseMax, baseMax * 2);
			case Tier.Ultimate:
				return (baseMax * 2, baseMax * 3);
			default:
				return (baseMin, baseMax);
		}
	}
	

	

	private void GenerateWeaponStats()
	{
		Random random = new Random();

		// Definieren Sie die Basiswerte für Waffenstatistiken
		int baseMinSTG = 5, baseMaxSTG = 15;
		int baseMinInt = 5, baseMaxInt = 15;
		int baseMinMana = 10, baseMaxMana = 100;
		int baseMinCritHitDamage = 1, baseMaxCritHitDamage = 15;
		int baseMinCritHitChance = 1, baseMaxCritHitChance = 15;
		int baseMinAttackPower = 10, baseMaxAttackPower = 100;
		int baseMinMagicPower = 10, baseMaxMagicPower = 100;
		int baseMinAttackSpeed = 1, baseMaxAttackSpeed = 14;
		int baseMinMoveSpeed = 1, baseMaxMoveSpeed = 100;
		int baseMinBonusScore = 10, baseMaxBonusScore = 100;
		int baseMinBonusEXP = 10, baseMaxBonusEXP = 100;
		int baseMinBonusVyndalium = 10, baseMaxBonusVyndalium = 100;

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
		() => STG = random.Next(minSTG, maxSTG + 1),
		() => INT = random.Next(minInt, maxInt + 1),
		() => Mana = random.Next(minMana, maxMana + 1),
		() => CritHitDamage = random.Next(minCritHitDamage, maxCritHitDamage + 1),
		() => CritHitChance = random.Next(minCritHitChance, maxCritHitChance + 1),
		() => AttackPower = random.Next(minAttackPower, maxAttackPower + 1),
		() => MagicPower = random.Next(minMagicPower, maxMagicPower + 1),
		() => AttackSpeed = random.Next(minAttackSpeed, maxAttackSpeed + 1),
		() => MoveSpeed = random.Next(minMoveSpeed, maxMoveSpeed + 1),
		() => BonusScore = random.Next(minBonusScore, maxBonusScore + 1),
		() => BonusEXP = random.Next(minBonusEXP, maxBonusEXP + 1),
		() => BonusVyndalium = random.Next(minBonusVyndalium, maxBonusVyndalium + 1),
		() => ItemLevel = GenerateRandomItemLevel(random),
	};

		// Wählen Sie zufällig eine bestimmte Anzahl von Statistiken aus
		statsGenerators.OrderBy( x => random.Next() ).Take( maxStats ).ToList().ForEach( action => action() );
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
		int baseMinPER = 5, baseMaxPER = 15;
		int baseMinDex = 5, baseMaxDex = 15;
		int baseMinTenacity = 1, baseMaxTenacity = 10;
		int baseMinMoveSpeed = 1, baseMaxMoveSpeed = 100;
		int baseMinArmor = 5, baseMaxArmor = 50;
		int baseMinMagicDefense = 5, baseMaxMagicDefense = 50;
		int baseMinEvasion = 1, baseMaxEvasion = 10;
		int baseMinCover = 1, baseMaxCover = 10;
		int baseMinAbilityHaste = 1, baseMaxAbilityHaste = 10;
		int baseMinMana = 10, baseMaxMana = 250;
		int baseMinHealth = 10, baseMaxHealth = 250;

		// Bestimmen Sie die maximalen Werte basierend auf dem Tier
		var (minPER, maxPER) = GetStatRange( baseMinPER, baseMaxPER );
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

		// Bestimmen Sie die maximalen Werte basierend auf dem Tier
		var (minBonusEXP, maxBonusEXP) = GetStatRange( baseMinBonusEXP, baseMaxBonusEXP );
		var (minBonusScore, maxBonusScore) = GetStatRange( baseMinBonusScore, baseMaxBonusScore );
		var (minBonusVyndalium, maxBonusVyndalium) = GetStatRange( baseMinBonusVyndalium, baseMaxBonusVyndalium );
		var (minTenacity, maxTenacity) = GetStatRange( baseMinTenacity, baseMaxTenacity );
		var (minStunResistance, maxStunResistance) = GetStatRange( baseMinStunResistance, baseMaxStunResistance );

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

		

		//GenerateRandomStats();
	}
	private void UpdateState()
	{
		GameObject.Enabled = State != ItemState.Backpack;
		if ( this is ItemEquipment equipment )
			equipment.UpdateEquipped();
	}

	protected override void OnStart()
	{
		GameObject.SetupNetworking();
		var interactions = Components.GetOrCreate<Interactions>();
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

	
}
