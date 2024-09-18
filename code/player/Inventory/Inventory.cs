using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneralGame.Event;
using GeneralGame.HUD;


namespace GeneralGame;

public sealed class Inventory : Component
{
	
	[Property] Player Player { get; set; }

	public const int MAX_BACKPACK_SLOTS = 30;
	public const int MAX_STORAGE_SLOTS = 48;
	private const int ItemsPerPage = 12;
	public const int MAX_UPGRADE_SLOTS = 1;

	[Property]public IReadOnlyList<ItemComponent> BackpackItems => _backpackItems;
	[Property] public IReadOnlyList<ItemComponent> EquippedItems => _equippedItems;
	[Property] public IReadOnlyList<ItemComponent> StorageBoxItems => _storageBoxItems;
	[Property]public IReadOnlyList<ItemComponent> StorageItems => _storageItems;
	[Property] public IReadOnlyList<ItemComponent> UpgradeItems => _upgradeItems;

	[Property] public readonly  List<ItemComponent> _backpackItems;
	[Property] public readonly List<ItemComponent> _equippedItems;
	[Property] public readonly List<ItemComponent> _storageBoxItems;
	[Property] public readonly List<ItemComponent> _storageItems;
	[Property] public readonly List<ItemComponent> _upgradeItems;

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
		

		return false;
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
		Player.Local.AttackValue += item.DMG;
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
		Player.Local.AttackValue -= item.DMG;
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
	public ItemComponent GetNextEquippedWeapon( ItemComponent currentWeapon )
	{
		// Filtern der ausgerüsteten Waffen, die vollständig initialisiert sind (nicht null und haben gültige Werte)
		var equippedWeapons = _equippedItems
			.Where( item => item is ItemEquipment && item != null && item.DMG > 0 && item.HE > 0 )
			.Cast<ItemEquipment>()
			.ToList();

		if ( !equippedWeapons.Any() )
		{
			return null; // Keine gültigen Waffen ausgerüstet
		}

		var currentIndex = equippedWeapons.IndexOf( currentWeapon as ItemEquipment );
		var nextIndex = (currentIndex + 1) % equippedWeapons.Count; // Nächsten Index ermitteln, zyklisch durch die Liste gehen
		return equippedWeapons.ElementAtOrDefault( nextIndex );
	}
	


	public Inventory()
	{
		

		_backpackItems = new List<ItemComponent>( new ItemComponent[MAX_BACKPACK_SLOTS] );
		_storageItems = new List<ItemComponent>( new ItemComponent[MAX_STORAGE_SLOTS] );
		_equippedItems = new List<ItemComponent>( new ItemComponent[Enum.GetNames( typeof( EquipSlot ) ).Length] );
		_storageBoxItems = new List<ItemComponent>();
		_upgradeItems = new List<ItemComponent>( new ItemComponent[MAX_UPGRADE_SLOTS] );
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
		else 
		{
			return _backpackItems.Contains( item ) ? _backpackItems.IndexOf( item ) : _storageBoxItems.IndexOf( item );
		}
		
		
		
	}

	public bool HasSpaceInBackpack()
		=> _backpackItems.IndexOf( null ) != -1;

	public ItemComponent GetItemInSlot( EquipSlot slot ) => _equippedItems.ElementAtOrDefault( (int)slot );
	public bool IsSlotOccupied( EquipSlot slot ) => GetItemInSlot( slot ) is not null;
	

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
	private int FindNextFreeStorageSlotOnPage( int currentPage )
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

