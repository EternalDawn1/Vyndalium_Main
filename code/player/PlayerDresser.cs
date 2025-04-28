using Sandbox;
using GeneralGame;

namespace GeneralGame;

public partial class PlayerDresser : Component, Component.INetworkSpawn
{
    [Property] public SkinnedModelRenderer BodyRenderer { get; set; }

    // Referenz zum Player-Objekt, um auf den CameraMode zuzugreifen
    private Player playerComponent;

    // Speichert den letzten Kameramodus, um unnötige Updates zu vermeiden
    private int lastCameraMode = -1;

    public void OnNetworkSpawn( Connection owner )
    {
        if ( owner == null )
        {

            return;
        }

        var clothing = ClothingContainer.CreateFromLocalUser();
        clothing.Apply( BodyRenderer );

        // Player-Komponente suchen
        playerComponent = GameObject.Components.Get<Player>();
    }

    protected override void OnUpdate()
    {
        // Nur aktualisieren, wenn wir eine Referenz zum Player haben
        if ( playerComponent == null || BodyRenderer == null )
            return;

        // Nur aktualisieren, wenn sich der Kameramodus geändert hat
        if ( lastCameraMode == playerComponent.CameraMode )
            return;

        lastCameraMode = playerComponent.CameraMode;

        // CameraMode 0 ist typischerweise First-Person
        bool isFirstPerson = (playerComponent.CameraMode == 0);

        // Im First-Person-Modus Kleidung ausblenden, sonst anzeigen
        UpdateClothingVisibility( !isFirstPerson );
    }

    public void UpdateClothingVisibility( bool visible )
    {
        if ( BodyRenderer == null )
            return;

        BodyRenderer.Enabled = visible;
    }

    public void RemoveClothing()
    {
        // Hier müssen Sie den Code hinzufügen, der die Kleidung vom BodyRenderer entfernt
        if ( BodyRenderer != null )
        {
            BodyRenderer.Destroy();
            BodyRenderer = null;
        }
    }
}