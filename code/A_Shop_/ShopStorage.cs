using GeneralGame.HUD;
namespace GeneralGame
{
    public class ShopStorage : Component
    {
        private static ShopStorage instance;
        public bool IsOpened { get; set; }
        public bool IsDoorOpen { get; set; }
        [Property] public ShopInteractable shopInteractable { get; set; }
        private ShopPanel shopPanel { get; set; }
        [Property] public List<ItemComponent> AvailableItems { get; private set; } = new List<ItemComponent>();
        [Property] public List<ItemComponent> WeaponItems { get; private set; } = new List<ItemComponent>();
        [Property] public List<ItemComponent> ArmorItems { get; private set; } = new List<ItemComponent>(); // Hinzugefügt
        [Property] public List<ItemComponent> AccessoryItems { get; private set; } = new List<ItemComponent>(); // Hinzugefügt
        [Property] public List<ItemComponent> ConsumableItems { get; private set; } = new List<ItemComponent>(); // Hinzugefügt
        [Property] public List<ItemComponent> MaterialItems { get; private set; } = new List<ItemComponent>(); // Hinzugefügt

        [Property] public List<ItemComponent> UpgradeItems { get; private set; } = new List<ItemComponent>();

 
 
     
       
        public static ShopStorage Instance
        {
            get
            {
                if ( instance == null )
                {
                    instance = new ShopStorage();
                }
                return instance;
            }
        }
        private bool prefabsLoaded = false;
        protected override void OnAwake()
        {
            shopInteractable = this.Components.Get<ShopInteractable>();
            base.OnAwake();
            if ( shopPanel == null )
            {
                shopPanel = new ShopPanel();
                
            }
           
            
        }
		protected override void OnStart()
		{
			base.OnStart();
           shopInteractable = new ShopInteractable();
           
            
		}
		public ShopStorage() 
        {
            
            AvailableItems = new List<ItemComponent>();
            WeaponItems = new List<ItemComponent>();
            ArmorItems = new List<ItemComponent>(); // Hinzugefügt
            AccessoryItems = new List<ItemComponent>(); // Hinzugefügt
            ConsumableItems = new List<ItemComponent>(); // Hinzugefügt
            MaterialItems = new List<ItemComponent>();
            UpgradeItems = new List<ItemComponent>();
        }

