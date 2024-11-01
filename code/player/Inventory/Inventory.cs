using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneralGame.Event;
using GeneralGame.HUD;
using Sandbox.ui.Hud;


namespace GeneralGame;
public enum SortDirection
{
	Ascending,
	Descending
}
public enum SortOption
{
	Tier,
	ItemLevel,
	RequiredLevel,
	Material,
	Favorite
}
public sealed class Inventory : Component
{
	
	[Property] Player Player { get; set; }
	public CharacterRenderer _renderer;
	private event Action _onBackpackSlotsChanged;
	public event Action OnBackpackSlotsChanged
	{
		add
		{
			_onBackpackSlotsChanged += value;
		}
		remove
		{
			_onBackpackSlotsChanged -= value;
		}
	}
	public int MAX_BACKPACK_SLOTS = 30;
	public  int MAX_STORAGE_SLOTS = 100;
	private const int ItemsPerPage = 20;
	public const int MAX_UPGRADE_SLOTS = 1;
	public const int MAX_ASPECT_SLOTS = 1;
	public static int MAX_BACKPACKBAG_SLOTS = 0;


	[Property]public IReadOnlyList<ItemComponent> BackpackItems => _backpackItems;
	[Property] public IReadOnlyList<ItemComponent> EquippedItems => _equippedItems;
	[Property] public IReadOnlyList<ItemComponent> StorageBoxItems => _storageBoxItems;
	[Property]public IReadOnlyList<ItemComponent> StorageItems => _storageItems;
	[Property] public IReadOnlyList<ItemComponent> UpgradeItems => _upgradeItems;
	[Property] public IReadOnlyList<ItemComponent> AspectItems => _aspectItems;
	[Property] public IReadOnlyList<ItemComponent> BackpackBagItems => _backpackBagItems;
	
	

	[Property] public readonly  List<ItemComponent> _backpackItems;
	[Property] public readonly List<ItemComponent> _equippedItems;
	[Property] public readonly List<ItemComponent> _storageBoxItems;
	[Property] public readonly List<ItemComponent> _storageItems;
	[Property] public readonly List<ItemComponent> _upgradeItems;
	[Property] public readonly List<ItemComponent> _aspectItems;
	[Property] public readonly List<ItemComponent> _backpackBagItems;
	public void SortBackpackBagItems( SortOption sortOption )
	{
		ToggleSortDirection();
		currentSortOption = sortOption;
		_backpackBagItems.Sort( CompareItems );
		// Aktualisieren Sie das UI nach dem Sortieren
		_onBackpackSlotsChanged?.Invoke();
	}
	public bool MoveItemToBackpackPackSlot( ItemComponent item, int backpackPackSlotIndex )
	{
		if ( backpackPackSlotIndex < 0 || backpackPackSlotIndex >= _backpackBagItems.Count )
			return false;

		if ( _backpackBagItems[backpackPackSlotIndex] != null )
			return false;

		// Entferne das Item aus dem ursprünglichen Slot
		int originalSlotIndex = _backpackItems.IndexOf( item );
		if ( originalSlotIndex >= 0 )
		{
			_backpackItems[originalSlotIndex] = null;
		}

		_backpackBagItems[backpackPackSlotIndex] = item;

		return true;
	}
	public bool RemoveItem( ItemComponent item )
	{
		if ( item == null )
			return false;

		if ( _backpackItems.Contains( item ) )
		{
			int index = _backpackItems.IndexOf( item );
			_backpackItems[index] = null; // Setze den Slot auf null, anstatt das Item zu entfernen
			item.State = ItemState.None;
			
			return true;
		}
		else if (_storageItems.Contains(item))
		{
			int index = _storageItems.IndexOf( item );
			_storageItems[index] = null;
			item.State = ItemState.None;
			return true;
		}
		else if (_upgradeItems.Contains(item))
		{
			int index = _upgradeItems.IndexOf( item );
			_upgradeItems[index] = null;
			item.State = ItemState.None;
			return true;
		}
		else if (_aspectItems.Contains(item))
		{
			int index = _aspectItems.IndexOf( item );
			_aspectItems[index] = null;
			item.State = ItemState.None;
			return true;
		}
		else if (_backpackBagItems.Contains(item))
		{
			int index = _backpackBagItems.IndexOf( item );
			_backpackBagItems[index] = null;
			item.State = ItemState.None;
			return true;
		}
		
		
		
		
		

		return false;
	}
	
	private SortOption currentSortOption = SortOption.Tier;
	private SortDirection currentSortDirection = SortDirection.Ascending;

	public void SortBackpackItems( SortOption sortOption )
	{
		ToggleSortDirection();
		currentSortOption = sortOption;
		_backpackItems.Sort( CompareItems );
		 // Aktualisieren Sie das UI nach dem Sortieren
	}
	
	public void SortStorageItems( SortOption sortOption )
	{
		ToggleSortDirection();
		currentSortOption = sortOption;

		int startIndex = ShopPanel.Instance.currentPage * ShopPanel.Instance.itemsPerPage;
		int endIndex = Math.Min( (ShopPanel.Instance.currentPage + 1) * ShopPanel.Instance.itemsPerPage, _storageItems.Count );

		var itemsToSort = _storageItems.GetRange( startIndex, endIndex - startIndex );
		itemsToSort.Sort( CompareItems );

		// Set the sorted items back to the original list
		for ( int i = startIndex; i < endIndex; i++ )
		{
			_storageItems[i] = itemsToSort[i - startIndex];
		}

		// Aktualisieren Sie das UI nach dem Sortieren
		
	}
	private void ToggleSortDirection()
	{
		currentSortDirection = currentSortDirection == SortDirection.Ascending
			? SortDirection.Descending
			: SortDirection.Ascending;
	}

	private int CompareItems( ItemComponent x, ItemComponent y )
	{
		if ( x == null && y == null ) return 0;
		if ( x == null ) return 1;
		if ( y == null ) return -1;

		int comparisonResult = 0;

		switch ( currentSortOption )
		{
			case SortOption.Tier:
				comparisonResult = CompareByTier( x, y );
				break;
			case SortOption.ItemLevel:
				comparisonResult = x.ItemLevel.CompareTo( y.ItemLevel );
				break;
			case SortOption.RequiredLevel:
				comparisonResult = x.RequiredLevel.CompareTo( y.RequiredLevel );
				break;
			case SortOption.Material:
				comparisonResult = CompareByType( x, y );
				break;
			case SortOption.Favorite:
				comparisonResult = CompareByFavorite( x, y );
				break;
			default:
				comparisonResult = 0;
				break;
		}

		return currentSortDirection == SortDirection.Ascending ? comparisonResult : -comparisonResult;
	}

