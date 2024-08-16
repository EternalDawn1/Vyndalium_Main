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
            if ( Scene.IsEditor )
                return;

            if ( !GameNetworkSystem.IsActive && !IsProxy && StartServer )
            {
                await Task.DelayRealtimeSeconds( 0.1f );
                GameNetworkSystem.CreateLobby();
                
                return;
            }

        }
        public static void ToggleLobby()
        {
            if ( !Connection.Local.IsHost )
                return;

            // Start lobby.
            if ( !GameNetworkSystem.IsActive )
            {
                GameNetworkSystem.CreateLobby();
                return;
            }

            // Close lobby.
            ServerClose( true );
            GameNetworkSystem.Disconnect();

            for ( int i = 0; i < Player.All.Count; i++ )
            {
                var p = Player.All.ElementAtOrDefault( i );
                if ( p is null || p == Player.Local )
                    continue;

                Player._InternalPlayers.Remove( p );
                p.Destroy();
            }
        }
        [Broadcast( NetPermission.HostOnly )]
        public static void ServerClose( bool ignoreHost )
        {
            if ( ignoreHost && Connection.Local.Id == HostId )
                return;

            GameNetworkSystem.Disconnect();
           
            SceneHandler.ChangeScene( GeneralScene.MainMenu );
        }
        [Broadcast]
        public void BroadcastDisconnect( Guid id )
        {
            Player._InternalPlayers.RemoveAll( ( p ) => p is null || p.ConnectionID == id );
        }
        void INetworkListener.OnDisconnected( Connection connection )
        {
            if ( connection.IsHost )
                ServerClose( true );

            BroadcastDisconnect( connection.Id );
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
        public void OnServerShutdown()
        {
            Player.RemoveAllPlayers();

        }




    }
    
    public class PlayerInfo
    {
        public string Name { get; set; }
    }
}
