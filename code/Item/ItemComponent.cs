

namespace GeneralGame;

public enum ItemState
{
	None,
	Backpack,
	Equipped,
	Chest
}
public enum Tier 
{
	C,
	B,
	A,
	S,
	SS,
	SSS,
}
public class SerializedItemComponent
{
	public int Id { get; set; }
	public string Name { get; set; }
	public float DMG { get; set; }
	public float HE { get; set; }
	public float Armor { get; set; }
	public float STG { get; set; }
	public float DEX { get; set; }
	public float PER { get; set; }
	public float INT { get; set; }
	public float Mana { get; set; }
	public float Health { get; set; }
	public float CritHitDamage { get; set; }
	public float CritHitChance { get; set; }
	public float AbilityHaste { get; set; }
	public float AttackPower { get; set; }
	public float MagicPower { get; set; }
	public float AttackSpeed { get; set; }
	public float MoveSpeed { get; set; }
	public float MagicDefense { get; set; }
	public float Evasion { get; set; }
	public float Block { get; set; }
	public float BonusEXP { get; set; }
	public float BonusScore { get; set; }
	public float BonusVyndalium { get; set; }
	public float Tenacity { get; set; }
	public float StunResistance { get; set; }
	public float BlindResistance { get; set; }
	public float BleedResistance { get; set; }
	public float SlowResistence { get; set; }
	public float FireResistence { get; set; }
	public float PoisonResistence { get; set; }
	public float IceResistence { get; set; }
	public float LightningResistence { get; set; }
	public float HolyResistence { get; set; }
}



public class ItemComponent : Component
{
	
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
	[Property, Group( "Weapon" ), Range( 200, 1800 )] public int DMG { get; set; }
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
	public static SerializedItemComponent Serialize( ItemComponent item )
	{
		return new SerializedItemComponent
		{
			Id = item.Id.GetHashCode(),
			Name = item.Name,
			DMG = item.DMG,
			HE = item.HE,
			Armor = item.Armor,
			STG = item.STG,
			DEX = item.DEX,
			PER = item.PER,
			INT = item.INT,
			Mana = item.Mana,
			Health = item.Health,
			CritHitDamage = item.CritHitDamage,
			CritHitChance = item.CritHitChance,
			AbilityHaste = item.AbilityHaste,
			AttackPower = item.AttackPower,
			MagicPower = item.MagicPower,
			AttackSpeed = item.AttackSpeed,
			MoveSpeed = item.MoveSpeed,
			MagicDefense = item.MagicDefense,
			Evasion = item.Evasion,
			Block = item.Cover,
			BonusEXP = item.BonusEXP,
			BonusScore = item.BonusScore,
			BonusVyndalium = item.BonusVyndalium,
			Tenacity = item.Tenacity,
			StunResistance = item.StunResistance,
			BlindResistance = item.BlindResistance,
			BleedResistance = item.BleedResistance,
			SlowResistence = item.SlowResistence,
			FireResistence = item.FireResistence,
			PoisonResistence = item.PoisonResistence,
			IceResistence = item.IceResistence,
			LightningResistence = item.LightningResistence,
			HolyResistence = item.HolyResistence
		};
	}
	

	public int Price { get; set; }


	public bool IsEquipped { get; set; }

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
	public bool InInventory
	{
		get => State != ItemState.None;
	}