		// Wenn kein freier Slot auf der aktuellen Seite gefunden wurde, erweitern wir die Liste um eine neue Seite
		_storageItems.AddRange( new ItemComponent[ItemsPerPage] );
		return _storageItems.Count - ItemsPerPage; // Rückgabe des ersten Slots der neuen Seite
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
				Log.Error( "Kein freier Slot im Upgrade verfügbar." );
			}
		}
	}
	
	public void MoveItemToStorage( ItemComponent item, int currentPage )
	{
		if ( item == null ) return;

		if ( _backpackItems.Contains( item ) )
		{
			int freeSlot = FindNextFreeStorageSlotOnPage( currentPage );
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
				Log.Error( "Kein freier Slot im Storage verfügbar." );
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
				Log.Error( "Kein freier Slot im Rucksack verfügbar." );
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
				Log.Error( "Kein freier Slot im Rucksack verfügbar." );
			}
		}
	}

	public bool GiveItem( PrefabFile prefabFile )
	{
		var obj = SceneUtility.GetPrefabScene( prefabFile ).Clone();
		obj.NetworkMode = NetworkMode.Object;
		obj.NetworkSpawn();

		var res = GiveItem( obj.Components.Get<ItemComponent>() );
		if ( !res )
			obj.Destroy();
		
		return res;
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
			return false;

		GiveEquipmentItem( equipment );
		equipment.State = ItemState.Equipped;
		TaskMaster.SubmitTriggerSignal( $"item.equipped.{item.Name}", Player );
		
		var weaponContainer = Player.Components.Get<WeaponContainer>();
		if ( weaponContainer != null )
		{
			weaponContainer.Give( item.GameObject, true );
		}
		else
		{
			Log.Info( "Item is equipment, skipping Give." );
		}
		

		index = _backpackItems?.IndexOf( item ) ?? -1; // Erneutes Ermitteln des Indexes, falls notwendig

		return true;
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

		SetOwner( item );

		GiveEquipmentItem( equipment );
		
		equipment.State = ItemState.Equipped;
		TaskMaster.SubmitTriggerSignal( $"item.received.{item.Name}", Player );

		var weaponContainer = Player.Components.Get<WeaponContainer>();
		if ( weaponContainer != null )
		{
			weaponContainer.Give( item.GameObject, true );
		}
		else
		{
			Log.Info( "WeaponContainer is null" );
		}

		return true;
	}
	public WeaponContainer Weapons { get; set; }


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
			Log.Error( "Item is not the equipped item in the expected slot." );
			return false;
		}

		var firstFreeSlot = _backpackItems.IndexOf( null );
		if ( firstFreeSlot == -1 )
		{
			Log.Error( "No free slot in the backpack." );
			return false;
		}

		// Entfernen der Statistiken des Items
		RemoveEquipmentItem( equipment );

		// Hinzufügen des Items zum Rucksack

		// Sicherstellen, dass das Item nicht zerstört wird, wenn es unequipped wird
		var weaponContainer = Player.Components.Get<WeaponContainer>();
		if ( weaponContainer != null )
		{
			weaponContainer.RemoveWeapon( item.GameObject, false );
		}
		else
		{
			Log.Info( "WeaponContainer ist null" );
		}

		GiveBackpackItem( equipment, firstFreeSlot );
		equipment.State = ItemState.Backpack;

		TaskMaster.SubmitTriggerSignal( $"item.unequipped.{item.Name}", Player );

		return true;
	}


	public bool DropItem( ItemComponent item )
	{
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

		item.GameObject.Transform.Rotation = Rotation.Identity;
		item.GameObject.Transform.Position = trace.EndPosition;

		var velocity = Player.Velocity + Player.ViewRay.Forward * 150f;
		if ( item.GameObject.Components.TryGet<Rigidbody>( out var rigidbody, FindMode.EverythingInSelf ) )
		{
			rigidbody.Velocity = velocity;
			rigidbody.MotionEnabled = true;
		}
		else if ( item.GameObject.Components.TryGet<ModelPhysics>( out var modelPhysics, FindMode.EverythingInSelf ) )
		{
			item.GameObject.Enabled = false;
			item.GameObject.Transform.Position = trace.EndPosition;
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
		}
		else
		{
			Log.Info( "Item is equipment, skipping Give." );
		}


		return true;
	}
	private void RemoveStorageItem( ItemComponent item, int index )
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
		SetOwner( item );
		GiveBackpackItem( item, index );
		item.State = ItemState.Backpack;
		item.GameObject.Enabled = false;
	}
	public void SetStorageItem( ItemComponent item, int index )
	{
		SetOwner( item );
		GiveStorageItem( item, index );
		item.State = ItemState.Storage;
		item.GameObject.Enabled = false;
	}
	

	public bool RemoveAmountEasy( string name, int count = 1, bool destroy = true )
	{
		foreach ( var item in _backpackItems )
			if ( item.Name.ToLower().Replace( " ", "" ) == name.ToLower().Replace( " ", "" ) )
				return RemoveAmount( item, count, destroy );

		return false;
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

		var firstFreeSlot = _backpackItems.IndexOf( null );
		if ( firstFreeSlot != -1 )
		{
			_backpackItems[firstFreeSlot] = item;
			item.State = ItemState.Backpack;
			item.GameObject.Enabled = false;
			
		}
		else
		{
			Log.Error( "Kein freier Slot im Rucksack." );
			Hudmaster.Instance.ShowNotification( "No Place in the Backpack.", "/ui/hud/inventory.png" );
		}
	}



	private void SetOwner( ItemComponent item )
	{
		if ( item.GameObject != null )
		{
			item.GameObject.SetupNetworking();
			item.GameObject.Network.TakeOwnership();
			item.GameObject.Parent = Player.GameObject;
			item.GameObject.Transform.Position = Player.GameObject.Transform.Position;
			item.GameObject.Transform.Rotation = Player.GameObject.Transform.Rotation;
			item.LastOwner = Player;
			item.GameObject.Enabled = false;
		}
		else
		{
			// Handle the case where GameObject is null or not initialized
		}
	}

	/// <summary>
	/// The item is given to the backpack.
	/// </summary>
	public void GiveBackpackItem(ItemComponent item, int index)
	{
		
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
	private void RemoveBackpackItem( ItemComponent item, int index )
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
		UnequipItemStats( equipment );

		_equippedItems[(int)equipment.Slot] = null;

		// Finde den ersten freien Slot im Inventar
		int freeSlotIndex = GetFirstFreeBackpackSlot();
		if ( freeSlotIndex != -1 )
		{
			_backpackItems[freeSlotIndex] = equipment;
			
		}
		else
		{
			Log.Error( "No free slot in backpack." );
		}
	}
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

	public bool HasItem( string name, int count )
	{
		int totalCount = 0;
		foreach ( var item in _backpackItems )
		{
			if ( item != null && item.Name == name )
			{
				totalCount += item.Count;
				if ( totalCount >= count )
				{
					return true;
				}
			}
		}
		return false;
	}
	protected override void OnUpdate()
	{
		if ( Player != null )
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
		}


		base.OnUpdate();
	}
	



	
	public static void GiveItem( string name )
	{
		var player = Player.Local;
		if ( player == null )
			return;

		var item = PrefabLibrary.FindByComponent<ItemComponent>()
			.FirstOrDefault( x => x.Name.ToLower() == name.ToLower() )
			?.Prefab;

		if ( item == null )
		{
			var itemNames = string.Join( ',', PrefabLibrary.FindByComponent<ItemComponent>().Select( v => v.Name ) );
			Log.Warning( $"couldn't find {name}" );
			Log.Warning( $"valid item names: {itemNames}" );
			return;
		}

		var obj = SceneUtility.GetPrefabScene( item ).Clone();
		obj.NetworkMode = NetworkMode.Object;
		obj.NetworkSpawn();
		player.Inventory.GiveItem( obj );
	}



	
	public static void DebugGiveItem( string name )
	{
		var allItems = PrefabLibrary.FindByComponent<ItemComponent>();
		var foundItem = allItems.Where( itemPrefab =>
		{
			var toFind = name.ToLower().Replace( " ", "" ).Replace( "_", "" ).Replace( ".", "" );
			var item = itemPrefab.GetComponent<ItemComponent>();
			var itemName = item.Get<string>( "Name" ).ToLower().Replace( " ", "" ).Replace( "_", "" ).Replace( ".", "" );
			var objectName = itemPrefab.Name.ToLower().Replace( " ", "" ).Replace( "_", "" ).Replace( ".", "" );

			if ( itemName == toFind || objectName == toFind )
				return true;

			if ( itemName.Contains( toFind, StringComparison.OrdinalIgnoreCase ) || objectName.Contains( toFind, StringComparison.OrdinalIgnoreCase ) )
				return true;

			return false;
		} ).FirstOrDefault();


		if ( foundItem != null )
		{
			var obj = SceneUtility.GetPrefabScene( foundItem.Prefab ).Clone();
			obj.NetworkMode = NetworkMode.Object;
			obj.NetworkSpawn();
			Player.Local.Inventory.GiveItem( obj );
		}
		else
		{
			Log.Info( $"The item was not found, here is a list of available items:" );

			var availableItems = "";

			foreach ( var availableItem in allItems )
				availableItems += $"[{availableItem.GetComponent<ItemComponent>().Get<string>( "Name" )}], ";

			Log.Info( availableItems );
			Log.Info( "You may also use partial item names or any combination of words and letters, I'll try my best to find the item." );
		}
	}

}
