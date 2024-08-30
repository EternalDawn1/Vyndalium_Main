using GeneralGame;
using GeneralGame.HUD;
using Sandbox;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using static Sandbox.GameObjectSystem;
namespace GeneralGame;
public class ItemStorage : Component
{
	[Property] ItemInteractable ItemInteractable { get; set; }
    public bool IsOpened { get;  set; }
	public bool IsDoorOpen { get; set; }

    private StorageBox storageBox;

   private List<ItemStorage> StorageInteraction;
   [Property] public int ChestSlotCount { get; set; } = 10;

	[Property]
	public List<GameObject> WeaponPrefabs { get; set; } = new List<GameObject>();
	[Property]public List<ItemComponent> Items { get; set; } = new List<ItemComponent>();

	public ItemStorage()
	{
		Items = new List<ItemComponent>();
		for ( int i = 0; i < ChestSlotCount; i++ )
		{
			Items.Add( null );
		}
	}


	// Entfernen eines Items


	// Initialisieren der Liste mit einer bestimmten Anzahl von Slots (leeren Items)
	public void InitializeChestSlots()
	{
		Random random = new Random();

		for ( int i = 0; i < ChestSlotCount; i++ )
		{
			if ( random.NextDouble() <= 0.3 && WeaponPrefabs.Count > 0 )
			{
				int prefabIndex = random.Next( WeaponPrefabs.Count );
				GameObject weaponPrefab = WeaponPrefabs[prefabIndex];
				ItemComponent itemComponent = CreateItemComponentFromPrefab( weaponPrefab );

				if ( itemComponent != null )
				{
					Items[i] =  itemComponent ; // Füge das Item nur hinzu, wenn es nicht null ist
					Log.Info( $"Item {itemComponent.Name} in Slot {i} hinzugefügt." );
					GiveItemToChest( itemComponent );
				}
				else
				{
					Log.Warning( $"ItemComponent für Prefab {weaponPrefab} ist null." );
				}
			}
			else
			{
				Items[i] = null;// Füge null nicht hinzu, wenn keine Bedingung erfüllt ist
				Log.Info( $"Kein Item in Slot {i} hinzugefügt." );
			}
		}
	}

	private ItemComponent CreateItemComponentFromPrefab( GameObject prefab )
	{
		if ( prefab == null )
		{
			Log.Warning( "CreateItemComponentFromPrefab: Prefab is null" );
			return null;
		}

		ItemComponent itemComponent = prefab.Components.Get<ItemComponent>();
		if ( itemComponent == null )
		{
			itemComponent = prefab.Components.Create<ItemComponent>();
		}

		return itemComponent;
	}
	// Methode zum Hinzufügen eines Items zur Chest
	public bool GiveItemToChest( ItemComponent item )
	{
		Log.Info( $"GiveItemToChest: Item is {(item == null ? "null" : "not null")}" );
		var firstFreeSlot = Items.IndexOf( null );
		if ( firstFreeSlot == -1 )
			return false;

		AddItem( item );
		//AddToChestInventory( item, firstFreeSlot );
		item.State = ItemState.Chest;
		return true;
	}

	// Methode zum Hinzufügen eines Items zum Chest-Inventar
	public void AddToChestInventory( ItemComponent item, int index )
	{
		Log.Info( $"AddToChestInventory: Checking if item {item} is already in chest." );

		if ( Items.Contains( item ) )
		{
			Log.Warning( $"AddToChestInventory: Item {item} already in chest." );
			return;
		}

		if ( index >= 0 && index < Items.Count )
		{
			Items[index] = item;
			Log.Info( $"AddToChestInventory: Added item {item} to slot {index}." );
		}
		else
		{
			Log.Warning( $"AddToChestInventory: Invalid index {index}." );
		}
	}

	public void AddItem( ItemComponent item )
	{
		for ( int i = 0; i < Items.Count; i++ )
		{
			if ( Items[i] == null )
			{
				Items[i] = item;
				Log.Info( $"Item {item} in Slot {i} hinzugefügt." );
				return;
			}
		}
		Log.Warning( "Kein freier Slot verfügbar." );
	}

	protected override void OnAwake()
    {
        
        StorageInteraction ??= new();
        storageBox = new StorageBox();
		InitializeChestSlots();
		
	}

	private DateTime lastOpenedTime;

	public void OpenInventory()
	{
		if ( !IsOpened )
		{
			
			IsOpened = true;
			lastOpenedTime = DateTime.Now;

			if ( storageBox == null )
			{
				storageBox = new StorageBox();
			}
			storageBox.ToggleVisibility();

			
			IsOpened = true;
		}
		else
		{
			// Schließe das Inventar, wenn es bereits geöffnet ist
			CloseInventory();
		}
	}

	public void CloseInventory()
	{
		if ( IsOpened )
		{
			
			IsOpened = false;
			if ( storageBox != null )
			{
				storageBox.ToggleVisibility();
			}
			IsOpened = false;
		}
	}
	public void ResetStorage()
	{
		IsOpened = false;

		if ( storageBox != null )
		{
			storageBox.ResetVisibility();
		}
	}
	public void ToggleInventory()
	{
		if ( IsOpened )
		{
			CloseInventory();
		}
		else
		{
			OpenInventory();
		}
	}
	public void ToggleDoorState()
	{
		IsDoorOpen = !IsDoorOpen;

		if ( ItemInteractable != null )
		{
			var components = ItemInteractable.Components;
			if ( components != null )
			{
				HingeJoint hingeJoint = components.Get<HingeJoint>();
				if ( hingeJoint != null )
				{
					hingeJoint.MinAngle = hingeJoint.MinAngle == 0 ? -90 : 0;
				}
				else
				{
					Log.Error( "HingeJoint is null." );
				}
			}
			else
			{
				Log.Error( "Components are null." );
			}
		}
		else
		{
			Log.Error( "ItemInteractable is null." );
		}
	}
}
