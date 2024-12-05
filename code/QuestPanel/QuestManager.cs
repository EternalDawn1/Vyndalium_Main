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

        public void CompleteQuest( Quest quest, string task )
        {
            quest.CompleteTask( task );
        }

        public Quest CreateCustomQuest( string title, string description, List<string> tasks, List<string> rewards )
        {
            var quest = new Quest( title, description );
            foreach ( var task in tasks )
            {
                quest.AddTask( task );
            }
            foreach ( var reward in rewards )
            {
                quest.AddReward( reward );
            }
            AddQuest( quest );
            return quest;
        }

        public void LoadQuestsFromResources()
        {
            var questDefinitions = ResourceLibrary.GetAll<Quest>();
            foreach ( var questDefinition in questDefinitions )
            {
                CreateCustomQuest( questDefinition.Title, questDefinition.Description, questDefinition.Tasks, questDefinition.Rewards );
            }
        }
    }
}