	/// <summary>
	/// Whether the item can be sold.
	/// </summary>
	public bool IsSellable => SellPrice != -1;

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
			
		}
	}
	public ItemComponent Split( int amount )
	{
		if ( amount <= 0 || amount >= Count )
			return null;

		Count -= amount;
		var newItem = (ItemComponent)GameObject.Clone();
		newItem.Count = amount;
		return newItem;
	}
	public TierClass ItemTier { get; set; }
	
	[Property]public List<int> Stats { get;  set; } = new List<int>();
	public int CalculateSellPrice()
	{
		int basePrice = 0;
		int additionalPricePerStat = 400;
		int numberOfStats = GetNumberOfStats();

		switch ( Tier )
		{
			case Tier.C:
				basePrice = 0;
				break;
			case Tier.B:
				basePrice = 200;
				break;
			case Tier.A:
				basePrice = 400;
				break;
			case Tier.S:
				basePrice = 600;
				break;
			case Tier.SS:
				basePrice = 800;
				break;
			case Tier.SSS:
				basePrice = 1000;
				break;
		}
		int levelPrice = CalculateLevelPrice( ItemLevel );
		SellPrice = basePrice + (numberOfStats * additionalPricePerStat) + levelPrice;
		return SellPrice;
	}
	private int CalculateLevelPrice( int level )
	{
		if ( level == 0 ) return 0;
		int price = 300; // Preis für Level 1
		for ( int i = 2; i <= level; i++ )
		{
			price += 250 + (i - 1) * 50;
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
		Random random = new Random();

		// Definieren Sie die Bereiche für jede Statistik
		int minDMG = 10, maxDMG = 100;
		int minSTG = 5, maxSTG = 50;
		int minHE = 1, maxHE = 10;
		int minDEX = 2, maxDEX = 20;
		int minPER = 3, maxPER = 30;
		int minINT = 4, maxINT = 40;
		int minMana = 10, maxMana = 100;
		int minHealth = 50, maxHealth = 100;
		int minCritHitDamage = 1, maxCritHitDamage = 10;
		int minCritHitChance = 1, maxCritHitChance = 10;
		int minAbilityHaste = 1, maxAbilityHaste = 10;
		int minAttackPower = 10, maxAttackPower = 100;
		int minMagicPower = 10, maxMagicPower = 100;
		int minAttackSpeed = 1, maxAttackSpeed = 10;
		int minMoveSpeed = 1, maxMoveSpeed = 10;
		int minArmor = 5, maxArmor = 50;

		// Bestimmen Sie die maximale Anzahl der Statistiken basierend auf dem Tier
		int maxStats = 0;
		switch ( Tier )
		{
			case Tier.C:
				maxStats = 1;
				break;
			case Tier.B:
				maxStats = 2;
				break;
			case Tier.A:
				maxStats = 5;
				break;
			case Tier.S:
				maxStats = 6;
				break;
			case Tier.SSS:
				maxStats = 7;
				break;
		}

		// Generieren Sie zufällige Werte innerhalb der definierten Bereiche
		List<Action> statsGenerators = new List<Action>
		{
			() => DMG = random.Next(minDMG, maxDMG + 1),
			() => STG = random.Next(minSTG, maxSTG + 1),
			() => HE = random.Next(minHE, maxHE + 1),
			() => DEX = random.Next(minDEX, maxDEX + 1),
			() => PER = random.Next(minPER, maxPER + 1),
			() => INT = random.Next(minINT, maxINT + 1),
			() => Mana = random.Next(minMana, maxMana + 1),
			() => Health = random.Next(minHealth, maxHealth + 1),
			() => CritHitDamage = random.Next(minCritHitDamage, maxCritHitDamage + 1),
			() => CritHitChance = random.Next(minCritHitChance, maxCritHitChance + 1),
			() => AbilityHaste = random.Next(minAbilityHaste, maxAbilityHaste + 1),
			() => AttackPower = random.Next(minAttackPower, maxAttackPower + 1),
			() => MagicPower = random.Next(minMagicPower, maxMagicPower + 1),
			() => AttackSpeed = random.Next(minAttackSpeed, maxAttackSpeed + 1),
			() => MoveSpeed = random.Next(minMoveSpeed, maxMoveSpeed + 1),
			() => Armor = random.Next(minArmor, maxArmor + 1),
			() => ItemLevel = GenerateRandomItemLevel(random)
		};

		// Mischen Sie die Statistiken und wählen Sie die maximale Anzahl aus
		statsGenerators = statsGenerators.OrderBy( x => random.Next() ).ToList();
		for ( int i = 0; i < maxStats; i++ )
		{
			statsGenerators[i]();
		}

		// Weitere zufällige Statistiken können hier hinzugefügt werden...
	}
	
	private int GenerateRandomItemLevel( Random random )
	{
		double roll = random.NextDouble() * 100;
		if ( roll < 90 )
			return random.Next( 1, 6 ); // 1-5
		else if ( roll < 90 + 50 )
			return random.Next( 5, 11 ); // 5-10
		else if ( roll < 90 + 50 + 25 )
			return random.Next( 10, 16 ); // 10-15
		else if ( roll < 90 + 50 + 25 + 12 )
			return random.Next( 15, 21 ); // 15-20
		else if ( roll < 90 + 50 + 25 + 12 + 6 )
			return 20; // 20-21
		else if ( roll < 90 + 50 + 25 + 12 + 6 + 3 )
			return 21; // 21-22
		else if ( roll < 90 + 50 + 25 + 12 + 6 + 3 + 1 )
			return 22; // 22-23
		else if ( roll < 90 + 50 + 25 + 12 + 6 + 3 + 1 + 0.5 )
			return 23; // 23-24
		else
			return random.Next( 24, 28 ); // 25-27
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

		if ( Stats == null )
		{
			Stats = new List<int>();
		}

		//GenerateRandomStats();
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

	protected override void OnDestroy()
	{
		if ( IsProxy || !Game.IsPlaying )
			return;

		Player.Local?.Inventory?.ClearItem( this );
	}
}
