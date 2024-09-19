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
        [Property] SkinnedModelRenderer skinnedModelRenderer { get; set; }
        [Property] public List<ItemComponent> Items { get; set; } = new List<ItemComponent>();
        [Property] public List<ItemComponent> items => Items;
        
        private bool itemsGenerated = false;

        protected override void OnAwake()
        {
            itemInteractable = this.Components.Get<ItemInteractable>();
            skinnedModelRenderer = this.Components.Get<SkinnedModelRenderer>();
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
                    if ( nonRandomStatItems.Contains( itemComponent.Prefab ) )
                    {
                        continue; // Überspringen Sie die Generierung zufälliger Statistiken für dieses Item
                    }

                    var tier = (GeneralGame.Tier)GetRandomTier();
                    itemComponent.ItemTier = new ItemComponent.TierClass { Tier = (GeneralGame.Tier)GetRandomTier() };
                    itemComponent.GenerateRandomStats();
                    itemComponent.CalculateSellPrice();
                    //itemComponent.GenerateRandomDMG(tier);
                }
            }
        }
        private int GetRandomTier()
        {
            // Implementieren Sie hier die Logik zur Generierung eines zufälligen Tiers
            Random random = new Random();
            return random.Next( 1, 5 ); // Beispiel: Zufälliger Tier zwischen 1 und 4
        }
        private List<string> nonRandomStatItems = new List<string>
        {
            "prefabs/items/wood_log.prefab",
            // Fügen Sie hier weitere Items hinzu, die keine zufälligen Statistiken erhalten sollen
        };

        private void LoadPrefabs()
        {
            
            var tierCPrefabs = new List<string>
            {
                "prefabs/weapons/aksu/c.prefab",
                "prefabs/weapons/facepunch/usp/uspc.prefab",
                "prefabs/weapons/facepunch/shotgun/shotgunc.prefab",
                "prefabs/weapons/facepunch/mp5/mp5c.prefab",
                "prefabs/weapons/m4a1/m4a1-c.prefab",
                "prefabs/weapons/pm/glock-c.prefab",
                "prefabs/clothes/armor/armor-c.prefab",
                "prefabs/clothes/helmet/helmet-c.prefab",
                "prefabs/clothes/legarmor/legarmor-c.prefab",
                "prefabs/items/wood_log.prefab",
                // Fügen Sie hier weitere C-Tier-Prefab-Dateien hinzu
            };

            var tierBPrefabs = new List<string>
            {
                "prefabs/weapons/aksu/b.prefab",
                "prefabs/weapons/facepunch/usp/uspb.prefab",
                "prefabs/weapons/facepunch/shotgun/shotgunb.prefab",
                "prefabs/weapons/facepunch/mp5/mp5b.prefab",
                "prefabs/weapons/m4a1/m4a1-b.prefab",
                "prefabs/weapons/pm/glock-b.prefab",
                "prefabs/clothes/armor/armor-b.prefab",
                "prefabs/clothes/helmet/helmet-b.prefab",
                "prefabs/clothes/legarmor/legarmor-b.prefab",
                "prefabs/items/wood_log.prefab",
                

                // Fügen Sie hier weitere B-Tier-Prefab-Dateien hinzu
            };

            var tierAPrefabs = new List<string>
            {
                "prefabs/weapons/aksu/a.prefab",
                "prefabs/weapons/facepunch/usp/uspa.prefab",
                "prefabs/weapons/facepunch/shotgun/shotguna.prefab",
                "prefabs/weapons/facepunch/mp5/mp5a.prefab",
                "prefabs/weapons/m4a1/m4a1-a.prefab",
                "prefabs/weapons/pm/glock-a.prefab",
                "prefabs/clothes/armor/armor-a.prefab",
                "prefabs/clothes/helmet/helmet-a.prefab",
                "prefabs/clothes/legarmor/legarmor-a.prefab",
                // Fügen Sie hier weitere A-Tier-Prefab-Dateien hinzu
            };

            var tierSPrefabs = new List<string>
            {
                "prefabs/weapons/aksu/s.prefab",
                "prefabs/weapons/facepunch/usp/usps.prefab",
                "prefabs/weapons/facepunch/shotgun/shotguns.prefab",
                "prefabs/weapons/facepunch/mp5/mp5s.prefab",
                "prefabs/weapons/m4a1/m4a1-s.prefab",
                "prefabs/weapons/pm/glock-s.prefab",
                "prefabs/clothes/armor/armor-s.prefab",
                "prefabs/clothes/helmet/helmet-s.prefab",
                "prefabs/clothes/legarmor/legarmor-s.prefab",
                // Fügen Sie hier weitere S-Tier-Prefab-Dateien hinzu
            };

            var tierSSPrefabs = new List<string>
            {
                "prefabs/weapons/aksu/ss.prefab",
                "prefabs/weapons/facepunch/usp/uspss.prefab",
                "prefabs/weapons/facepunch/shotgun/shotgunss.prefab",
                "prefabs/weapons/facepunch/mp5/mp5ss.prefab",
                "prefabs/weapons/m4a1/m4a1-ss.prefab",
                "prefabs/weapons/pm/glock-ss.prefab",
                "prefabs/clothes/armor/armor-ss.prefab",
                "prefabs/clothes/helmet/helmet-ss.prefab",
                "prefabs/clothes/legarmor/legarmor-ss.prefab",
                // Fügen Sie hier weitere SS-Tier-Prefab-Dateien hinzu
            };

            var tierSSSPrefabs = new List<string>
            {
                "prefabs/weapons/aksu/sss.prefab",
                "prefabs/weapons/facepunch/usp/uspsss.prefab",
                "prefabs/weapons/facepunch/shotgun/shotgunsss.prefab",
                "prefabs/weapons/facepunch/mp5/mp5sss.prefab",
                "prefabs/weapons/m4a1/m4a1-sss.prefab",
                "prefabs/weapons/pm/glock-sss.prefab",
                "prefabs/clothes/armor/armor-sss.prefab",
                "prefabs/clothes/helmet/helmet-sss.prefab",
                "prefabs/clothes/legarmor/legarmor-sss.prefab",
                
                // Fügen Sie hier weitere SSS-Tier-Prefab-Dateien hinzu
            };

            var random = new Random();
            var selectedPrefabs = new List<string>();

            // Wahrscheinlichkeit basierend auf dem Tier der Kiste
            int tierChance = itemInteractable?.Tier switch
            {
                GeneralGame.Tier.SSS => 15,
                GeneralGame.Tier.SS => 9,
                GeneralGame.Tier.S => 8,
                GeneralGame.Tier.A => 3,
                GeneralGame.Tier.B => 2,
                GeneralGame.Tier.C => 1,
                _ => 1
            };

            // Auswahl der Prefabs basierend auf der Wahrscheinlichkeit
            var tiers = new (int chance, List<string> prefabs)[]
            {
                (tierChance, tierSSSPrefabs), // Seltenste Items
                (tierChance + 10, tierSSPrefabs),
                (tierChance + 20, tierSPrefabs),
                (tierChance + 30, tierAPrefabs),
                (tierChance + 40, tierBPrefabs),
                (100, tierCPrefabs) // Häufigste Items
            };

            foreach ( var (chance, prefabs) in tiers )
            {
                if ( random.Next( 100 ) < chance )
                {
                    selectedPrefabs.AddRange( prefabs );
                    break;
                }
            }

            // Zufällige Auswahl der Prefabs aus der ausgewählten Liste
            int weaponCount = DetermineWeaponCount( random, itemInteractable?.Tier );

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
        private int DetermineWeaponCount( Random random, GeneralGame.Tier? tier )
        {
            int chance = random.Next( 100 );
            double tierModifier = tier switch
            {
                GeneralGame.Tier.SSS => 0.25,
                GeneralGame.Tier.SS => 0.5,
                GeneralGame.Tier.S => 0.75,
                GeneralGame.Tier.A => 1.0,
                GeneralGame.Tier.B => 1.25,
                GeneralGame.Tier.C => 1.5,
                _ => 1.5
            };

            if ( chance < 1 * tierModifier )
            {
                return 7;
            }
            else if ( chance < 2 * tierModifier )
            {
                return 6;
            }
            else if ( chance < 3 * tierModifier )
            {
                return 5;
            }
            else if ( chance < 4 * tierModifier )
            {
                return 4;
            }
            else if ( chance < 5 * tierModifier )
            {
                return 3;
            }
            else if ( chance < 6 * tierModifier )
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
                if(skinnedModelRenderer != null)
                {
                    skinnedModelRenderer.Set( "chest_open", true );

                }
                

            }
            else
            {
                CloseInventory();
                
            }
        }

        public void CloseInventory()
        {
            if ( FullScreenManager.Instance != null )
            {
                // Überprüfen, ob die StorageBox noch vorhanden ist
                if ( FullScreenManager.Instance.ActivePanel == FullScreenManager.FullScreenPanel.StorageBox )
                {
                    FullScreenManager.Instance.Display( FullScreenManager.FullScreenPanel.InGameHud );
                }
                else
                {
                    // Animation starten, wenn die StorageBox nicht mehr vorhanden ist
                    if ( skinnedModelRenderer != null )
                    {
                        skinnedModelRenderer.Set( "chest_close", true );
                    }
                }
            }

            if ( Player.Local != null )
            {
                Player.Local.BlockInputs = false;
            }

            if ( skinnedModelRenderer != null )
            {
                skinnedModelRenderer.Set( "chest_open", false );
            }

             // Setzen der Variable, um anzuzeigen, dass die Kiste geschlossen ist
        }

    }
}