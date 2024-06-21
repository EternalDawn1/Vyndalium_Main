using Sandbox;
using System;

namespace GeneralGame
{
    public sealed class AmmoPickup : Component, Component.ITriggerListener
    {
        [Property] public SoundEvent TriggerSoundPath { get; set; }
        [Property] public AmmoType AmmoTypen { get; set; }
        [Property] public int Count { get; set; }

        public void OnTriggerEnter( Collider other )
        {
            var player = other.Components.Get<Player>();
            if ( player != null )
            {
                GiveAmmoToPlayer( player );
                Sound.Play( TriggerSoundPath, Transform.Position );
                GameObject.Destroy();
            }
        }

        private void GiveAmmoToPlayer( Player player )
        {
            // Fügen Sie hier den Code hinzu, um dem Spieler Munition hinzuzufügen
            // Zum Beispiel:
            player.Ammo.Give( AmmoTypen, Count );
            player.DefaultAmmo += Count;
            var ammoContainer = player.Components.Get<AmmoContainer>();
            if ( ammoContainer != null )
            {
                // Add ammo to the player's AmmoContainer
                ammoContainer.Give( AmmoTypen, Count );
            }
        }

        public void OnTriggerExit( Collider other )
        {
            // Hier können Sie ggf. eine Aktion hinzufügen, die ausgeführt wird,
            // wenn der Spieler den Triggerbereich verlässt.
        }
    }
}
