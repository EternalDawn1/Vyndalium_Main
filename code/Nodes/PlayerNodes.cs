using Sandbox;
using Sandbox.Utility;
using GeneralGame;
using static Sandbox.PhysicsContact;

public static partial class PlayerNodes
{
	/// <summary>
	/// Punches the player, detaches them from the ground and throws them away local to their transform
	/// </summary>
	[ActionGraphNode( "event.localpunchplayer" )]
	[Title( "Local Punch Player" ), Group( "Events" ), Icon( "sports_mma" )]
	public static void LocalPunch( Player player, Vector3 launchDirection, float strength = 0f, float extraVerticalStrength = 0f )
	{
		if ( player == null ) return;

		var punchDirection = launchDirection.Normal;
		var punch = punchDirection * strength + Vector3.Up * extraVerticalStrength;

		if ( player.Components.TryGet<MoveHelper>( out MoveHelper moveHelper ) )
			moveHelper.Punch( punch );
	}

	/// <summary>
	/// Punches the player, detaches them from the ground and throws them away starting from the worldSource
	/// </summary>
	[ActionGraphNode( "event.worldpunchplayer" )]
	[Title( "World Punch Player" ), Group( "Events" ), Icon( "sports_mma" )]
	public static void WorldPunch( Player player, Vector3 worldSource, float strength = 0f, float extraVerticalStrength = 0f )
	{
		if ( player == null ) return;

		var punchVector = player.WorldPosition + Vector3.Up * 36f - worldSource;
		var punchDirection = punchVector.Normal;
		var punch = punchDirection * strength + Vector3.Up * extraVerticalStrength;

		if ( player.Components.TryGet<MoveHelper>( out MoveHelper moveHelper ) )
			moveHelper.Punch( punch );
	}

	

	/// <summary>
	/// Fades to black
	/// </summary>
	[ActionGraphNode( "event.blackscreen" )]
	[Title( "Black Screen" ), Group( "Events" ), Icon( "blinds" )]
	public static async Task BlackScreen( Player player, float startingTransition = 2f, float blackTransition = 2f, float endingTransition = 1f )
	{
		if ( player == null ) return;

		

		await GameTask.DelaySeconds( startingTransition + blackTransition + endingTransition );
	}

	[ActionGraphNode( "inventory.amountofitem" ), Pure]
	[Title( "Amount of Item in Inventory" ), Group( "Player" ), Icon( "categories" )]
	public static int AmountInInventory( Inventory inventory, string nameOfItem )
	{
		return inventory.GetTotalItemCount( nameOfItem );
	}

	[ActionGraphNode( "inventory.amountofitemwithtag" ), Pure]
	[Title( "Amount of Item in Inventory with Tag" ), Group( "Player" ), Icon( "categories" )]
	public static int AmountInInventoryWithTag( Inventory inventory, string tag )
	{
		return inventory.GetTotalItemCountWithTag( tag );
	}

	[ActionGraphNode( "player.getrandom" )]
	[Title( "Get Random Player" ), Group( "Player" ), Icon( "escalator_warning" )]
	public static Player GetRandomPlayer()
	{
		return Game.Random.FromList( Game.ActiveScene
			.GetAllComponents<Player>()
			.ToList() );
	}
	[ActionGraphNode("player.getplayerwhoiskiller")]
	[Title("Get Player Who Is Killer"), Group("Player"), Icon("escalator_warning")]
	public static Player GetPlayerWhoIsKiller()
	{
		return Game.Random.FromList(Game.ActiveScene
			.GetAllComponents<Player>()
			.ToList());
			
	}
	[ActionGraphNode( "player.getself" )]
	[Title( "Get Self Player" ), Group( "Player" ), Icon( "person" )]
	public static Player GetSelfPlayer()
	{
	
		return Player.Local;
	}

	[ActionGraphNode( "quest.checkrequirements" )]
	[Title( "Check Quest Requirements" ), Group( "Quest" ), Icon( "check_circle" )]
	public static bool CheckQuestRequirements( Quest quest )
	{
		// Überprüfen, ob alle Aufgaben der Quest abgeschlossen sind
		bool allTasksCompleted = quest.Tasks.All( task => quest.CompletedTasks.Contains( task ) );

		// Wenn alle Aufgaben abgeschlossen sind, die Quest abschließen
		if ( allTasksCompleted )
		{
			quest.IsCompleted = true;
		}

		return allTasksCompleted;
	}
	
}
