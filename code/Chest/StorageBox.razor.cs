using Sandbox.UI;

namespace GeneralGame.HUD
{
    [StyleSheet]
    public partial class StorageBox : PanelComponent
    {
		public static bool IsVisible { get; set; }
       
        private  ItemStorage itemStorage;
		
		private bool visibilityChanged = false;
		private bool isInitialized = false;
		

		public StorageBox()
		{
			// Initialisierung von itemStorage
			itemStorage = GetItemStorageInstance();
			IsVisible = false; // Stellen Sie sicher, dass die StorageBox anfangs nicht sichtbar ist
		}
		public StorageBox( ItemStorage itemStorage )
		{
			this.itemStorage = itemStorage;
			IsVisible = false; // Stellen Sie sicher, dass die StorageBox anfangs nicht sichtbar ist
		}

		// Beispielmethoden
		private ItemStorage GetItemStorageInstance()
		{
			// Implementieren Sie die Logik, um eine Instanz von ItemStorage zu erhalten.
			// Dies könnte das Abrufen einer bestehenden Instanz aus einem Manager oder das Erstellen einer neuen Instanz sein.
			return new ItemStorage();
		}
		protected override void OnAwake()
		{

			base.OnAwake();
			if ( itemStorage != null )
			{
				itemStorage.IsOpened = false; // Stellen Sie sicher, dass itemStorage anfangs geschlossen ist
				IsVisible = false;
				itemStorage = GetItemStorageInstance();
				itemStorage.InitializeSlots( 10 );// Stellen Sie sicher, dass die Komponente anfangs nicht sichtbar ist
			}
		}


		protected override void OnUpdate()
		{
			
			if ( !isInitialized )
			{
				// Entfernen Sie die Initialisierung von itemStorage.IsOpened und IsVisible auf true
				// und setzen Sie sie stattdessen auf false, wenn das Ihr gewünschtes Anfangsverhalten ist.
				itemStorage.IsOpened = false;
				IsVisible = false;
				isInitialized = true;
				// Markieren Sie, dass die Initialisierung erfolgt ist
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
                
            IsVisible
            
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
