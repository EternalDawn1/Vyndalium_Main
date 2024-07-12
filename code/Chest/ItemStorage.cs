using GeneralGame;
using Sandbox;
using System.Collections.Generic;
namespace GeneralGame;
public class ItemStorage
{
    public List<ItemComponent> Items { get; set; } = new List<ItemComponent>();
    public bool IsOpened { get; private set; } = false;

    

    public void OpenInventory()
    {
        
        if ( !IsOpened )
        {
            // Logik zum Anzeigen der UI mit den Items im Inventar
            IsOpened = true;
            
        }
    }
}