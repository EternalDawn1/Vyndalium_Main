using Sandbox;
using Sandbox.UI;

namespace GeneralGame.HUD
{
    [StyleSheet]
    public partial class PauseMenu : Panel
    {
        public bool IsPaused { get; set; }

        public static PauseMenu Instance { get; private set; }

        public PauseMenu()
        {
            Instance = this;
            Style.Display = DisplayMode.None; // Versteckt das PauseMenu initial
        }

        public override void Tick()
        {
            // Keine zusätzliche Logik hier erforderlich, da FullScreenManager die Anzeige steuert
        }

        public void Show()
        {
            IsPaused = true;
            Style.Display = DisplayMode.Flex;
        }

        public void Hide()
        {
            IsPaused = false;
            Style.Display = DisplayMode.None;
        }
    }
}