using GeneralGame;
using Sandbox;
using Sandbox.Citizen;

public partial class Slime : Npc, IHealthComponent
{
	[Property]
	public bool HasSpecialFireRingAttack { get; set; } = false;

	[Property]
	public string FireRingPrefabPath { get; set; }

	private List<GameObject> activeFireRingObjects = new();
	private DateTime lastFireRingAttackTime = DateTime.MinValue;
	private const float proximityRange = 100.0f; // Proximity range in units
	private const float fireRingCooldown = 10.0f; // Cooldown in seconds
	public Vector3 PlayerProximityDistance { get; set; } = new Vector3( 1000f, 1000f, 1000f );

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( HasSpecialFireRingAttack && DateTime.Now >= lastFireRingAttackTime.AddSeconds( fireRingCooldown ) && IsPlayerInProximity() )
		{
			ExecuteFireRingAttack();
			lastFireRingAttackTime = DateTime.Now;
		}
	}

	private bool IsPlayerInProximity()
	{
		if ( Network.IsProxy )
			return false;

		var players = Scene.GetAllComponents<Player>();
		if ( players == null )
		{
			return false;
		}
		foreach ( var player in players )
		{
			if ( (player.WorldPosition - this.WorldPosition).Length < PlayerProximityDistance.Length )
				return true;
		}
		return false;
	}

	private async void ExecuteFireRingAttack()
	{
		if ( string.IsNullOrEmpty( FireRingPrefabPath ) )
		{
			Log.Error( "FireRingPrefabPath is not set." );
			return;
		}

		var prefab = ResourceLibrary.Get<PrefabFile>( FireRingPrefabPath );
		if ( prefab == null )
		{
			Log.Error( "FireRingPrefab could not be loaded." );
			return;
		}

		const int numberOfWaves = 5;
		const int objectsPerWave = 12;
		const float waveInterval = 1.0f; // Interval between waves in seconds
		const float objectSpeed = 100.0f; // Speed at which objects move outward
		const float maxDistance = 500.0f; // Maximum distance before objects are destroyed

		for ( int wave = 0; wave < numberOfWaves; wave++ )
		{
			for ( int i = 0; i < objectsPerWave; i++ )
			{
				float angle = (360.0f / objectsPerWave) * i;
				Vector3 direction = new Vector3( MathF.Cos( angle ), MathF.Sin( angle ), 0 );
				var fireObject = GameObject.Clone( prefab );
				fireObject.WorldPosition = WorldPosition;
				fireObject.WorldRotation = Rotation.Identity;
				fireObject.NetworkSpawn();
				activeFireRingObjects.Add( fireObject );

				_ = MoveFireObject( fireObject, direction, objectSpeed, maxDistance );
			}

			await Task.Delay( (int)(waveInterval * 1000) );
		}
	}

	private async Task MoveFireObject( GameObject fireObject, Vector3 direction, float speed, float maxDistance )
	{
		float distanceTraveled = 0.0f;

		while ( distanceTraveled < maxDistance )
		{
			await Task.Delay( 100 ); // Update every 100ms
			fireObject.WorldPosition += direction * speed * 0.1f; // Move the object
			distanceTraveled += speed * 0.1f;
		}

		fireObject.Destroy();
		activeFireRingObjects.Remove( fireObject );
	}
}

public partial class Prometheus : Npc , IHealthComponent
{
	protected override void OnUpdate()
	{
		base.OnUpdate();
	}
	

}