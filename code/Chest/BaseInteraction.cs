using GeneralGame;
using GeneralGame.HUD;
using Sandbox;

namespace GeneralGame;

public class BaseInteraction : Component
{
    [Property] public float inventorySize = 10f;
    [Property] public bool isOpen;
    [Property] public bool isLocked;
    public bool IsPanelVisible { get; set; }
    public event Action OnOpen;
    public event Action OnClose;
    public bool isHighlighted = false;

    public bool IsHighlighted => isHighlighted;


    protected override void OnAwake()
    {
        isOpen = false;
        isLocked = false;
    }
    protected override void OnStart()
    {
        inventorySize = 10f;
    }
    public void Open()
    {
        if ( isLocked || isOpen )
        {
            return;
        }
        isOpen = true;
        IsPanelVisible = true;
        OnOpen?.Invoke();


    }
    public void Close()
    {
        if ( !isOpen )
        {
            return;
        }
        isOpen = false;
        OnClose?.Invoke(); // Ereignis auslösen
    }



}
