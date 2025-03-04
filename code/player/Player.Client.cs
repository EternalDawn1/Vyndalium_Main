using GeneralGame.HUD;

namespace GeneralGame
{
    partial class Player
    {
        public static bool DevsAreAdmins { get; set; } = true;
        public static ulong ETERNAL_STEAM_ID { get; set; } = 76561198040689780;
        public static IReadOnlyList<Player> All => _InternalPlayers;
        public static List<Player> _InternalPlayers = new List<Player>();
        public Dictionary<AmmoType, int> AmmoReserve { get; set; } = new Dictionary<AmmoType, int>();

        public static Player Local { get; set; }
        public System.UInt64 SteamId { get; set; }
        public FullScreenManager FullScreenManager { get; set; }

        private Guid _guid;
        public bool IsHost()
        {
            return Connection != null && Connection.IsHost;
        }
      
        
        

        [Sync]
        public Guid ConnectionID
        {
            get => _guid;
            set
            {
                _guid = value;
                Connection = Connection.Find( _guid );

                if ( _guid == Connection.Local.Id )
                {
                    Local = this;
                    LocalID = Guid.NewGuid();
                }
                if ( _guid == Connection.Host.Id )
                {
                    HostID = Guid.NewGuid();
                }

                if ( !_InternalPlayers.Contains( this ) )
                    _InternalPlayers.Add( this );
            }
        }
        public static void RemoveAllPlayers()
        {
            _InternalPlayers.Clear();
        }

        public Connection Connection { get; private set; }
        public Guid LocalID { get; set; }
        public Guid HostID { get; set; }
        public bool IsReady { get; set; }
        public void SetupConnection( Connection connection )
        {
            ConnectionID = connection.Id;
            GameObject.Name = $"{Local} / {SteamId}";

            if ( connection.IsHost )
            {
                HostID = Guid.NewGuid();
            }
            else
            {
                LocalID = Guid.NewGuid();
            }
        }

        public static Player GetByID( Guid id )
        {

            foreach ( var player in _InternalPlayers )
            {

                if ( player.ConnectionID == id )
                {

                    return player;
                }
            }

            return null;
        }


    }
}
