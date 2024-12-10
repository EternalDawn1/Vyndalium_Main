using System.Collections.Generic;

namespace GeneralGame
{
    public class QuestManager : Component
    {
        [Property] public List<Quest> ActiveQuests { get; private set; } = new List<Quest>();

        

        public void AddQuest( Quest quest )
        {
            ActiveQuests.Add( quest );
        }

        public void CompleteQuest( Quest quest, Quest.Task task )
        {
            quest.CompleteTask( task );
        }


        public Quest CreateCustomQuest( string title, string description, List<(string Description, Action Task)> tasks, List<Action> rewards )
        {
            var quest = new Quest( title, description );
            foreach ( var (taskDescription, taskAction) in tasks )
            {
                var descriptionToUse = string.IsNullOrEmpty( taskDescription ) ? "Custom Task" : taskDescription;
                quest.AddTask( new Quest.Task( descriptionToUse, taskAction ) );
            }
            foreach ( var reward in rewards )
            {
                quest.Rewards.Add( reward );
            }
            AddQuest( quest );
            return quest;
        }

        public void LoadQuestsFromResources()
        {
            var questDefinitions = ResourceLibrary.GetAll<Quest>();
            foreach ( var questDefinition in questDefinitions )
            {
                var tasks = questDefinition.Tasks?
                    .Select( t => (t.Description, t.Action) )
                    .Where( task => task.Action != null )
                    .ToList() ?? new List<(string Description, Action Task)>();

                var rewards = questDefinition.Rewards?
                    .Where( reward => reward != null )
                    .ToList() ?? new List<Action>();

                CreateCustomQuest( questDefinition.Title, questDefinition.Description, tasks, rewards );
            }
        }
    }
}