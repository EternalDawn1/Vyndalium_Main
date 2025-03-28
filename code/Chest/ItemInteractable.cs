namespace GeneralGame;
using GeneralGame.HUD;
using Sandbox;



public class ItemInteractable : BaseInteraction , IMinimapElement
{
    public bool IsVisible( Player viewer )
    {
        // Logik, um zu bestimmen, ob der NPC für den Spieler sichtbar ist
        return true;
    }
    public ItemStorage Storage { get; set; }
    [Property] public string RagdollPrefabPath { get; set; } = "models/npcs/slime/chest.prefab";
    [Property] public bool IsDoor { get; set; }
    [Property]public bool IsBossChest { get; set; } = false;
    [Property] public string RequiredTier { get; set; } = "C";

    public void Interact()
    {
        if ( Storage == null )
        {
            Storage = Components.Create<ItemStorage>();
            Storage.IsBossChest = IsBossChest;
          
            Storage.LoadPrefabs(); // Prefabs nur einmal laden
        }

        if ( IsBossChest )
        {
       
            int minLevel = 0;
            int maxLevel = 100;
            int playerLevel = Storage.GetPlayerLevel();
            Storage.LoadBossTierPrefabs( playerLevel,minLevel,maxLevel );
        }
        
    }
    protected override void OnStart()
    {
       
        var interactions = Components.GetOrCreate<Interactions>();





        DetermineAndSetRequiredTier();

        {
            interactions.AddInteraction( new Interaction()
            {
               
                Identifier = "item.openloot",
                Action = ( Player interactor, GameObject obj ) =>
                {
                 
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
                                    ragdoll.Network.DropOwnership();
                                }
                                else
                                {
                                 
                                }
                            }
                            else
                            {
                                
                            }

                           
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

            
        }
        

        
    }
    private void DetermineAndSetRequiredTier()
    {
        // Hier können Sie die Logik hinzufügen, um das Tier basierend auf bestimmten Bedingungen zu bestimmen
        // Zum Beispiel:
        if ( IsBossChest )
        {
            RequiredTier = "SSS";
        }
        else if ( this is ItemComponent itemComponent )
        {
            switch ( itemComponent.Tier )
            {
                case Tier.C:
                    RequiredTier = "C";
                    break;
                case Tier.B:
                    RequiredTier = "B";
                    break;
                case Tier.A:
                    RequiredTier = "A";
                    break;
                case Tier.S:
                    RequiredTier = "S";
                    break;
                case Tier.SS:
                    RequiredTier = "SS";
                    break;
                case Tier.SSS:
                    RequiredTier = "SSS";
                    break;
                default:
                    RequiredTier = "C"; // Standardwert
                    break;
            }
        }
        else
        {
            RequiredTier = "C"; // Standardwert, falls keine Bedingungen erfüllt sind
        }
    }










}
