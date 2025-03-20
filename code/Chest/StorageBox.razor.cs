namespace GeneralGame.HUD
{
	[StyleSheet]
	public partial class StorageBox : Panel
	{
		public static new bool IsVisible { get; set; }
		
		public ItemStorage itemStorage {	get; set; }
		
		private bool visibilityChanged = false;
		private bool isInitialized = false;
		private static bool IsDragging { get; set; }
		public static StorageBox Instance { get; private set; }

		public List<ItemComponent> selectedItems = new List<ItemComponent>();

		public StorageBox()
		{
			
			Instance = this;
			itemStorage = ItemStorage.Instance ?? new ItemStorage();


			IsVisible = false;
		}
		protected void OnAwake()
		{
			
			if ( itemStorage == null )
			{
				itemStorage = ItemStorage.Instance ?? new ItemStorage();
				itemStorage.IsOpened = false; // Stellen Sie sicher, dass itemStorage anfangs geschlossen ist
				IsVisible = false;
			}
			
		}

		protected  void OnUpdate()
		{
			
			if ( !isInitialized )
			{
				itemStorage.IsOpened = false;
				IsVisible = false;
				isInitialized = true;
				 // Aktualisieren Sie die UI
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
			
		}
	
		private void TakeAllItems()
		{
			var playerInventory = Player.Local.Inventory;
			var itemsToRemove = new List<ItemComponent>();

			foreach ( var item in itemStorage.Items.ToList() )
			{
				if ( playerInventory.GiveItem( item ) )
				{
					Player.Local.PlaySuccessSoundFromPath( "sounds/item.pickup.sound", 0.125f );
					itemStorage.Items.Remove( item );
					itemsToRemove.Add( item );
				}
				else
				{
					// Handle case where player inventory is full or item cannot be added
					break;
				}
			}
			foreach ( var item in itemsToRemove )
			{
				itemStorage.Items.Remove( item );
			}

			// Fügen Sie leere Inventarslots hinzu
			AddEmptySlots( itemsToRemove.Count );
			Inventory.Instance?.OnChanged();
		}
		private void TakeSelectedItems()
		{
			var playerInventory = Player.Local.Inventory;
			var itemsToRemove = new List<ItemComponent>();

			foreach ( var item in selectedItems.ToList() )
			{
				if ( playerInventory.GiveItem( item ) )
				{
					Player.Local.PlaySuccessSoundFromPath( "sounds/item.pickup.sound",0.125f );
					itemStorage.Items.Remove( item );
					itemsToRemove.Add( item );
				}
				else
				{
					// Handle case where player inventory is full or item cannot be added
					break;
				}
			}

			foreach ( var item in itemsToRemove )
			{
				selectedItems.Remove( item );
			}

			// Fügen Sie leere Inventarslots hinzu
			AddEmptySlots( itemsToRemove.Count );

			Inventory.Instance?.OnChanged();
		}

		private void AddEmptySlots( int count )
		{
			for ( int i = 0; i < count; i++ )
			{
				itemStorage.Items.Add( null ); // Fügen Sie einen leeren Slot hinzu
			}
		}
		public void SellSelectedItems()
		{
			foreach ( var item in selectedItems )
			{
				Player.Local.Inventory.RemoveItem( item );
				Player.Local.Vyndalium += item.SellPrice;
			}
			Player.Save();
			selectedItems.Clear();
		}


		public void CloseStorage()
		{
			if ( itemStorage != null && itemStorage.IsOpened )
			{
				itemStorage.IsOpened = false;
				IsVisible = false;
				Log.Info( "Storage closed" );
				 // Aktualisiert die UI
			}
		}



		protected override int BuildHash()
		{
			return HashCode.Combine(
				IsVisible,
				Player.Local.Inventory.BackpackItems.HashCombine( i => i?.GetHashCode() ?? -1 ),
				itemStorage?.Items.HashCombine( i => i?.GetHashCode() ?? -1 ) ?? 0
			);
		}

	}
}