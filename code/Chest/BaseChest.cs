using GeneralGame;
using GeneralGame.HUD;
using Sandbox;

namespace GeneralGame
{
    public class BaseChest : Component
    {
        [Property] public float inventorySize = 10f;
        [Property] public bool isOpen;
        [Property] public bool isLocked;
        public event Action OnOpen;
        public event Action OnClose;

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
            OnOpen?.Invoke(); // Ereignis auslösen
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
}