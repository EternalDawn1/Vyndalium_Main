namespace GeneralGame
{
    [GameResource( "Quest", "quest", "a simple Quest", Icon = "event_busy", IconBgColor = "#4a4fa8", IconFgColor = "#ffffff" )]
    public class Quest : GameResource
    {
        [Feature("Quest")] public string Title { get; set; }
        [Feature( "Quest" )] public string Description { get; set; }
        [Feature( "Quest" )] public bool IsCompleted { get; private set; }
        [Feature( "Quest" )] private List<string> tasks;
        [Feature( "Quest" )] public List<string> Rewards { get; set; }
        [Feature( "Quest" )] public List<string> Tasks { get; set; }

        // Parameterloser Konstruktor
        public Quest()
        {
            tasks = new List<string>();
        }

        public Quest( string title, string description )
        {
            Title = title;
            Description = description;
            tasks = new List<string>();
        }

        public void AddTask( string task )
        {
            tasks.Add( task );
        }

        public void CompleteTask( string task )
        {
            if ( tasks.Contains( task ) )
            {
                tasks.Remove( task );
                if ( tasks.Count == 0 )
                {
                    IsCompleted = true;
                }
            }
        }

        public void AddReward( string reward )
        {
            // Implement reward logic
        }
    }
}

namespace GeneralGame.HUD
{
    public class QuestInteraction : Component
    {
        [Property] public QuestManager QuestManager { get; private set; }

        public void CloseQuest()
        {
            // Implementieren Sie die Logik zum Schließen der Quest
        }
        protected override void OnAwake()
        {
            
            QuestManager = Components.GetOrCreate<QuestManager>();

            var interactions = Components.GetOrCreate<Interactions>();
            interactions.AddInteraction( new Interaction()
            {
                Identifier = "quest.starting",
                Action = ( Player interactor, GameObject obj ) =>
                {
                    Hudmaster.Instance.ShowCustomDialog( "Do you want to start the Merchant Tutorial?", ( input ) =>
                    {
                        // Logik für das Merchant Tutorial
                        OpenMerchantTutorial();
                    } );
                },
                Keybind = "use",
                Description = "Start the Merchant Tutorial",
                Stats = "Quest",
                ShowWhenDisabled = () => true,
                Accessibility = AccessibleFrom.All,
            } );
        }

        public void OpenMerchantTutorial()
        {
            Log.Info( "Merchant Tutorial started" );

            if ( QuestManager == null )
            {
                Log.Error( "QuestManager is not initialized" );
                return;
            }

            // Laden der Quest-GameResource
            var quest = ResourceLibrary.Get<Quest>( "/task/test.quest" );
            if ( quest == null )
            {
                Log.Error( "Quest 'merchant_tutorial' could not be loaded" );
                return;
            }
            Log.Info( $"Quest '{quest.Title}' loaded" );
            QuestManager.AddQuest( quest );

            Log.Info( $"Quest '{quest.Title}' added to QuestManager. Total active quests: {QuestManager.ActiveQuests.Count}" );

            Hudmaster.Instance.ShowQuestPanel();
        }
    }
}

