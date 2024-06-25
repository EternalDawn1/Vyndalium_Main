
public class BaseChest : Component
{
    private float inventorySize;
    private Inventory inventory;
    private bool isOpen;
    private bool isLocked;

    public void Open()
    {
        if (isLocked)
        {
            return;
        }

        isOpen = true;
    }
    public void Close()
    {
        isOpen = false;
    }

}