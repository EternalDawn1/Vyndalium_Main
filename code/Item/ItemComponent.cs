

namespace GeneralGame;

public enum ItemState
{
	None,
	Backpack,
	Equipped
}
public enum Tier : byte
{
	C,
	B,
	A,
	S,
	SS,
	SSS,
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
	[Property, Group( "Armor" ),  Range( 0, 100 )] public float StunResistance { get; set; }
	[Property,Group( "Accessory" ),  Range( 0, 100 )] public float BlindResistance { get; set; }
	[Property, Group( "Accessory" ), Range( 0, 100 )] public float SlowResistence { get; set; }
	[Property, Group( "Accessory" ), Range( 0, 100 )] public float FireResistence { get; set; }
	[Property, Group( "Accessory" ), Range( 0, 100 )] public float BleedResistance { get; set; }
	[Property, Group( "Accessory" ), Range( 0, 100 )] public float PoisonResistence { get; set; }
	[Property, Group( "Accessory" ), Range( 0, 100 )] public float IceResistence { get; set; }
	[Property, Group( "Accessory" ), Range( 0, 100 )] public float LightningResistence { get; set; }
	[Property, Group( "Accessory" ), Range( 0, 100 )] public float HolyResistence { get; set; }
	[Property, Group( "Accessory" ), Range( 0, 100 )] public float ShadowResistence { get; set; }
	
	public int Price { get; set; }
    
		
	public bool IsEquipped { get; set; }

	public class TierClass
	{
		private Tier _tier;

		public Tier Tier
		{

			get {return _tier;}
			set {_tier = value;}

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
	[Property, Sync] public int SellPrice { get; set; } = -1;

	/// <summary>
	/// Maximum amount of items in this stack, default is 0 for not stackable.
	/// </summary>
	[Property]
	public int MaxStack
	{
		get => _maxStack;
		set
		{
			_maxStack = value;
			Count = value;
		}
	}

	private int _maxStack;

	[Property, Sync, HideIf( "MaxStack", 0 ), TargetSave( IgnoreIf = 0 )] public int Count { get; set; }
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
	[Property]public Player LastOwner { get; set; }

	private readonly SoundEvent _pickupSound = ResourceLibrary.Get<SoundEvent>( "sounds/misc/pickup.sound" );

	private ItemState _state;
	

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

	private void UpdateState()
	{
		GameObject.Enabled = State != ItemState.Backpack;
		if ( this is ItemEquipment equipment )
			equipment.UpdateEquipped();
			
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		Prefab = GameObject.PrefabInstanceSource;
	}

	protected override void OnStart()
	{
		GameObject.SetupNetworking();

		// Pickup
		var interactions = Components.GetOrCreate<Interactions>();
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

	protected override void OnDestroy()
	{
		if ( IsProxy || !Game.IsPlaying )
			return;

		Player.Local?.Inventory?.ClearItem( this );
	}
}
