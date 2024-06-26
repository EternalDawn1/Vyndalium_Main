using GeneralGame;
using GeneralGame.HUD;
using Sandbox;
public class BaseChest : Component
{

    public float inventorySize;


    public bool isOpen;
    public bool isLocked;



    public void Open()
    {
        if ( isLocked )
        {
            return;
        }

        isOpen = true;
    }
    public void Close()
    {
        isOpen = false;
    }
    protected override void OnAwake()
    {

        inventorySize = 10;
        isOpen = false;
        isLocked = false;



    }

    protected override void OnStart()
    {


    }



}