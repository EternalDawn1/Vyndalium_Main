using Sandbox;
using System;
using GeneralGame;

namespace GeneralGame.HUD
{
    public partial class Scoreboard : PanelComponent
    {
        public bool PausePanelEnabled { get; set; }
        private bool ShowConfirmationDialog { get; set; }
        private bool ShowAbandonDialog { get; set; }
        private bool ShowDiscordPanel { get; set; }
        private Player player { get; set; }
        protected override void OnUpdate()
        {
            if ( player == null && player.IsValid() && LifeState.Alive != LifeState.Dead )
            {
                return;
            }
            if ( Input.EscapePressed )
            {
                Input.EscapePressed = false;
               
                PausePanelEnabled = !PausePanelEnabled;
                CloseDiscordPanel();

                StateHasChanged();
            }
        }
        private void CloseDiscordPanel()
        {
            ShowDiscordPanel = false;
            StateHasChanged();
        }
        private void JoinDiscord()
        {
            ShowDiscordPanel = true;
            PausePanelEnabled = false;
            StateHasChanged();
        }

        private void OpenDiscordLink()
        {
            ShowDiscordPanel = true;
            StateHasChanged();
        }
        private void Quit()
        {
            if ( ShowAbandonDialog || ShowConfirmationDialog )
            {
                return;
            }
            ShowConfirmationDialog = true;
          
            StateHasChanged();
        }
        private void AbandonGame()
        {
            if ( ShowAbandonDialog || ShowConfirmationDialog )
            {
                return;
            }
            Player.Save();
            ShowAbandonDialog = true;
            StateHasChanged();
        }
        public async void CloseMenu()
        {
            if(player == null && player.IsValid() && LifeState.Alive != LifeState.Dead)
            {
                return;
            }
            
            ShowConfirmationDialog = false;
            ShowAbandonDialog = false;
            PausePanelEnabled = false;
            CloseDiscordPanel();
            StateHasChanged();
            await Task.Delay( 500 ); // Wartezeit für die Transition
            StateHasChanged();
        }
        private void Abondon()
        {

            Game.ActiveScene.LoadFromFile( "scenes/lobby.scene" );
        }
        private void ConfirmAbandon()
        {
            ShowConfirmationDialog = true;
            StateHasChanged();
        }
        private void CancelAbandon()
        {
            
            ShowAbandonDialog = false;
            StateHasChanged();
        }

        private void ConfirmQuit()
        {
           
            Game.Close();
        }

        private void CancelQuit()
        {
           
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