	private int CompareByTier( ItemComponent x, ItemComponent y )
	{
		return x.Tier.CompareTo( y.Tier );
	}

	private int CompareByType( ItemComponent x, ItemComponent y )
	{
		if ( x.IsWeapon && !y.IsWeapon ) return -1;
		if ( !x.IsWeapon && y.IsWeapon ) return 1;

		if ( x.IsArmor && !y.IsArmor ) return -1;
		if ( !x.IsArmor && y.IsArmor ) return 1;

		if ( x.IsMaterial && !y.IsMaterial ) return -1;
		if ( !x.IsMaterial && y.IsMaterial ) return 1;

		if ( x.IsPotion && !y.IsPotion ) return -1;
		if ( !x.IsPotion && y.IsPotion ) return 1;

		if (x.IsAccessory && !y.IsAccessory) return -1;
		if (!x.IsAccessory && y.IsAccessory) return 1;

		if(x.IsAspect && !y.IsAspect) return -1;
		if(!x.IsAspect && y.IsAspect) return 1;

		return 0;
	}

	private int CompareByFavorite( ItemComponent x, ItemComponent y )
	{
		if ( x.IsFavorite && !y.IsFavorite ) return -1;
		if ( !x.IsFavorite && y.IsFavorite ) return 1;
		return 0;
	}
	public bool BackpackHasMaterial( string materialName, int quantity )
	{
		if ( _backpackItems == null )
		{
			return false;
		}

		int totalAmount = _backpackItems.Where( m => m != null && m.Name == materialName )
										.Sum( m => m.Count );

		return totalAmount >= quantity;
	}

	public int GetBackpackMaterialCount( string materialName )
	{
		if ( _backpackItems == null )
		{
			return 0;
		}

		return _backpackItems.Where( m => m != null && m.Name == materialName )
							 .Sum( m => m.Count );
	}

	public void RemoveBackpackMaterial( string materialName, int quantity )
	{
		if ( _backpackItems == null )
		{
			return;
		}

		var materialsToRemove = _backpackItems.Where( m => m != null && m.Name == materialName ).ToList();
		int remainingQuantity = quantity;

		foreach ( var material in materialsToRemove )
		{
			if ( material.Count > remainingQuantity )
			{
				material.Count -= remainingQuantity;
				if ( material.Count <= 0 )
				{
					_backpackItems[_backpackItems.IndexOf( material )] = null;
					material.GameObject.Destroy();
				}
				return;
			}
			else
			{
				remainingQuantity -= material.Count;
				_backpackItems[_backpackItems.IndexOf( material )] = null;
				material.GameObject.Destroy();
				if ( remainingQuantity <= 0 )
				{
					return;
				}
			}
		}
	}
	public bool RemoveItemUpgrade( string name, int count )
	{
		int remainingCount = count;
		for ( int i = 0; i < _backpackItems.Count; i++ )
		{
			var item = _backpackItems[i];
			if ( item != null && item.Name == name )
			{
				if ( item.Count > remainingCount )
				{
					item.Count -= remainingCount;
					return true;
				}
				else
				{
					remainingCount -= item.Count;
					_backpackItems[i] = null;
					item.State = ItemState.None;
					if ( remainingCount <= 0 )
					{
						return true;
					}
				}
			}
		}
		return false;
	}
	public static void EquipItemStats( ItemComponent item )
	{
		//Player.Local.AttackValue += item.DMG;

		Player.Local.MinAttackValue += item.MinAttackValue;
		Player.Local.MaxAttackValue += item.MaxAttackValue;

		Player.Local.MinArmorValue += item.MinArmorValue;
		Player.Local.MaxArmorValue += item.MaxArmorValue;

		Player.Local.Health += item.HE;
		Player.Local.Armor += item.Armor;
		Player.Local.STG += item.STG;
		Player.Local.HE += item.HE;
		Player.Local.DEX += item.DEX;
		Player.Local.PER += item.PER;
		Player.Local.INT += item.INT;
		Player.Local.MaxMana += item.Mana;
		Player.Local.MaxHealth += item.Health;
		Player.Local.IncreaseCritHitDamage( item.CritHitDamage );
		Player.Local.IncreaseCritHitChance( item.CritHitChance );
		Player.Local.AbilityHaste += item.AbilityHaste;
		Player.Local.AttackPower += item.AttackPower;
		Player.Local.MagicPower += item.MagicPower;
		Player.Local.AttackSpeed += item.AttackSpeed;
		Player.Local.MoveSpeed += item.MoveSpeed;
		Player.Local.Armor += item.Armor;
		Player.Local.MagicDefense += item.MagicDefense;
		Player.Local.Evasion += item.Evasion;
		Player.Local.Block += item.Cover;
		Player.Local.BonusEXPGain += item.BonusEXP;
		Player.Local.BonusScore += item.BonusScore;
		Player.Local.BonusVyndalium += item.BonusVyndalium;
		Player.Local.Tenacity += item.Tenacity;
		Player.Local.StunResist += item.StunResistance;
		Player.Local.BlindResist += item.BlindResistance;
		Player.Local.BleedResist += item.BleedResistance;
		Player.Local.SlowResist += item.SlowResistence;
		Player.Local.FireResist += item.FireResistence;
		Player.Local.PoisonResist += item.PoisonResistence;
		Player.Local.IceResist += item.IceResistence;
		Player.Local.LightningResist += item.LightningResistence;
		Player.Local.LightResist += item.HolyResistence;
		Player.Local.ShadowResist += item.ShadowResistence;
	}

	public static void UnequipItemStats( ItemComponent item )
	{
		Player.Local.MinAttackValue -= item.MinAttackValue;
		Player.Local.MaxAttackValue -= item.MaxAttackValue;

		Player.Local.MinArmorValue -= item.MinArmorValue;
		Player.Local.MaxArmorValue -= item.MaxArmorValue;

		//Player.Local.AttackValue -= item.DMG;
		Player.Local.Armor -= item.Armor;
		
		Player.Local.STG -= item.STG;
		Player.Local.HE -= item.HE;
		Player.Local.DEX -= item.DEX;
		Player.Local.PER -= item.PER;
		Player.Local.INT -= item.INT;
		Player.Local.MaxMana -= item.Mana;
		Player.Local.MaxHealth -= item.Health;
		Player.Local.CritHitDamage -= item.CritHitDamage;
		Player.Local.CritHitChance -= item.CritHitChance;
		Player.Local.AbilityHaste -= item.AbilityHaste;
		Player.Local.AttackPower -= item.AttackPower;
		Player.Local.MagicPower -= item.MagicPower;
		Player.Local.AttackSpeed -= item.AttackSpeed;
		Player.Local.MoveSpeed -= item.MoveSpeed;
		Player.Local.BleedResist -= item.BleedResistance;
		Player.Local.Armor -= item.Armor;
		Player.Local.MagicDefense -= item.MagicDefense;
		Player.Local.Evasion -= item.Evasion;
		Player.Local.Block -= item.Cover;
		Player.Local.BonusEXPGain -= item.BonusEXP;
		Player.Local.BonusScore -= item.BonusScore;
		Player.Local.BonusVyndalium -= item.BonusVyndalium;
		Player.Local.Tenacity -= item.Tenacity;
		Player.Local.StunResist -= item.StunResistance;
		Player.Local.IceResist -= item.IceResistence;
		Player.Local.BlindResist -= item.BlindResistance;
		Player.Local.SlowResist -= item.SlowResistence;
		Player.Local.FireResist -= item.FireResistence;
		Player.Local.PoisonResist -= item.PoisonResistence;
		Player.Local.LightningResist -= item.LightningResistence;
		Player.Local.LightResist -= item.HolyResistence;
		Player.Local.ShadowResist -= item.ShadowResistence;
	}
	