        public void LoadPrefabs()
        {
            if ( prefabsLoaded ) return;
            
            var potionPrefabs = new List<string>
            {
                "prefabs/potions/potion_big.prefab",
                "prefabs/potions/potion_mid.prefab",
                "prefabs/potions/potion_small.prefab",
                
            };

            var weaponPrefabs = new List<string>
            {


                "prefabs/pickupammo.prefab",
                "prefabs/weapons/facepunch/usp/uspc.prefab",
                "prefabs/weapons/facepunch/mp5/mp5c.prefab",
                "prefabs/weapons/facepunch/shotgun/shotgunc.prefab",



            };
            var armorPrefabs = new List<string> // Hinzugefügt
            {
                "prefabs/clothes/helmet/helmet.prefab",
                "prefabs/clothes/armor/armor.prefab",
                "prefabs/clothes/legarmor/legarmor.prefab",

            };
            var accessoryPrefabs = new List<string> // Hinzugefügt
            {
                "prefabs/entitys/chestsystem/test_chest.prefab",
            };
            var consumablePrefabs = new List<string> // Hinzugefügt
            {
               
            };
            var materialPrefabs = new List<string>
            {
                "prefabs/items/wood_log.prefab",
                
            };

            LoadPrefabsFromList( potionPrefabs, AvailableItems );
            LoadPrefabsFromList( weaponPrefabs, WeaponItems );
            LoadPrefabsFromList( armorPrefabs, ArmorItems );
            LoadPrefabsFromList( accessoryPrefabs, AccessoryItems );
            LoadPrefabsFromList( consumablePrefabs, ConsumableItems );
            LoadPrefabsFromList( materialPrefabs, MaterialItems );

            prefabsLoaded = true;
        }
        private void LoadPrefabsFromList( List<string> prefabPaths, List<ItemComponent> targetList )
        {
            foreach ( var prefabPath in prefabPaths )
            {
                var prefab = ResourceLibrary.Get<PrefabFile>( prefabPath );
                if ( prefab != null )
                {
                    var itemComponent = ConvertPrefabToItemComponent( prefab );
                    if ( itemComponent != null && !targetList.Contains( itemComponent ) )
                    {
                        itemComponent.GameObject.Enabled = false;
                        targetList.Add( itemComponent );
                    }
                }
            }
        }
        public void AddItem( ItemComponent item, int index )
        {
            if ( item == null )
            {
                return;
            }

            AvailableItems.Insert( index, item );

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
        public void OpenLeaderboard()
        {
            if ( !IsOpened )
            {
                FullScreenManager.Instance.Display( FullScreenManager.FullScreenPanel.Leaderboard );
                //Player.Local.BlockInputs = true;
            }
            else
            {
                CloseLeaderboard();
            }
        }
        public void CloseLeaderboard()
        {
            Player.Local.BlockInputs = false;
            FullScreenManager.Instance.Display( FullScreenManager.FullScreenPanel.InGameHud );
        }
        public void OpenMisson()
        {
            if ( !IsOpened )
            {
                FullScreenManager.Instance.Display( FullScreenManager.FullScreenPanel.MissonPanel );
                //Player.Local.BlockInputs = true;
            }
            else
            {
                CloseMisson();
            }
        }
        public void CloseMisson()
        {
            
            Player.Local.BlockInputs = false;
            FullScreenManager.Instance.Display( FullScreenManager.FullScreenPanel.InGameHud );
        }

        public void OpenShop()
        {
            if ( !IsOpened )
            {
                
                FullScreenManager.Instance.Display( FullScreenManager.FullScreenPanel.ShopPanel );
                //Player.Local.BlockInputs = true;
                IsOpened = true;
            }
            else
            {
                CloseShop();
            }
        }

        
        public void CloseShop()
        {
            Player.Save();
            Player.Local.BlockInputs = false;
            FullScreenManager.Instance.Display( FullScreenManager.FullScreenPanel.InGameHud );
            IsOpened = false;
        }

        public void BuyItem( ItemComponent item )
        {
            var player = Player.Local;
            if ( player.Vyndalium >= item.BuyPrice )
            {
                player.Vyndalium -= item.BuyPrice;

                // Generieren Sie ein neues Item basierend auf dem gekauften Item
                var newItem = GenerateNewItem( item );
                player.Inventory.AddItem( newItem );
                newItem.GameObject.Enabled = false;

                Log.Info( $"Item {newItem.Name} gekauft für {newItem.BuyPrice} Vyndalium." );
                Hudmaster.Instance.ShowNotification( $"Item {newItem.Name} was bought for {newItem.BuyPrice} Vyndalium.", "/ui/hud/shop.png" );
            }
            else
            {
                Log.Info( "Nicht genug Vyndalium." );
                Hudmaster.Instance.ShowNotification( "Nicht genug Vyndalium.", "/ui/hud/shop.png" );
            }
        }
        private ItemComponent GenerateNewItem( ItemComponent item )
        {
            // Erstellen Sie eine neue Instanz des Items basierend auf dem vorhandenen Item
            var newItem = new ItemComponent
            {
                Name = item.Name,
                State = item.State,
                SellPrice = item.SellPrice,
                BuyPrice = item.BuyPrice,
                MinAttackValue = item.MinAttackValue,
                MaxAttackValue = item.MaxAttackValue,
                STG = item.STG,
                HE = item.HE,
                DEX = item.DEX,
                PER = item.PER,
                INT = item.INT,
                Mana = item.Mana,
                Health = item.Health,
                ItemLevel = item.ItemLevel,
                CritHitDamage = item.CritHitDamage,
                CritHitChance = item.CritHitChance,
                AbilityHaste = item.AbilityHaste,
                AttackPower = item.AttackPower,
                MagicPower = item.MagicPower,
                FireRate = item.FireRate,
                BulletSpeed = item.BulletSpeed,
                Tier = item.Tier,
                DamageBalance = item.DamageBalance,
                Durability = item.Durability,
                AttackSpeed = item.AttackSpeed,
                MoveSpeed = item.MoveSpeed,
                Armor = item.Armor,
                MagicDefense = item.MagicDefense,
                Evasion = item.Evasion,
                Cover = item.Cover,
                BonusEXP = item.BonusEXP,
                BonusScore = item.BonusScore,
                BonusVyndalium = item.BonusVyndalium,
                Tenacity = item.Tenacity,
                StunResistance = item.StunResistance,
                BlindResistance = item.BlindResistance,
                SlowResistence = item.SlowResistence,
                FireResistence = item.FireResistence,
                BleedResistance = item.BleedResistance,
                PoisonResistence = item.PoisonResistence,
                IceResistence = item.IceResistence,
                LightningResistence = item.LightningResistence,
                HolyResistence = item.HolyResistence
            };
            Log.Info( $"Neues Item {newItem.Name} wurde generiert." );

            return newItem;
        }

    }
}