namespace GeneralGame;
using GeneralGame.HUD;
using Sandbox;



public class ItemInteractable : BaseInteraction
{

	public ItemStorage Storage { get; private set; }
   


    protected override void OnAwake()
    {
        base.OnAwake();
      



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
                    if ( itemInteractable != null && itemInteractable.Storage != null )
                    {
                            itemInteractable.Storage.OpenInventory();
                    }
                    
                },
                Keybind = "use",
                Description = "Open/Close",
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
    




   





}
