namespace GeneralGame;
using GeneralGame.HUD;
using Sandbox;



public class ItemInteractable : BaseInteraction
{

    public ItemStorage Storage { get; set; }
    [Property] public bool IsDoor { get; set; }
   

    protected override void OnStart()
    {
        var interactions = Components.GetOrCreate<Interactions>();

        Storage = Components.Create<ItemStorage>();

        if ( IsDoor )
        {
            interactions.AddInteraction( new Interaction()
            {
                Identifier = "door.toggle",
                Action = ( Player interactor, GameObject obj ) =>
                {
                    var itemInteractable = obj.Components.Get<ItemInteractable>();
                    if ( itemInteractable != null && itemInteractable.Storage != null )
                    {
                        // Tür-Interaktion
                    }
                },
                Keybind = "use",
                Description = "Open/Close Door",
                Stats = "Toggle",
                ShowWhenDisabled = () => true,
                Accessibility = AccessibleFrom.All,
            } );
        }
        else
        {
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

    








}
