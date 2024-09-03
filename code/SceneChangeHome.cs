using Sandbox;
using System;

namespace GeneralGame
{
    public sealed class SceneChangeTrigger : Component, Component.ITriggerListener
    {
        private bool sceneChanged = false;

        [Property]
        public GeneralScene SceneToLoad { get; set; }

        public void OnTriggerEnter(Collider other)
        {
            var player = other.Components.Get<Player>();
            
            Player.Save(); // Stelle sicher, dass du die Instanzmethode aufrufst
            
            
            
           
            if (player != null && !sceneChanged)
            {
                ChangeScene(player);
                sceneChanged = true;
                
            }
            else
            {
                
            }
        }

        private async void ChangeScene(Player player)
        {
            
            await Task.DelaySeconds(.01f);
            SceneHandler.ChangeScene(SceneToLoad);
        }

        public void OnTriggerExit(Collider other)
        {
            // Hier können Sie optional eine Aktion hinzufügen, die ausgeführt wird,
            // wenn der Spieler den Triggerbereich verlässt.
        }
    }
}
