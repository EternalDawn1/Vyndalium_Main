namespace GeneralGame.HUD
{
    [StyleSheet]
    public partial class StorageBox : PanelComponent
    {
        public bool IsVisible { get; set; }
       
        private ItemStorage itemStorage;
        private bool visibilityChanged = false;

        
        protected override void OnUpdate()
        {
            bool isOpened = itemStorage?.IsOpened ?? false;

            if ( isOpened != IsVisible ) // Prüft, ob der Zustand synchronisiert werden muss
            {
                ToggleVisibility( isOpened ); // Aktualisiert IsVisible basierend auf dem Zustand von IsOpened
                visibilityChanged = isOpened;
            }
        }
        
        public void ToggleVisibility( bool isOpened )
        {
            // Direkte Anpassung der Sichtbarkeit basierend auf dem Parameter
            IsVisible = isOpened;
            if ( itemStorage != null )
            {
                itemStorage.IsOpened = isOpened; // Direktes Setzen basierend auf dem Parameter
            }
        }
        
        public void OpenStorage()
        {
            IsVisible = true;
            StateHasChanged();
            Log.Info( $"StorageBox opened" );
        }
        protected override int BuildHash()
        {
            
            return HashCode.Combine(
                
            IsVisible
            
            );
            
        }
        
    }
    
}