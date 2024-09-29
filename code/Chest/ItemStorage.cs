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
        public bool IsZombieSpawner { get; set; }
        public int Level { get; set; }

        private Dictionary<string, (int MinAttack, int MaxAttack)> tierAttackValues = new Dictionary<string, (int MinAttack, int MaxAttack)>
        {
            { "C", (27, 34) },
            { "B", (62, 69) },
            { "A", (107, 114) },
            { "S", (192, 199) },
            { "SS", (207, 214) },
            { "SSS", (232, 239) }
        };

        private Dictionary<string, (int MinArmor, int MaxArmor)> tierArmorValues = new Dictionary<string, (int MinArmor, int MaxArmor)>
        {
            { "C", (27, 34) },
            { "B", (62, 69) },
            { "A", (107, 114) },
            { "S", (192, 199) },
            { "SS", (207, 214) },
            { "SSS", (232, 239) }
        };

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


                    //itemComponent.GenerateRandomStats();
                   
                    itemComponent.CalculateSellPrice();
                    //itemComponent.GenerateRandomDMG(tier);
                }
            }
        }
       
        private List<string> nonRandomStatItems = new List<string>
        {
            "prefabs/items/wood_log.prefab",
            "prefabs/potions/potion_small.prefab",
            "prefabs/potions/potion_mid.prefab",
            "prefabs/potions/potion_big.prefab",
            // Fügen Sie hier weitere Items hinzu, die keine zufälligen Statistiken erhalten sollen
        };

      

        private List<string> tierCPrefabs = new List<string>
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
            "prefabs/weapons/new/hands.prefab",
            "prefabs/weapons/new/knife.prefab",
            "prefabs/weapons/new/machete.prefab",
            // Fügen Sie hier weitere C-Tier-Prefab-Dateien hinzu
        };

        private List<string> tierBPrefabs = new List<string>
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

        private List<string> tierAPrefabs = new List<string>
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

        private List<string> tierSPrefabs = new List<string>
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

        private List<string> tierSSPrefabs = new List<string>
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

        private List<string> tierSSSPrefabs = new List<string>
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
        private void LoadPrefabs()
        {
            
            int minLevel = 0;
            int maxLevel = 100;
            int playerLevel = GetPlayerLevel(); // Spielerlevel ermitteln
            LoadRandomTierPrefabs( playerLevel, minLevel, maxLevel );

           
        }

        public void LoadRandomTierPrefabs( int playerLevel, int minLevel, int maxLevel )
        {
            var random = new Random();
            var tierPrefabs = new List<(List<string> prefabs, string tier, double probability)>
        {
            (tierCPrefabs, "C", 0.80),
            (tierBPrefabs, "B", 0.10),
            (tierAPrefabs, "A", 0.05),
            (tierSPrefabs, "S", 0.025),
            (tierSSPrefabs, "SS", 0.015),
            (tierSSSPrefabs, "SSS", 0.01)
        };

            int itemsToSpawn;

            // Wahrscheinlichkeit für die Anzahl der zu spawnenden Items
            int chance = random.Next( 100 ); // Verwenden Sie 100, um Dezimalstellen zu ermöglichen
            if ( chance < 70 ) // 70%
            {
                itemsToSpawn = 2;
            }
            else if ( chance < 80 ) // 10%
            {
                itemsToSpawn = 3;
            }
            else if ( chance < 85 ) // 5%
            {
                itemsToSpawn = 4;
            }
            else if ( chance < 87 ) // 2%
            {
                itemsToSpawn = 5;
            }
            else if ( chance < 89 ) // 2%
            {
                itemsToSpawn = 6;
            }
            else if ( chance < 91 ) // 2%
            {
                itemsToSpawn = 7;
            }
            else if ( chance < 93 ) // 2%
            {
                itemsToSpawn = 8;
            }
            else if ( chance < 95 ) // 2%
            {
                itemsToSpawn = 9;
            }
            else // Rest (5%)
            {
                itemsToSpawn = 1;
            }

            var selectedPrefabs = new List<(string prefab, string tier)>();

            for ( int i = 0; i < itemsToSpawn; i++ )
            {
                double roll = random.NextDouble();
                double cumulative = 0.0;

                foreach ( var (prefabs, tier, probability) in tierPrefabs )
                {
                    cumulative += probability;
                    if ( roll < cumulative )
                    {
                        var selectedPrefab = prefabs[random.Next( prefabs.Count )];
                        selectedPrefabs.Add( (selectedPrefab, tier) );
                        break;
                    }
                }
            }

            foreach ( var (prefabPath, tier) in selectedPrefabs )
            {
                LoadTierPrefab( prefabPath, tier, minLevel, maxLevel);
            }
        }
        

        public void LoadTierPrefab( string prefabPath, string tier, int minLevel, int maxLevel)
        {
            int playerLevel = GetPlayerLevel();
            var prefab = ResourceLibrary.Get<PrefabFile>( prefabPath );
            if ( prefab != null )
            {
                var itemComponent = ConvertPrefabToItemComponent( prefab );
                if ( itemComponent != null )
                {
                    var requiredLevel = DetermineRequiredLevelForTier( tier );
                    if ( requiredLevel >= minLevel && requiredLevel <= maxLevel )
                    {
                        itemComponent.GameObject.Enabled = false;
                        if ( !nonRandomStatItems.Contains( prefabPath ) )
                        {
                            itemComponent.RequiredLevel = requiredLevel;

                            if ( itemComponent.IsWeapon )
                            {
                                var attackValues = CalculateAttackValues( tier, itemComponent);
                                itemComponent.MinAttackValue = attackValues.MinAttack;
                                itemComponent.MaxAttackValue = attackValues.MaxAttack;
                                itemComponent.Tier = Enum.Parse<Tier>( tier );
                                itemComponent.GenerateRandomStats();
                            }
                            else if ( itemComponent.IsArmor )
                            {
                                var armorValues = CalculateArmorValues( tier, Level, itemComponent, playerLevel );
                                itemComponent.MinArmorValue = armorValues.MinArmor;
                                itemComponent.MaxArmorValue = armorValues.MaxArmor;
                                itemComponent.GenerateRandomStats();
                                itemComponent.Tier = Enum.Parse<Tier>( tier );
                            }
                        }
                        Items.Add( itemComponent );
                    }
                }
            }
        }
        
        private int GetPlayerLevel()
        {
            Task.Delay(1);
            if ( Player.Local != null )
            {
                return Player.Local.Level;
            }
            else
            {
                // Fallback-Wert, wenn Player.Local null ist
                return Player.Local.Level; // Beispielwert, kann angepasst werden
            }

        }
        public virtual int DetermineRequiredLevelForTier( string tier )
        {
            int playerLevel = GetPlayerLevel();
            var random = new Random();
            int baseLevel = tier switch
            {
                "C" => 1,
                "B" => 1,
                "A" => 1,
                "S" => 1,
                "SS" => 1,
                "SSS" => 1,
                _ => 0
            };

            int minRequiredLevel = Math.Max( baseLevel, playerLevel );
            int maxRequiredLevel = Math.Min( baseLevel + 15, 100 ); // Maximallevel auf 60 begrenzen

            int requiredLevel;

            if ( playerLevel < 10 )
            {
                // Spielerlevel unter 10: zufälliges Level zwischen 0 und maxRequiredLevel
                requiredLevel = random.Next( 0, maxRequiredLevel + 1 );
            }
            else
            {
                // Spielerlevel 10 oder höher: Wahrscheinlichkeitsbasierte Berechnung
                int chance = random.Next( 100 );

                if ( chance < 50 ) // 50% Chance auf Level innerhalb von 5 Leveln tiefer oder 15 Leveln höher
                {
                    int lowerBound = Math.Max( playerLevel - 5, 0 );
                    int upperBound = Math.Min( playerLevel + 15, 60 );
                    requiredLevel = random.Next( lowerBound, upperBound + 1 );
                }
                else if ( chance < 80 ) // 30% Chance auf Level innerhalb von 1-3 Leveln höher oder tiefer
                {
                    int lowerBound = Math.Max( playerLevel - 3, 0 );
                    int upperBound = Math.Min( playerLevel + 3, 60 );
                    requiredLevel = random.Next( lowerBound, upperBound + 1 );
                }
                else // 20% Chance auf Level innerhalb von 80% des Spielerlevels
                {
                    int lowerBound = Math.Max( (int)(playerLevel * 0.8), 0 );
                    int upperBound = Math.Min( (int)(playerLevel * 1.2), 60 );
                    requiredLevel = random.Next( lowerBound, upperBound + 1 );
                }
            }

            return requiredLevel;
        }

        public( int MinAttack, int MaxAttack ) CalculateAttackValues( string tier, ItemComponent itemComponent )
        {
            int level = Player.Local.Level;
           
            var baseValues = tierAttackValues[tier];
            double attackIncreasePerLevel = 16 * 0.3;

            int minAttack = baseValues.MinAttack + (int)(attackIncreasePerLevel * level);
            int maxAttack = baseValues.MaxAttack + (int)(attackIncreasePerLevel * level);

            // Zusätzliche Werte basierend auf dem Waffentyp und dem erforderlichen Level
            int requiredLevel = DetermineRequiredLevelForTier( tier);

            // Skalierung der Angriffswerte basierend auf dem Level der Waffe und dem erforderlichen Level
            double tierMultiplier = 1.0;
            switch ( tier )
            {
                case "SSS":
                    tierMultiplier = 1 + (8.0 * level / 100);
                    break;
                case "SS":
                    tierMultiplier = 1 + (4.0 * level / 100);
                    break;
                case "S":
                    tierMultiplier = 1 + (3.0 * level / 100);
                    break;
                case "A":
                    tierMultiplier = 1 + (2.0 * level / 100);
                    break;
                case "B":
                    tierMultiplier = 1 + (1.0 * level / 100);
                    break;
                case "C":
                    tierMultiplier = 1; // Kein Multiplikator für C-Tier
                    break;
            }

            minAttack = (int)(minAttack * tierMultiplier);
            maxAttack = (int)(maxAttack * tierMultiplier);

            minAttack += (requiredLevel ) + (level );
            maxAttack += (requiredLevel ) + (level );

            // Zufallsfaktor hinzufügen
            var random = new Random();
            int randomFactor = random.Next( -5, 6 ); // Zufallswert zwischen -5 und 5

            minAttack += randomFactor;
            maxAttack += randomFactor;

            UpdateDescription( itemComponent );

            return (minAttack, maxAttack);
        }
        public static void UpdateDescription( ItemComponent item )
        {
            string randomName;
            if ( item.IsWeapon )
            {
                randomName = NameGenerator.GenerateRandomName( item.Tier, true );
            }
            else if ( item.IsArmor )
            {
                randomName = NameGenerator.GenerateRandomName( item.Tier, false );
            }
            else
            {
                randomName = "Unknown Item"; // Fallback für den Fall, dass weder Waffe noch Rüstung
            }

            string color = item.Tier switch
            {
                Tier.SSS => "gold",
                Tier.SS => "purple",
                Tier.S => "blue",
                Tier.A => "green",
                Tier.B => "white",
                Tier.C => "gray",
                _ => "white"
            };

            item.Description = randomName;
        }

        public (int MinArmor, int MaxArmor) CalculateArmorValues( string tier, int level, ItemComponent itemComponent, int playerLevel )
        {
            var baseValues = tierArmorValues[tier];
            var levelBonus = (level / 5) * (baseValues.MinArmor / 2);

            int minArmor = baseValues.MinArmor + levelBonus;
            int maxArmor = baseValues.MaxArmor + levelBonus;

            // Zusätzliche Werte basierend auf dem Rüstungstyp und dem erforderlichen Level
            int requiredLevel = DetermineRequiredLevelForTier( tier);
            if ( requiredLevel > 45 )
            {
                requiredLevel = 90;
            }

            // Skalierung der Rüstungswerte basierend auf dem Level der Rüstung und dem erforderlichen Level
            double tierMultiplier = 1.0;
            switch ( tier )
            {
                case "SSS":
                    tierMultiplier = 1 + (8.0 * level / 100);
                    break;
                case "SS":
                    tierMultiplier = 1 + (4.0 * level / 100);
                    break;
                case "S":
                    tierMultiplier = 1 + (3.0 * level / 100);
                    break;
                case "A":
                    tierMultiplier = 1 + (2.0 * level / 100);
                    break;
                case "B":
                    tierMultiplier = 1 + (1.0 * level / 100);
                    break;
                case "C":
                    tierMultiplier = 1; // Kein Multiplikator für C-Tier
                    break;
            }

            minArmor = (int)(minArmor * tierMultiplier);
            maxArmor = (int)(maxArmor * tierMultiplier);

            minArmor += (requiredLevel / 2) + (level / 2);
            maxArmor += (requiredLevel / 2) + (level / 2);

            // Zufallsfaktor hinzufügen
            var random = new Random();
            int randomFactor = random.Next( -5, 6 ); // Zufallswert zwischen -5 und 5

            minArmor += randomFactor;
            maxArmor += randomFactor;

            UpdateDescription( itemComponent );

            return (minArmor, maxArmor);
        }




        public ItemComponent ConvertPrefabToItemComponent( PrefabFile prefab )
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
                  
               
                    itemsGenerated = true; // Setzen der Variable, um anzuzeigen, dass die Objekte erstellt wurden
                }

                FullScreenManager.Instance.Display( FullScreenManager.FullScreenPanel.StorageBox );
                Player.Local.BlockInputs = true;
                if ( skinnedModelRenderer != null )
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
public class NameGenerator
{
    private static readonly Dictionary<Tier, List<string>> WeaponNames = new Dictionary<Tier, List<string>>
    {
        { Tier.SSS, new List<string> {
            "Golden", "Divine", "Legendary", "Mythic", "Eternal", "Ascendant", "Celestial", "Transcendent", "Immortal", "Radiant",
            "Empyrean", "Godly", "Exemplary", "Omniscient", "Primordial", "Invincible", "Supreme", "Exalted", "Seraphic", "Infinite",
            "Almighty", "Paragon", "Venerated", "Sublime", "Enlightened" } },

        { Tier.SS, new List<string> {
            "Epic", "Mystic", "Arcane", "Enchanted", "Celestial", "Runic", "Phantasmal", "Spectral", "Revered", "Exalted",
            "Resplendent", "Glorified", "Majestic", "Ornate", "Shimmering", "Ethereal", "Illustrious", "Mythical", "Radiant", "Otherworldly",
            "Magnificent", "Resonant", "Divinized", "Ancestral", "Myriad" } },

        { Tier.S, new List<string> {
            "Rare", "Ancient", "Sacred", "Fabled", "Heroic", "Glorious", "Valorous", "Venerable", "Imperial", "Exquisite",
            "Prestigious", "Honored", "Exemplary", "Virtuous", "Noble", "Chivalrous", "Gallant", "Dignified", "Sovereign", "Gallant",
            "Majestic", "Hallowed", "Illustrious", "Revered", "Lauded" } },

        { Tier.A, new List<string> {
            "Uncommon", "Valiant", "Noble", "Gallant", "Brave", "Stalwart", "Resilient", "Resolute", "Steadfast", "Bold",
            "Courageous", "Fearless", "Vigilant", "Loyal", "Dependable", "Dutiful", "Honorable", "Fierce", "Intrepid", "Dauntless",
            "Vigorous", "Staunch", "Indomitable", "Fearless", "Fortified" } },

        { Tier.B, new List<string> {
            "Common", "Sturdy", "Reliable", "Trusty", "Solid", "Dependable", "Durable", "Robust", "Steady", "Firm",
            "Resilient", "Tough", "Hardy", "Secure", "Unyielding", "Resistant", "Faithful", "Sound", "Lasting", "Proven",
            "Ironclad", "Formidable", "Rugged", "Steady", "Constant" } },

        { Tier.C, new List<string> {
            "Basic", "Plain", "Simple", "Ordinary", "Mundane", "Modest", "Unremarkable", "Average", "Standard", "Routine",
            "Unadorned", "Dull", "Practical", "Basic", "Unimpressive", "Humble", "Serviceable", "Everyday", "Functional", "Drab",
            "Utilitarian", "Plain", "Crude", "Standardized", "Standard" } }
    };

