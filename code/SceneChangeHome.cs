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
            Player.Save();
            var player = other.Components.Get<Player>();
            if (player != null && !sceneChanged)
            {
                ChangeScene(player);
                sceneChanged = true;
            }
        }

        private void ChangeScene(Player player)
        {
            // Fügen Sie hier den Code hinzu, der die Szene ändert
            // Zum Beispiel:
            SceneHandler.ChangeScene(SceneToLoad);
        }

        public void OnTriggerExit(Collider other)
        {
            // Hier können Sie optional eine Aktion hinzufügen, die ausgeführt wird,
            // wenn der Spieler den Triggerbereich verlässt.
        }
    }
}
