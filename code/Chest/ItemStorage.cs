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
        public bool IsOpened { get; set; }
        public bool IsDoorOpen { get; set; }

        private StorageBox storageBox { get; set; }
        [Property] ItemInteractable itemInteractable { get; set; }
        [Property] public List<ItemComponent> Items { get; set; } = new List<ItemComponent>();
        [Property] public List<ItemComponent> items => Items;
       
        protected override void OnAwake()
        {
            itemInteractable = this.Components.Get<ItemInteractable>();
            base.OnAwake();
            if ( storageBox == null )
            {
                storageBox = new StorageBox();
            }
        }
        public ItemStorage()
        {
            LoadPrefabs();
            GenerateRandomStatsForItems();
        }
        private void GenerateRandomStatsForItems()
        {
            foreach ( var itemComponent in Items )
            {
                if ( itemComponent != null )
                {
                    itemComponent.ItemTier = new ItemComponent.TierClass { Tier = (GeneralGame.Tier)GetRandomTier() };
                    itemComponent.GenerateRandomStats();
                    itemComponent.CalculateSellPrice();
                }
            }
        }
        private int GetRandomTier()
        {
            // Implementieren Sie hier die Logik zur Generierung eines zufälligen Tiers
            Random random = new Random();
            return random.Next( 1, 5 ); // Beispiel: Zufälliger Tier zwischen 1 und 4
        }


        private void LoadPrefabs()
        {
            var prefabFiles = new List<string>
            {
                "prefabs/weapons/aksu/a.prefab",
                "prefabs/weapons/aksu/s.prefab",
                "prefabs/weapons/aksu/c.prefab",
                // Fügen Sie hier weitere Prefab-Dateien hinzu
            };

            foreach ( var prefabPath in prefabFiles )
            {
                var prefab = ResourceLibrary.Get<PrefabFile>( prefabPath );
                if ( prefab != null )
                {
                    var itemComponent = ConvertPrefabToItemComponent( prefab );
                    if ( itemComponent != null )
                    {
                        Items.Add( itemComponent );
                    }
                }
            }
        }

        private ItemComponent ConvertPrefabToItemComponent( PrefabFile prefab )
        {
            var obj = SceneUtility.GetPrefabScene( prefab ).Clone();
            obj.NetworkMode = NetworkMode.Object;
            obj.NetworkSpawn();

            var itemComponent = obj.Components.Get<ItemComponent>();
            if ( itemComponent == null )
            {
                obj.Destroy();
                return null;
            }

            return itemComponent;
        }
        public void AddItem( ItemComponent item, int index )
        {
            if ( item == null )
            {
                return;
            }

            Items.Insert( index, item );
        }

        public void OpenInventory()
        {
            if ( !IsOpened )
            {
                IsOpened = true;

                
                storageBox.ToggleVisibility();
                Player.Local.BlockInputs = true;


                storageBox.StateHasChanged();
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
                    Player.Local.BlockInputs = false;
                }
            }
        }
    }
}