	[ConCmd( "reset_attackvalue" )]
	public static void SetPlayerAttackValuesToZero()
	{
		if ( Player.Local != null )
		{
			Player.Local.MinAttackValue = 0;
			Player.Local.MaxAttackValue = 0;
			Log.Info( "MinAttackValue und MaxAttackValue des Spielers wurden auf 0 gesetzt." );
		}
		else
		{
			Log.Info( "Spieler nicht gefunden." );
			
		}
	}
	[ConCmd( "reset_armor" )]
	public static void SetPlayerArmorValuesToZero()
	{
		if ( Player.Local != null )
		{
			Player.Local.MinArmorValue = 0;
			Player.Local.MaxArmorValue = 0;
			Log.Info( "MinArmorValue und MaxArmorValue des Spielers wurden auf 0 gesetzt." );
		}
		else
		{
			Log.Info( "Spieler nicht gefunden." );
		}
	}



	public static Inventory Instance { get; private set; }

	public Inventory()
	{
		_storageItems = new List<ItemComponent>( new ItemComponent[MAX_STORAGE_SLOTS] );
		_equippedItems = new List<ItemComponent>( new ItemComponent[Enum.GetNames( typeof( EquipSlot ) ).Length] );
		_storageBoxItems = new List<ItemComponent>();
		_upgradeItems = new List<ItemComponent>( new ItemComponent[MAX_UPGRADE_SLOTS] );
		_aspectItems = new List<ItemComponent>(new ItemComponent[MAX_ASPECT_SLOTS] );
		_backpackBagItems = new List<ItemComponent>(new ItemComponent[MAX_BACKPACKBAG_SLOTS] );
	}
	public void InitializeBackpackSlots()
	{
		for ( int i = 0; i < MAX_BACKPACKBAG_SLOTS; i++ )
		{
			_backpackBagItems.Add( null ); // Initialisiere den Slot, wenn er nicht existiert
		}
	}

	public int IndexOf( ItemComponent item )
	{
		if ( item == null )
		{
			return -1;
		}

		if ( item is ItemEquipment equipment && equipment.Equipped )
		{
			return _equippedItems.IndexOf( item );
		}
		else if(item.State == ItemState.Storage)
		{
			return _storageItems.Contains( item ) ? _storageItems.IndexOf( item ) : _backpackItems.IndexOf( item );
		}
		else if ( item.State == ItemState.Upgrade )
		{
			return _upgradeItems.IndexOf( item );
		}
		else if ( item.State == ItemState.Aspect )
		{
			return _aspectItems.IndexOf( item );
		}
		
		else 
		{
			return _backpackItems.Contains( item ) ? _backpackItems.IndexOf( item ) : _storageBoxItems.IndexOf( item );
		}
		
		
		
	}

	public bool HasSpaceInBackpack()
		=> _backpackItems.IndexOf( null ) != -1;

	public ItemComponent GetItemInSlot( EquipSlot slot ) => _equippedItems.ElementAtOrDefault( (int)slot );
	public bool IsSlotOccupied( EquipSlot slot ) => GetItemInSlot( slot ) is not null;

	
	// wenn der spieler etwas aufhebt		
	public bool GiveItem( ItemComponent item )
	{
		
		

		var firstFreeSlot = _backpackItems.IndexOf( null );
		if ( firstFreeSlot == -1 )
			return false;

		SetOwner( item );
		GiveBackpackItem( item, firstFreeSlot );
		item.State = ItemState.Backpack;
		item.GameObject.Enabled = false;
		
		TaskMaster.SubmitTriggerSignal( $"item.received.{item.Name}", Player );

		return true;
	}
	private int totalPages => (int)Math.Ceiling( (double)_storageItems.Count / ItemsPerPage );
	private int FindNextFreeStorageSlot()
	{
		for ( int currentPage = 0; currentPage < totalPages; currentPage++ )
		{
			int startIndex = currentPage * ItemsPerPage;
			int endIndex = Math.Min( startIndex + ItemsPerPage, _storageItems.Count );

			for ( int i = startIndex; i < endIndex; i++ )
			{
				if ( _storageItems[i] == null )
				{
					return i;
				}
			}
		}

		// Wenn kein freier Slot gefunden wurde, geben wir -1 zurück
		return -1;
	}

	
	public void MoveItemToUpgrade( ItemComponent item )
	{
		if ( item == null ) return;

		if ( _backpackItems.Contains( item ) )
		{
			int freeSlot = _upgradeItems.IndexOf( null );
			if ( freeSlot != -1 )
			{
				int itemIndex = _backpackItems.IndexOf( item );
				_backpackItems[itemIndex] = null; // Setze den Slot im Rucksack auf null
				_upgradeItems[freeSlot] = item;
				item.State = ItemState.Upgrade;
				item.GameObject.Enabled = false;
				ShopPanel.Instance?.CheckUpgradeSlot();
			}
			else
			{
				
				Hudmaster.Instance.ShowNotification( "Slot occupied.", "/ui/hud/exit.gif"  );
			}
		}
	}
	private int FindNextFreeSlotInPage( int currentPage )
	{
		// Implementiere die Logik, um den nächsten freien Slot auf der aktuellen Seite zu finden
		// Beispiel:
		int startIndex = currentPage * ItemsPerPage;
		int endIndex = startIndex + ItemsPerPage;

		for ( int i = startIndex; i < endIndex; i++ )
		{
			if ( _storageItems[i] == null )
			{
				return i;
			}
		}
		return -1;
	}
	public void MoveItemToStorage( ItemComponent item, int currentPage )
	{
		if ( item == null ) return;

		if ( _backpackItems.Contains( item ) )
		{
			int freeSlot = FindNextFreeSlotInPage( currentPage );
			if ( freeSlot == -1 )
			{
				freeSlot = FindNextFreeStorageSlot();
			}

			if ( freeSlot != -1 )
			{
				int itemIndex = _backpackItems.IndexOf( item );
				_backpackItems[itemIndex] = null; // Setze den Slot im Rucksack auf null
				_storageItems[freeSlot] = item;
				item.State = ItemState.Storage;
				item.GameObject.Enabled = false;
			}
			else
			{
				Hudmaster.Instance.ShowNotification( "Slot occupied / too full.", "/ui/hud/exit.gif" );
			}
		}
	}
	public void AspectToBackpack( ItemComponent item )
	{
		if ( item == null ) return;

		if ( _aspectItems.Contains( item ) )
		{
			int freeSlot = _backpackItems.IndexOf( null );
			if ( freeSlot != -1 )
			{
				int itemIndex = _aspectItems.IndexOf( item );
				_aspectItems[itemIndex] = null; // Setze den Slot im Aspekt auf null
				_backpackItems[freeSlot] = item;
				item.State = ItemState.Backpack;
				item.GameObject.Enabled = false;
				ShopPanel.Instance?.CheckAspectSlot();
			}
			else
			{
				
				Hudmaster.Instance.ShowNotification( "No Slots in Backpack available.", "/ui/hud/exit.gif" );
			}
		}
	}
	public void MoveItemFromUpgradeToBackpack( ItemComponent item )
	{
		if ( item == null ) return;

		if ( _upgradeItems.Contains( item ) )
		{
			int freeSlot = _backpackItems.IndexOf( null );
			if ( freeSlot != -1 )
			{
				int itemIndex = _upgradeItems.IndexOf( item );
				_upgradeItems[itemIndex] = null; // Setze den Slot im Upgrade auf null
				_backpackItems[freeSlot] = item;
				item.State = ItemState.Backpack;
				item.GameObject.Enabled = false;
				
			}
			else
			{
				
				Hudmaster.Instance.ShowNotification( "No Slots in Backpack available.", "/ui/hud/exit.gif" );
			}
		}
	}

