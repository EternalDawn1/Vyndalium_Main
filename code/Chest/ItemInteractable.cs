namespace GeneralGame;
using GeneralGame.HUD;
using Sandbox;



public class ItemInteractable : BaseInteraction
{

    public ItemStorage Storage { get; set; }
    [Property] public string RagdollPrefabPath { get; set; } = "models/npcs/slime/chest.prefab";
    [Property] public bool IsDoor { get; set; }
    [Property]public bool IsBossChest { get; set; } = false;

    public void Interact()
    {
        if ( Storage == null )
        {
            Storage = Components.Create<ItemStorage>();
            Storage.IsBossChest = IsBossChest; // Attribut übernehmen
            Storage.LoadPrefabs(); // Prefabs nur einmal laden
        }

        if ( IsBossChest )
        {
            Log.Info( "BossChest" );
            Storage.LoadBossItems();
        }
        
    }
    protected override void OnStart()
    {
       
        var interactions = Components.GetOrCreate<Interactions>();

       

        

        
        
        {
            interactions.AddInteraction( new Interaction()
            {
               
                Identifier = "item.openloot",
                Action = ( Player interactor, GameObject obj ) =>
                {
                    Log.Info( "XXXXXXXXXXXXXX" );
                    var itemInteractable = obj.Components.Get<ItemInteractable>();
                    if ( itemInteractable != null )
                    {
                        if ( itemInteractable.Storage == null )
                        {
                            Interact(); // Attribut übernehmen
                        }

                        if ( itemInteractable.Storage != null )
                        {
                            itemInteractable.Storage.OpenInventory();

                            var ragdollPrefab = ResourceLibrary.Get<PrefabFile>( RagdollPrefabPath );
                            if ( ragdollPrefab != null )
                            {
                                var ragdoll = SceneUtility.GetPrefabScene( ragdollPrefab ).Clone();
                                if ( ragdoll != null )
                                {
                                    ragdoll.WorldPosition = WorldPosition;
                                    ragdoll.WorldRotation = WorldRotation;
                                    ragdoll.NetworkSpawn();
                                }
                                else
                                {
                                    Log.Error( "Failed to spawn ragdoll from prefab." );
                                }
                            }
                            else
                            {
                                Log.Error( $"Failed to load prefab: {RagdollPrefabPath}" );
                            }

                            Task.Delay( 10000 );
                            //itemInteractable.Storage.DestroyAfterOpen();
                        }
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
