namespace GeneralGame;
using GeneralGame.HUD;
using Sandbox;



public class ItemInteractable : BaseInteraction
{

    public ItemStorage Storage { get; set; }
    [Property] public bool IsDoor { get; set; }
    [Property] public List<PrefabFile> PrefabList { get; set; } = new List<PrefabFile>();

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

        AddPrefabsToStorage( PrefabList );
    }

    public void AddPrefabsToStorage( List<PrefabFile> prefabList )
    {
        if ( Storage == null )
        {
            return;
        }

        int index = Storage.Items.Count; // Startindex basierend auf der Anzahl der vorhandenen Elemente
        foreach ( var prefab in prefabList )
        {
            var itemComponent = ConvertPrefabToItemComponent( prefab );
            if ( itemComponent != null )
            {
                Storage.AddItem( itemComponent, index );
                index++;
            }
        }
    }

    private ItemComponent ConvertPrefabToItemComponent( PrefabFile prefab )
    {
        var obj = SceneUtility.GetPrefabScene( prefab ).Clone();
        obj.NetworkMode = NetworkMode.Object;
        obj.NetworkSpawn();
        
        var itemComponent = obj.Components.Get<ItemComponent>();
        if ( itemComponent == null )
        {
            obj.Destroy();
            return null;
        }

        return itemComponent;
    }








}
