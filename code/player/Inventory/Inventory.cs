using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneralGame.Event;


namespace GeneralGame;

public sealed class Inventory : Component
{
	protected override void OnStart()
	{
		if ( IsProxy ) return;
		Player = Scene.GetAllComponents<Player>().FirstOrDefault( x => !x.IsProxy );
	}
	[Property] Player Player { get; set; }

	public const int MAX_BACKPACK_SLOTS = 20;

	public IReadOnlyList<ItemComponent> BackpackItems => _backpackItems;
	public IReadOnlyList<ItemComponent> EquippedItems => _equippedItems;

	private readonly List<ItemComponent> _backpackItems;
	private readonly List<ItemComponent> _equippedItems;

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
		_equippedItems = new List<ItemComponent>( new ItemComponent[Enum.GetNames( typeof( EquipSlot ) ).Length] );
	}

	public int IndexOf( ItemComponent item )
	{
		if ( item == null )
		{
			return -1;
		}
		return (item is ItemEquipment equipment && equipment.Equipped ? _equippedItems : _backpackItems).IndexOf( item );
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
		TaskMaster.SubmitTriggerSignal( $"item.received.{item.Name}", Player );

		return true;
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
		{
			Log.Error( "ItemComponent ist null." );
			return false;
		}

		if ( _backpackItems == null )
		{
			Log.Error( "Backpack items list is null." );
			return false;
		}

		var index = _backpackItems.IndexOf( item );
		if ( index == -1 )
			return false;

		if ( item is not ItemEquipment equipment )
			return false;

		if ( _equippedItems == null )
		{
			Log.Error( "Equipped items list is null." );
			return false;
		}

		var slotIndex = equipment.IsBackable ? (int)EquipSlot.Back : (int)equipment.Slot;
		var previouslyEquippedItem = _equippedItems[slotIndex];

		if ( previouslyEquippedItem == item )
		{
			return true; // Das Item ist bereits ausgerüstet
		}

		if ( previouslyEquippedItem != null )
		{
			RemoveEquipmentItem( previouslyEquippedItem as ItemEquipment );
			// Hier wird der Index des zuvor ausgerüsteten Items übergeben
			GiveBackpackItem( previouslyEquippedItem, index ); // Angenommen, der Index ist hier relevant
			previouslyEquippedItem.State = ItemState.Backpack;
		}

		GiveEquipmentItem( equipment );
		equipment.State = ItemState.Equipped;

		index = _backpackItems.IndexOf( item ); // Erneutes Ermitteln des Indexes, falls notwendig
		if ( index != -1 )
		{
			_backpackItems.RemoveAt( index );
		}

		if ( Player == null )
		{
			Log.Error( "Player is null." );
			return false;
		}

		var weaponContainer = Player.Components?.Get<WeaponContainer>();
		if ( weaponContainer != null )
		{
			weaponContainer.Give( item.GameObject, true );
		}
		else
		{
			Log.Error( "WeaponContainer is null." );
		}

		return true;
	}


	public bool EquipItemFromWorld( ItemComponent item, bool forceReplace = false )
	{
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
		if ( item is ItemEquipment equipment && equipment.Equipped )
		{
			if ( item == null )
			{
				
				return false;
			}
			var slotIndex = (int)equipment.Slot;
			var equippedItem = _equippedItems[slotIndex];
			if ( equippedItem != item ) // Check if the item is the one equipped
				return false;

			var firstFreeSlot = _backpackItems.IndexOf( null );
			if ( firstFreeSlot == -1 )
				return false;

			
			RemoveEquipmentItem( equipment );
			GiveBackpackItem( equipment, firstFreeSlot );
			equipment.State = ItemState.Backpack;
			TaskMaster.SubmitTriggerSignal( $"item.unequipped.{item.Name}", Player );

			var weaponContainer = Player.Components.Get<WeaponContainer>();
			if ( weaponContainer != null )
			{
				weaponContainer.RemoveWeapon( item.GameObject, false );
				
			}

			
		}
		return true;
	}


	public bool DropItem( ItemComponent item )
	{
		if ( item is ItemEquipment equipment && equipment.Equipped )
			RemoveEquipmentItem( equipment );
		else
			RemoveBackpackItem( item, _backpackItems.IndexOf( item ) );

		item.State = ItemState.None;
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


		return true;
	}
	private bool CanStack( ItemComponent first, ItemComponent second )
		=> first.Prefab == second.Prefab
		&& first.IsStackable && second.IsStackable
		&& second.Count < second.MaxStack;

	public bool SwapItems( int firstIndex, int secondIndex )
	{
		var firstItem = _backpackItems.ElementAtOrDefault( firstIndex );
		if ( firstItem is null )
			return false;

		RemoveBackpackItem( firstItem, firstIndex );

		var secondItem = _backpackItems.ElementAtOrDefault( secondIndex );
		var invert = false;

		if ( secondItem is not null )
		{
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
					GiveBackpackItem( to, secondIndex );
					return true;
				}
			}

			GiveBackpackItem( secondItem, invert ? secondIndex : firstIndex );
		}

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



		return true;
	}
	public void SetItem( ItemComponent item, int index )
	{
		SetOwner( item );
		GiveBackpackItem( item, index );
		item.State = ItemState.Backpack;
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
		}
		else
		{
			// Handle the case where GameObject is null or not initialized
		}
	}

	/// <summary>
	/// The item is given to the backpack.
	/// </summary>
	private void GiveBackpackItem( ItemComponent item, int index )
	{
		// Überprüfen Sie, ob das Item bereits in der Liste ist
		if ( _backpackItems.Contains( item ) )
			return;

		if ( index >= 0 && index < _backpackItems.Count )
			_backpackItems[index] = item;
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

		// Entfernen Sie den Gegenstand aus dem Rucksack
		int index = _backpackItems.IndexOf( equipment );
		if ( index != -1 )
		{
			_backpackItems.RemoveAt( index );
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

		// Fügen Sie den Gegenstand zum Rucksack hinzu
		_backpackItems.Add( equipment );

		UpdateBodygroups();
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

	public bool HasItem( string name )
	{
		return BackpackItems.Any( x => x.Name == name );
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



	[ConCmd]
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



	[ConCmd( "newgame_item_give" )]
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
