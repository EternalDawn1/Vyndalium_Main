using GeneralGame;
using GeneralGame.HUD;
using Sandbox;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
namespace GeneralGame;
public class ItemStorage
{

    public bool IsOpened { get; set; }
   
    

    
    private StorageBox storageBox;
    private List<ItemStorage> StorageInteraction;
   
    
    
    
    
    protected void OnAwake()
    {
        Log.Info( "OnAwake aufgerufen." );
        StorageInteraction ??= new();
        storageBox = new StorageBox();
    }

    public void OpenInventory()
    {
        if ( !IsOpened )
        {
            Log.Info( "Öffne Inventar." );
            IsOpened = true;

            if ( storageBox == null )
            {
                storageBox = new StorageBox();
            }
            storageBox.ToggleVisibility( IsOpened ); // Stellt sicher, dass IsVisible in StorageBox aktualisiert wird
            Log.Info( "Inventory opened" );
        }
    }
}