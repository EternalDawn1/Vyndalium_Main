using System.ComponentModel.Design.Serialization;
using System.Diagnostics.Metrics;
using Sandbox.UI;

namespace GeneralGame.HUD
{
    [StyleSheet]
    public partial class StorageBox : PanelComponent
    {
		public static bool IsVisible { get; set; }
       
        private  ItemStorage itemStorage { get; set; }
		private ItemInteractable itemInteractable;
		
		private bool visibilityChanged = false;
		private bool isInitialized = false;
		private static bool IsDragging { get;  set; }



		public StorageBox()
		{
			itemStorage = GetItemStorageInstance();
			itemInteractable = new ItemInteractable
			{
				Storage = itemStorage
			};

			IsVisible = false;

			// Debug-Ausgabe hinzufügen
			Log.Info( $"Initialisierung von StorageBox. Anzahl der Items in itemStorage: {itemStorage.Items.Count}" );

			List<ItemComponent> itemList = GetItemComponentList(); // Erhalte die Prefab-Liste
			AddItemsFromComponents( itemList ); // Stellen Sie sicher, dass die StorageBox anfangs nicht sichtbar ist
		}

		private List<ItemComponent> GetItemComponentList()
		{
			Log.Info( $"Anzahl der Items in itemStorage: {itemStorage.Items.Count}" );
			foreach ( var item in itemStorage.Items )
			{
				Log.Info( $"Item: {item?.Name}" );
			}
			return itemStorage.Items
				.Where( item => item != null ) // Filtere ungültige Items
				.ToList();
		}

		private void AddItemsFromComponents( List<ItemComponent> itemList )
		{
			if ( itemList == null || itemList.Count == 0 )
			{
				Log.Info( "Keine Items in der Liste." );
				return;
			}

			foreach ( var itemComponent in itemList )
			{
				if ( itemComponent != null )
				{
					itemStorage.AddItem( itemComponent, itemStorage.Items.Count );
					Log.Info( $"Item hinzugefügt: {itemComponent.Name}, Gesamtanzahl der Elemente: {itemStorage.Items.Count}" );
				}
				else
				{
					Log.Error( "Fehler beim Hinzufügen des Items." );
				}
			}
			StateHasChanged(); // Aktualisiert die UI
		}

		// Beispielmethoden
		private static ItemStorage GetItemStorageInstance()
		{
			ItemStorage storage = new ItemStorage();
			var itemInteractable = new ItemInteractable { Storage = storage };

			// Fügen Sie alle vorhandenen Items aus der Liste hinzu
			foreach ( var item in storage.Items )
			{
				storage.AddItem( item, storage.Items.Count );
				Log.Info( $"Item hinzugefügt: {item.Name}, Gesamtanzahl der Elemente: {storage.Items.Count}" );
			}

			return storage;
		}
		protected override void OnAwake()
		{
			base.OnAwake();
			if ( itemStorage != null )
			{
				itemStorage.IsOpened = false; // Stellen Sie sicher, dass itemStorage anfangs geschlossen ist
				IsVisible = false;
			}
			
		}


		protected override void OnUpdate()
		{
			if ( !isInitialized )
			{
				itemStorage.IsOpened = false;
				IsVisible = false;
				isInitialized = true;
				StateHasChanged(); // Aktualisieren Sie die UI
			}
			else
			{
				bool isOpened = itemStorage?.IsOpened ?? false;

				if ( isOpened != IsVisible ) // Prüft, ob der Zustand synchronisiert werden muss
				{
					ToggleVisibility(); // Aktualisiert IsVisible basierend auf dem Zustand von IsOpened
					visibilityChanged = isOpened;
				}
			}
		}

		public void ToggleVisibility()
		{
			if ( itemStorage == null )
			{
				return;
			}

			// Umschalten des Zustands
			bool newState = !itemStorage.IsOpened;
			itemStorage.IsOpened = newState;
			IsVisible = newState;

			// Optional: Aufrufen von StateHasChanged(), wenn Sie in einer Blazor-Komponente sind, um die UI zu aktualisieren
			StateHasChanged();
		}

		public void OpenStorage()
		{
			if ( itemStorage != null && !itemStorage.IsOpened )
			{
				itemStorage.IsOpened = true;
				IsVisible = true;
				// Optional: UI aktualisieren
				StateHasChanged();
			}
		}

		public void CloseStorage()
		{
			if ( itemStorage != null && itemStorage.IsOpened )
			{
				itemStorage.IsOpened = false;
				IsVisible = false;
				StateHasChanged(); // Aktualisiert die UI
			}
		}
		protected override int BuildHash()
		{
			return HashCode.Combine(
				IsVisible,
				Player.Local.Inventory.BackpackItems.HashCombine(i => i?.GetHashCode() ?? -1),
				itemStorage?.Items.HashCombine(i => i?.GetHashCode() ?? -1) ?? 0
			);
		}
		public void ClosePanel()
		{
			CloseStorage(); // Ruft die Methode zum Schließen des Speichers auf
		}
		public void ResetVisibility()
		{
			IsVisible = false;
			StateHasChanged(); // Aktualisiert die UI
		}
		public void SetPanelVisibility( bool isVisible )
		{
			if ( Player.Local.BlockMovements )
			{
				return; // Frühzeitiger Rückkehr, um Bewegung zu verhindern
			}
		}

	}
    
}
