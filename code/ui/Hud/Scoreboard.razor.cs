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
        private bool ShowConfirmationDialogKill { get; set; }

        private Player player { get; set; }

        private bool ShowSettingsDialog { get; set; }
        private double mouseSensitivityInput { get; set; } = 1.0;




        public enum PanelType
        {
            General,
            Player,
         
            Misc
        }
        public void IncreaseFOV(float amount)
        {
            if (player == null)
            {
                player = GetPlayer(); // Methode zum Abrufen des Spielers
                if (player == null)
                {
                    // Handle the case where player is still null
                    return;
                }
            }

            player.DefaultFov += amount;
            if (player.DefaultFov > 120f) player.DefaultFov = 120f; // Maximal-FOV
            UpdateCameraFOV();
        }
        public void DecreaseFOV(float amount)
        {
            if (player == null)
            {
                player = GetPlayer(); // Methode zum Abrufen des Spielers
                if (player == null)
                {
                    // Handle the case where player is still null
                    return;
                }
            }

            player.DefaultFov -= amount;
            if (player.DefaultFov > 120f) player.DefaultFov = 120f; // Maximal-FOV
            UpdateCameraFOV();
        }

        private void UpdateCameraFOV()
        {
            if (player.PlyCamera != null)
            {
                player.PlyCamera.FieldOfView = player.DefaultFov;
            }
        }


        /// <summary>
        /// Einstellungen increase sensitivity
        /// </summary>

        private PanelType selectedTab = PanelType.General;
        private void IncreaseSensitivity(double amount)
        {
            mouseSensitivityInput = Math.Min(mouseSensitivityInput + amount, 10.0);
        }

        private void DecreaseSensitivity(double amount)
        {
            mouseSensitivityInput = Math.Max(mouseSensitivityInput - amount, 0.1);
        }


        private void SaveSettings()
        {
            if (player == null)
            {
                player = GetPlayer(); // Methode zum Abrufen des Spielers
            }
            if (player != null && player.IsValid())
            {
                player.MouseSensitivity = (float)mouseSensitivityInput;
                Hudmaster.Instance.ShowNotification("Sensitivity changed to" , "/ui/hud/inventory.png");
                Player.Local?.PlaySuccessSoundFromPath("sounds/upgrade/failing.sound", 0.0125f);
                // Logik zum Speichern der Einstellungen
            }
        }
        /// <summary>
        /// Einstellungen öffnen
        /// </summary>

        private void SelectTab(PanelType tab)
        {
            selectedTab = tab;
        }
        private void OpenSettingsDialog()
        {
            ShowSettingsDialog = true;
            PausePanelEnabled = false;
            StateHasChanged();
        }

       
        private void CloseSettingsDialog()
        {
            ShowSettingsDialog = false;
            PausePanelEnabled = true;
            StateHasChanged();
        }

        /// <summary>
        /// Kill Player
        /// </summary>
        /// <returns></returns>
        private Player GetPlayer()
        {
            // Logik zum Abrufen des Spielers
            return Player.Local as Player;
        }
        
        private void KillPlayer()
        {
            if (player == null)
            {
                player = GetPlayer(); // Methode zum Abrufen des Spielers
            }

            if (player != null && player.IsValid())
            {
                Log.Info("Kill Player");
                // Beispielwerte für die Parameter
                DamageType damageType = DamageType.Bullet;
                float amount = 99999;
                Vector3 position = player.Position;
                Vector3 force = Vector3.Zero;
                Guid attacker = Guid.Empty;
                Guid weapon = Guid.Empty;

                player.TakeDamage(damageType, amount, position, force, attacker, weapon);
                StateHasChanged();
            }
            else
            {
                Log.Warning("Player is null or not valid.");
            }
        }
        private void ConfirmKillPlayer()
        {
            ShowConfirmationDialogKill = true;
            StateHasChanged();
        }

        private void KillPlayerConfirmed()
        {
            ShowConfirmationDialogKill = false;
            KillPlayer();
        }

        private void CancelKillPlayer()
        {
            ShowConfirmationDialogKill = false;
            StateHasChanged();
        }



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
       
        /// <summary>
        /// Discord Panel öffnen
        /// </summary>
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
        /// <summary>
        /// Spiel verlassen
        /// </summary>
        private void Quit()
        {
            if ( ShowAbandonDialog || ShowConfirmationDialog )
            {
                return;
            }
            ShowConfirmationDialog = true;
          
            StateHasChanged();
        }
        /// <summary>
        /// Spiel verlassen
        /// </summary>
        private void AbandonGame()
        {
            if ( ShowAbandonDialog || ShowConfirmationDialog )
            {
                return;
            }
           
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
            ShowSettingsDialog = false;
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
            ShowConfirmationDialog = false;
            ShowAbandonDialog = false;
            ShowSettingsDialog = false;
            ShowDiscordPanel = false;


            // Logik zum Beenden des Spiels
            StateHasChanged();
        }
        protected override int BuildHash() => HashCode.Combine( RealTime.Now.CeilToInt(), ShowConfirmationDialog, ShowSettingsDialog, selectedTab);
    }
}
