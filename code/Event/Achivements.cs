namespace GeneralGame;
using Sandbox;
using Sandbox.Services;

public sealed class Achievements : Component
{

    public static void IncrementDungeonCompletion()
    {
        Sandbox.Services.Stats.Increment( "won_one_game", 1 );
        Log.Info( "Dungeon completion incremented." );
    }
}