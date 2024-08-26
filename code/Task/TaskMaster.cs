using Sandbox;
using GeneralGame.Event;
using GeneralGame.HUD;
using System.Linq;
using System.Threading.Tasks;
using static GeneralGame.GeneralTask;


namespace GeneralGame;

[Icon( "live_help" )]
public partial class TaskMaster : Component, Component.INetworkListener
{
	public static TaskMaster _instance;

	public static IReadOnlyList<GeneralTask> ActiveTasks => _instance.CurrentTasks;

	[Property]
	public List<GeneralTask> CurrentTasks { get; set; }

	public bool HasStarted { get; set; } = false;




	/// <summary>
	/// Get how many tasks the player has completed so far (From 0 to 1) - Fetching this runs a check so don't overuse it
	/// </summary>




	protected override void OnStart()
	{
		_instance = this;

		if ( Connection.Local.IsHost )
			

		DelayStart();
	}

	async void DelayStart()
	{
		await Task.Delay( 1000 );
		HasStarted = true;
	}

	protected override void OnDestroy()
	{
		var allTasks = ResourceLibrary.GetAll<GeneralTask>();

		foreach ( var task in allTasks )
		{
			task.Reset();
		}
	}

	



	protected override void OnFixedUpdate()
	{
		if ( !HasStarted ) return;

		foreach ( var task in CurrentTasks )
		{
			if ( !task.Started ) // Start the task if it has been added
				task.Start();

			if ( !task.Completed )
			{
				var activeSubtasks = task.ActiveSubtasks;

				// Check if the subtasks have met the conditions (Previous subtask orders will always remain completed)
				foreach ( var subtask in activeSubtasks )
				{
					if ( subtask.EvaluateOnTick != null ) // Manually set current completion progress if we have an EvaluateOnTick
						if ( Player.Local != null )
							subtask.CurrentAmount = subtask.EvaluateOnTick.Invoke( Player.Local );

					subtask.SetComplete( subtask.CurrentAmount >= subtask.AmountToComplete );
				}

				// If all subtasks have been completed, the task has been completed succesfully
				if ( task.Subtasks.All( x => x.Completed ) )
					task.Succeed();

				// Check for the fail condition every tick
				if ( task.FailConditionCheck != null )
				{
					var hasFailed = task.FailConditionCheck.Invoke( Player.Local );

					if ( hasFailed ) // Fail the task if the fail condition has been met
						task.Fail();
				}

				// Time limit fail condition
				if ( task.TimeLimited && task.TaskTimer )
					task.Fail();

				if ( activeSubtasks.All( x => x.Completed ) )
				{
					task.CurrentSubtaskOrder++;

					foreach ( var subtask in task.ActiveSubtasks )
					{
						subtask.OnStart?.Invoke( Player.Local );
					}
				}
			}
		}
	}

	[Broadcast( NetPermission.Anyone )]
	static void SubmitTriggerNetworked( string signalIdentifier, Guid playerid )
	{
		var player = Player.GetByID( playerid );

		if ( player.IsValid() )
		{
			if ( player != Player.Local )
			{
				SubmitTriggerSignal( signalIdentifier, player, false );
			}
		}
	}

	TimeSince _lastTrigger = 0;
	string _lastId;
	Player _lastPlayer;
	/// <summary>
	/// Let all the tasks know a trigger has been activated
	/// </summary>
	/// <param name="signalIdentifier"></param>
	/// <param name="triggerer"></param>
	/// <param name="network"></param>
	public static void SubmitTriggerSignal( string signalIdentifier, Player triggerer, bool network = true )
	{
		
		if ( signalIdentifier == null || signalIdentifier == "" || signalIdentifier == String.Empty || signalIdentifier == "null" ) return;


		if ( _instance != null )
		{
			if ( _instance._lastTrigger <= 0.05f && _instance._lastId == signalIdentifier && _instance._lastPlayer == triggerer ) return;

			_instance._lastTrigger = 0;
			_instance._lastId = signalIdentifier;
			_instance._lastPlayer = triggerer;

			if ( network )
				SubmitTriggerNetworked( signalIdentifier, triggerer.ConnectionID );

			var allActiveTasks = _instance.CurrentTasks.Where( x => !x.Completed ); // Get active tasks

			foreach ( var task in allActiveTasks )
			{
				if ( !task.Global && triggerer != Player.Local ) continue; // If this task isn't global and the triggerer isn't our player, ignore it

				var currentOrder = task.CurrentSubtaskOrder;
				var activeSubtasks = task.Subtasks
					.Where( x => x.SubtaskOrder == currentOrder ); // Current active subtasks

				foreach ( var subtask in activeSubtasks )
				{
					if ( subtask.TriggerSignal.Identifier == signalIdentifier || subtask != null && subtask.TriggerSignal != null && signalIdentifier.Contains( subtask.TriggerSignal.Identifier ) ) // If the given signal is the one we're looking for, increase the subtask's progress
						subtask.CurrentAmount++;
				}
			}

			

		}
	}

	
	

	
	

	
	}






