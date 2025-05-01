using Sandbox;
using GeneralGame;

namespace GeneralGame;

public partial class PlayerDresser : Component, Component.INetworkSpawn
{
    [Property] public SkinnedModelRenderer PlayerModel { get; set; }
    [Property] public List<SkinnedModelRenderer> ClothingRenderers { get; set; } = new();

  

    // Referenz zum Player-Objekt, um auf den CameraMode zuzugreifen
  

    // Speichert den letzten Kameramodus, um unnötige Updates zu vermeiden
    private int lastCameraMode = -1;

    


    public void OnNetworkSpawn( Connection owner )
    {
        if ( owner == null )
        {
            return;
        }

        var clothing = ClothingContainer.CreateFromLocalUser();
        clothing.Apply( PlayerModel );

        // Alternativ zu .Has verwenden wir eine andere Methode, um Komponenten zu finden
        var clothingObjects = GameObject.Children.Where( go => go.Components.Get<SkinnedModelRenderer>() != null );

        if ( clothingObjects.Any() )
        {
            ClothingRenderers.Clear();
            foreach ( var clothingObject in clothingObjects )
            {
                var renderer = clothingObject.Components.Get<SkinnedModelRenderer>();
                if ( renderer != null && renderer != PlayerModel )
                {
                    ClothingRenderers.Add( renderer );
                }
            }
        }

        // Stellt sicher, dass die Kamera den "viewer"-Tag ignoriert
        if ( Player.Local != null && Player.Local.PlyCamera != null )
        {
            var cam = Player.Local.PlyCamera;
            if ( !cam.RenderExcludeTags.Contains( "viewer" ) )
            {
                cam.RenderExcludeTags.Add( "viewer" );
            }
        }
    }



    protected override void OnUpdate()
    {
        // Nur aktualisieren, wenn wir eine Referenz zum Player haben
        if ( Player.Local == null || PlayerModel == null )
            return;

        // Nur aktualisieren, wenn sich der Kameramodus geändert hat
        if ( lastCameraMode == Player.Local.CameraMode )
            return;

        lastCameraMode = Player.Local.CameraMode;

        // CameraMode 0 ist typischerweise First-Person
        bool isFirstPerson = (Player.Local.CameraMode == 0);

        // Im First-Person-Modus Kleidung ausblenden, sonst anzeigen
        UpdateClothingVisibility( !isFirstPerson );
    }

    public void UpdateClothingVisibility( bool isVisible )
    {
        if ( Player.Local == null )
        {
            return;
        }

       

       
        bool shouldHide = false;

      
        if ( Player.Local.CameraMode == 0 && !GameObject.IsProxy )
        {
            shouldHide = true; // Verstecke den Körper in First-Person
          
        }
        
        // Setze den viewer-Tag für das PlayerModel
        if ( PlayerModel != null && PlayerModel.GameObject.IsValid() )
        {
            PlayerModel.GameObject.Tags.Set( "viewer", shouldHide );
        }

        // Setze den viewer-Tag für alle Kleidungsstücke
        foreach ( var renderer in ClothingRenderers )
        {
            if ( renderer != null && renderer.GameObject.IsValid() )
            {
                renderer.GameObject.Tags.Set( "viewer", shouldHide );
            }
        }

        // Stelle sicher, dass die Kamera den "viewer"-Tag ignoriert
        if ( Player.Local.PlyCamera != null )
        {
            var cam = Player.Local.PlyCamera;
            if ( !cam.RenderExcludeTags.Contains( "viewer" ) )
            {
                cam.RenderExcludeTags.Add( "viewer" );
            }
        }
    }
    public void RemoveClothing()
    {
        // Hier müssen Sie den Code hinzufügen, der die Kleidung vom BodyRenderer entfernt
        if ( PlayerModel != null )
        {
            PlayerModel.Destroy();
            PlayerModel = null;
        }
    }
}