    private static readonly Dictionary<Tier, List<string>> ArmorNames = new Dictionary<Tier, List<string>>
    {
        { Tier.SSS, new List<string> {
            "Golden", "Divine", "Legendary", "Mythic", "Eternal", "Ascendant", "Celestial", "Transcendent", "Immortal", "Radiant",
            "Empyrean", "Godly", "Exemplary", "Omniscient", "Primordial", "Invincible", "Supreme", "Exalted", "Seraphic", "Infinite",
            "Almighty", "Paragon", "Venerated", "Sublime", "Enlightened" } },

        { Tier.SS, new List<string> {
            "Epic", "Mystic", "Arcane", "Enchanted", "Celestial", "Runic", "Phantasmal", "Spectral", "Revered", "Exalted",
            "Resplendent", "Glorified", "Majestic", "Ornate", "Shimmering", "Ethereal", "Illustrious", "Mythical", "Radiant", "Otherworldly",
            "Magnificent", "Resonant", "Divinized", "Ancestral", "Myriad" } },

        { Tier.S, new List<string> {
            "Rare", "Ancient", "Sacred", "Fabled", "Heroic", "Glorious", "Valorous", "Venerable", "Imperial", "Exquisite",
            "Prestigious", "Honored", "Exemplary", "Virtuous", "Noble", "Chivalrous", "Gallant", "Dignified", "Sovereign", "Gallant",
            "Majestic", "Hallowed", "Illustrious", "Revered", "Lauded" } },

        { Tier.A, new List<string> {
            "Uncommon", "Valiant", "Noble", "Gallant", "Brave", "Stalwart", "Resilient", "Resolute", "Steadfast", "Bold",
            "Courageous", "Fearless", "Vigilant", "Loyal", "Dependable", "Dutiful", "Honorable", "Fierce", "Intrepid", "Dauntless",
            "Vigorous", "Staunch", "Indomitable", "Fearless", "Fortified" } },

        { Tier.B, new List<string> {
            "Common", "Sturdy", "Reliable", "Trusty", "Solid", "Dependable", "Durable", "Robust", "Steady", "Firm",
            "Resilient", "Tough", "Hardy", "Secure", "Unyielding", "Resistant", "Faithful", "Sound", "Lasting", "Proven",
            "Ironclad", "Formidable", "Rugged", "Steady", "Constant" } },

        { Tier.C, new List<string> {
            "Basic", "Plain", "Simple", "Ordinary", "Mundane", "Modest", "Unremarkable", "Average", "Standard", "Routine",
            "Unadorned", "Dull", "Practical", "Basic", "Unimpressive", "Humble", "Serviceable", "Everyday", "Functional", "Drab",
            "Utilitarian", "Plain", "Crude", "Standardized", "Standard" } }
    };

