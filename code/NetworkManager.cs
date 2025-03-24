using GeneralGame.HUD;
using Sandbox;
using Sandbox.Network;

namespace GeneralGame
{
    public sealed class NetworkManager : Component, Component.INetworkListener
    {
        public const int MAX_PLAYERS = 16;
        [Property] public GameObject Prefab { get; set; }
        [Sync( SyncFlags.FromHost )] public static Guid HostId { get; set; }
       
        [Property] public List<GameObject> SpawnPoints { get; set; }


        
        protected override async Task OnLoad()
        {


            if (!Networking.IsActive && !IsProxy)
            {
                await Task.DelayRealtimeSeconds(0.1f);
                ToggleLobby();
                return;
            }
          
            if ( Player.All == null || Player.All.Count >= MAX_PLAYERS )
            {
                SceneHandler.ChangeScene( GeneralScene.MainMenu );
                Networking.Disconnect();
                return;
            }
            


        }





        public void OnActive( Connection channel )
        {
            if ( Player.All == null || Player.All.Count >= MAX_PLAYERS )
            {
                SceneHandler.ChangeScene( GeneralScene.MainMenu );
                Networking.Disconnect();
                return;
            }

            if ( Prefab == null )
            {
                return;
            }

            var startLocation = FindSpawnLocation().WithScale( 1 );
            var playerObject = Prefab.Clone( startLocation, name: $"Player - {channel.DisplayName}" );

            var playerComponent = playerObject.Components.Get<Player>( FindMode.EverythingInSelfAndDescendants );
            if ( playerComponent == null )
            {
                return;
            }

            AssignComponentsToAllPlayers( playerComponent );

            playerComponent.SetupConnection( channel );

            if ( Player._InternalPlayers == null )
            {
                return;
            }
            Player._InternalPlayers?.Clear();
            Player._InternalPlayers?.Add( playerComponent );
            playerObject.NetworkSpawn( channel );

            if ( channel.IsHost )
            {
                HostId = playerComponent.HostID;
            }

            if ( !Player.Setup( playerComponent ) )
            {

            }
        }
        void INetworkListener.OnDisconnected( Connection connection )
        {
            if ( connection.IsHost )
                ServerClose( true );

            BroadcastDisconnect( connection.Id );
        }
        [Rpc.Broadcast]
        public void BroadcastDisconnect( Guid id )
        {
            Player._InternalPlayers.RemoveAll( ( p ) => p is null || p.Connection.Id == id );
        }
        [Rpc.Broadcast( NetFlags.HostOnly)]
        public static void ServerClose( bool ignoreHost )
        {
            if ( ignoreHost && Connection.Local.Id == HostId )
                return;

            Networking.Disconnect();
           
            SceneHandler.ChangeScene( GeneralScene.MainMenu );
        }

        public static void ToggleLobby()
        {
            if (!Connection.Local.IsHost)
                return;

            // Start lobby.
            if (!Networking.IsActive)
            {
                // Erstelle eine neue Lobby-Konfiguration
                var lobbyConfig = new LobbyConfig
                {
                    Privacy = LobbyPrivacy.Public,
                    MaxPlayers = 16,
                   
                };

                Networking.CreateLobby(lobbyConfig);
               
                return;
            }

            // Close lobby.
            //ServerClose(true);
            //Networking.Disconnect();

            
        }
        void INetworkListener.OnBecameHost( Connection previousHost )
        {
            // Broadcast for everyone to leave!
            ServerClose( false );
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

        [Rpc.Broadcast( NetFlags.SendImmediate )]
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
    

}
