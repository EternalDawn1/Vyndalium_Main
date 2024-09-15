using GeneralGame.HUD;
namespace GeneralGame
{
    public class ShopStorage : Component
    {
        public bool IsOpened { get; set; }
        public bool IsDoorOpen { get; set; }
        [Property] public ShopInteractable shopInteractable { get; set; }
        private ShopPanel shopPanel { get; set; }
        [Property]public List<ItemComponent> AvailableItems { get;private  set; } = new List<ItemComponent>();

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
            LoadPrefabs();
        }

        public void LoadPrefabs()
        {
            // Beispiel-Prefabs laden
            var prefabs = new List<string>
            {
                "prefabs/potion_big.prefab",
                "path/to/sword.prefab"
            };

            foreach ( var prefabPath in prefabs )
            {
                var prefab = ResourceLibrary.Get<PrefabFile>( prefabPath );
                if ( prefab != null )
                {
                    var itemComponent = ConvertPrefabToItemComponent( prefab );
                    if ( itemComponent != null )
                    {
                        AvailableItems.Add( itemComponent );
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

        public void OpenShop()
        {
            if ( !IsOpened )
            {
                LoadPrefabs();
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
                player.Inventory.AddItem( item );
                Log.Info( $"Item {item.Name} gekauft für {item.BuyPrice} Vyndalium." );
                Hudmaster.Instance.ShowNotification( $"Item {item.Name} was bought for {item.BuyPrice} Vyndalium.", "/ui/hud/shop.png" );
            }
            else
            {
                Log.Info( "Nicht genug Vyndalium." );
                Hudmaster.Instance.ShowNotification( "Nicht genug Vyndalium.", "/ui/hud/shop.png" );
            }
        }
        public void SetShopItem( ItemComponent item, int index )
        {
            if ( index < 0 || index >= AvailableItems.Count )
                return;

            AvailableItems[index] = item;
            item.State = ItemState.Shop;
        }

        public void RemoveShopItem( ItemComponent item, int index )
        {
            if ( item == null || index < 0 || index >= AvailableItems.Count )
                return;

            AvailableItems[index] = null;
            item.State = ItemState.None;
        }
    }
}