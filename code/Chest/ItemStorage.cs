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
        private bool itemsGenerated = false;

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
            Items = new List<ItemComponent>();
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
            var tierCPrefabs = new List<string>
            {
                "prefabs/weapons/aksu/c.prefab",
                "prefabs/weapons/facepunch/usp/uspc.prefab",
                "prefabs/weapons/facepunch/shotgun/shotgunc.prefab",
                "prefabs/weapons/facepunch/ak47/mp5c.prefab",
                "prefabs/weapons/m4a1/m4a1-c.prefab",
                "prefabs/weapons/facepunch/pm/glock-c.prefab",
                // Fügen Sie hier weitere C-Tier-Prefab-Dateien hinzu
            };

            var tierBPrefabs = new List<string>
            {
                "prefabs/weapons/aksu/b.prefab",
                "prefabs/weapons/facepunch/usp/uspb.prefab",
                "prefabs/weapons/facepunch/shotgun/shotgunb.prefab",
                "prefabs/weapons/facepunch/ak47/mp5b.prefab",
                "prefabs/weapons/m4a1/m4a1-b.prefab",
                "prefabs/weapons/facepunch/pm/glock-b.prefab",
                

                // Fügen Sie hier weitere B-Tier-Prefab-Dateien hinzu
            };

            var tierAPrefabs = new List<string>
            {
                "prefabs/weapons/aksu/a.prefab",
                "prefabs/weapons/facepunch/usp/uspa.prefab",
                "prefabs/weapons/facepunch/shotgun/shotguna.prefab",
                "prefabs/weapons/facepunch/ak47/mp5a.prefab",
                "prefabs/weapons/m4a1/m4a1-a.prefab",
                "prefabs/weapons/facepunch/pm/glock-a.prefab",
                // Fügen Sie hier weitere A-Tier-Prefab-Dateien hinzu
            };

            var tierSPrefabs = new List<string>
            {
                "prefabs/weapons/aksu/s.prefab",
                "prefabs/weapons/facepunch/usp/usps.prefab",
                "prefabs/weapons/facepunch/shotgun/shotguns.prefab",
                "prefabs/weapons/facepunch/ak47/mp5s.prefab",
                "prefabs/weapons/m4a1/m4a1-s.prefab",
                "prefabs/weapons/facepunch/pm/glock-s.prefab",
                // Fügen Sie hier weitere S-Tier-Prefab-Dateien hinzu
            };

            var tierSSPrefabs = new List<string>
            {
                "prefabs/weapons/aksu/ss.prefab",
                "prefabs/weapons/facepunch/usp/uspss.prefab",
                "prefabs/weapons/facepunch/shotgun/shotgunss.prefab",
                "prefabs/weapons/facepunch/ak47/mp5ss.prefab",
                "prefabs/weapons/m4a1/m4a1-ss.prefab",
                "prefabs/weapons/facepunch/pm/glock-ss.prefab",
                // Fügen Sie hier weitere SS-Tier-Prefab-Dateien hinzu
            };

            var tierSSSPrefabs = new List<string>
            {
                "prefabs/weapons/aksu/sss.prefab",
                "prefabs/weapons/facepunch/usp/uspsss.prefab",
                "prefabs/weapons/facepunch/shotgun/shotgunsss.prefab",
                "prefabs/weapons/facepunch/ak47/mp5sss.prefab",
                "prefabs/weapons/m4a1/m4a1-sss.prefab",
                "prefabs/weapons/facepunch/pm/glock-sss.prefab",
                // Fügen Sie hier weitere SSS-Tier-Prefab-Dateien hinzu
            };

            var random = new Random();
            var selectedPrefabs = new List<string>();

            // Wahrscheinlichkeit basierend auf dem Tier der Kiste
            int tierChance = itemInteractable?.Tier switch
            {
                GeneralGame.Tier.SSS => 10,
                GeneralGame.Tier.SS => 9,
                GeneralGame.Tier.S => 8,
                GeneralGame.Tier.A => 7,
                GeneralGame.Tier.B => 6,
                GeneralGame.Tier.C => 5,
                _ => 5
            };

            // Auswahl der Prefabs basierend auf der Wahrscheinlichkeit
            if ( random.Next( 100 ) < tierChance )
            {
                selectedPrefabs.AddRange( tierSSSPrefabs );
            }
            else if ( random.Next( 100 ) < tierChance + 10 )
            {
                selectedPrefabs.AddRange( tierSSPrefabs );
            }
            else if ( random.Next( 100 ) < tierChance + 20 )
            {
                selectedPrefabs.AddRange( tierSPrefabs );
            }
            else if ( random.Next( 100 ) < tierChance + 30 )
            {
                selectedPrefabs.AddRange( tierAPrefabs );
            }
            else if ( random.Next( 100 ) < tierChance + 40 )
            {
                selectedPrefabs.AddRange( tierBPrefabs );
            }
            else
            {
                selectedPrefabs.AddRange( tierCPrefabs );
            }

            // Zufällige Auswahl der Prefabs aus der ausgewählten Liste
            int weaponCount = DetermineWeaponCount( random );

            // Zufällige Auswahl der Prefabs aus der ausgewählten Liste
            var finalPrefabs = selectedPrefabs.OrderBy( x => random.Next() ).Take( weaponCount ).ToList();

            foreach ( var prefabPath in finalPrefabs )
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
        private int DetermineWeaponCount( Random random )
        {
            int chance = random.Next( 100 );
            if ( chance < 1 )
            {
                return 7;
            }
            else if ( chance < 6 )
            {
                return 6;
            }
            else if ( chance < 16 )
            {
                return 5;
            }
            else if ( chance < 31 )
            {
                return 4;
            }
            else if ( chance < 51 )
            {
                return 3;
            }
            else if ( chance < 76 )
            {
                return 2;
            }
            else
            {
                return 1;
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
            obj.Enabled = false;
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

                if ( !itemsGenerated ) // Überprüfen, ob die Objekte bereits erstellt wurden
                {
                    LoadPrefabs();
                    GenerateRandomStatsForItems();
                    itemsGenerated = true; // Setzen der Variable, um anzuzeigen, dass die Objekte erstellt wurden
                }

                FullScreenManager.Instance.Display( FullScreenManager.FullScreenPanel.StorageBox );
                Player.Local.BlockInputs = true;
            }
            else
            {
                CloseInventory();
            }
        }

        public void CloseInventory()
        {
            FullScreenManager.Instance.Display( FullScreenManager.FullScreenPanel.InGameHud );
            Player.Local.BlockInputs = false;
           



        }
        
    }
}