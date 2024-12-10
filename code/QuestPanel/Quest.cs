namespace GeneralGame
{
    [GameResource( "Quest", "quest", "a simple Quest", Icon = "event_busy", IconBgColor = "#4a4fa8", IconFgColor = "#ffffff" )]
    public class Quest : GameResource
    {
        [Feature( "Quest" )] public string Title { get; set; }
        [Feature( "Quest" )] public string Description { get; set; }
        [Feature( "Quest" )] public bool IsCompleted { get; set; }
        [Feature( "Quest" )] public List<Action> Rewards { get; set; }
        [Feature( "Quest" )] public List<Task> Tasks { get; set; }
        [Feature( "Quest" )] public HashSet<Task> CompletedTasks { get; set; }
        [Feature( "Quest" )] public List<int> TaskOrder { get; set; }

        public class Task
        {
            public string Description { get; set; }
            public Action Action { get; set; }

            public Task() { }

            public Task( string description, Action action )
            {
                if ( string.IsNullOrEmpty( description ) )
                {
                    throw new ArgumentNullException( nameof( description ), "Description cannot be null or empty" );
                }

                if ( action == null )
                {
                    throw new ArgumentNullException( nameof( action ), "Action cannot be null" );
                }

                Description = description;
                Action = action;
            }
            protected static void OnAwake()
            {
                Log.Info("Task is awake");
            }
        }
       
        // Parameterloser Konstruktor
        public Quest()
        {
            Tasks = new List<Task>();
            Rewards = new List<Action>();
            CompletedTasks = new HashSet<Task>();
            TaskOrder = new List<int>();
        }

        public Quest( string title, string description )
        {
            Title = title;
            Description = description;
            Tasks = new List<Task>();
            Rewards = new List<Action>();
            CompletedTasks = new HashSet<Task>();
            TaskOrder = new List<int>();
        }

        public void AddTask( Task task )
        {
            Tasks.Add( task );
            TaskOrder.Add( Tasks.Count - 1 );
        }

        public void AddTaskAt( Task task, int position )
        {
            if ( position < 0 || position > Tasks.Count )
            {
                throw new ArgumentOutOfRangeException( nameof( position ), "Position must be within the range of the task list." );
            }
            Tasks.Insert( position, task );
            TaskOrder.Insert( position, position );
            // Update TaskOrder to reflect the new positions
            for ( int i = position + 1; i < TaskOrder.Count; i++ )
            {
                TaskOrder[i]++;
            }
        }

        public void CompleteTask( Task task )
        {
            if ( Tasks.Contains( task ) && !CompletedTasks.Contains( task ) )
            {
                task.Action.Invoke();
                CompletedTasks.Add( task );
                CheckCompletion();
            }
        }

        private void CheckCompletion()
        {
            if ( CompletedTasks.Count == Tasks.Count )
            {
                IsCompleted = true;
            }
        }

        public Task GetNextTask()
        {
            foreach ( var index in TaskOrder )
            {
                var task = Tasks[index];
                if ( !CompletedTasks.Contains( task ) )
                {
                    return task;
                }
            }
            return null;
        }

        public void ClaimRewards()
        {
            if ( IsCompleted )
            {
                foreach ( var reward in Rewards )
                {
                    reward.Invoke();
                }
            }
            else
            {
                throw new InvalidOperationException( "Quest is not completed yet. Complete all tasks to claim rewards." );
            }
        }
    }
}

namespace GeneralGame.HUD
{
    public class QuestInteraction : Component
    {
        [Property] public QuestManager QuestManager { get; private set; }

        [Property] Quest quest { get; set; }

		protected override void OnStart()
		{
           
		}



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
                        AddQuest();
                    } );
                },
                Keybind = "use",
                Description = "Start the Merchant Tutorial",
                Stats = "Quest",
                ShowWhenDisabled = () => true,
                Accessibility = AccessibleFrom.All,
            } );
        }

        public void AddQuest()
        {
           
            if ( QuestManager == null )
            {
                Log.Error( "QuestManager is not initialized" );
                return;
            }
            QuestManager.AddQuest( quest );
            Hudmaster.Instance.ShowQuestPanel();
        }
    }
}


