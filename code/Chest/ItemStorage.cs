using GeneralGame;
using GeneralGame.HUD;
using Sandbox;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
namespace GeneralGame;
public class ItemStorage
{

    public bool IsOpened { get;  set; }

    private StorageBox storageBox;
    private List<ItemStorage> StorageInteraction;





    protected void OnAwake()
    {
        Log.Info( "OnAwake aufgerufen." );
        StorageInteraction ??= new();
        storageBox = new StorageBox();
    }

	private DateTime lastOpenedTime;

	public void OpenInventory()
	{
		if ( !IsOpened )
		{
			Log.Info( "Öffne Inventar." );
			IsOpened = true;
			lastOpenedTime = DateTime.Now;

			if ( storageBox == null )
			{
				storageBox = new StorageBox();
			}
			storageBox.ToggleVisibility();

			Log.Info( "Inventory opened" );
		}
		else
		{
			// Optional: Feedback geben, dass das Inventar bereits geöffnet ist
			Log.Info( "Inventar ist bereits geöffnet." );
		}
	}

	public void CloseInventory()
	{
		if ( IsOpened )
		{
			Log.Info( "Inventar geschlossen." );
			IsOpened = false;
			if ( storageBox != null )
			{
				storageBox.ToggleVisibility();
			}
		}
	}
	public void ResetStorage()
	{
		// Setzen Sie hier den Zustand zurück, z.B.:
		IsOpened = false;
		// Fügen Sie weitere Zurücksetzungen hinzu, falls nötig

		// Optional: Benachrichtigen Sie die StorageBox, dass sie ihre Sichtbarkeit aktualisieren soll
		if ( storageBox != null )
		{
			storageBox.ResetVisibility();
		}
	}
}
