using Sandbox;
using System;

namespace GeneralGame
{
    public sealed class SceneChangeTriggertest : Component, Component.ITriggerListener
    {
        private bool sceneChanged = false;
        public Player player { get; set; }

        [Property]
        public GeneralScene SceneToLoad { get; set; }

        public void OnTriggerEnter( Collider other )
        {

            Player.Save();
            var player = other.Components.Get<Player>( FindMode.EverythingInSelfAndDescendants );

            if ( player != null && !sceneChanged )
            {
                ChangeScene( player );
                sceneChanged = true;

            }
        }

        private async void ChangeScene( Player player )
        {
            Sound.Play( "sounds/fart.sound" );
            await Task.DelaySeconds( .01f );
            SceneHandler.ChangeScene( SceneToLoad );
        }

        public void OnTriggerExit( Collider other )
        {
            // Hier können Sie optional eine Aktion hinzufügen, die ausgeführt wird,
            // wenn der Spieler den Triggerbereich verlässt.
        }
    }
}