    private static readonly Dictionary<Tier, List<string>> RandomWeaponNames = new Dictionary<Tier, List<string>>
    {
        { Tier.SSS, new List<string> {
            "Excalibur", "Thunderfury", "Doomhammer", "Ashbringer", "Dragonwrath",
            "Atiesh", "Soulreaper", "Moonblade", "Starbreaker", "Skyshatter" } },

        { Tier.SS, new List<string> {
            "Shadowmourne", "Frostmourne", "Warglaive", "Sulfuron", "Bloodthirst",
            "Oblivion", "Stormbreaker", "Nightfall", "Phantomstrike", "Earthsplitter" } },

        { Tier.S, new List<string> {
            "Skullcrusher", "Firebrand", "Dreadblade", "Warbringer", "Darkbane",
            "Ravager", "Obsidian Edge", "Silver Fang", "Thunderstrike", "Flamecaller" } },

        { Tier.A, new List<string> {
            "Valiant Edge", "Brave Sword", "Hero's Fang", "Mystic Blade", "Storm Edge",
            "Arcane Saber", "Fierce Mace", "Fiery Blade", "Noble Cleaver", "Resolute Bow" } },

        { Tier.B, new List<string> {
            "Iron Blade", "Sturdy Axe", "Solid Mace", "Common Sword", "Bronze Spear",
            "Battle Hammer", "Reliable Staff", "Trusty Sword", "Hardy Cleaver", "Solid Bow" } },

        { Tier.C, new List<string> {
            "Basic Gun", "Plain Dagger", "Simple Weapon", "Crude Mace", "Rusty Knife",
            "Wooden Spear", "Unpolished Blade", "Simple Hammer", "Weak Staff", "Rusty Cleaver" } }
    };

