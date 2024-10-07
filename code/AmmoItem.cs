using Sandbox;
using System;
using GeneralGame.HUD;
namespace GeneralGame
{
    public sealed class AmmoPickup : Component, Component.ITriggerListener
    {
        [Property] public SoundEvent TriggerSoundPath { get; set; }
        [Property] public AmmoType AmmoTypen { get; set; }
        [Property] public int MinCount { get; set; } // Mindestanzahl der Munition
        [Property] public int MaxCount { get; set; }
        private static Random random = new Random();
        public void OnTriggerEnter( Collider other )
        {
            var player = other.Components.Get<Player>();
            if ( player != null )
            {
                GiveAmmoToPlayer( player );
                Sound.Play( TriggerSoundPath, WorldPosition );
                GameObject.Destroy();
            }
        }

        private void GiveAmmoToPlayer( Player player )
        {
            int count = random.Next( MinCount, MaxCount + 1 ); // Generiere eine zufällige Anzahl von Munition

            // Fügen Sie hier den Code hinzu, um dem Spieler Munition hinzuzufügen
            player.Ammo.Give( AmmoTypen, count );
            player.DefaultAmmo += count;
            var ammoContainer = player.Components.Get<AmmoContainer>();
            if ( ammoContainer != null )
            {
                // Add ammo to the player's AmmoContainer
                ammoContainer.Give( AmmoTypen, count );
            }
            Hudmaster.Instance.ShowNotification( $"You picked up {count}x {AmmoTypen} ammo.", "/ui/hud/bullet.png" );
        }

        public void OnTriggerExit( Collider other )
        {
            // Hier können Sie ggf. eine Aktion hinzufügen, die ausgeführt wird,
            // wenn der Spieler den Triggerbereich verlässt.
        }
    }
}
