using Sandbox;
using System;


namespace GeneralGame.HUD
{
    public partial class Scoreboard : PanelComponent
    {
        public bool PausePanelEnabled { get; set; }
        private bool ShowConfirmationDialog { get; set; }
        protected override void OnUpdate()
        {
            if ( Input.EscapePressed )
            {
                Input.EscapePressed = false;
               
                PausePanelEnabled = !PausePanelEnabled;
                

                StateHasChanged();
            }
        }
        private void Quit()
        {
            Log.Info( "Quit" );
            ShowConfirmationDialog = true;
            PausePanelEnabled = false;
            StateHasChanged();
        }
        public async void CloseMenu()
        {
            ShowConfirmationDialog = false;
            PausePanelEnabled = false;
            StateHasChanged();
            await Task.Delay( 500 ); // Wartezeit für die Transition
            StateHasChanged();
        }
        private void Abondon()
        {

            Game.ActiveScene.LoadFromFile( "scenes/lobby.scene" );
        }

        private void ConfirmQuit()
        {
            Log.Info( "ConfirmQuit" );
            Game.Close();
        }

        private void CancelQuit()
        {
            Log.Info( "CancelQuit" );
            ShowConfirmationDialog = false;
            StateHasChanged();
            
        }

        private void ExitGame()
        {
            PausePanelEnabled = false;
            // Logik zum Beenden des Spiels
            StateHasChanged();
        }
        protected override int BuildHash() => HashCode.Combine( RealTime.Now.CeilToInt(), ShowConfirmationDialog );
    }
}