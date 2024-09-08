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

        private StorageBox storageBox;
        [Property] ItemInteractable itemInteractable { get; set; }
        [Property] public List<ItemComponent> items { get; set; } = new List<ItemComponent>();
        [Property] public List<ItemComponent> Items => items;
        [Property] public int ItemCount => items.Count;
        public Vector3 Position { get; set; }

        protected override void OnAwake()
        {
            itemInteractable = this.Components.Get<ItemInteractable>();
            base.OnAwake();
        }

        public void AddItem( ItemComponent item, int index )
        {
            if ( item == null )
            {
               
                return;
            }

            if ( index < 0 || index > items.Count )
            {
               
                return;
            }

            items.Insert( index, item );
       
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
                Player.Local.BlockInputs = true;

                // Prefabs zur Storage hinzufügen

                // Initialisiere die Slots und zeige sie an

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