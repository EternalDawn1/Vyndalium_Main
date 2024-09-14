namespace GeneralGame;
using GeneralGame.HUD;
using Sandbox;

public class ShopInteractable : BaseInteraction
{
    public ShopStorage Storage { get; set; }
   

    protected override void OnStart()
    {
        var interactions = Components.GetOrCreate<Interactions>();

        Storage = Components.Create<ShopStorage>();

        
        
        {
            interactions.AddInteraction( new Interaction()
            {
                Identifier = "shop.open",
                Action = ( Player interactor, GameObject obj ) =>
                {
                    var shopInteractable = obj.Components.Get<ShopInteractable>();
                    if ( shopInteractable != null && shopInteractable.Storage != null )
                    {
                        shopInteractable.Storage.OpenShop();
                    }
                },
                Keybind = "use",
                Description = "Open/Close Shop",
                Stats = "Shop",
                ShowWhenDisabled = () => true,
                Accessibility = AccessibleFrom.All,
            } );

            interactions.AddInteraction( new Interaction()
            {
                Identifier = "shop.buy",
                Action = ( Player interactor, GameObject obj ) =>
                {
                    var shopInteractable = obj.Components.Get<ShopInteractable>();
                    if ( shopInteractable != null && shopInteractable.Storage != null )
                    {
                        // Kauf-Interaktion
                    }
                },
                Keybind = "use2",
                Description = "Quests",
                Stats = "Buy",
                ShowWhenDisabled = () => true,
                Accessibility = AccessibleFrom.All,
            } );
        }
    }
}