	public void MoveItemFromAspectToBackpack( ItemComponent item )
	{
		if ( item == null ) return;
		if ( _backpackItems.Contains( item ) )
		{
			int freeSlot = _aspectItems.IndexOf( null );
			if ( freeSlot != -1 )
			{
				int itemIndex = _backpackItems.IndexOf( item );
				_backpackItems[itemIndex] = null; // Setze den Slot im Aspekt auf null
				_aspectItems[freeSlot] = item;
				item.State = ItemState.Aspect;
				item.GameObject.Enabled = false;
				ShopPanel.Instance?.CheckAspectSlot();
			}
			else
			{
				Hudmaster.Instance.ShowNotification( "No Slots in Backpack available.", "/ui/hud/exit.gif" );
			}
		}
	}
	public void MoveItemToBackpackBagBag( ItemComponent item )
	{
		if ( item == null ) return;

		if ( _backpackBagItems.Contains( item ) )
		{
			int freeSlot = _backpackItems.IndexOf( null );
			if ( freeSlot != -1 )
			{
				int itemIndex = _backpackBagItems.IndexOf( item );
				_backpackBagItems[itemIndex] = null; // Setze den Slot im Storage auf null
				_backpackItems[freeSlot] = item;
				item.State = ItemState.Backpack;
				item.GameObject.Enabled = false;
			}
			else
			{

				Hudmaster.Instance.ShowNotification( "Slot occupied / no Slots available.", "/ui/hud/exit.gif" );
			}
		}
	}

	public void MoveItemToBackpack( ItemComponent item )
	{
		if ( item == null ) return;

		if ( _storageItems.Contains( item ) )
		{
			int freeSlot = _backpackItems.IndexOf( null );
			if ( freeSlot != -1 )
			{
				int itemIndex = _storageItems.IndexOf( item );
				_storageItems[itemIndex] = null; // Setze den Slot im Storage auf null
				_backpackItems[freeSlot] = item;
				item.State = ItemState.Backpack;
				item.GameObject.Enabled = false;
			}
			else
			{
				
				Hudmaster.Instance.ShowNotification( "Slot occupied / no Slots available.", "/ui/hud/exit.gif" );
			}
		}
	}



	public bool EquipItemFromBackpack( ItemComponent item )
	{
		if ( item == null )
			return false;

		if ( IsProxy )
			return true;
		

		var index = _backpackItems?.IndexOf( item ) ?? -1;
		if ( index == -1 )
			return false;

		if ( item is not ItemEquipment equipment )
			return false;

		if ( _equippedItems == null )
		{
			Log.Error( "Equipped items list is null." );
			return false;
		}

		if ( IsSlotOccupied( equipment.Slot ) )
		{
			var equippedItem = GetItemInSlot( equipment.Slot );
			var placedInBackpack = UnequipItem( equippedItem );
			if ( !placedInBackpack )
			{
				DropItem( equippedItem );
			}
		}




		if ( item.CanEquip( Player.Level ) )
		{
			
			GiveEquipmentItem( equipment );
			equipment.State = ItemState.Equipped;
			TaskMaster.SubmitTriggerSignal( $"item.equipped.{item.Name}", Player );
			

			var weaponContainer = Player.Components.Get<WeaponContainer>();
			if ( weaponContainer != null )
			{
				weaponContainer.Give( item.GameObject, true );
				
				Player.Local?.PlaySuccessSoundFromPath( "sounds/guns/switch/weapon_switch.sound", 0.025f );
			}
			if ( item is Backpack backpack )
			{
				MAX_BACKPACKBAG_SLOTS = (int)backpack.SlotAmount;
				
				if ( _backpackBagItems.Count < MAX_BACKPACKBAG_SLOTS )
				{
					for ( int i = _backpackBagItems.Count; i < MAX_BACKPACKBAG_SLOTS; i++ )
					{
						_backpackBagItems.Add( null );
					}
					var modelRenderer = item.GameObject.Components.Get<SkinnedModelRenderer>();
					{
						modelRenderer.Enabled = false;
					}
					return true;
				}
			}


			index = _backpackItems?.IndexOf( item ) ?? -1;
			return true;
		}
		else
		{
			
			Hudmaster.Instance.ShowNotification( "player level too low.", "/ui/hud/exit.gif" );
			Player.Local?.PlaySuccessSoundFromPath( "sounds/upgrade/failing.sound", 0.0125f );
			return false;
		}
		
	}
	public int GetFirstFreeBackpackSlot()
	{
		for ( int i = 0; i < _backpackItems.Count; i++ )
		{
			if ( _backpackItems[i] == null )
			{
				return i;
			}
		}
		return -1;
	}


