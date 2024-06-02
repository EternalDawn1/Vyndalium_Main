using GeneralGame.HUD;
namespace GeneralGame;

public class ShopComponent : Component
{
    [Property] public Action ShopAction { get; set; }
    private bool isPlayerNear = false;


    protected override void OnUpdate()
    {
isPlayerNear = CheckIfPlayerIsNear(); // Implement this method

if (isPlayerNear)
{
	if (Input.Down("Shop"))
	{
		ShopAction?.Invoke();

	
	}
}
        
    }

    private bool CheckIfPlayerIsNear()
    {
        // Implement logic to check if player is near
        return false;
        
    }

   
}