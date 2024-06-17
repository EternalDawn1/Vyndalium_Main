using Sandbox;
using GeneralGame;

namespace GeneralGame;

public partial class PlayerDresser : Component, Component.INetworkSpawn
{
    [Property] public SkinnedModelRenderer BodyRenderer { get; set; }

    public void OnNetworkSpawn(Connection owner)
    {
        if (owner == null)
        {
           
            return;
        }

		//var clothing = ClothingContainer.CreateFromLocalUser();
		//clothing.Apply( BodyRenderer );
	}
    public void RemoveClothing()
    {
        // Hier müssen Sie den Code hinzufügen, der die Kleidung vom BodyRenderer entfernt
        if (BodyRenderer != null)
        {
            BodyRenderer.Destroy();
            BodyRenderer = null;
            
        }
    }
}
