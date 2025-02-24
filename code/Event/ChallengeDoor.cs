using GeneralGame.HUD;
namespace GeneralGame;

public sealed class ChallengeDoor : Component
{
	[Property] ModelRenderer modelRenderer;
	[Property] BoxCollider boxCollider;

	[Property, Group( "General" )]
	public float CloseDuration { get; set; } = 30f;
	[Property, Group( "General" )] public float timer;
	[Property, Group( "General" )] private bool isTimerActive = false;
	[Property, Group( "General" )] public NpcSpawnArea SpawnArea { get; set; }
	[Property, Group( "General" )] public ChallengeDoorPanel ChallengeDoorPanel { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		timer = CloseDuration;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if ( SpawnArea != null && SpawnArea.IsPlayerInRoom() )
		{
			isTimerActive = true;
			ActivateDoor();
		}
		else
		{
			isTimerActive = false;
		}

		if ( isTimerActive )
		{
			timer -= Time.Delta;
			if ( timer <= 0 )
			{
				OpenDoor();
				isTimerActive = false;
			}
		}
	}

	public void ActivateDoor()
	{
		CloseDoor();
		isTimerActive = true;
		if ( ChallengeDoorPanel != null )
		{
			ChallengeDoorPanel.Enabled = true; // Aktiviert das ChallengeDoorPanel
		}
		else
		{
			Log.Error( "ChallengeDoorPanel is null in ActivateDoor" );
		}
	}

	private void CloseDoor()
	{
		// Logik zum Schließen der Tür
	}

	private void OpenDoor()
	{
		GameObject.Destroy();
	}

	public bool IsTimerActive()
	{
		return isTimerActive;
	}

	public bool IsTimerExpired()
	{
		return timer <= 0;
	}
}