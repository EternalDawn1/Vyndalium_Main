namespace GeneralGame;
using GeneralGame.HUD;
using Sandbox;

public class ShopInteractable : BaseInteraction
{
    public ShopStorage Storage { get; set; }
    [Property]public bool Missions { get; set; }
    

    protected override void OnStart()
    {
        var interactions = Components.GetOrCreate<Interactions>();

      

        if ( Missions )
        {
            interactions.AddInteraction( new Interaction()
            {
                Identifier = "shop.special",
                Action = ( Player interactor, GameObject obj ) =>
                {
                    Storage = Components.GetOrCreate<ShopStorage>();
                    var shopInteractable = obj.Components.Get<ShopInteractable>();
                    if ( shopInteractable != null && shopInteractable.Storage != null )
                    {
                        // Spezielle Interaktion basierend auf Boolean
                        shopInteractable.Storage.OpenLeaderboard();
                    }
                },
                Keybind = "use2",
                Description = "Leaderboard",
                Stats = "Leaderboard",
                ShowWhenDisabled = () => true,
                Accessibility = AccessibleFrom.All,
            } );
            interactions.AddInteraction( new Interaction()
            {
                Identifier = "leader.open",
                Action = ( Player interactor, GameObject obj ) =>
                {
                    Storage = Components.GetOrCreate<ShopStorage>();
                    var shopInteractable = obj.Components.Get<ShopInteractable>();
                    if ( shopInteractable != null && shopInteractable.Storage != null )
                    {
                        shopInteractable.Storage.OpenMisson();
                    }
                },
                Keybind = "use",
                Description = "Missons",
                Stats = "Missions",
                ShowWhenDisabled = () => true,
                Accessibility = AccessibleFrom.All,
            } );
            // Interaktionen, wenn Missions true ist
            

            
        }
        else
        {
            // Interaktionen, wenn Missions false ist
            interactions.AddInteraction( new Interaction()
            {
                Identifier = "shop.open",
                Action = ( Player interactor, GameObject obj ) =>
                {
                    var shopInteractable = obj.Components.Get<ShopInteractable>();
                    Storage = Components.GetOrCreate<ShopStorage>();
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
                Identifier = "shop.quests",
                Action = ( Player interactor, GameObject obj ) =>
                {
                    var shopInteractable = obj.Components.Get<ShopInteractable>();
                    Storage = Components.GetOrCreate<ShopStorage>();

                    if ( shopInteractable != null && shopInteractable.Storage != null )
                    {
                        shopInteractable.Storage.OpenQuest();
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