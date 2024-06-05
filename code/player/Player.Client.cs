using GeneralGame.HUD;

namespace GeneralGame
{
    partial class Player
    {
        public static IReadOnlyList<Player> All => _InternalPlayers;
        public static List<Player> _InternalPlayers = new List<Player>();

        public static Player Local { get; set; }
		System.UInt64 SteamId { get; set; }
        public FullScreenManager FullScreenManager { get; set; }

        private Guid _guid;
        
        

        [HostSync]
        public Guid ConnectionID
        {
            get => _guid;
            set
            {
                _guid = value;
                Connection = Connection.Find(_guid);

                if (_guid == Connection.Local.Id)
                {
                    Local = this;
                    LocalID = Guid.NewGuid();
                }
                if (_guid == Connection.Host.Id)
                {
                    HostID = Guid.NewGuid();
                }

                if (!_InternalPlayers.Contains(this))
                    _InternalPlayers.Add(this);
            }
        }

        public Connection Connection { get; private set; }
        public Guid LocalID { get; set; }
        public Guid HostID { get; set; }

        public void SetupConnection(Connection connection)
        {
            ConnectionID = connection.Id;

            if (connection.IsHost)
            {
                HostID = Guid.NewGuid();
                
            }
            else 
            {
                LocalID = Guid.NewGuid();
                
            }
        }

        public static Player GetByID(Guid id)
		{
			
			foreach (var player in _InternalPlayers)
			{
				
				if (player.ConnectionID == id)
				{
					
					return player;
				}
			}
			
			return null;
		}

        

        

         
    }
}
