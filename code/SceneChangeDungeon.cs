using Sandbox;
using System;
using System.Threading.Tasks;

namespace GeneralGame
{
    public sealed class SceneChangeDungeon : Component, Component.ITriggerListener
    {
        [Property] public GeneralScene SceneToLoad { get;private set; } = GeneralScene.Game;
        private bool canChangeScene = true;

        [Rpc.Broadcast( NetFlags.SendImmediate )]
        public void OnTriggerEnter(Collider other)
        {
            if( other == null)
            {
                Log.Info("collider is null");
            }
            Player.Save();
            var player = other.Components.Get<Player>();
            if (player != null && canChangeScene)
            if(player == null)
            {
                Log.Info("player is null");
            }

            {
                
                LoadSaveAndChangeScene(player);
				_ = Scene.GetAllComponents<SpawnPoint>().ToArray();
				canChangeScene = false;

            }
        }

        [Rpc.Broadcast( NetFlags.SendImmediate )]
        public void OnTriggerExit(Collider other)
        {
            // Optional: Code hier hinzufügen, der ausgeführt wird, wenn der Spieler den Triggerbereich verlässt
        }

        [Rpc.Broadcast( NetFlags.SendImmediate )]
        private  void LoadSaveAndChangeScene( Player player )
        {
            
            
          
         
            SceneHandler.ChangeScene(GeneralScene.Game);
        }
    }
}
