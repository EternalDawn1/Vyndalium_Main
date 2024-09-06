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
        [Property]public IReadOnlyList<ItemComponent> Items => items;
        [Property]public int ItemCount { get; set; }
        



        protected override void OnAwake()
		{
            itemInteractable = this.Components.Get<ItemInteractable>();
			base.OnAwake();
            
		}


        

        public void AddItem(ItemComponent item, int index)
        {
            if (items.Contains(item))
            {
                return;
            }

            if (index >= 0 && index < items.Count)
            {
                if (items[index] == null)
                {
                    items[index] = item;
                    item.State = ItemState.Chest;
                    
                    
                

                }
            }
            else
            {
                items.Add(item);
                item.State = ItemState.Chest;
            }
            Log.Info($"Item hinzugefügt: {item.Name}, Gesamtanzahl der Elemente: {items.Count}");

            // UI aktualisieren
            storageBox?.StateHasChanged();
        }




        public void OpenInventory()
        {
            if (!IsOpened)
            {
                IsOpened = true;

                if (storageBox == null)
                {
                    storageBox = new StorageBox();
                }
                storageBox.ToggleVisibility();

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
                }
            }
        }
        

        

        
    }
}