	public bool EquipItemFromWorld( ItemComponent item, bool forceReplace = false )
	{
		if ( item == null )
			return false;
		if (IsProxy)
			return true;
			
		if ( item is not ItemEquipment equipment )
			return false;

		
		if ( IsSlotOccupied( equipment.Slot ) && !forceReplace )
			return false;

		if ( IsSlotOccupied( equipment.Slot ) && forceReplace )
		{
			var equippedItem = GetItemInSlot( equipment.Slot );
			var placedInBackpack = UnequipItem( equippedItem );
			if ( !placedInBackpack )
				DropItem( equippedItem );
				


		}
		

		if(item.CanEquip(Player.Level))
		{
			GiveEquipmentItem( equipment );
			equipment.State = ItemState.Equipped;
			TaskMaster.SubmitTriggerSignal( $"item.equipped.{item.Name}", Player );

			var weaponContainer = Player.Components.Get<WeaponContainer>();
			if ( weaponContainer != null )
			{
				weaponContainer.Give( item.GameObject, true );
				Player.Local?.PlaySuccessSoundFromPath( "sounds/guns/switch/weapon_switch.sound", 0.0125f );
			}
			if ( item is Backpack backpack )
			{
				MAX_BACKPACKBAG_SLOTS = (int)backpack.SlotAmount;

				if ( _backpackBagItems.Count < MAX_BACKPACKBAG_SLOTS )
				{
					for ( int i = _backpackBagItems.Count; i < MAX_BACKPACKBAG_SLOTS; i++ )
					{
						_backpackBagItems.Add( null );
					}
					return true;
				}
			}

		

			return true;
		}
		else
		{
			
			Hudmaster.Instance.ShowNotification( "player level too low", "/ui/hud/exit.gif" );
			Player.Local?.PlaySuccessSoundFromPath( "sounds/upgrade/failing.sound", 0.0125f );

			return false;
		}
	}



	public bool UnequipItem( ItemComponent item )
	{
		if ( item == null )
		{
			Log.Error( "Item is null." );
			return false;
		}

		if ( item is not ItemEquipment equipment || !equipment.Equipped )
		{
			Log.Error( "Item is not equipment or not equipped." );
			return false;
		}

		var slotIndex = (int)equipment.Slot;
		var equippedItem = _equippedItems[slotIndex];
		if ( equippedItem != item )
		{
			Hudmaster.Instance.ShowNotification( "Item is not the equipped item in the expected slot.", "/ui/hud/exit.gif" );
			Player.Local?.PlaySuccessSoundFromPath( "sounds/upgrade/failing.sound", 0.0125f );
			return false;
		}

		var firstFreeSlot = _backpackItems.IndexOf( null );
		if ( firstFreeSlot == -1 )
		{
			Hudmaster.Instance.ShowNotification( "No Place in the Backpack.", "/ui/hud/inventory.png" );
			Player.Local?.PlaySuccessSoundFromPath( "sounds/upgrade/failing.sound", 0.0125f );
			return false;
		}

		// Entfernen der Statistiken des Items
		RemoveEquipmentItem( equipment );
		
		Player.Local?.PlaySuccessSoundFromPath( "sounds/weapons/weapon_holster4.sound", 0.0125f );

		// Sicherstellen, dass das Item nicht zerstört wird, wenn es unequipped wird
		if ( equipment.Slot == EquipSlot.Hand )
		{
			var weaponContainer = Player.Components.Get<WeaponContainer>();
			if ( weaponContainer != null )
			{
				weaponContainer.RemoveWeapon( item.GameObject, false );
			}
			else
			{
				Log.Info( "WeaponContainer ist null" );
			}
		}
		if ( item is Backpack )
		{
			MAX_BACKPACKBAG_SLOTS = 0; // Zurücksetzen auf den Standardwert

			// Entfernen der zusätzlichen Slots
			_backpackBagItems.RemoveRange( 0, _backpackBagItems.Count );
		}

		item.GameObject.Enabled = false;
		GiveBackpackItem( equipment, firstFreeSlot );
		equipment.State = ItemState.Backpack;

		TaskMaster.SubmitTriggerSignal( $"item.unequipped.{item.Name}", Player );

		return true;
	}





	public bool DropItem( ItemComponent item )
	{
		if(IsProxy) return true;

		if ( item is ItemEquipment equipment && equipment.Equipped )
			RemoveEquipmentItem( equipment );

		else if ( item.State == ItemState.Backpack )
			RemoveBackpackItem( item, _backpackItems.IndexOf( item ) );

		else if ( item.State == ItemState.Storage )
			RemoveStorageItem( item, _storageItems.IndexOf( item ) );


		



		item.State = ItemState.None;
		item.GameObject.Enabled = true;
		item.GameObject.Components.GetOrCreate<SkinnedModelRenderer>().Enabled = true;
		var ModelRenderer = item.GameObject.Components.Get<ModelRenderer>();
		if ( ModelRenderer != null )
		{
			ModelRenderer.Enabled = true;
		}
		var ModelColider = item.GameObject.Components.Get<ModelCollider>();
		if ( ModelColider != null )
		{
			ModelColider.Enabled = true;
		}
		
		TaskMaster.SubmitTriggerSignal( $"item.dropped.{item.Name}", Player );
		
		

		item.GameObject.Parent = null;

		var trace = Scene.Trace.FromTo( Player.ViewRay.Position + Player.ViewRay.Forward, Player.ViewRay.Position + Player.ViewRay.Forward * 80f )
				.IgnoreGameObject( Player.GameObject )
				.IgnoreGameObject( item.GameObject )
				.Radius( 1.0f )
				.Run();

		item.GameObject.WorldRotation = Rotation.Identity;
		item.GameObject.WorldPosition = trace.EndPosition;

		var velocity = Player.Velocity + Player.ViewRay.Forward * 150f;
		if ( item.GameObject.Components.TryGet<Rigidbody>( out var rigidbody, FindMode.EverythingInSelf ) )
		{
			rigidbody.Velocity = velocity;
			rigidbody.MotionEnabled = true;
		}
		else if ( item.GameObject.Components.TryGet<ModelPhysics>( out var modelPhysics, FindMode.EverythingInSelf ) )
		{
			item.GameObject.Enabled = false;
			item.GameObject.WorldPosition = trace.EndPosition;
			item.GameObject.Enabled = true;
			modelPhysics.PhysicsGroup?.AddVelocity( velocity );
		}
		else if ( item.GameObject.Components.TryGet<PhysicsBody>( out var physicsBody, FindMode.EverythingInSelf ) )
		{
			physicsBody.Velocity = velocity;
		}
		else if(item.GameObject.Components.TryGet<ModelRenderer>(out var modelRenderer, FindMode.EverythingInSelf))
		{
			modelRenderer.Enabled = true;
		}
		
		

		return true;
	}
	
