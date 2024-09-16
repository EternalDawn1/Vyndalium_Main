using Sandbox;
using System;

namespace GeneralGame.HUD
{
    [StyleSheet]
    public partial class ShopPanel : Panel
    {
        public int currentPage = 0;
        private int itemsPerPage = 12; // Anzahl der Items pro Seite
        private int totalPages => (int)Math.Ceiling( (double)Player.Local.Inventory.StorageItems.Count / itemsPerPage );

        private void PreviousPage()
        {
            if ( currentPage > 0 )
            {
                currentPage--;
            }
        }

        private void NextPage()
        {
            if ( currentPage < totalPages - 1 )
            {
                currentPage++;
            }
        }

        public static bool IsDragging { get; private set; }
        public static new bool IsVisible { get; set; }
        public ShopStorage shopStorage { get; private set; }
        private ShopInteractable shopInteractable;
        private bool isInitialized = false;
        public static ShopPanel Instance { get; private set; }

        public enum PanelType
        {
            Inventory,
            StoreItems,
            Weapons,
            Armor,
            Accessories
        }

        public PanelType currentPanel = PanelType.Inventory;

        public void ShowPanel( PanelType panel )
        {
            currentPanel = panel;
        }

        public ShopPanel()
        {
            Instance = this;
            shopStorage = new ShopStorage();
            shopInteractable = new ShopInteractable();
            IsVisible = false;
            shopStorage.LoadPrefabs(); // Prefabs beim Erstellen des Panels laden
        }

        protected void OnAwake()
        {
            if ( shopStorage == null )
            {
                shopStorage = new ShopStorage();
                shopStorage.IsOpened = false;
                IsVisible = false;
                shopStorage.LoadPrefabs(); // Prefabs beim Erwachen laden
            }
        }

        public void OnUpdate()
        {
            if ( !isInitialized )
            {
                shopStorage.IsOpened = false;
                IsVisible = false;
                isInitialized = true;
            }
            else
            {
                bool isOpened = shopStorage?.IsOpened ?? false;

                if ( isOpened != IsVisible )
                {
                    ToggleVisibility();
                }
            }
        }

        public void ToggleVisibility()
        {
            if ( shopStorage == null )
            {
                return;
            }

            bool newState = !shopStorage.IsOpened;
            shopStorage.IsOpened = newState;
            IsVisible = newState;
        }

        public void OpenShop()
        {
            if ( shopStorage != null && !shopStorage.IsOpened )
            {
                shopStorage.IsOpened = true;
                IsVisible = true;
            }
        }

        public void CloseShop()
        {
            if ( shopStorage != null && shopStorage.IsOpened )
            {
                shopStorage.IsOpened = false;
                IsVisible = false;
            }
        }

        public void ClosePanel()
        {
            CloseShop();
        }

        public void ResetVisibility()
        {
            IsVisible = false;
        }

        protected override int BuildHash()
        {
            return HashCode.Combine(
                IsVisible,
                Player.Local.Inventory.BackpackItems.HashCombine( i => i?.GetHashCode() ?? -1 ),
                shopStorage?.AvailableItems.HashCombine( i => i?.GetHashCode() ?? -1 ) ?? 0
            );
        }

        public void SetPanelVisibility( bool isVisible )
        {
            IsVisible = isVisible;
        }
    }
}