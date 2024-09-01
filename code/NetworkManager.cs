using GeneralGame.HUD;

namespace GeneralGame
{
    public sealed class NetworkManager : Component, Component.INetworkListener
    {
        public const int MAX_PLAYERS = 4;
        [Property] public GameObject Prefab { get; set; }
        [HostSync] public static Guid HostId { get; set; }
        [Property] public bool StartServer { get; set; } = true;
        [Property] public List<GameObject> SpawnPoints { get; set; }
       
       
        protected override async Task OnLoad()
        {
            

            if ( !GameNetworkSystem.IsActive && !IsProxy && StartServer )
            {

                await Task.DelayRealtimeSeconds( 0.1f );
                var obj = Prefab.Clone();
                var player = obj.Components.Get<Player>( FindMode.EverythingInSelfAndDescendants );
                player.Respawn();
                obj.NetworkMode = NetworkMode.Object;
                obj.NetworkSpawn();
                obj.Name = $"Host Player";

                // Setup host stuff.
                Player._InternalPlayers?.Clear();
                Player._InternalPlayers?.Add( player );
                Player.Local = player;
                GameNetworkSystem.CreateLobby();
                
                return;
            }
            if ( Player.All.Count >= MAX_PLAYERS )
            {
                
                SceneHandler.ChangeScene( GeneralScene.MainMenu );
                GameNetworkSystem.Disconnect();
                return;
            }

        }
        
        
       
       

        public void OnActive( Connection channel )
        {




            var startLocation = FindSpawnLocation().WithScale( 1 );
            var playerObject = Prefab.Clone( startLocation, name: $"Player - {channel.DisplayName}" );

            var playerComponent = playerObject.Components.Get<Player>( FindMode.EverythingInSelfAndDescendants );
            if ( playerComponent == null )
            {

                return;
            }
            
            playerComponent.SetupConnection( channel );
            Player._InternalPlayers?.Clear();
            Player._InternalPlayers?.Add( playerComponent );
            playerObject.NetworkSpawn( channel );
            Player.Setup();



            AssignComponentsToAllPlayers( playerComponent );

            // Set the HostId if this player is the host
            if ( channel.IsHost )
            {
                HostId = playerComponent.HostID;

            }
        }

        Transform FindSpawnLocation()
        {
            if ( SpawnPoints != null && SpawnPoints.Count > 0 )
            {
                var spawnPoint = Random.Shared.FromList( SpawnPoints, default );
                if ( spawnPoint != null )
                {
                    return spawnPoint.Transform.World;
                }
            }

            var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToArray();
            if ( spawnPoints.Length > 0 )
            {
                var spawnPoint = Random.Shared.FromArray( spawnPoints );
                if ( spawnPoint != null )
                {
                    return spawnPoint.Transform.World;
                }
            }

            return Transform.World;
        }

        void AssignComponentsToAllPlayers( Player playerComponent )
        {
            foreach ( var player in Player.All )
            {
                if ( player != playerComponent )
                {
                    Components.Get<Player>( FindMode.EverythingInSelfAndDescendants );
                }
            }
        }
        




    }
    
    public class PlayerInfo
    {
        public string Name { get; set; }
    }
}