	public bool SwapBackpackPackItems( int fromIndex, int toIndex )
	{
		var fromItem = _backpackBagItems.ElementAtOrDefault( fromIndex );
		var toItem = _backpackItems.ElementAtOrDefault( toIndex );

		if ( fromItem is null || toItem is null )
			return false;

		_backpackBagItems[fromIndex] = toItem;
		_backpackItems[toIndex] = fromItem;

		return true;
	}

	public bool MoveItemToBackpackSlot( ItemComponent item, int backpackSlotIndex )
	{
		if ( backpackSlotIndex < 0 || backpackSlotIndex >= _backpackItems.Count )
			return false;

		if ( _backpackItems[backpackSlotIndex] != null )
			return false;

		// Entferne das Item aus dem ursprünglichen Slot
		int originalSlotIndex = _backpackBagItems.IndexOf( item );
		if ( originalSlotIndex >= 0 )
		{
			_backpackBagItems[originalSlotIndex] = null;
		}

		_backpackItems[backpackSlotIndex] = item;

		return true;
	}
	public void RemoveBackpackPackItem( ItemComponent item, int index )
	{
		if ( index >= 0 && index < _backpackBagItems.Count )
		{
			_backpackBagItems[index] = null;
		}
	}


	public bool SwapItems( int index, EquipSlot slot )
	{
		var item = _backpackItems.ElementAtOrDefault( index );
		if ( item is null )
			return false;

		if ( item is not ItemEquipment equipment || equipment.Slot != slot )
			return false;

		RemoveBackpackItem( item, index );


		var previouslyEquippedItem = _equippedItems[(int)slot];
		if ( previouslyEquippedItem is not null )

		{
			RemoveEquipmentItem( previouslyEquippedItem as ItemEquipment );
			GiveBackpackItem( previouslyEquippedItem, index );
			previouslyEquippedItem.State = ItemState.Backpack;

		}
		var weaponContainer = Player.Components.Get<WeaponContainer>();
		if ( weaponContainer != null )
		{
			weaponContainer.RemoveWeapon( item.GameObject, false );

		}

		

		GiveEquipmentItem( equipment );
		equipment.State = ItemState.Equipped;

		
		if ( weaponContainer != null )
		{
			weaponContainer.Give( item.GameObject, true );
			Player.Local?.PlaySuccessSoundFromPath( "sounds/guns/switch/weapon_switch.sound", 0.0125f );
		}
		else
		{
			Log.Info( "Item is equipment, skipping Give." );
		}


		return true;
	}

	/// <summary>
	/// Move item to BackpackBag - Backpack inventory Slot
	/// </summary>
	/// <param name="item"></param>
	public void MoveItemToBackpackBag( ItemComponent item )
	{
		if ( item == null ) return;

		if ( _backpackItems.Contains( item ) )
		{
			int freeSlot = _backpackBagItems.IndexOf( null );
			if ( freeSlot != -1 )
			{
				int itemIndex = _backpackItems.IndexOf( item );
				_backpackItems[itemIndex] = null; // Setze den Slot im Rucksack auf null
				_backpackBagItems[freeSlot] = item;
				item.State = ItemState.BackpackBag;
				item.GameObject.Enabled = false;
				
			}
			else
			{
				Hudmaster.Instance.ShowNotification( "Slot occupied.", "/ui/hud/exit.gif" );
			}
		}
	}
	public void MoveItemToBackpack( ItemComponent item, int backpackSlotIndex )
	{
		if ( item == null ) return;

		if ( _backpackItems.Contains( item ) )
		{
			int freeSlot = _storageItems.IndexOf( null );
			if ( freeSlot != -1 )
			{
				int itemIndex = _backpackItems.IndexOf( item );
				_backpackItems[itemIndex] = null; // Setze den Slot im Rucksack auf null
				_storageItems[freeSlot] = item;
				item.State = ItemState.Storage;
				item.GameObject.Enabled = false;
			}
			else
			{
				Hudmaster.Instance.ShowNotification( "Slot occupied.", "/ui/hud/exit.gif" );
			}
		}
	}



	public void RemoveStorageItem( ItemComponent item, int index )
	{
		if ( item == null || index < 0 || index >= _storageItems.Count )
			return;

		_storageItems[index] = null;
		item.State = ItemState.None;
	}
	private bool CanStack( ItemComponent first, ItemComponent second )
		=> first.Prefab == second.Prefab
		&& first.IsStackable && second.IsStackable
		&& second.Count < second.MaxStack
		&& first.Tier == second.Tier;

