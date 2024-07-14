namespace GeneralGame;
using GeneralGame.HUD;
using Sandbox;



public class ItemInteractable : BaseInteraction
{

    public ItemStorage Storage { get; private set; }
  
   

    protected override void OnAwake()
    {
        base.OnAwake();
        OnOpen += HandleOpen; // Ereignisabonnent hinzufügen
        OnClose += HandleClose;
      

    }
    
    
    
    protected override void OnStart()
    {
        
        var interactions = Components.GetOrCreate<Interactions>();
        
        
        
            Storage = new ItemStorage();
            interactions.AddInteraction( new Interaction()
            {
                Identifier = "item.openloot",
                Action = ( Player interactor, GameObject obj ) =>
                {
                    
                    var itemInteractable = obj.Components.Get<ItemInteractable>();
                    if ( itemInteractable != null )
                    {
                     
                        if ( itemInteractable.Storage != null )
                        {
                           
                            itemInteractable.Storage.OpenInventory();
                        }
						else
						{
							itemInteractable.Storage.CloseInventory();
						}
						
                        
                    }
                    
                },
                Keybind = "use",
                Description = "Open",
                Stats = "Take",
                Disabled = () => !Player.Local.Inventory.HasSpaceInBackpack(),
                ShowWhenDisabled = () => true,
                Accessibility = AccessibleFrom.All,

            } );
            interactions.AddInteraction( new Interaction()
            {
                Identifier = "item.drop",
                Keybind = "use2",
                Description = "Loot",
                Stats = "Drop",
                ShowWhenDisabled = () => true,
                Accessibility = AccessibleFrom.All,

            } );
        
        
       

    }


    private void HandleOpen()
    {
        // Logik für das Öffnen der Truhe, z.B. visuelles Feedback
        Highlight( true );
    }

    private void HandleClose()
    {
        // Logik für das Schließen der Truhe, z.B. visuelles Feedback entfernen
        Highlight( false );
    }

    public void Highlight( bool shouldHighlight )
    {
        if ( IsProxy )
            return;

        var chestObject = this; // Direkte Nutzung des aktuellen Objekts
        var outline = chestObject.GameObject.Components.Get<HighlightOutline>();

        // Überprüfen, ob der aktuelle Highlight-Zustand sich vom gewünschten Zustand unterscheidet
        bool isCurrentlyHighlighted = outline != null;
        if ( shouldHighlight == isCurrentlyHighlighted )
        {
            // Keine Änderung notwendig, da der gewünschte Zustand bereits erreicht ist
            return;
        }

        if ( shouldHighlight )
        {
            if ( outline == null )
            {
                outline = chestObject.GameObject.Components.Create<HighlightOutline>();
                outline.Color = Color.White;
                outline.Width = 0.5f;
                outline.ObscuredColor = Color.White;
            }
        }
        else
        {
            if ( outline != null )
            {
                outline.Destroy();
            }
        }
    }





}
