using GeneralGame;
using Sandbox;
using Sandbox.Citizen;

public partial class Slime : Npc, IHealthComponent
{
	[Property]
	public bool HasSpecialFireRingAttack { get; set; } = false;

	[Property]
	public string FireRingPrefabPath { get; set; }

	[Property]
	public string FireBallPrefabPath { get; set; }
	[Property]
	public string FireBallAvatarPrefabPath { get; set; }

	private List<GameObject> activeFireRingObjects = new();
	private DateTime lastFireRingAttackTime = DateTime.MinValue;
	private DateTime lastFireBallAttackTime = DateTime.MinValue;
	private DateTime lastAvatarModeAttackTime = DateTime.MinValue;

	[Property] SoundEvent FireBallSound { get; set; }
	[Property] SoundEvent FireRingSound { get; set; }

	[Property] SoundEvent FireBallMultipleSound { get; set; }

	[Property] SoundEvent FireBallAvatarSound { get; set; }
	[Property] SoundEvent FireBallAvatarChargeSound { get; set; }	



	private const float proximityRange = 100.0f; // Proximity range in units
	private const float fireRingCooldown = 10.0f; // Cooldown in seconds
	private const float fireBallCooldown = 10.0f; // Cooldown in seconds

	private const float avatarModeCooldown = 10.0f; // Cooldown in seconds
	public Vector3 PlayerProximityDistance { get; set; } = new Vector3( 1000f, 1000f, 1000f );

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( IsPlayerInProximity() )
		{
			var players = Scene.GetAllComponents<Player>();
			var targetPlayer = players.FirstOrDefault();

			if ( targetPlayer != null )
			{
				float distanceToPlayer = (targetPlayer.WorldPosition - this.WorldPosition).Length;

				if ( distanceToPlayer <= proximityRange )
				{
					if ( HasSpecialFireRingAttack && DateTime.Now >= lastFireRingAttackTime.AddSeconds( fireRingCooldown ) )
					{
						ExecuteFireRingAttack();
						lastFireRingAttackTime = DateTime.Now;
					}
				}
				else if ( distanceToPlayer > proximityRange && distanceToPlayer <= PlayerProximityDistance.Length )
				{
					if ( DateTime.Now >= lastFireBallAttackTime.AddSeconds( fireBallCooldown ) )
					{
						ExecuteFireBallAttack();
						lastFireBallAttackTime = DateTime.Now;
					}
				}
				else if (distanceToPlayer > proximityRange && distanceToPlayer > PlayerProximityDistance.Length)
				{
					if ( Health < MaxHealth * 0.5f && lastAvatarModeAttackTime.AddSeconds( avatarModeCooldown ) <= DateTime.Now )
					{
						ExecuteAvatarModeAttack( targetPlayer );
						lastAvatarModeAttackTime = DateTime.Now;
					}
				}

				// Überprüfen Sie, ob die Gesundheit unter 50% liegt und führen Sie den Avatar-Modus-Angriff aus
				
			}
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

	private void ExecuteFireBallAttack()
	{
		if ( string.IsNullOrEmpty( FireBallPrefabPath ) )
		{
			return;
		}

		var prefab = ResourceLibrary.Get<PrefabFile>( FireBallPrefabPath );
		if ( prefab == null )
		{
			return;
		}

		var players = Scene.GetAllComponents<Player>();
		if ( players == null || !players.Any() )
		{
			return;
		}

		var targetPlayer = players.FirstOrDefault(); // Wähle den ersten Spieler als Ziel
		if ( targetPlayer == null )
		{
			return;
		}

		var random = new Random();
		bool useDirectAttack = random.Next( 3 ) == 0;

		if ( Health < MaxHealth * 0.5f )
		{
			
			ExecuteAvatarModeAttack( targetPlayer );
			
		}

		if ( useDirectAttack )
		{
			// Direkter Angriff mit drei Kugeln in einer Reihe
			const float spacing = 20.0f; // Abstand zwischen den Kugeln

			for ( int i = -1; i <= 1; i++ )
			{
				var fireBallObject = GameObject.Clone( prefab );
				fireBallObject.WorldPosition = WorldPosition + Vector3.Right * spacing * i; // Setze die Startposition mit Abstand
				fireBallObject.WorldRotation = Rotation.Identity;
				fireBallObject.NetworkSpawn();
				if(FireBallMultipleSound != null)
				{
					Sound.Play( FireBallMultipleSound );
				}

				
				if ( i == 0 )
				{
					// Kugel 1 zielt direkt auf den Spieler
					_ = MoveFireBallObjectDirect( fireBallObject, targetPlayer );
				}
				else
				{
					// Kugel 2 und 3 zielen leicht versetzt
					var offsetDirection = (targetPlayer.WorldPosition - fireBallObject.WorldPosition).Normal + Vector3.Right * i * 0.1f;
					_ = MoveFireBallObjectDirect( fireBallObject, targetPlayer, offsetDirection );
				}
			}
		}
		else
		{
			// Artillerie-Angriff mit einer Kugel
			var fireBallObject = GameObject.Clone( prefab );
			fireBallObject.WorldPosition = WorldPosition; // Setze die Startposition
			fireBallObject.WorldRotation = Rotation.Identity;
			fireBallObject.NetworkSpawn();
			_ = MoveFireBallObjectArtillery( fireBallObject, targetPlayer );
		}

		float distanceToPlayer = (targetPlayer.WorldPosition - this.WorldPosition).Length;

		if ( distanceToPlayer < 450.0f )
		{
			if ( DateTime.Now >= lastFireRingAttackTime.AddSeconds( fireRingCooldown ) )
			{
				

				ExecuteFireRingAttack();
				lastFireRingAttackTime = DateTime.Now;
			}
		}
		else
		{
			// Reset the cooldown if the player moves out of range
			lastFireRingAttackTime = DateTime.MinValue;
		}

		
	}
	private async void ExecuteAvatarModeAttack( Player targetPlayer )
	{
		if ( string.IsNullOrEmpty( FireBallAvatarPrefabPath ) )
		{
			return;
		}

		var prefab = ResourceLibrary.Get<PrefabFile>( FireBallAvatarPrefabPath );
		if ( prefab == null )
		{
			return;
		}

		var fireBallObjects = new List<GameObject>();

		// Erstelle vier Feuerkugeln
		for ( int i = 0; i < 4; i++ )
		{
			var fireBallObject = GameObject.Clone( prefab );
			fireBallObject.WorldPosition = WorldPosition + new Vector3( 0, 0, 150 ); // 50 Einheiten über dem NPC
			fireBallObject.WorldRotation = Rotation.Identity;
			fireBallObject.NetworkSpawn();
			fireBallObjects.Add( fireBallObject );

			if(FireBallAvatarChargeSound != null)
			{
				Sound.Play( FireBallAvatarChargeSound );
			}

			// Verzögerung zwischen den Spawns
			await Task.Delay( 1000 ); // 500ms Verzögerung
		}

		if(FireBallAvatarSound != null)
		{
			Sound.Play( FireBallAvatarSound );
		}
		// Bewege die Feuerkugeln in einem Bogen
		var directions = new Vector3[]
		{
		new Vector3(0, 0, 1), // Oben
        new Vector3(0, 0, -1), // Unten
        new Vector3(0, 1, 0), // Rechts
        new Vector3(0, -1, 0) // Links
		};

		for ( int i = 0; i < fireBallObjects.Count; i++ )
		{
			var fireBallObject = fireBallObjects[i];
			var direction = directions[i];
			_ = MoveFireBallObjectAvatarMode( fireBallObject, targetPlayer, direction );
		}
	}
	private async Task MoveFireBallObjectAvatarMode( GameObject fireBallObject, Player targetPlayer, Vector3 initialDirection )
	{
		const float speed = 400.0f; // Geschwindigkeit des Feuerballs
		const float updateInterval = 0.01f; // Update alle 10ms für eine flüssigere Bewegung
		const float homingDuration = 1.0f; // Dauer des Homing-Effekts in Sekunden
		const float straightFlightDuration = 5.0f; // Dauer des geraden Flugs in Sekunden
		const float increasedSpeed = 600.0f; // Erhöhte Geschwindigkeit nach dem Homing-Effekt

		// Schieße die Kugel in die angegebene Richtung
		fireBallObject.WorldPosition += initialDirection * 200.0f;

		
		
		float elapsedTime = 0.0f;

		while ( elapsedTime < homingDuration )
		{
			await Task.Delay( (int)(updateInterval * 1000) ); // Update alle 10ms
			var targetPosition = targetPlayer.WorldPosition;
			var direction = (targetPosition - fireBallObject.WorldPosition).Normal;
			fireBallObject.WorldPosition += direction * speed * updateInterval; // Bewege das Objekt

			// Kollisionserkennung mit Spielern
			var players = Scene.GetAllComponents<Player>();
			foreach ( var player in players )
			{
				if ( (player.WorldPosition - fireBallObject.WorldPosition).Length < 1.0f )
				{
					// Füge dem Spieler Schaden zu
					var playerHealthComponent = player.GetComponent<IHealthComponent>();
					if ( playerHealthComponent != null )
					{
						playerHealthComponent.TakeDamage( DamageType.fire, 50, fireBallObject.WorldPosition, Vector3.Zero, Guid.Empty, fireBallObject.Id );
					}

					// Lösche das Feuerballobjekt nach einer Sekunde Verzögerung
					fireBallObject.Destroy();
					return;
				}
			}

			elapsedTime += updateInterval;
		}

		// Fliege für eine Sekunde geradeaus und erhöhe die Geschwindigkeit
		var straightFlightDirection = (targetPlayer.WorldPosition - fireBallObject.WorldPosition).Normal;
		elapsedTime = 0.0f;

		while ( elapsedTime < straightFlightDuration )
		{
			await Task.Delay( (int)(updateInterval * 1000) ); // Update alle 10ms
			fireBallObject.WorldPosition += straightFlightDirection * increasedSpeed * updateInterval; // Bewege das Objekt

			elapsedTime += updateInterval;
		}

		
		
		fireBallObject.Destroy();
	}

	

	private async Task MoveFireBallObjectArtillery( GameObject fireBallObject, Player targetPlayer )
	{
		const float speed = 400.0f; // Geschwindigkeit des Feuerballs
		const float updateInterval = 0.01f; // Update alle 10ms für eine flüssigere Bewegung
		const float homingDuration = 1.0f; // Dauer des Homing-Effekts in Sekunden
		const float straightFlightDuration = 5.0f; // Dauer des geraden Flugs in Sekunden
		const float increasedSpeed = 600.0f; // Erhöhte Geschwindigkeit nach dem Homing-Effekt

		// Schieße die Kugel nach oben
		fireBallObject.WorldPosition += Vector3.Up * 200.0f;
		if(FireBallSound != null)
		{
			Sound.Play( FireBallSound );
		}

		float elapsedTime = 0.0f;

		while ( elapsedTime < homingDuration )
		{
			await Task.Delay( (int)(updateInterval * 1000) ); // Update alle 10ms
			var targetPosition = targetPlayer.WorldPosition;
			var direction = (targetPosition - fireBallObject.WorldPosition).Normal;
			fireBallObject.WorldPosition += direction * speed * updateInterval; // Bewege das Objekt

			// Kollisionserkennung mit Spielern
			var players = Scene.GetAllComponents<Player>();
			foreach ( var player in players )
			{
				if ( (player.WorldPosition - fireBallObject.WorldPosition).Length < 1.0f )
				{
					// Füge dem Spieler Schaden zu
					var healthComponent = player.GetComponent<IHealthComponent>();
					if ( healthComponent != null )
					{
						healthComponent.TakeDamage( DamageType.fire, 50, fireBallObject.WorldPosition, Vector3.Zero, Guid.Empty, fireBallObject.Id );
					}

					// Lösche das Feuerballobjekt nach einer Sekunde Verzögerung
					
					fireBallObject.Destroy();
					return;
				}
			}

			elapsedTime += updateInterval;
		}

		// Fliege für eine Sekunde geradeaus und erhöhe die Geschwindigkeit
		var straightFlightDirection = (targetPlayer.WorldPosition - fireBallObject.WorldPosition).Normal;
		elapsedTime = 0.0f;

		while ( elapsedTime < straightFlightDuration )
		{
			await Task.Delay( (int)(updateInterval * 1000) ); // Update alle 10ms
			fireBallObject.WorldPosition += straightFlightDirection * increasedSpeed * updateInterval; // Bewege das Objekt

			elapsedTime += updateInterval;
		}

		// Lösche das Feuerballobjekt nach einer Sekunde Verzögerung
		
		fireBallObject.Destroy();
	}

	private async Task MoveFireBallObjectDirect( GameObject fireBallObject, Player targetPlayer, Vector3? customDirection = null )
	{
		const float speed = 630.0f; // Geschwindigkeit des Feuerballs
		const float updateInterval = 0.01f; // Update alle 10ms für eine flüssigere Bewegung
		const float straightFlightDuration = 5.0f; // Dauer des geraden Flugs in Sekunden

		float elapsedTime = 0.0f;

		// Fliege direkt auf den Spieler zu oder in eine benutzerdefinierte Richtung
		var direction = customDirection ?? (targetPlayer.WorldPosition - fireBallObject.WorldPosition).Normal;

		while ( elapsedTime < straightFlightDuration )
		{
			await Task.Delay( (int)(updateInterval * 1000) ); // Update alle 10ms
			fireBallObject.WorldPosition += direction * speed * updateInterval; // Bewege das Objekt

			// Kollisionserkennung mit Spielern
			var players = Scene.GetAllComponents<Player>();
			foreach ( var player in players )
			{
				if ( (player.WorldPosition - fireBallObject.WorldPosition).Length < 1.0f )
				{
					// Füge dem Spieler Schaden zu
					var healthComponent = player.GetComponent<IHealthComponent>();
					if ( healthComponent != null )
					{
						healthComponent.TakeDamage( DamageType.fire, 50, fireBallObject.WorldPosition, Vector3.Zero, Guid.Empty, fireBallObject.Id );
					}

					// Lösche das Feuerballobjekt nach einer Sekunde Verzögerung
					
					fireBallObject.Destroy();
					return;
				}
			}

			elapsedTime += updateInterval;
		}

		// Lösche das Feuerballobjekt nach einer Sekunde Verzögerung
		await Task.Delay( 1000 );
		fireBallObject.Destroy();
	}
	private async void ExecuteFireRingAttack()
	{
		if ( string.IsNullOrEmpty( FireRingPrefabPath ) )
		{
			
			return;
		}

		var prefab = ResourceLibrary.Get<PrefabFile>( FireRingPrefabPath );
		if ( prefab == null )
		{
			
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
				if ( FireRingSound != null )
				{
					Sound.Play( FireBallSound );
				}

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