	public bool SwapItems( int firstIndex, int secondIndex, bool isStorage = false )
	{
		var firstItem = isStorage ? _storageItems.ElementAtOrDefault( firstIndex ) : _backpackItems.ElementAtOrDefault( firstIndex );
		if ( firstItem is null )
			return false;

		if ( isStorage )
			RemoveStorageItem( firstItem, firstIndex );
		else
			RemoveBackpackItem( firstItem, firstIndex );

		var secondItem = isStorage ? _storageItems.ElementAtOrDefault( secondIndex ) : _backpackItems.ElementAtOrDefault( secondIndex );
		var invert = false;

		if ( secondItem is not null )
		{
			if ( isStorage )
				RemoveStorageItem( secondItem, secondIndex );
			else
				RemoveBackpackItem( secondItem, secondIndex );

			// Stacking
			if ( CanStack( firstItem, secondItem ) )
			{
				var from = firstItem;
				var to = secondItem;
				invert = true;

				var amount = Math.Min( Math.Abs( to.Count - to.MaxStack ), from.Count );
				RemoveAmount( from, amount );
				to.Count += amount;

				if ( from == null || from.Count <= 0 )
				{
					if ( isStorage )
						GiveStorageItem( to, secondIndex );
					else
						GiveBackpackItem( to, secondIndex );
					return true;
				}
			}
			else if ( CanStack( secondItem, firstItem ) )
			{
				var from = secondItem;
				var to = firstItem;
				invert = true;

				var amount = Math.Min( Math.Abs( to.Count - to.MaxStack ), from.Count );
				RemoveAmount( from, amount );
				to.Count += amount;

				if ( from == null || from.Count <= 0 )
				{
					if ( isStorage )
						GiveStorageItem( to, secondIndex );
					else
						GiveBackpackItem( to, secondIndex );
					return true;
				}
			}

			if ( isStorage )
				GiveStorageItem( secondItem, invert ? secondIndex : firstIndex );
			else
				GiveBackpackItem( secondItem, invert ? secondIndex : firstIndex );
		}

		if ( isStorage )
			GiveStorageItem( firstItem, invert ? firstIndex : secondIndex );
		else
			GiveBackpackItem( firstItem, invert ? firstIndex : secondIndex );

		return true;
	}
	public bool SwapItems( EquipSlot slot, int index )
	{
		var item = _equippedItems[(int)slot];
		if ( item is null )
			return false;

		var previousBackpackItem = _backpackItems[index];
		if ( previousBackpackItem is not null && (previousBackpackItem is not ItemEquipment itemToEquip || slot != itemToEquip.Slot) )
			return false;

		RemoveEquipmentItem( item as ItemEquipment );

		if ( previousBackpackItem is not null )
		{
			RemoveBackpackItem( previousBackpackItem, index );

			GiveEquipmentItem( previousBackpackItem as ItemEquipment );
			previousBackpackItem.State = ItemState.Equipped;

		}
		

		GiveBackpackItem( item, index );
		item.State = ItemState.Backpack;
		
		var weaponContainer = Player.Components.Get<WeaponContainer>();
		if ( weaponContainer != null )
		{
			weaponContainer.Give( item.GameObject, true );
		}
		else
		{
			Log.Info( "Item is equipment, skipping Give." );
		}


		return true;
	}
	public void SetItem( ItemComponent item, int index )
	{
		
		if ( item == null )
		{
			
		}

		if ( Player == null )
		{
			
		}

		SetOwner( item ); // Zeile 913
		GiveBackpackItem( item, index );
		item.State = ItemState.Backpack;
		item.GameObject.Enabled = false;
	}

	

	
	/// <summary>
	/// Removes a specific amount from an item if the item is stackable and has more than the amount.
	/// </summary>
	/// <param name="item"></param>
	/// <param name="count"></param>
	/// <param name="destroy"></param>
	/// <param name="predicate"></param>
	/// <returns></returns>
	public bool RemoveAmount( ItemComponent item, int count = 1, bool destroy = true, Func<ItemComponent, bool> predicate = null )
	{
		// Non stackables.
		if ( !item.IsStackable )
		{
			var items = BackpackItems
				.Where( x => x != item && x?.Prefab == item?.Prefab && (predicate?.Invoke( x ) ?? true) )
				.ToList();

			if ( count > items.Count + 1 )
				return false;

			ClearItem( item );
			if ( destroy )
			{
				item.State = ItemState.None;
				item?.GameObject?.Destroy();
			}

			for ( int i = 0; i < count - 1; i++ )
			{
				var target = items[i];
				ClearItem( target );
				if ( destroy )
				{
					target.State = ItemState.None;
					target?.GameObject?.Destroy();
				}
			}

			return true;
		}

		// Stackables.
		if ( count > item.Count )
			return false;

		item.Count -= count;

		if ( item.Count <= 0 )
		{
			ClearItem( item );
			if ( destroy )
			{
				item.State = ItemState.None;
				item?.GameObject?.Destroy();
			}
		}



		return true;
	}

	/// <summary>
	/// Bypasses any restrictions and clears the item. Do not use this for regular inventory usage.
	/// </summary>
	public void ClearItem( ItemComponent item )
	{
		if ( _backpackItems.Contains( item ) )
		{
			_backpackItems[_backpackItems.IndexOf( item )] = null;
			item.State = ItemState.None;
		}
		else if ( _equippedItems.Contains( item ) )
		{
			_equippedItems[_equippedItems.IndexOf( item )] = null;
			item.State = ItemState.None;
		}
	}
	public void AddItem( ItemComponent item )
	{
		if ( item == null ) return;

		// Überprüfen, ob das Item ein Material oder ein Trank ist und bereits im Inventar vorhanden ist
		if ( item.IsMaterial || item.IsPotion )
		{
			var existingItem = _backpackItems.FirstOrDefault( i => i != null && i.Name == item.Name && i.Count < i.MaxStack );
			if ( existingItem != null )
			{
				// Berechnen Sie die verbleibende Menge, die in den vorhandenen Stapel passt
				int remainingSpace = existingItem.MaxStack - existingItem.Count;
				if ( item.Count <= remainingSpace )
				{
					// Erhöhen Sie die Menge des vorhandenen Materials oder Tranks
					existingItem.Count += item.Count;
					return;
				}
				else
				{
					// Füllen Sie den vorhandenen Stapel und erstellen Sie ein neues Item für den Rest
					existingItem.Count = existingItem.MaxStack;
					item.Count -= remainingSpace;
					AddItem( item ); // Rekursiver Aufruf, um den Rest hinzuzufügen
					return;
				}
			}
		}

		// Fügen Sie das Item als neues Item hinzu, wenn es kein Material oder Trank ist oder nicht im Inventar vorhanden ist
		var firstFreeSlot = _backpackItems.IndexOf( null );
		if ( firstFreeSlot != -1 )
		{
			_backpackItems[firstFreeSlot] = item;
			item.State = ItemState.Backpack;

			// Deaktivieren der ModelRenderer-Komponenten
			if(item.IsWorld)
			{
				Log.Info( "Item is world item." );
			}
			else
			{
				Log.Info( "Item is not world item." );	
				var modelRenderer = item.GameObject.Components.Get<ModelRenderer>();
				if ( modelRenderer != null )
				{
					modelRenderer.Enabled = false;
				}

				var skinnedModelRenderer = item.GameObject.Components.Get<SkinnedModelRenderer>();
				if ( skinnedModelRenderer != null )
				{
					skinnedModelRenderer.Enabled = false;
				}

				item.GameObject.Enabled = false;
			}
		}
		else
		{
			// Überprüfen, ob das Item gestapelt werden kann, auch wenn das Inventar voll ist
			if ( item.IsMaterial || item.IsPotion )
			{
				var existingItem = _backpackItems.FirstOrDefault( i => i != null && i.Name == item.Name && i.Count < i.MaxStack );
				if ( existingItem != null )
				{
					// Berechnen Sie die verbleibende Menge, die in den vorhandenen Stapel passt
					int remainingSpace = existingItem.MaxStack - existingItem.Count;
					if ( item.Count <= remainingSpace )
					{
						// Erhöhen Sie die Menge des vorhandenen Materials oder Tranks
						existingItem.Count += item.Count;
						return;
					}
					else
					{
						// Füllen Sie den vorhandenen Stapel und erstellen Sie ein neues Item für den Rest
						existingItem.Count = existingItem.MaxStack;
						item.Count -= remainingSpace;
						AddItem( item ); // Rekursiver Aufruf, um den Rest hinzuzufügen
						return;
					}
				}
			}

			Log.Error( "Kein freier Slot im Rucksack." );
			Hudmaster.Instance.ShowNotification( "No Place in the Backpack.", "/ui/hud/inventory.png" );
			Player.Local?.PlaySuccessSoundFromPath( "sounds/upgrade/failing.sound", 0.0125f );
		}
	}


