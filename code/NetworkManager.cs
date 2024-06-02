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
            if (Scene.IsEditor)
                return;

            if (!GameNetworkSystem.IsActive && !IsProxy && StartServer)
            {
                await Task.DelayRealtimeSeconds(0.1f);
                GameNetworkSystem.CreateLobby();
            }
            
        }

        public void OnActive(Connection channel)
        {
            Log.Info($"Player '{channel.DisplayName}' has joined the game");

            

            var startLocation = FindSpawnLocation().WithScale(1);
            var playerObject = Prefab.Clone(startLocation, name: $"Player - {channel.DisplayName}");

            var playerComponent = playerObject.Components.Get<Player>(FindMode.EverythingInSelfAndDescendants);
            if (playerComponent == null)
            {
                Log.Error("Player component not found in prefab");
                return;
            }
            
            playerComponent.SetupConnection(channel);
            Player._InternalPlayers?.Add(playerComponent);
            playerObject.NetworkSpawn(channel);

            AssignComponentsToAllPlayers(playerComponent);

            // Set the HostId if this player is the host
            if (channel.IsHost)
            {
                HostId = playerComponent.HostID;
                Log.Info($"HostId gesetzt: {HostId}");
            }
        }

        Transform FindSpawnLocation()
        {
            if (SpawnPoints != null && SpawnPoints.Count > 0)
            {
                return Random.Shared.FromList(SpawnPoints, default).Transform.World;
            }

            var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToArray();
            if (spawnPoints.Length > 0)
            {
                return Random.Shared.FromArray(spawnPoints).Transform.World;
            }

            return Transform.World;
        }

        void AssignComponentsToAllPlayers(Player playerComponent)
        {
            foreach (var player in Player.All)
            {
                if (player != playerComponent)
                {
                    Components.Get<Player>(FindMode.EverythingInSelfAndDescendants);
                }
            }
        }
    }
}
