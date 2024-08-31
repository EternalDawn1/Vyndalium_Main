using GeneralGame;
using GeneralGame.HUD;
using Sandbox;
using System.Collections.Generic;
using System.Linq;
using static Sandbox.GameObjectSystem;

namespace GeneralGame
{
    public class ItemStorage : Component
    {
        [Property] ItemInteractable ItemInteractable { get; set; } = new();
        public bool IsOpened { get; set; }
        public bool IsDoorOpen { get; set; }

        private StorageBox storageBox;
        private List<ItemStorage> StorageInteraction;
        [Property] public int ChestSlotCount { get; set; } = 4;

        [Property]
        public List<GameObject> WeaponPrefabs { get; set; } = new List<GameObject>();
        [Property] public List<ItemComponent> Items { get; set; } = new List<ItemComponent>();

        protected override void OnAwake()
        {
            StorageInteraction ??= new();
            storageBox = new StorageBox();
            InitializeChestSlots();
        }

        public ItemStorage()
        {
            InitializeItemsFromPrefabs();
        }

        public void InitializeChestSlots()
        {
            Random random = new Random();
            for ( int i = 0; i < Items.Count; i++ )
            {
                if ( random.NextDouble() <= 0.3 && WeaponPrefabs.Count > 0 )
                {
                    var validPrefabs = WeaponPrefabs.Where( prefab => prefab != null ).ToList();
                    if ( validPrefabs.Count == 0 )
                    {
                        Log.Warning( "InitializeChestSlots: Keine gültigen WeaponPrefabs vorhanden" );
                        continue;
                    }

                    int prefabIndex = random.Next( validPrefabs.Count );
                    GameObject selectedPrefab = validPrefabs[prefabIndex];
                    ItemComponent itemComponent = CreateItemComponentFromPrefab( selectedPrefab );
                    Items[i] = itemComponent;
                }
            }
        }

        private ItemComponent CreateItemComponentFromPrefab( GameObject prefab )
        {
            if ( prefab == null )
            {
                return null;
            }

            ItemComponent itemComponent = prefab.Components.Get<ItemComponent>();
            if ( itemComponent != null )
            {
                return itemComponent; // Annahme: Es gibt eine Clone-Methode
            }

            return null;
        }

        private void InitializeItemsFromPrefabs()
{
    Random random = new Random();
    
    
    for (int i = 0; i < ChestSlotCount; i++)
    {
        if (WeaponPrefabs.Count > 0)
        {
            int prefabIndex = random.Next(WeaponPrefabs.Count);
            GameObject selectedPrefab = WeaponPrefabs[prefabIndex];
            ItemComponent itemFromPrefab = CreateItemComponentFromPrefab(selectedPrefab);
            if (itemFromPrefab != null)
            {
                Items.Add(itemFromPrefab);
            }
            else
            {
                Log.Warning($"InitializeItemsFromPrefabs: Kein ItemComponent für Prefab an Index {prefabIndex} gefunden.");
            }
        }
        else
        {
            
        }
    }
}
        public bool GiveItemToChest( ItemComponent item )
        {
            Log.Info( $"GiveItemToChest: Item is {(item == null ? "null" : "not null")}" );
            var firstFreeSlot = Items.IndexOf( null );
            if ( firstFreeSlot == -1 )
                return false;

            Items[firstFreeSlot] = item;
            item.State = ItemState.Chest;
            return true;
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

        public void RemoveItem( ItemComponent item )
        {
            Items.Remove( item );
        }

        public void OpenInventory()
        {
            if ( !IsOpened )
            {
                IsOpened = true;

                if ( storageBox == null )
                {
                    storageBox = new StorageBox();
                }
                storageBox.ToggleVisibility();
            }
            else
            {
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
}