    private static readonly Dictionary<Tier, List<string>> RandomArmorNames = new Dictionary<Tier, List<string>>
    {
        { Tier.SSS, new List<string> {
            "Aegis", "Titan", "Vanguard", "Colossus", "Fortress",
            "Iron Wall", "Bulwark of Dawn", "Sanctum", "Citadel", "Watchtower" } },

        { Tier.SS, new List<string> {
            "Bulwark", "Defender", "Sentinel", "Warden", "Protector",
            "Palisade", "Stronghold", "Safeguard", "Retreat", "Redoubt" } },

        { Tier.S, new List<string> {
            "Guardian", "Garrison", "Bastion", "Rampart", "Armament",
            "Iron Guard", "Shield of Light", "Wall of Valor", "Barrier", "Shield of Honor" } },

        { Tier.A, new List<string> {
            "Sturdy Shield", "Valiant Guard", "Gallant Armor", "Steel Helm", "Iron Greaves",
            "Defender's Plate", "Fortified Chestplate", "Steel Leggings", "Vigorous Shield", "Noble Breastplate" } },

        { Tier.B, new List<string> {
            "Reliable Armor", "Trusty Shield", "Solid Helm", "Common Armor", "Bronze Greaves",
            "Sturdy Gauntlets", "Durable Chestplate", "Iron Helm", "Tough Boots", "Solid Shield" } },

        { Tier.C, new List<string> {
            "Basic Shield", "Plain Armor", "Simple Helm", "Crude Shield", "Rusty Greaves",
            "Weak Gauntlets", "Simple Chestplate", "Rough Boots", "Plain Helm", "Crude Shield" } }
    };

    private static readonly Random Random = new Random();

    public static string GenerateRandomName( Tier tier, bool isWeapon )
    {
        var tierNameList = isWeapon ? WeaponNames[tier] : ArmorNames[tier];
        var randomNameList = isWeapon ? RandomWeaponNames[tier] : RandomArmorNames[tier];
        string tierName = tierNameList[Random.Next( tierNameList.Count )];
        string randomName = randomNameList[Random.Next( randomNameList.Count )];
        return $"{tierName} {randomName}";
    }
}