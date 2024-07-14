using GeneralGame;
using GeneralGame.HUD;
using Sandbox;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using static Sandbox.GameObjectSystem;
namespace GeneralGame;
public class ItemStorage
{

    public bool IsOpened { get;  set; }

    private StorageBox storageBox;

    private List<ItemStorage> StorageInteraction;


	public List<ItemComponent> Items { get; set; }

	public ItemStorage()
	{
		Items = new List<ItemComponent>();
	}
	public void AddItem( ItemComponent item )
	{
		Items.Add( item );
	}

	// Entfernen eines Items
	public void RemoveItem( ItemComponent item )
	{
		Items.Remove( item );
	}

	// Entfernen eines Items an einem bestimmten Index
	public void RemoveItemAt( int index )
	{
		if ( index >= 0 && index < Items.Count )
		{
			Items.RemoveAt( index );
		}
	}

	// Initialisieren der Liste mit einer bestimmten Anzahl von Slots (leeren Items)
	public void InitializeSlots( int numberOfSlots )
	{
		for ( int i = 0; i < numberOfSlots; i++ )
		{
			// Fügen Sie hier die Logik zum Initialisieren und Hinzufügen eines neuen ItemComponent hinzu
			Items.Add( new ItemComponent() );
		}
	}


	protected void OnAwake()
    {
        Log.Info( "OnAwake aufgerufen." );
        StorageInteraction ??= new();
        storageBox = new StorageBox();
		InitializeSlots( 10 );
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
			IsOpened = true;
		}
		else
		{
			// Schließe das Inventar, wenn es bereits geöffnet ist
			CloseInventory();
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
			IsOpened = false;
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
	public void ToggleInventory()
	{
		if ( IsOpened )
		{
			CloseInventory();
		}
		else
		{
			OpenInventory();
		}
	}
}
