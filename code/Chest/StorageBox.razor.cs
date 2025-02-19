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

		public void OnUpdate()
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
			foreach ( var item in itemStorage.Items.ToList() )
			{
				if ( playerInventory.GiveItem( item ) )
				{
					itemStorage.Items.Remove( item );
				}
				else
				{
					// Handle case where player inventory is full or item cannot be added
					break;
				}
			}
			Inventory.Instance?.OnChanged();
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