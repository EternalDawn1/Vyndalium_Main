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

        public static ItemStorage Instance { get; private set; } = new ItemStorage();
        [Property] ItemInteractable itemInteractable { get; set; }
        [Property] SkinnedModelRenderer skinnedModelRenderer { get; set; }
        [Property] public List<ItemComponent> Items { get; set; } = new List<ItemComponent>();
    
        [Property]public bool IsBossChest { get; set; } = false;
        public bool IsZombieSpawner { get; set; }
        public int Level { get; set; } = 1;

        private Dictionary<string, (int MinAttack, int MaxAttack)> tierAttackValues = new Dictionary<string, (int MinAttack, int MaxAttack)>
        {
            { "C", (5, 20) },
            { "B", (20, 25) },
            { "A", (30, 55) },
            { "S", (60, 70) },
            { "SS", (70, 80) },
            { "SSS", (90, 110) },
            { "Ultimate", (250, 300) }
        };

        private Dictionary<string, (int MinArmor, int MaxArmor)> tierArmorValues = new Dictionary<string, (int MinArmor, int MaxArmor)>
        {
            { "C", (5, 10) },
            { "B", (10, 20) },
            { "A", (20, 30) },
            { "S", (30, 35) },
            { "SS", (45, 55) },
            { "SSS", (60, 70) },
            { "Ultimate", (250, 250) }
        };

        private bool itemsGenerated = false;


        public ItemStorage()
        {
            Instance = this;
            //LoadPrefabs();
        }

        protected override void OnAwake()
        {
            
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


                    itemComponent.GenerateRandomStats();
                   
                    itemComponent.CalculateSellPrice();
                   
                }
            }
        }
        private List<string> basePrefabs = new List<string>
        {

            "prefabs/weapons/custom_weapons/glock/glock.prefab",
            "prefabs/weapons/facepunch/usp/uspc.prefab",
            "prefabs/weapons/facepunch/shotgun/shotgunc.prefab",
            "prefabs/weapons/facepunch/mp5/mp5c.prefab",
            "prefabs/weapons/m4a1/m4a1-c.prefab",
            "prefabs/weapons/custom_weapons/deagle/deagle.prefab",
            "prefabs/clothes/armor/armor-c.prefab",
            "prefabs/clothes/helmet/helmet-c.prefab",
            "prefabs/clothes/legarmor/legarmor-c.prefab",
            "prefabs/items/wood_log.prefab",
            "prefabs/weapons/new/hands.prefab",
            "prefabs/weapons/new/knife.prefab",
            "prefabs/weapons/custom_weapons/katana/katana.prefab",
            "prefabs/items/wood_log.prefab",
            "prefabs/potions/potion_small.prefab",
            "prefabs/potions/potion_mid.prefab",
            "prefabs/potions/potion_big.prefab",
            "prefabs/clothes/clothes/base.prefab",
            "prefabs/clothes/clothes/base1.prefab",
            "prefabs/clothes/clothes/base2.prefab",
            "prefabs/clothes/clothes/base3.prefab",
            "prefabs/clothes/clothes/base4.prefab",
            "prefabs/clothes/clothes/base5.prefab",
            "prefabs/clothes/clothes/base6.prefab",
            "prefabs/clothes/clothes/base7.prefab",
            "prefabs/weapons/custom_weapons/aug/aug.prefab",
            "prefabs/weapons/custom_weapons/aug/aug2.prefab",
            "prefabs/weapons/custom_weapons/aug/aug3.prefab",

            "prefabs/clothes/clothes/base9.prefab",
            "prefabs/clothes/clothes/base10.prefab",
            "prefabs/clothes/clothes/base11.prefab",
            "prefabs/clothes/clothes/base12.prefab",
            "prefabs/clothes/clothes/base13.prefab",
          
            "prefabs/clothes/clothes/base15.prefab",
            "prefabs/clothes/clothes/base16.prefab",
            "prefabs/clothes/clothes/base17.prefab",
            "prefabs/clothes/clothes/base18.prefab",
            "prefabs/clothes/clothes/base19.prefab",
            "prefabs/clothes/clothes/base20.prefab",
            "prefabs/clothes/clothes/base21.prefab",
            "prefabs/clothes/clothes/base22.prefab",
            "prefabs/clothes/clothes/base23.prefab",
            "prefabs/clothes/clothes/base24.prefab",
           
            "prefabs/clothes/clothes/base26.prefab",
            "prefabs/clothes/clothes/base27.prefab",
            "prefabs/clothes/clothes/base28.prefab",
            "prefabs/clothes/clothes/base29.prefab",
            "prefabs/clothes/clothes/base30.prefab",
            "prefabs/clothes/clothes/base31.prefab",
         
            "prefabs/clothes/clothes/base33.prefab",
            "prefabs/clothes/clothes/base34.prefab",
            "prefabs/clothes/clothes/base35.prefab",
         
            "prefabs/clothes/clothes/base37.prefab",
            "prefabs/clothes/clothes/base38.prefab",
            "prefabs/clothes/clothes/base39.prefab",
            "prefabs/clothes/clothes/base40.prefab",
            "prefabs/clothes/clothes/base41.prefab",
            "prefabs/clothes/clothes/base42.prefab",
            "prefabs/clothes/clothes/base43.prefab",
        
            
            "prefabs/clothes/clothes/base47.prefab",
            "prefabs/clothes/clothes/base48.prefab",
            "prefabs/clothes/clothes/base49.prefab",
            "prefabs/clothes/clothes/base50.prefab",
            "prefabs/clothes/clothes/base51.prefab",
            "prefabs/clothes/clothes/base52.prefab",
            "prefabs/clothes/clothes/base53.prefab",
            "prefabs/clothes/clothes/base54.prefab",
            "prefabs/clothes/clothes/base55.prefab",
            "prefabs/clothes/clothes/base56.prefab",
            "prefabs/clothes/clothes/base57.prefab",
            "prefabs/clothes/clothes/base58.prefab",
            "prefabs/clothes/clothes/base59.prefab",
            "prefabs/clothes/clothes/base60.prefab",
            "prefabs/clothes/clothes/base61.prefab",
            "prefabs/clothes/clothes/base62.prefab",
            "prefabs/clothes/clothes/base63.prefab",
            "prefabs/clothes/clothes/base64.prefab",
            "prefabs/clothes/clothes/base65.prefab",
            "prefabs/clothes/clothes/base66.prefab",
            "prefabs/clothes/clothes/base67.prefab",
            "prefabs/clothes/clothes/base68.prefab",
            "prefabs/clothes/clothes/base69.prefab",
         
         
            "prefabs/clothes/clothes/base72.prefab",
            "prefabs/clothes/clothes/base73.prefab",
         
          
            "prefabs/clothes/clothes/base76.prefab",
            "prefabs/clothes/clothes/base77.prefab",
            "prefabs/clothes/clothes/base78.prefab",
            "prefabs/clothes/clothes/base79.prefab",
        
            "prefabs/clothes/clothes/base81.prefab",
         
           
            "prefabs/clothes/clothes/base84.prefab",
            "prefabs/clothes/clothes/base85.prefab",
            "prefabs/clothes/clothes/base86.prefab",
            "prefabs/clothes/clothes/base87.prefab",
            "prefabs/clothes/clothes/base88.prefab",
           
            "prefabs/clothes/clothes/base90.prefab",

            



           
            // Füge hier weitere Basis-Prefabs hinzu
        };
        

        private List<string> bossItems = new List<string>
        {
            "prefabs/weapons/custom_weapons/katana/katana.prefab",
            "prefabs/entitys/aspects/variants/air.prefab",
            "prefabs/entitys/aspects/variants/bleed.prefab",
            "prefabs/entitys/aspects/variants/earth.prefab",
            "prefabs/entitys/aspects/variants/fire.prefab",
            "prefabs/entitys/aspects/variants/holy.prefab",
            "prefabs/entitys/aspects/variants/ice.prefab",
            "prefabs/entitys/aspects/variants/lightning.prefab",
            "prefabs/entitys/aspects/variants/water.prefab",
            "prefabs/entitys/aspects/variants/shadow.prefab",
            "prefabs/items/bags/backpack.prefab",
            "prefabs/items/bags/backpack_big.prefab",
            "prefabs/items/bags/backpack_small2.prefab",
            "prefabs/entitys/rings/ringsv1.prefab",
            "prefabs/entitys/rings/ringsv2.prefab",
            "prefabs/weapons/custom_weapons/deagle/deagle.prefab",
            "prefabs/clothes/clothes/sbase1.prefab",
            "prefabs/weapons/custom_weapons/minigun/minigun.prefab",
            "prefabs/clothes/clothes/sbase3.prefab",
            "prefabs/clothes/clothes/sbase4.prefab",
            "prefabs/clothes/clothes/sbase5.prefab",
            "prefabs/clothes/clothes/sbase6.prefab",
            "prefabs/clothes/clothes/sbase7.prefab",
            "prefabs/clothes/clothes/sbase8.prefab",
            "prefabs/clothes/clothes/sbase9.prefab",
            "prefabs/clothes/clothes/sbase10.prefab",
            "prefabs/clothes/clothes/sbase11.prefab",
            "prefabs/clothes/clothes/sbase12.prefab",
            "prefabs/clothes/clothes/sbase13.prefab",
            "prefabs/clothes/clothes/sbase14.prefab",
            "prefabs/clothes/clothes/sbase15.prefab",
            "prefabs/clothes/clothes/sbase16.prefab",
            "prefabs/clothes/clothes/sbase17.prefab",
            "prefabs/clothes/clothes/sbase18.prefab",
            "prefabs/clothes/clothes/sbase19.prefab",
            "prefabs/clothes/clothes/sbase20.prefab",
            "prefabs/clothes/clothes/sbase21.prefab",
            "prefabs/clothes/clothes/sbase22.prefab",
            "prefabs/clothes/clothes/sbase23.prefab",
            "prefabs/clothes/clothes/sbase24.prefab",
            

            // Füge hier weitere Boss-Items hinzu
        };
        private static readonly List<string> tiers = new List<string> { "C", "B", "A", "S", "SS", "SSS", "Ultimate" };



        public List<string> nonRandomStatItems = new List<string>
        {
            "prefabs/items/wood_log.prefab",
            "prefabs/potions/potion_small.prefab",
            "prefabs/potions/potion_mid.prefab",
            "prefabs/potions/potion_big.prefab",
         
         
        };

      

        private List<string> tierCPrefabs = new List<string>
        {

            "prefabs/weapons/facepunch/usp/uspc.prefab",
            "prefabs/weapons/facepunch/shotgun/shotgunc.prefab",
            "prefabs/weapons/facepunch/mp5/mp5c.prefab",
            "prefabs/weapons/m4a1/m4a1-c.prefab",
            "prefabs/clothes/armor/armor-c.prefab",
            "prefabs/clothes/helmet/helmet-c.prefab",
            "prefabs/clothes/legarmor/legarmor-c.prefab",
            "prefabs/items/wood_log.prefab",
            "prefabs/weapons/new/hands.prefab",
            "prefabs/weapons/new/knife.prefab",
            "prefabs/weapons/custom_weapons/deagle/deagle.prefab",
            "prefabs/weapons/custom_weapons/glock/glock.prefab",
             "prefabs/weapons/custom_weapons/katana/katana.prefab",
         
 
            // Fügen Sie hier weitere C-Tier-Prefab-Dateien hinzu
        };

        private List<string> tierBPrefabs = new List<string>
        {
       "prefabs/weapons/custom_weapons/minigun/minigun.prefab",
            "prefabs/weapons/facepunch/usp/uspb.prefab",
            "prefabs/weapons/facepunch/shotgun/shotgunb.prefab",
            "prefabs/weapons/facepunch/mp5/mp5b.prefab",
            "prefabs/weapons/m4a1/m4a1-b.prefab",

            "prefabs/clothes/armor/armor-b.prefab",
            "prefabs/clothes/helmet/helmet-b.prefab",
            "prefabs/clothes/legarmor/legarmor-b.prefab",
            "prefabs/items/wood_log.prefab",
             "prefabs/weapons/new/hands.prefab",
            "prefabs/weapons/new/knife.prefab",
            "prefabs/weapons/custom_weapons/deagle/deagle.prefab",
     "prefabs/weapons/custom_weapons/glock/glock.prefab",
      "prefabs/weapons/custom_weapons/katana/katana.prefab",
     

            // Fügen Sie hier weitere B-Tier-Prefab-Dateien hinzu
        };

        private List<string> tierAPrefabs = new List<string>
        {

            "prefabs/weapons/facepunch/usp/uspa.prefab",
            "prefabs/weapons/facepunch/shotgun/shotguna.prefab",
            "prefabs/weapons/facepunch/mp5/mp5a.prefab",
            "prefabs/weapons/m4a1/m4a1-a.prefab",

            "prefabs/clothes/armor/armor-a.prefab",
            "prefabs/clothes/helmet/helmet-a.prefab",
            "prefabs/clothes/legarmor/legarmor-a.prefab",
             "prefabs/weapons/new/hands.prefab",
            "prefabs/weapons/new/knife.prefab",
        "prefabs/weapons/custom_weapons/deagle/deagle.prefab",
          "prefabs/weapons/custom_weapons/glock/glock.prefab",
           "prefabs/weapons/custom_weapons/katana/katana.prefab",
    
            // Fügen Sie hier weitere A-Tier-Prefab-Dateien hinzu
        };

        private List<string> tierSPrefabs = new List<string>
        {
        "prefabs/weapons/custom_weapons/glock/glock.prefab",
            "prefabs/weapons/facepunch/usp/usps.prefab",
            "prefabs/weapons/facepunch/shotgun/shotguns.prefab",
            "prefabs/weapons/facepunch/mp5/mp5s.prefab",
            "prefabs/weapons/m4a1/m4a1-s.prefab",
     "prefabs/weapons/custom_weapons/minigun/minigun.prefab",
            "prefabs/clothes/armor/armor-s.prefab",
            "prefabs/clothes/helmet/helmet-s.prefab",
            "prefabs/clothes/legarmor/legarmor-s.prefab",
             "prefabs/weapons/new/hands.prefab",
            "prefabs/weapons/new/knife.prefab",
          "prefabs/weapons/custom_weapons/deagle/deagle.prefab",
           "prefabs/weapons/custom_weapons/katana/katana.prefab",
           "prefabs/weapons/custom_weapons/aug/aug.prefab",
      
        
            // Fügen Sie hier weitere S-Tier-Prefab-Dateien hinzu
        };

        private List<string> tierSSPrefabs = new List<string>
        {

            "prefabs/weapons/facepunch/usp/uspss.prefab",
            "prefabs/weapons/facepunch/shotgun/shotgunss.prefab",
            "prefabs/weapons/facepunch/mp5/mp5ss.prefab",
            "prefabs/weapons/m4a1/m4a1-ss.prefab",
      "prefabs/weapons/custom_weapons/minigun/minigun.prefab",
            "prefabs/clothes/armor/armor-ss.prefab",
            "prefabs/clothes/helmet/helmet-ss.prefab",
            "prefabs/clothes/legarmor/legarmor-ss.prefab",
             "prefabs/weapons/new/hands.prefab",
            "prefabs/weapons/new/knife.prefab",

     "prefabs/weapons/custom_weapons/deagle/deagle.prefab",
          "prefabs/weapons/custom_weapons/glock/glock.prefab",
           "prefabs/weapons/custom_weapons/katana/katana.prefab",

            "prefabs/weapons/custom_weapons/aug/aug2.prefab",
     
            // Fügen Sie hier weitere SS-Tier-Prefab-Dateien hinzu
        };

        private List<string> tierSSSPrefabs = new List<string>
        {
       "prefabs/weapons/custom_weapons/minigun/minigun.prefab",
            "prefabs/weapons/facepunch/usp/uspsss.prefab",
            "prefabs/weapons/facepunch/shotgun/shotgunsss.prefab",
            "prefabs/weapons/facepunch/mp5/mp5sss.prefab",
            "prefabs/weapons/m4a1/m4a1-sss.prefab",
            "prefabs/weapons/custom_weapons/deagle/deagle.prefab",
            "prefabs/clothes/armor/armor-sss.prefab",
            "prefabs/clothes/helmet/helmet-sss.prefab",
            "prefabs/clothes/legarmor/legarmor-sss.prefab",
             "prefabs/weapons/new/hands.prefab",
            "prefabs/weapons/new/knife.prefab",
       "prefabs/weapons/custom_weapons/glock/glock.prefab",
        "prefabs/weapons/custom_weapons/katana/katana.prefab",
  
            "prefabs/weapons/custom_weapons/aug/aug3.prefab",
   
            // Fügen Sie hier weitere SSS-Tier-Prefab-Dateien hinzu
        };
        private List<string> tierUltimatePrefabs = new List<string>
        {

            "prefabs/weapons/facepunch/usp/uspsss.prefab",
            "prefabs/weapons/facepunch/shotgun/shotgunsss.prefab",
            "prefabs/weapons/facepunch/mp5/mp5sss.prefab",
            "prefabs/weapons/m4a1/m4a1-sss.prefab",

            "prefabs/clothes/armor/armor-sss.prefab",
            "prefabs/clothes/helmet/helmet-sss.prefab",
            "prefabs/clothes/legarmor/legarmor-sss.prefab",
            "prefabs/weapons/new/hands.prefab",
            "prefabs/weapons/new/knife.prefab",
            "prefabs/weapons/custom_weapons/deagle/deagle.prefab",
            "prefabs/weapons/custom_weapons/minigun/minigun.prefab",
            "prefabs/weapons/custom_weapons/glock/glock.prefab",
            "prefabs/weapons/custom_weapons/katana/katana.prefab",
            "prefabs/weapons/custom_weapons/aug/aug.prefab",
            "prefabs/weapons/custom_weapons/aug/aug2.prefab",
            "prefabs/weapons/custom_weapons/aug/aug3.prefab",
        };
        
      
        private bool itemsLoaded = false;
       
        public void LoadPrefabs()
        {
            if ( itemsLoaded ) return; // Überprüfen, ob die Items bereits geladen wurden

            if ( IsBossChest )
            {
                
                int minLevel = 0;
                int maxLevel = Player.Local?.GetMaxLevel() ?? 100;
                int playerLevel = GetPlayerLevel();
                LoadBossTierPrefabs(playerLevel, minLevel, maxLevel);
                GenerateRandomStatsForItems();
            }
            else
            {
              
                if ( Player.Local != null )
                {
                    int minLevel = 0;
                    int maxLevel = Player.Local?.GetMaxLevel() ?? 100;
                    int playerLevel = GetPlayerLevel();
                    LoadRandomTierPrefabs( playerLevel, minLevel, maxLevel );
                    GenerateRandomStatsForItems();
                }
            }
         

            itemsLoaded = true; // Setzen der Variable, um anzuzeigen, dass die Items geladen wurden
         
        }
    
        public void LoadBossTierPrefabs( int playerLevel, int minLevel, int maxLevel )
        {
            if ( itemsLoaded ) return; // Überprüfen, ob die Items bereits geladen wurden
            var random = new Random();
            var tierPrefabs = new List<(List<string> prefabs, string tier, double probability)>
    {
        (tierSPrefabs, "A", 0.15),  // 15%
        (tierSPrefabs, "S", 0.10),  // 10%
        (tierSSPrefabs, "SS", 0.04), // 4%
        (tierSSSPrefabs, "SSS", 0.009), // 0.9%
        (tierUltimatePrefabs, "Ultimate", 0.001) // 0.1%
    };
            var bossPrefabs = new List<(List<string> prefabs, string tier, double probability)>
    {
        (bossItems, "C", 0.3),  // 30%
        (bossItems, "B", 0.2),  // 20%
        (bossItems, "A", 0.1),  // 10%
        (bossItems, "S", 0.08),  // 8%
        (bossItems, "SS", 0.05), // 5%
        (bossItems, "SSS", 0.03), // 3%
        (bossItems, "Ultimate", 0.01) // 1%
    };

            int itemsToSpawn;
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

            Items.Clear();

            var selectedPrefabs = new List<(string prefab, string tier)>();
            var addedPrefabPaths = new HashSet<string>();
            int totalGenerated = 0;

            // Generiere Tier-Items
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
                        totalGenerated++;
                        break;
                    }
                }
            }

            // Generiere Boss-Items
            for ( int i = 0; i < itemsToSpawn; i++ )
            {
                double roll = random.NextDouble();
                double cumulative = 0.0;

                foreach ( var (prefabs, tier, probability) in bossPrefabs )
                {
                    cumulative += probability;
                    if ( roll < cumulative )
                    {
                        var selectedPrefab = prefabs[random.Next( prefabs.Count )];
                        selectedPrefabs.Add( (selectedPrefab, tier) );
                        totalGenerated++;
                        break;
                    }
                }
            }

            Items.Clear();
            int totalAdded = 0;

            // Begrenze die Anzahl der hinzugefügten Items auf die Anzahl der ausgewählten Prefabs
            foreach ( var (prefabPath, tier) in selectedPrefabs )
            {
                if ( totalAdded >= itemsToSpawn ) break; // Begrenze die Anzahl der hinzugefügten Items
                if ( !addedPrefabPaths.Contains( prefabPath ) )
                {
                    LoadTierPrefab( prefabPath, tier, minLevel, maxLevel );
                    addedPrefabPaths.Add( prefabPath );
                    totalAdded++;
                 
                    // Zu 80% ein zufälliges Item aus nonRandomStatItems hinzufügen
                    if ( random.NextDouble() <= 0.20 )
                    {
                        var randomNonRandomStatItem = nonRandomStatItems[random.Next( nonRandomStatItems.Count )];
                        LoadNonRandomStatItem( randomNonRandomStatItem, minLevel, maxLevel );
                        addedPrefabPaths.Add( randomNonRandomStatItem );
                        totalAdded++;
                      
                    }
                }
            }

     
            itemsLoaded = true;
        }

        public void LoadRandomTierPrefabs( int playerLevel, int minLevel, int maxLevel )
        {
            if ( itemsLoaded ) return; // Überprüfen, ob die Items bereits geladen wurden

            var random = new Random();
            var tierProbabilities = GetTierProbabilities( playerLevel );
            var baseProbabilities = GetTierBaseProbabilities( playerLevel );

            var tierPrefabs = new List<(List<string> prefabs, string tier, double probability)>();
            var baseTierPrefabs = new List<(List<string> prefabs, string tier, double probability)>();

            // Füge basePrefabs nur hinzu, wenn baseProbabilities nicht null ist
            if ( baseProbabilities != null )
            {
                baseTierPrefabs.Add( (basePrefabs, "C", baseProbabilities["C"]) );
                baseTierPrefabs.Add( (basePrefabs, "B", baseProbabilities["B"]) );
                baseTierPrefabs.Add( (basePrefabs, "A", baseProbabilities["A"]) );
                baseTierPrefabs.Add( (basePrefabs, "S", baseProbabilities["S"]) );
                baseTierPrefabs.Add( (basePrefabs, "SS", baseProbabilities["SS"]) );
                baseTierPrefabs.Add( (basePrefabs, "SSS", baseProbabilities["SSS"]) );
                baseTierPrefabs.Add( (basePrefabs, "Ultimate", baseProbabilities["Ultimate"]) );
            }

            // Füge tierPrefabs immer hinzu
            tierPrefabs.Add( (tierCPrefabs, "C", tierProbabilities["C"]) );
            tierPrefabs.Add( (tierBPrefabs, "B", tierProbabilities["B"]) );
            tierPrefabs.Add( (tierAPrefabs, "A", tierProbabilities["A"]) );
            tierPrefabs.Add( (tierSPrefabs, "S", tierProbabilities["S"]) );
            tierPrefabs.Add( (tierSSPrefabs, "SS", tierProbabilities["SS"]) );
            tierPrefabs.Add( (tierSSSPrefabs, "SSS", tierProbabilities["SSS"]) );
            tierPrefabs.Add( (tierUltimatePrefabs, "Ultimate", tierProbabilities["Ultimate"]) );

            int itemsToSpawn = DetermineItemsToSpawn( playerLevel );
            Items.Clear();

            var selectedPrefabs = new List<(string prefab, string tier)>();
            var addedPrefabPaths = new HashSet<string>();
            int totalGenerated = 0;

            // Generiere Items basierend auf den Wahrscheinlichkeiten
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
                        totalGenerated++;
                        break;
                    }
                }

                // Füge auch Items aus baseTierPrefabs hinzu
                if ( baseTierPrefabs.Count > 0 )
                {
                    roll = random.NextDouble();
                    cumulative = 0.0;
                    foreach ( var (prefabs, tier, probability) in baseTierPrefabs )
                    {
                        cumulative += probability;
                        if ( roll < cumulative )
                        {
                            var selectedPrefab = prefabs[random.Next( prefabs.Count )];
                            selectedPrefabs.Add( (selectedPrefab, tier) );
                            totalGenerated++;
                            break;
                        }
                    }
                }
            }

            // Stelle sicher, dass mindestens ein Item erzeugt wird
            if ( selectedPrefabs.Count == 0 )
            {
                // Fallback: Füge mindestens ein C-Tier-Item hinzu
                var fallbackPrefab = tierCPrefabs[random.Next( tierCPrefabs.Count )];
                selectedPrefabs.Add( (fallbackPrefab, "C") );
            }

            Items.Clear();
            int totalAdded = 0;

            foreach ( var (prefabPath, tier) in selectedPrefabs )
            {
                if ( totalAdded >= itemsToSpawn ) break;

                if ( !addedPrefabPaths.Contains( prefabPath ) )
                {
                    LoadTierPrefab( prefabPath, tier, minLevel, maxLevel );
                    addedPrefabPaths.Add( prefabPath );
                    totalAdded++;
                }
            }

            itemsLoaded = true;
        }
        private Dictionary<string, double> GetTierProbabilities( int playerLevel )
        {
            // Basiswahrscheinlichkeiten für jedes Tier
            double cProbability = Math.Max( 50 - (playerLevel * 0.5), 5 ); // C sinkt mit steigendem Level, min. 5%
            double bProbability = Math.Max( 30 - (playerLevel * 0.3), 10 ); // B sinkt, min. 10%
            double aProbability = Math.Min( 15 + (playerLevel * 0.2), 25 ); // A steigt, max. 25%
            double sProbability = Math.Min( 4 + (playerLevel * 0.1), 15 ); // S steigt, max. 15%
            double ssProbability = Math.Min( 0.9 + (playerLevel * 0.05), 10 ); // SS steigt, max. 10%
            double sssProbability = Math.Min( 0.09 + (playerLevel * 0.01), 5 ); // SSS steigt, max. 5%
            double ultimateProbability = Math.Min( 0.01 + (playerLevel * 0.005), 2 ); // Ultimate steigt, max. 2%

            // Normalisierung der Wahrscheinlichkeiten
            double total = cProbability + bProbability + aProbability + sProbability + ssProbability + sssProbability + ultimateProbability;

            return new Dictionary<string, double>
        {
            { "C", cProbability / total },
            { "B", bProbability / total },
            { "A", aProbability / total },
            { "S", sProbability / total },
            { "SS", ssProbability / total },
            { "SSS", sssProbability / total },
            { "Ultimate", ultimateProbability / total }
        };
        }
        private Dictionary<string,double> GetTierBaseProbabilities( int playerLevel )
        {
            var random = new Random();
            if ( random.NextDouble() <= 0.4 ) // 40% Wahrscheinlichkeit, null zurückzugeben
            {
                return null;
            }

            // Basiswahrscheinlichkeiten für jedes Tier
            double cProbability = Math.Max( 50 - (playerLevel * 0.5), 5 ); // C sinkt mit steigendem Level, min. 5%
            double bProbability = Math.Max( 30 - (playerLevel * 0.3), 10 ); // B sinkt, min. 10%
            double aProbability = Math.Min( 15 + (playerLevel * 0.2), 25 ); // A steigt, max. 25%
            double sProbability = Math.Min( 4 + (playerLevel * 0.1), 15 ); // S steigt, max. 15%
            double ssProbability = Math.Min( 0.9 + (playerLevel * 0.05), 10 ); // SS steigt, max. 10%
            double sssProbability = Math.Min( 0.09 + (playerLevel * 0.01), 5 ); // SSS steigt, max. 5%
            double ultimateProbability = Math.Min( 0.01 + (playerLevel * 0.005), 2 ); // Ultimate steigt, max. 2%

            double total = cProbability + bProbability + aProbability + sProbability + ssProbability + sssProbability + ultimateProbability;
            return new Dictionary<string, double>
            {
                { "C", cProbability /total },
                { "B", bProbability / total },
                { "A", aProbability / total },
                { "S", sProbability / total },
                { "SS", ssProbability / total },
                { "SSS", sssProbability / total },
                { "Ultimate", ultimateProbability / total }
            
            };
        }

        private double CalculateProbabilityForLevel( int playerLevel, int itemLevel )
        {
            int levelDifference = itemLevel - playerLevel;

            if ( levelDifference < 0 ) return 0; // Keine Items unter dem Spielerlevel

            // Dynamische Anpassung der maximalen Leveldifferenz basierend auf dem Spielerlevel
            int maxLevelDifference = playerLevel >= 100 ? 50 : 20 + (playerLevel / 2); // Erhöhe die Differenz dynamisch

            if ( levelDifference > maxLevelDifference ) return 0; // Keine Items mehr als maxLevelDifference über dem Spielerlevel

            // Basiswahrscheinlichkeiten für jedes Tier
            double baseProbability = 0;
            if ( levelDifference <= 5 ) baseProbability = 0.5; // 50% für Items innerhalb von 5 Leveln
            else if ( levelDifference <= 10 ) baseProbability = 0.25; // 25% für Items innerhalb von 10 Leveln
            else if ( levelDifference <= 15 ) baseProbability = 0.1; // 10% für Items innerhalb von 15 Leveln
            else if ( levelDifference <= maxLevelDifference ) baseProbability = 0.05; // 5% für Items innerhalb der maximalen Leveldifferenz

            // Skalierung der Wahrscheinlichkeit basierend auf dem Spielerlevel
            double levelScalingFactor = 1 + (playerLevel / 100.0); // Erhöhe die Wahrscheinlichkeit proportional zum Spielerlevel
            return baseProbability * levelScalingFactor;
        }

        private int DetermineItemsToSpawn( int playerLevel )
        {
            // Reduzierte Anzahl von Items basierend auf Spielerlevel
            var random = new Random();

            // Basiswerte für die Anzahl der Items
            int baseAmount = 1; // Mindestens 1 Item

            // Wahrscheinlichkeiten für zusätzliche Items
            if ( random.NextDouble() < 0.60 ) // 60% Chance für 1 Item
            {
                return baseAmount;
            }
            else if ( random.NextDouble() < 0.25 ) // 25% Chance für 2 Items
            {
                return baseAmount + 1;
            }
            else if ( random.NextDouble() < 0.10 ) // 10% Chance für 3 Items
            {
                return baseAmount + 2;
            }
            else // 5% Chance für mehr Items basierend auf Spielerlevel
            {
                // Max 4 Items bei Level unter 50, max 5 bei höherem Level
                int maxExtra = playerLevel >= 50 ? 4 : 3;
                return baseAmount + random.Next( 1, maxExtra );
            }
        }
        private void LoadNonRandomStatItem( string prefabPath, int minLevel, int maxLevel )
        {
           
            int playerLevel = GetPlayerLevel();
            var prefab = ResourceLibrary.Get<PrefabFile>( prefabPath );
            if ( prefab != null )
            {
                var itemComponent = ConvertPrefabToItemComponent( prefab );
                if ( itemComponent != null )
                {
                    var requiredLevel = DetermineRequiredLevelForTier( "NonRandom" );
                    if ( requiredLevel >= minLevel && requiredLevel <= maxLevel )
                    {
                        itemComponent.RequiredLevel = requiredLevel;
                        Items.Add( itemComponent );
                    }
                }
            }
        }

    
        public void LoadTierPrefab( string prefabPath, string tier, int minLevel, int maxLevel )
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
                        
                            itemComponent.RequiredLevel = requiredLevel;
                            
                            

                            if ( itemComponent.IsWeapon )
                            {
                                var attackValues = CalculateAttackValues( tier, itemComponent );
                                itemComponent.MinAttackValue = attackValues.MinAttack;
                                itemComponent.MaxAttackValue = attackValues.MaxAttack;
                                itemComponent.Tier = Enum.Parse<Tier>( tier );
                                itemComponent.GenerateRandomStats();
                                
                                Items.Add( itemComponent );
                            }
                            else if ( itemComponent.IsArmor )
                            {
                                var armorValues = CalculateArmorValues( tier, Level, itemComponent, playerLevel );
                                itemComponent.MinArmorValue = armorValues.MinArmor;
                                itemComponent.MaxArmorValue = armorValues.MaxArmor;
                                itemComponent.GenerateRandomStats();
                                itemComponent.Tier = Enum.Parse<Tier>( tier );
                                Items.Add( itemComponent );
                            }
                            else if ( itemComponent.IsAccessory )
                            {
                                itemComponent.GenerateRandomStats();
                                itemComponent.Tier = Enum.Parse<Tier>( tier );
                                Items.Add( itemComponent );
                            }
                            else if ( itemComponent.IsConsumable )
                            {
                                itemComponent.Tier = Enum.Parse<Tier>( tier );
                            Items.Add( itemComponent );
                            }
                            else if ( itemComponent.IsMaterial )
                                {
                                    itemComponent.Tier = Enum.Parse<Tier>( tier );
                                Items.Add( itemComponent );
                            }
                            else if ( itemComponent.IsAspect )
                                {
                                    itemComponent.Tier = Enum.Parse<Tier>( tier );
                                Items.Add( itemComponent );
                            }
                            else if ( itemComponent.IsPotion )
                                {
                                    itemComponent.Tier = Enum.Parse<Tier>( tier );
                                    var healthPotion = itemComponent as HealthPotion;
                                    healthPotion?.GeneratePotionStats();
                                    Items.Add( itemComponent );
                            }
                            else
                            {
                               
                            }


                     


                    }
                    
                   
                }
                
            }
            
        }
        private int GetMaxItemsForTier( string tier )
        {
            switch ( tier )
            {
                case "C":
                    return 1;
                case "B":
                    return 2;
                case "A":
                    return 3;
                case "S":
                    return 4;
                case "SS":
                    return 5;
                case "SSS":
                    return 6;
                case "Ultimate":
                    return 7;
                default:
                    return 1;
            }
        }

        public int GetPlayerLevel()
        {
            var player = Player.Local;
            if ( Player.Local != null )
            {
                return player.Level;
            }
            else if( Player.Local == null )
            {
                return Level;
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
            int maxLevel = Player.Local?.GetMaxLevel() ?? 100; // Verwenden Sie das maximale Level des Spielers oder 100 als Fallback
            var random = new Random();
            int requiredLevel;

            // Bestimme die Basislevel in 5er-Schritten
            int baseLevel = (playerLevel / 5) * 5;

            // Wahrscheinlichkeitsbasierte Berechnung
            int chance = random.Next( 100 );
            if ( chance < 60 ) // 70% Chance auf Level innerhalb von 5 Leveln höher oder gleich dem Basislevel
            {
                int lowerBound = Math.Max( baseLevel, 0 );
                int upperBound = Math.Min( baseLevel + 5, maxLevel );
                requiredLevel = random.Next( lowerBound, upperBound + 1 );
            }
            else if ( chance < 70 ) // 20% Chance auf Level innerhalb von 10 Leveln höher oder gleich dem Basislevel
            {
                int lowerBound = Math.Max( baseLevel, 0 );
                int upperBound = Math.Min( baseLevel + 10, maxLevel );
                requiredLevel = random.Next( lowerBound, upperBound + 1 );
            }
            else if( chance < 75 ) // 10% Chance auf Level innerhalb von 15 Leveln höher oder gleich dem Basislevel
            {
                int lowerBound = Math.Max( baseLevel, 0 );
                int upperBound = Math.Min( baseLevel + 15, maxLevel );
                requiredLevel = random.Next( lowerBound, upperBound + 1 );
            }
            else if( chance < 80)
            {
                int lowerBound = Math.Max( baseLevel, 0 );
                int upperBound = Math.Min( baseLevel + 20, maxLevel );
                requiredLevel = random.Next( lowerBound, upperBound + 1 );
            }
            else if( chance < 85)
            {
                int lowerBound = Math.Max( baseLevel, 0 );
                int upperBound = Math.Min( baseLevel + 25, maxLevel );
                requiredLevel = random.Next( lowerBound, upperBound + 1 );
            }
            else if( chance < 90)
            {
                int lowerBound = Math.Max( baseLevel, 0 );
                int upperBound = Math.Min( baseLevel + 30, maxLevel );
                requiredLevel = random.Next( lowerBound, upperBound + 1 );
            }
            else if( chance < 95)
            {
                int lowerBound = Math.Max( baseLevel, 0 );
                int upperBound = Math.Min( baseLevel + 35, maxLevel );
                requiredLevel = random.Next( lowerBound, upperBound + 1 );
            }
            else if( chance < 100)
            {
                int lowerBound = Math.Max( baseLevel, 0 );
                int upperBound = Math.Min( baseLevel + 40, maxLevel );
                requiredLevel = random.Next( lowerBound, upperBound + 1 );
            }
            else if( chance < 105)
            {
                int lowerBound = Math.Max( baseLevel, 0 );
                int upperBound = Math.Min( baseLevel + 45, maxLevel );
                requiredLevel = random.Next( lowerBound, upperBound + 1 );
            }
            else if( chance < 110)
            {
                int lowerBound = Math.Max( baseLevel, 0 );
                int upperBound = Math.Min( baseLevel + 50, maxLevel );
                requiredLevel = random.Next( lowerBound, upperBound + 1 );
            }
            else if( chance < 115)
            {
                int lowerBound = Math.Max( baseLevel, 0 );
                int upperBound = Math.Min( baseLevel + 55, maxLevel );
                requiredLevel = random.Next( lowerBound, upperBound + 1 );
            }
            else if( chance < 120)
            {
                int lowerBound = Math.Max( baseLevel, 0 );
                int upperBound = Math.Min( baseLevel + 60, maxLevel );
                requiredLevel = random.Next( lowerBound, upperBound + 1 );
            }
            else if( chance < 125)
            {
                int lowerBound = Math.Max( baseLevel, 0 );
                int upperBound = Math.Min( baseLevel + 65, maxLevel );
                requiredLevel = random.Next( lowerBound, upperBound + 1 );
            }
            else if( chance < 130)
            {
                int lowerBound = Math.Max( baseLevel, 0 );
                int upperBound = Math.Min( baseLevel + 70, maxLevel );
                requiredLevel = random.Next( lowerBound, upperBound + 1 );
            }
            else if( chance < 135)
            {
                int lowerBound = Math.Max( baseLevel, 0 );
                int upperBound = Math.Min( baseLevel + 75, maxLevel );
                requiredLevel = random.Next( lowerBound, upperBound + 1 );
            }
            else if( chance < 140)
            {
                int lowerBound = Math.Max( baseLevel, 0 );
                int upperBound = Math.Min( baseLevel + 80, maxLevel );
                requiredLevel = random.Next( lowerBound, upperBound + 1 );
            }
            else if( chance < 145)
            {
                int lowerBound = Math.Max( baseLevel, 0 );
                int upperBound = Math.Min( baseLevel + 85, maxLevel );
                requiredLevel = random.Next( lowerBound, upperBound + 1 );
            }
            
            else
            {
                int lowerBound = Math.Max( baseLevel, 0 );
                int upperBound = Math.Min( baseLevel + 45, maxLevel );
                requiredLevel = random.Next( lowerBound, upperBound + 1 );
            }

           
          

            // Füge die Möglichkeit hinzu, dass Items auch 5 oder 10 Level unter dem Spielerlevel droppen können
            if ( random.Next( 100 ) < 20 ) // 20% Chance auf Level innerhalb von 5 Leveln unter dem Spielerlevel
            {
                int lowerBound = Math.Max( playerLevel - 5, 0 );
                requiredLevel = random.Next( lowerBound, playerLevel + 1 );
            }
            else if ( random.Next( 100 ) < 10 ) // 10% Chance auf Level innerhalb von 10 Leveln unter dem Spielerlevel
            {
                int lowerBound = Math.Max( playerLevel - 10, 0 );
                requiredLevel = random.Next( lowerBound, playerLevel + 1 );
            }

            return requiredLevel;
        }
        public (int MinAttack, int MaxAttack) CalculateAttackValues( string tier, ItemComponent itemComponent )
        {
            if ( Player.Local == null )
            {
                return (0, 0);
            }

            int requiredLevel = DetermineRequiredLevelForTier( tier );
            var baseValues = tierAttackValues[tier];
            double attackIncreasePerLevel = 16 * 0.3;

            int minAttack = baseValues.MinAttack + (int)(attackIncreasePerLevel * requiredLevel);
            int maxAttack = baseValues.MaxAttack + (int)(attackIncreasePerLevel * requiredLevel);

            // Skalierung der Angriffswerte basierend auf dem Level der Waffe und dem erforderlichen Level
            double tierMultiplier = 1.0;
            switch ( tier )
            {
                case "Ultimate":
                    tierMultiplier = 1 + (16.0 * requiredLevel / 100);
                    break;
                case "SSS":
                    tierMultiplier = 1 + (8.0 * requiredLevel / 100);
                    break;
                case "SS":
                    tierMultiplier = 1 + (4.0 * requiredLevel / 100);
                    break;
                case "S":
                    tierMultiplier = 1 + (3.0 * requiredLevel / 100);
                    break;
                case "A":
                    tierMultiplier = 1 + (2.0 * requiredLevel / 100);
                    break;
                case "B":
                    tierMultiplier = 1 + (1.0 * requiredLevel / 100);
                    break;
                case "C":
                    tierMultiplier = 1; // Kein Multiplikator für C-Tier
                    break;
            }

            minAttack = (int)(minAttack * tierMultiplier);
            maxAttack = (int)(maxAttack * tierMultiplier);

            minAttack += requiredLevel;
            maxAttack += requiredLevel;

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
            else if ( item.IsAccessory )
            {
                randomName = NameGenerator.GenerateRandomName( item.Tier, false );
            }
            else
            {
                randomName = "Unknown Item"; // Fallback für den Fall, dass weder Waffe noch Rüstung
            }

            string color = item.Tier switch
            {
                Tier.Ultimate => "orange",
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
            int requiredLevel = DetermineRequiredLevelForTier( tier );

            // Skalierung der Rüstungswerte basierend auf dem Level der Rüstung und dem erforderlichen Level
            double tierMultiplier = 1.0;
            switch ( tier )
            {
                case "Ultimate":
                    tierMultiplier = 1 + (12.0 * requiredLevel / 100);
                    break;
                case "SSS":
                    tierMultiplier = 1 + (8.0 * requiredLevel / 100);
                    break;
                case "SS":
                    tierMultiplier = 1 + (4.0 * requiredLevel / 100);
                    break;
                case "S":
                    tierMultiplier = 1 + (3.0 * requiredLevel / 100);
                    break;
                case "A":
                    tierMultiplier = 1 + (2.0 * requiredLevel / 100);
                    break;
                case "B":
                    tierMultiplier = 1 + (1.0 * requiredLevel / 100);
                    break;
                case "C":
                    tierMultiplier = 1; // Kein Multiplikator für C-Tier
                    break;
            }

            minArmor = (int)(minArmor * tierMultiplier);
            maxArmor = (int)(maxArmor * tierMultiplier);

            minArmor += (requiredLevel / 2);
            maxArmor += (requiredLevel / 2);

            return (minArmor, maxArmor);
        }




        public ItemComponent ConvertPrefabToItemComponent( PrefabFile prefab )
        {
            var obj = SceneUtility.GetPrefabScene( prefab ).Clone();
            obj.NetworkMode = NetworkMode.Object;
            obj.NetworkSpawn();
            obj.Network.DropOwnership();
            

            var itemComponent = obj.Components.Get<ItemComponent>();
            if ( itemComponent == null )
            {
                obj.Destroy();
                return null;
            }
            obj.Enabled = false;
            return itemComponent;
        }









        public void OpenInventory()
        {
          
            if ( !IsOpened )
            {
                IsOpened = true;

                if ( !itemsGenerated ) // Überprüfen, ob die Objekte bereits erstellt wurden
                {
                    // Hier können wir sicherstellen, dass die Items zur StorageBox hinzugefügt werden

                    FullScreenManager.Instance.Display( FullScreenManager.FullScreenPanel.StorageBox );
                    //Player.Local.BlockInputs = true;
                    itemsGenerated = true; // Setzen der Variable, um anzuzeigen, dass die Objekte erstellt wurden
                }

               
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
                DestroyAfterOpen();
            }

            if ( Player.Local != null )
            {
                Player.Local.BlockInputs = false;
            }

            if ( skinnedModelRenderer != null )
            {
                skinnedModelRenderer.Set( "chest_open", false );
            }
            else
            {
                
            }
            DropRemainingItems();

            // Setzen der Variable, um anzuzeigen, dass die Kiste geschlossen ist
        }
       
        public void DestroyAfterOpen()
        {
          
            GameObject?.Destroy( );
        }
      
        private void DropRemainingItems()
        {
            if ( GameObject == null )
            {
                Log.Error( "GameObject is null in DropRemainingItems." );
                return;
            }

            var chestPosition = GameObject.LocalPosition; // Position der Kiste
            foreach ( var item in Items )
            {
                if ( item != null && item.GameObject != null )
                {
                    // Logik zum Werfen des Items auf den Boden an der Position der Kiste
                    var itemPosition = chestPosition;
                    itemPosition.z += 50.5f; // Erhöhe die Z-Achse um 0.5 Einheiten
                    item.GameObject.LocalPosition = itemPosition;
                    item.GameObject.NetworkMode = NetworkMode.Object;
                    item.GameObject.NetworkSpawn();
                    item.GameObject.Network.DropOwnership();
                    item.GameObject.Enabled = true;

                    var itemRigidbody = item.GameObject.Components.GetInDescendantsOrSelf<Rigidbody>();
                    if ( itemRigidbody != null )
                    {
                        itemRigidbody.Enabled = true;
                        itemRigidbody.MotionEnabled = true;
                        itemRigidbody.RigidbodyFlags = RigidbodyFlags.DisableCollisionSounds;
                    }
                    

                    // Wenn das Item eine Rüstung ist, erstelle ein Parcel
                    if ( item.IsArmor )
                    {
                        var itemRenderer = item.GameObject.Components.Get<ModelRenderer>();
                        if ( itemRenderer != null )
                        {
                            itemRenderer.Model = ItemEquipment.Parcel;
                        }
                        else
                        {
                            var newRenderer = item.GameObject.Components.Create<ModelRenderer>();
                            newRenderer.Model = ItemEquipment.Parcel;
                        }
                    }
                }
            }
            Items.Clear(); // Leere die Kiste
        }


    }
}
public class NameGenerator
{
    private static readonly Dictionary<Tier, List<string>> WeaponNames = new Dictionary<Tier, List<string>>
    {
        {Tier.Ultimate, new List<string> {
            "Ultimate", "Supreme", "Divine", "Eternal", "Legendary", "Mythic", "Celestial", "Radiant", "Empyrean", "Godly",}},
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
        {Tier.Ultimate, new List<string> {
            "Ultimate", "Supreme", "Divine", "Eternal", "Legendary", "Mythic", "Celestial", "Radiant", "Empyrean", "Godly",}},
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
        {Tier.Ultimate, new List<string> {
            "Reaper", "Furious", "Slayer", "Eraser", "Wrath",
            "Shiver", "Soulreaper", "Atyus", "Starbreaker", "Cluster", "Neon", "Zapper", "Venuizer", "Typhoon", "fall" } },
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
        {Tier.Ultimate, new List<string> {
            "Regis", "Citan", "guard", "Colo", "Fort",
            " Wall", "Dawn", "Maiden", "Unbreakable", "Shieldrig" } },
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