	private void SetOwner( ItemComponent item )
	{
		if ( item.GameObject != null )
		{
			item.GameObject.SetupNetworking();
			item.GameObject.Network.TakeOwnership();

			if ( Player != null && Player.GameObject != null )
			{
				item.GameObject.Parent = Player.GameObject;
				item.GameObject.WorldPosition = Player.GameObject.WorldPosition;
				item.GameObject.WorldRotation = Player.GameObject.WorldRotation;
				item.LastOwner = Player;
				
			}
			else
			{
				
				
			}
		}
		else
		{
			
		}
	}

	/// <summary>
	/// The item is given to the backpack.
	/// </summary>
	public void GiveBackpackItem(ItemComponent item, int index)
	{
		if(IsProxy)
			return;
		// Überprüfen Sie, ob das Item bereits in der Liste ist
		if (_backpackItems.Contains(item))
		{
			
			return;
		}

		// Überprüfen Sie, ob der Index gültig ist
		if (index >= 0 && index < _backpackItems.Count)
		{
			// Überprüfen Sie, ob der Slot im Rucksack leer ist
			if (_backpackItems[index] == null)
			{
				_backpackItems[index] = item;
				item.State = ItemState.Backpack;
				item.GameObject.Enabled = false; // Aktualisieren Sie den Zustand des Items
				
			}
			else
			{
			
			}
		}
	
	}
	public void GiveStorageItem( ItemComponent item, int index )
	{
		if ( _storageItems.Contains( item ) )
		{
			return;
		}
		if ( index >= 0 && index < _storageItems.Count )
		{
			_storageItems[index] = item;
			item.State = ItemState.Storage;
			item.GameObject.Enabled = false;
		}
	}

	

	/// <summary>
	/// The item is removed from the backpack.
	/// </summary>
	public void RemoveBackpackItem( ItemComponent item, int index )
	{
		if ( index >= 0 && index < _backpackItems.Count )
			_backpackItems[index] = null;

		var weaponContainer = Player.Components.Get<WeaponContainer>();
		if ( weaponContainer != null )
		{
			var weapon = weaponContainer.All.FirstOrDefault( w => w.GameObject == item.GameObject );
			if ( weapon != null )
			{
				weapon.Holster();
			}
		}

	}
	public ItemEquipment GetEquippedHandItem()
	{
		return _equippedItems[(int)EquipSlot.Hand] as ItemEquipment;
	}
	/// <summary>
	/// The item is equipped.
	/// </summary>
	public void GiveEquipmentItem( ItemEquipment equipment )
	{
		// Überprüfen Sie, ob der Slot bereits belegt ist
		if ( _equippedItems[(int)equipment.Slot] != null )
		{
			// Wenn ja, entfernen Sie die Statistiken der ausgerüsteten Waffe
			UnequipItemStats( _equippedItems[(int)equipment.Slot] );
		}
		else if ( equipment.IsBackable && _equippedItems[(int)EquipSlot.Back] == null )
		{
			// Wenn der Slot für den Rücken frei ist und die Waffe dort platziert werden kann
			_equippedItems[(int)EquipSlot.Back] = equipment;
		}
		else
		{
			// Rüsten Sie die neue Waffe aus
			_equippedItems[(int)equipment.Slot] = equipment;
		}

		// Entfernen Sie den Gegenstand aus dem Rucksack-Slot, aber nicht aus dem Index
		int index = _backpackItems.IndexOf( equipment );
		if ( index != -1 )
		{
			_backpackItems[index] = null; // Setze den Slot auf null, anstatt ihn zu entfernen
		}

		// Fügen Sie die Statistiken der neuen Waffe hinzu
		EquipItemStats( equipment );
		
		TaskMaster.SubmitTriggerSignal( $"item.equipped.{equipment.Name}", Player );
		
		UpdateBodygroups();
	}

	/// <summary>
	/// The item is unequipped.
	/// </summary>
	private void RemoveEquipmentItem( ItemEquipment equipment )
	{
		// Entfernen Sie die Statistiken der ausgerüsteten Waffe
		

		_equippedItems[(int)equipment.Slot] = null;

		// Finde den ersten freien Slot im Inventar
		int freeSlotIndex = GetFirstFreeBackpackSlot();
		if ( freeSlotIndex != -1 )
		{
			_backpackItems[freeSlotIndex] = equipment;

			if ( equipment.Slot == EquipSlot.Hand )
			{
				var weaponContainer = Player.Components.Get<WeaponContainer>();
				if ( weaponContainer != null )
				{
					weaponContainer.RemoveWeapon( equipment.GameObject, false );
					UnequipItemStats( equipment );

				}
			}
		}
		else
		{
			Hudmaster.Instance.ShowNotification( "no free slot in backpack", "/ui/hud/exit.gif" );
			Player.Local?.PlaySuccessSoundFromPath( "sounds/upgrade/failing.sound", 0.0125f );
		}
	}
	[Broadcast]
	private void UpdateBodygroups()
	{
		var bodygroups = HiddenBodyGroup.None;
		foreach ( var item in EquippedItems )
		{
			if ( item is not ItemEquipment equipment || !equipment.Equipped )
				continue;

			bodygroups |= equipment.HideBodygroups;

		}

		Player.HideBodygroups = bodygroups;
	}

	public int GetTotalItemCount( string name )
	{
		if ( BackpackItems == null ) return 0;
		return BackpackItems.Where( x => x.IsValid() && x.Name.ToLower() == name.ToLower() )?.Count() ?? 0;
	}

	public int GetTotalItemCountWithTag( string tag )
	{
		if ( BackpackItems == null ) return 0;
		return BackpackItems.Where( x => x.IsValid() && x.Tags.Has( tag ) )?.Count() ?? 0;
	}

	
	protected override void OnUpdate()
	{
		if ( Player != null )
			return;

		if(IsProxy)
			return;

		var weaponContainer = Player.Components.Get<WeaponContainer>();
		if ( weaponContainer != null )
		{
			var equippedItems = weaponContainer.GetEquippedItems( new EquipSlot[] { EquipSlot.Hand, EquipSlot.Back } );
			var equipped = weaponContainer.Equipped;
			if ( equipped != null )
			{
				var weapon = weaponContainer.All.FirstOrDefault( w => w.GameObject == equipped.GameObject );
				if ( weapon != null )
				{
					weapon.Deploy();
				}
			}
			var equippedmelee = weaponContainer.GetEquippedItems( new EquipSlot[] { EquipSlot.Hand } ).FirstOrDefault();
			if ( equippedmelee != null )
			{
				equippedmelee.Deploy();
			}

		}
		

		base.OnUpdate();
	}

}
