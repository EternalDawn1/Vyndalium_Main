using GeneralGame.HUD;
namespace GeneralGame
{
    public class ShopStorage : Component
    {
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

        public ShopStorage() 
        {
            AvailableItems = new List<ItemComponent>();
            WeaponItems = new List<ItemComponent>();
            ArmorItems = new List<ItemComponent>(); // Hinzugefügt
            AccessoryItems = new List<ItemComponent>(); // Hinzugefügt
            ConsumableItems = new List<ItemComponent>(); // Hinzugefügt
            MaterialItems = new List<ItemComponent>();
        }

        public void LoadPrefabs()
        {
            if ( prefabsLoaded ) return;
            // Beispiel-Prefabs laden
            var potionPrefabs = new List<string>
            {
                "prefabs/potion_big.prefab"
            };

            var weaponPrefabs = new List<string>
            {
                "path/to/sword.prefab",
                "prefabs/weapons/aksu/s.prefab"
            };
            var armorPrefabs = new List<string> // Hinzugefügt
            {
                "path/to/armor.prefab",
                "prefabs/armor/aksu/s.prefab"
            };
            var accessoryPrefabs = new List<string> // Hinzugefügt
            {
                "path/to/accessory.prefab",
                "prefabs/accessory/aksu/s.prefab"
            };
            var consumablePrefabs = new List<string> // Hinzugefügt
            {
                "path/to/consumable.prefab",
                "prefabs/consumable/aksu/s.prefab"
            };
            var materialPrefabs = new List<string>
            {
                "path/to/material.prefab",
                "prefabs/material/aksu/s.prefab"
            };

            foreach ( var prefabPath in potionPrefabs )
            {
                var prefab = ResourceLibrary.Get<PrefabFile>( prefabPath );
                if ( prefab != null )
                {
                    var itemComponent = ConvertPrefabToItemComponent( prefab );
                    if ( itemComponent != null )
                    {
                        itemComponent.GameObject.Enabled = false;
                        AvailableItems.Add( itemComponent );
                    }
                }
            }
            foreach ( var prefabPath in weaponPrefabs )
            {
                var prefab = ResourceLibrary.Get<PrefabFile>( prefabPath );
                if ( prefab != null )
                {
                    var itemComponent = ConvertPrefabToItemComponent( prefab );
                    if ( itemComponent != null )
                    {
                        itemComponent.GameObject.Enabled = false;
                        WeaponItems.Add( itemComponent );
                    }
                }
            }
            foreach ( var prefabPath in armorPrefabs ) // Hinzugefügt
            {
                var prefab = ResourceLibrary.Get<PrefabFile>( prefabPath );
                if ( prefab != null )
                {
                    var itemComponent = ConvertPrefabToItemComponent( prefab );
                    if ( itemComponent != null )
                    {
                        itemComponent.GameObject.Enabled = false;
                        ArmorItems.Add( itemComponent );
                    }
                }
            }
            foreach ( var prefabPath in accessoryPrefabs ) // Hinzugefügt
            {
                var prefab = ResourceLibrary.Get<PrefabFile>( prefabPath );
                if ( prefab != null )
                {
                    var itemComponent = ConvertPrefabToItemComponent( prefab );
                    if ( itemComponent != null )
                    {
                        itemComponent.GameObject.Enabled = false;
                        AccessoryItems.Add( itemComponent );
                    }
                }
            }
            foreach ( var prefabPath in consumablePrefabs ) // Hinzugefügt
            {
                var prefab = ResourceLibrary.Get<PrefabFile>( prefabPath );
                if ( prefab != null )
                {
                    var itemComponent = ConvertPrefabToItemComponent( prefab );
                    if ( itemComponent != null )
                    {
                        itemComponent.GameObject.Enabled = false;
                        ConsumableItems.Add( itemComponent );
                    }
                }
            }
            foreach ( var prefabPath in materialPrefabs )
            {
                var prefab = ResourceLibrary.Get<PrefabFile>( prefabPath );
                if ( prefab != null )
                {
                    var itemComponent = ConvertPrefabToItemComponent( prefab );
                    if ( itemComponent != null )
                    {
                        itemComponent.GameObject.Enabled = false;
                        MaterialItems.Add( itemComponent );
                    }
                }
            }
           
            prefabsLoaded = true;
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

        public void OpenShop()
        {
            if ( !IsOpened )
            {
                
                FullScreenManager.Instance.Display( FullScreenManager.FullScreenPanel.ShopPanel );
                Player.Local.BlockInputs = true;
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
                DMG = item.DMG,
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