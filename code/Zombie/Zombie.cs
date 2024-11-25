using GeneralGame;
using GeneralGame.HUD;
using Sandbox.Citizen;
using Sandbox.UI;
using System.Linq;
using Sandbox;


using System;
using System.ComponentModel.Design.Serialization;

public enum NpcState
{
	Idle,
	Attacking,
	Walking,
	Running
}
public enum HoldTypes
{
	None,
	Pistol,
	Rifle,
	Shotgun,
	HoldItem,
	Punch,
	Swing,
	RPG
}


public enum WeightType
{
	[Icon( "🐀" )]
	Feather,
	[Icon( "🐇" )]
	Light,
	[Icon( "🚶" )]
	Middle,
	[Icon( "🦌" )]
	Heavy,
	[Icon( "🐘" )]
	Massive
}



public partial class Npc : Component, IHealthComponent
{
	[Property]
	public string Name { get; set; }
	[Property,HostSync]public int Level { get; set; }
	
	[Property]
	public MoveHelper MoveHelper { get; set; }
	[Property] public GameObject ZombieRagedol { get; set; }

	
	[Property]
	private readonly List<string> prefabPaths = new List<string>
	{
		"prefabs/pickupammo.prefab", // 50% Wahrscheinlichkeit
		"prefabs/potions/potion.prefab", // 25% Wahrscheinlichkeit
		"prefabs/potions/potion.prefab", // 25% Wahrscheinlichkeit
		"prefabs/entitys/chestsystem/5.prefab" // 5% Wahrscheinlichkeit
	};
	private readonly List<float> probabilities = new List<float>
	{
		0.5f, // 50% Wahrscheinlichkeit für Munition
        0.2f, // 25% Wahrscheinlichkeit für Tränke
        0.2f,
		0.1f,  // 25% Wahrscheinlichkeit für Holz
    };

	// Methode zum Spawnen eines zufälligen Prefabs
	private void SpawnRandomPrefab( Vector3 position )
	{
		float totalProbability = 0f;
		foreach ( var probability in probabilities )
		{
			totalProbability += probability;
		}

		float randomValue = (float)random.NextDouble() * totalProbability;
		float cumulativeProbability = 0f;

		for ( int i = 0; i < prefabPaths.Count; i++ )
		{
			cumulativeProbability += probabilities[i];
			if ( randomValue <= cumulativeProbability )
			{
				var prefab = ResourceLibrary.Get<PrefabFile>( prefabPaths[i] );
				if ( prefab != null )
				{
					var gameObject = GameObject.Clone( prefab );
					if ( gameObject != null )
					{
						// Spawnen des Items in der Luft
						gameObject.WorldPosition = position + new Vector3( 0, 0, 25 );
						gameObject.NetworkSpawn();
		
						
					}
				}
				break;
			}
		}
	}

	[Property] public SkinnedModelRenderer Model { get; set; }
	[Sync, Property] public float MaxHealth { get; set; } = 100f;
	[Sync, Property] public float Health { get;  set; } = 100f;
	[Property] public HealthComponent Healthone { get; set; }

	[Property] public SoundEvent DeathSounds { get; set; }
	
	public Guid LastAttackerId { get; set; }

	[Property]private HoldTypes CurrentHoldType = HoldTypes.None;



	/// <summary>
	/// For animations. How many units per second the run animation is tuned to (This is automatically scaled by the scale)
	/// </summary>
	[Property]
	[Category( "Stats" )]
	
	public float MaxRunAnimationSpeed { get; set; } = 150f;

	/// <summary>
	/// How fast this NPC can walk
	/// </summary>
	[Property]
	[Category( "Stats" )]

	[Range( 0f, 600f, 10f, false )]
	public float WalkSpeed { get; set; } = 90f;

	/// <summary>
	/// How fast this NPC can run
	/// </summary>
	[Property]
	[Category( "Stats" )]

	[Range( 0f, 600f, 10f, false )]
	public float RunSpeed { get; set; } = 180f;

	/// <summary>
	/// Should we automatically make the NPC look towards its target
	/// </summary>
	[Property]
	[Category( "Stats" )]

	public bool FaceTowardsVelocity { get; set; } = true;

	/// <summary>
	/// How far the NPC can reach to attack with the target
	/// </summary>
	[Property]
	[Category( "Stats" )]
	[Range( 30f, 200f, 10f, false )]
	public float AttackRange { get; private set; } = 80f;

	/// <summary>
	/// How many seconds must pass before the NPC can attack with the target again
	/// </summary>
	[Property]
	[Category( "Stats" )]
	[Range( 0.5f, 10f, 0.1f, false )]
	public float AttackCooldown { get; private set; } = 5f;

	/// <summary>
	/// If a GameObject has one of these tags it will be considered an enemy
	/// </summary>
	[Property]
	[Category( "Stats" )]
	public TagSet EnemyTags { get; set; }

	/// <summary>
	
	/// How far away the NPC can detect an enemy
	/// </summary>
	[Property]
	[Category( "Stats" )]
	[Range( 0f, 1024f, 16f, false )]
	public float DetectRange { get; set; } = 356f;

	/// <summary>
	/// How far away the NPC can see the enemy before losing sight
	/// </summary>
	[Property]
	[Category( "Stats" )]
	[Range( 0f, 2024f, 16f, false )]
	public float VisionRange { get; set; } = 512f;

	/// <summary>
	/// When detecting an enemy, alerts everyone closeby as well
	/// </summary>
	[Property]
	[Category( "Stats" )]
	public bool AlertOthers { get; set; } = true;

	/// <summary>
	/// When the NPC has no target it will sometimes fire off the idle event
	/// </summary>
	[Property]
	[Category( "Stats" )]
	public bool Idle { get; set; } = true;

	/// <summary>
	/// Maximum time in between idle events being called when there is no target
	/// </summary>
	[Property]
	[Category( "Stats" )]
	[ShowIf( "Idle", true )]
	[Range( 0.1f, 10f, 0.1f, false )]
	public float MinimumIdleCooldown { get; set; } = 4;

	/// <summary>
	/// Minimum time in between idle events being called when there is no target
	/// </summary>
	[Property]
	[Category( "Stats" )]
	[ShowIf( "Idle", true )]
	[Range( 0.1f, 20f, 0.1f, false )]
	public float MaximumIdleCooldown { get; set; } = 6;

	/// <summary>
	/// When the NPC is first spawned in
	/// </summary>
	[Property]
	[Category( "Triggers" )]
	public Action OnSpawn { get; set; }

	public delegate void NpcTrigger( GameObject enemy );

	/// <summary>
	/// When an enemy enters the detect area or attacks the NPC, or a nearby NPC gets alerted. This won't get called if the NPC has been alerted already.
	/// </summary>
	[Property]
	[Category( "Triggers" )]
	public NpcTrigger OnDetect { get; set; }

	/// <summary>
	/// When the enemy is within attack range and our cooldown is up
	/// </summary>
	[Property]
	[Category( "Triggers" )]
	public NpcTrigger OnAttack { get; set; }

	/// <summary>
	/// When the enemy gets out of the Vision Range or dies
	/// </summary>
	[Property]
	[Category( "Triggers" )]
	public NpcTrigger OnEnemyEscaped { get; set; }

	/// <summary>
	/// When the NPC dies, enemy will be NULL if it died of natural causes (???)
	/// </summary>
	[Property]
	[Category( "Triggers" )]
	public NpcTrigger OnKilled { get; set; }
	/// <summary>
	/// Should the stats scale linearly with the scale of the object
	/// </summary>
	[Property]
	[Category( "Stats" )]
	public bool ScaleStats { get; set; } = true;
	public float Scale => ScaleStats ? MathF.Max( MathF.Max( GameObject.WorldScale.x, GameObject.WorldScale.y ), GameObject.WorldScale.z ) : 1f;
	/// <summary>
	/// When the NPC has no target it will occasionally fire this off
	/// </summary>
	[Property]
	[Category( "Triggers" )]
	[ShowIf( "Idle", true )]
	public Action OnIdle { get; set; }
	private Player player;
	[Property] public GameObject Body { get; set; }
	[Property] public GameObject Eye { get; set; }
	[HostSync] public int NpcId { get; set; }
	[HostSync] public Vector3 TargetPosition { get; set; }
	[HostSync] public bool FollowingTargetObject { get; set; } = false;
	[HostSync] public Vector3 SpawnPosition { get; set; }
	[HostSync] public TimeUntil NextIdle { get; set; }
	[HostSync] public TimeUntil NextAttack { get; set; }
	[Sync] public LifeState LifeState { get; private set; } = LifeState.Alive;
	[Property] public CitizenAnimationHelper AnimationHelper { get; set; }
	public GameObject TargetObject { get; private set; } = null;
	public Collider Collider { get; private set; }
	private NavMeshAgent agent;
	[Property] public SoundEvent HitSounds { get; set; }
	private TimeSince timeSinceHit = 0;
	public int VyndaliumPoints { get; private set; }
	public int Experience { get; private set; }
	public event Action<int> VyndaliumPointsChanged;
	public event Action<int> ExperienceChanged;
	public ZombieSpawner Spawner { get; set; }
	public bool IsIdle { get; set; } = false;
	public bool IsAttacking { get; set; } = false;
	public bool IsDamaged { get; set; } = false;

	[Property] private float PlayerProximityDistance { get; set; } = 80f;
	public Guid KillerId { get; set; } // Fügen Sie diese Eigenschaft hinzu
	public bool IsSlowed { get;set; }
	[Property] public bool IsBleeding { get; set; } = false;
	[Property] public bool HasIceAbility { get; set; }
	[Property] public bool HasWindAbility { get; set; }
	[Property] public bool HasFireAbility { get; set; }
	[Property] public float WindAbilityChance { get; set; } = 0.5f;
	[Property] public float WindAbilityCooldown { get; set; } = 10f; // Abklingzeit in Sekunden
	private DateTime lastWindAbilityUse = DateTime.MinValue;
	
	[Property] public float FireAbilityChance { get; set; } = 0.3f;
	[Property] public float FireAbilityCooldown { get; set; } = 15f; // Abklingzeit in Sekunden
	private DateTime lastFireAbilityUse = DateTime.MinValue;
	[Property] public float FireDamageRadius { get; set; } = 5f; // Radius des Schadensbereichs
	[Property] public float FireDamage { get; set; } = 10f;
	[Property]public NpcState CurrentState { get; set; } = NpcState.Idle;
	public static Random random = new Random();
	public Rotation Rotation { get; set; }
	public GameObject Hitprefab { get; set; }

	public int Armor { get; set; } = 25;

	[Property] public Vector3 Position { get; set; }



	[Property]
	public NavigationType WalkingType { get; set; } = NavigationType.Dumb;

	[Property]
	public NavigationType RunningType { get; set; } = NavigationType.Smart;

	public NavigationType NavigationType => IsRunning ? RunningType : WalkingType;
	[Property] public bool isChibi = false;
	public bool isSlime = false;

	public NavMeshAgent NavMeshAgent { get; private set; }


	[Property]
	public float MoveSpeed { get; set; }
	
	[Property]private List<StatusEffect> activeStatusEffects = new List<StatusEffect>();

	public void ApplyStatusEffect( StatusEffect effect )
	{
		effect.Apply( this , player );
		activeStatusEffects.Add( effect );

		// Setze einen Timer, um den Effekt nach der Dauer zu entfernen
		
	}



	protected override void OnStart()
	{
		Tags.Set( "npc", true );

		Hitprefab = SceneUtility.GetPrefabScene( ResourceLibrary.Get<PrefabFile>( "prefabs/hitinfo.prefab" ) );
		NpcId = Scene.GetAllComponents<Npc>().OrderByDescending( x => x.NpcId ).First().NpcId + 1;

		if ( MoveHelper != null )
			MoveHelper.AirFriction = 100f;

		Collider = Components.Get<Collider>();
		SceneWorld = Game.ActiveScene.SceneWorld;
	}

	protected override void OnAwake()
	{
		
		var spawnTrace = Scene.Trace.Ray( WorldPosition + Vector3.Up * 1f, WorldPosition - Vector3.Up * 200f )
			.Size( 5f )
			.IgnoreGameObjectHierarchy( GameObject )
			.WithoutTags( "player", "npc", "trigger" )
			.Run();

		player = Scene.GetAllComponents<Player>().FirstOrDefault();
		agent = Components.Get<NavMeshAgent>();

		SpawnPosition = spawnTrace.Hit ? spawnTrace.HitPosition : WorldPosition;


		


	}


	public void InitializeNPC()
	{
		// Set a random target position around the spawn point
		TargetPosition = GetRandomPositionAround( WorldPosition );
		FollowingTargetObject = false;
	}
	public void MoveToTargetPosition()
	{
		if ( WorldPosition.Distance( TargetPosition ) <= 5f )
		{
			TargetPosition = GetRandomPositionAround( WorldPosition );
		}
		else
		{
			var direction = (TargetPosition - WorldPosition).Normal;
			WorldPosition += direction * (IsRunning ? RunSpeed : WalkSpeed) * Time.Delta;
		}
	}

	public  void TryFreezePlayer( float durationInSeconds , Player player )
	{
		
		if ( HasIceAbility )
		{
			int freezeChance = Level switch
			{
				<= 15 => 15,
				<= 30 => 30,
				<= 55 => 55,
				<= 70 => 70,
				<= 90 => 90,
				_ => 100
			};

			if ( random.Next( 100 ) < freezeChance )
			{
				

				
			}
		}
	}
	


	private bool IsPlayerNearby()
	{
		if ( Network.IsProxy )
			return false;

		var players = Scene.GetAllComponents<Player>();
		foreach ( var player in players )
		{
			// Überprüfe, ob der Spieler in der Nähe ist
			if ( (player.WorldPosition - this.WorldPosition).Length < PlayerProximityDistance )
				return true;
		}
		return false;
	}

	protected override void OnUpdate()
	{
		// Überprüfe auf Vorbedingungen, um eine ungültige Ausführung zu vermeiden
		if ( Model == null || (Healthone != null && !Healthone.Alive) )
			return;

		bool isPlayerNearby = IsPlayerNearby();

			

		// Suchen Sie nach allen Spielern in der Szene
		var players = Scene.GetAllComponents<Player>();

		// Finden Sie den Spieler, der dem NPC am nächsten ist
		Player closestPlayer = players.FirstOrDefault();
		var closestDistanceSquared = float.MaxValue;

		foreach ( var player in players )
		{
			Vector3 direction = player.WorldPosition - WorldPosition;
			var distanceSquared = direction.LengthSquared;

			if ( distanceSquared < closestDistanceSquared )
			{
				closestDistanceSquared = distanceSquared;
				closestPlayer = player;
			}
		}

		if (PogMode)
		{
			
		}

		if ( closestPlayer != null )
		{
			var closestDistance = MathF.Sqrt( closestDistanceSquared );

			// Überprüfen, ob der Spieler innerhalb der Reichweite ist
			if ( closestDistance <= VisionRange || IsWithinRange( closestPlayer.GameObject, DetectRange ) )
			{
				
				SetTarget( closestPlayer.GameObject );

				// Richte den NPC auf die Bewegungsrichtung aus, falls erforderlich
				if ( Ragdoll == null && FaceTowardsVelocity )
				{
					if ( !MoveHelper.Velocity.IsNearlyZero( 1f ) )
					{
						WorldRotation = Rotation.Lerp( WorldRotation, Rotation.LookAt( MoveHelper.Velocity.WithZ( 0f ), Vector3.Up ), Time.Delta * 5.0f );
					}
				}
				

				UpdateAnimations( closestPlayer );
				float maxProximityDistance = 80f;

				// Überprüfe die Entfernung zum nächsten Spieler und passe die Bewegungsart entsprechend an
				if ( closestDistance < maxProximityDistance )
				{
					CurrentState = NpcState.Walking;
					
					AnimationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Walk;
					
					agent.Stop();
					NormalTrace();
				}
				else
				{
					CurrentState = NpcState.Attacking;

					// Setze den HoldType basierend auf dem aktuellen HoldType
					switch ( CurrentHoldType )
					{
						case HoldTypes.None:
							AnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
							break;
						case HoldTypes.Pistol:
							AnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.Pistol;
							break;
						case HoldTypes.Rifle:
							AnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.Rifle;
							break;
						case HoldTypes.Shotgun:
							AnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.Shotgun;
							break;
						case HoldTypes.HoldItem:
							AnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.HoldItem;
							break;
						case HoldTypes.Punch:
							AnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.Punch;
							break;
						case HoldTypes.Swing:
							AnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.Swing;
							break;
						case HoldTypes.RPG:
							AnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.RPG;
							break;
						default:
							AnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
							break;
					}

					agent.MoveTo( closestPlayer.WorldPosition );
					if ( !isPlayerNearby )
					{
						CurrentState = NpcState.Running;

						AnimationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Run;
						if ( isChibi && Model != null )
						{
							
						
							Model.Set( "chibi_run", true );
							
						}
							
					}
					
				}
				// Füge die Logik für den Chibi-Zombie hinzu
				
			}
			else
			{
				// Spieler ist außerhalb der Reichweite, NPC sollte aufhören, ihn zu verfolgen
				Undetected();
				
				MoveToTargetPosition();
			
			}
		}
		else
		{
			CurrentState = NpcState.Idle;
			if(isChibi && Model != null)
			{
				Model.Set( "chibi_idle", true );
				Log.Info( "Chibi Idle" );
			}
			if ( TargetObject != null )
			{
				// Überprüfen, ob das Ziel immer noch gültig ist, oder es außerhalb der Reichweite ist
				if ( !IsWithinRange( TargetObject ) )
				{
					// Ziel außerhalb der Reichweite, verfolge weiterhin das letzte Ziel
					agent.MoveTo( TargetObject.WorldPosition );

				}
				else
				{
					DetectAround();
					
				}
			}
			else
			{
				// Wenn kein Zielobjekt vorhanden ist, bewege den NPC zu einer zufälligen Position
				MoveToTargetPosition();
			
				DetectAround();
				
			}
		}
		if ( RecentlyDamaged && Time.Now - LastDamageTime > DamageCooldown )
		{
			RecentlyDamaged = false;
		}

		// Wenn kein Zielobjekt vorhanden ist, bewege den NPC zur letzten bekannten Position des Spielers
		

		UpdateFootAnimations();
	}
	

	void UpdateAnimations( Player player )
	{
		AnimationHelper.WithWishVelocity( agent.WishVelocity );
		AnimationHelper.WithVelocity( MoveHelper.Velocity );

		var playerPosition = player.GameObject.WorldPosition.WithZ( WorldPosition.z );
		var targetRotation = Rotation.LookAt( playerPosition - Body.WorldPosition );
		Body.WorldRotation = Rotation.Slerp( Body.WorldRotation, targetRotation, Time.Delta * 5.0f );

		// Setzen Sie die Bewegungsart nur, wenn sich die Geschwindigkeit ändert
		switch ( CurrentState )
		{
			case NpcState.Idle:
				// Set idle animations
				break;
			case NpcState.Walking:
				// Set walking animations
				break;
			case NpcState.Running:
				// Set running animations
				break;
			case NpcState.Attacking:
				// Set attacking animations
				break;
		}
		

	}
	

	void UpdateFootAnimations()
	{
		// Holen Sie die Geschwindigkeit des NPCs
		var scaledSpeed = MaxRunAnimationSpeed ;
		var forwardVelocity = Vector3.Dot( MoveHelper.Velocity, Model.WorldRotation.Forward ) / scaledSpeed;
		var rightVelocity = Vector3.Dot( MoveHelper.Velocity, Model.WorldRotation.Right ) / scaledSpeed;

		// Lerp nur, wenn sich die Geschwindigkeit ändert
		var oldX = Model.GetFloat( "move_x" );
		var oldY = Model.GetFloat( "move_y" );
		var newX = MathX.Lerp( oldX, forwardVelocity, Time.Delta * 5f );
		var newY = MathX.Lerp( oldY, rightVelocity, Time.Delta * 5f );

		// Batchen Sie die Set-Operationen
		Model.Set( "move_x", newX );
		Model.Set( "move_y", newY );
	}
	private Random random2 = new Random();
	public void NormalTrace()
	{
		
		var tr = Scene.Trace.Ray( Body.WorldPosition, Body.WorldPosition + Body.WorldRotation.Forward * 100 ).Run();

		if ( tr.Hit && timeSinceHit > 1.5f && GameObject != null )
		{
			IHealthComponent damageable = tr.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();

			if (damageable != null && tr.GameObject.Tags.Has( "player" ) || tr.GameObject.Tags.Has( "npc" ) )
			{
				// Annahme: tr.GameObject kann in Player umgewandelt werden
				var player = tr.GameObject.Components.Get<Player>();
				if ( player != null )
				{
					int baseDamage = random2.Next(1, 16);

					// Berechne den exponentiellen Schaden basierend auf dem Level des NPCs
					int npcLevel = this.Level; // Angenommen, der NPC hat eine Level-Eigenschaft
					int exponentialDamage = (int)(baseDamage * Math.Pow(1.1, npcLevel));

					// Berücksichtige die Rüstung des Spielers als Prozentsatz
					int playerDefensePercentage = random.Next((int)player.MinArmorValue / 10, (int)player.MaxArmorValue / 10 + 1);
					double damageReductionFactor = (100 - playerDefensePercentage) / 100.0;

					// Berechne den endgültigen Schaden unter Berücksichtigung der Rüstung
					int finalDamage = (int)(exponentialDamage * damageReductionFactor);

					if (player.Block > 0)
					{
						double coverReduction = Math.Min(player.Block / 50.0, 0.5); // Maximal 50% Reduktion
						finalDamage = (int)(finalDamage * (1 - coverReduction));
					}



					// Fügen Sie die GameObject.Id des angreifenden Spielers hinzu
					damageable.TakeDamage(DamageType.Bullet, finalDamage, tr.EndPosition, tr.Direction * 5, GameObject.Id, GameObject.Id);

					if (HasFireAbility)
					{
						// Generiere eine zufällige Brenndauer zwischen 1 und 5 Sekunden
						int burnDuration = random2.Next(1, 6);
						ApplyBurn(player, burnDuration);
					}
					AnimationHelper.Target.Set("b_attack", true);

					if (Model != null && isChibi)
					{
						Model.Set("chibi_attack", true);
					}
					if (Model != null && isSlime)
					{
						// Erzeuge eine Zufallszahl zwischen 0 und 1
						Random random2 = new Random();
						int randomNumber = random2.Next(0, 2); // 0 oder 1

						// Wähle zufällig zwischen den beiden Animationen
						if (randomNumber == 0)
						{
							Model.Set("slime_attack", true);
						}
						else
						{
							Model.Set("slime_attack_v2", true);

						}
					}


					timeSinceHit = 0;

					Sound.Play(HitSounds, WorldPosition);

				}
				else
				{
					
					
				}
				
				
			}
		}
	}
	private void ApplyBurn( Player player, int duration )
	{
		if ( player == null )
		{
			
			return;
		}

		
		player.ApplyStatusEffect( new GeneralGame.BurnEffect( duration ) );
	}


	protected override void OnFixedUpdate()
	{
		if ( Healthone != null && Healthone.Alive ) // If we are still alive
		{
			if ( Ragdoll == null ) // If we are not ragdolled
			{
				if ( !IsPlayerNearby() )


					return;
				if ( TargetObject == null )
				{
					if ( Idle && NextIdle )
					{
						BroadcastOnIdle();
						NextIdle = Game.Random.Float( MinimumIdleCooldown, MaximumIdleCooldown );
					}
				}

				if ( MoveHelper == null ) return;
				{
					
				}


			}
		}
		else
		{
			MoveHelper.WishVelocity = 0;

		}



	}
	[Broadcast]
	private void BroadcastOnIdle()
	{
		OnIdle?.Invoke();
	}



	/// <summary>
	/// Get all provokers inside of its detect area
	/// </summary>
	public IEnumerable<GameObject> ProvokersInArea { get; set; }

	public void DetectAround()
	{
		if ( TargetObject != null )
		{
			if ( IsWithinRange( TargetObject ) ) // Is the target within reach
			{
				if ( NextAttack )
				{
					BroadcastOnAttack();
					NextAttack = AttackCooldown;
				}
			}
		}

		var currentTick = (int)(Time.Now / Time.Delta);
		if ( currentTick % 20 != NpcId % 20 ) return; // Check every 20 ticks

		var foundAround = Scene.FindInPhysics( new Sphere( WorldPosition, DetectRange * Scale ) ) // Find gameobjects nearby
			.Where( x => x.Enabled )
			.Where( x => EnemyTags != null && x.Tags.HasAny( EnemyTags ) ) // Do they have any of our enemy tags
			.Where( x => x.Components.Get<HealthComponent>()?.Alive ?? true ); // Are they dead or undead

		if ( TargetObject == null )
		{
			if ( foundAround.Any() )
				Detected( foundAround.First(), true ); // If we don't have any target yet, pick the first one around us
		}
		else
		{
			var healthComponent = TargetObject.Components.Get<IHealthComponent>();
			var targetDead = healthComponent?.LifeState == LifeState.Dead;
			var targetEscaped = TargetObject.WorldPosition.Distance( WorldPosition ) > VisionRange * Scale; // Did our target get out of vision range

			if ( targetEscaped || targetDead ) // Did our target die or escape
				Undetected();
		}
	}


	[Broadcast]
	private void BroadcastOnAttack()
	{
		if ( TargetObject != null )
			OnAttack?.Invoke( TargetObject );
	}

	public void Detected( GameObject target, bool alertOthers = false )
	{
		if ( target == null ) return;
		target = target.Parent == null || target.Parent == Scene ? target : target.Parent;
		if ( target == TargetObject ) return;

		SetTarget( target );
		BroadcastOnDetect();



		if ( alertOthers && AlertOthers )
		{
			var otherNpcs = Scene.GetAllComponents<Npc>()
				.Where( x => x.WorldPosition.Distance( WorldPosition ) <= x.VisionRange  )
				.Where( x => x.Healthone?.Alive ?? true )
				.Where( x => x.TargetObject == null )
				.Where( x => x != this )
				.Where( x => x.GameObject != null )
				.Where( x => !x.EnemyTags.HasAny( Tags ) );

			foreach ( var npc in otherNpcs )
				npc.Detected( target, false );
		}

	}


	[Broadcast]
	private void BroadcastOnDetect()
	{
		if ( TargetObject is not null )
			OnDetect?.Invoke( TargetObject );
	}

	public void Damaged( GameObject target )
	{
		if ( TargetObject == null && TargetObject != target )
			Detected( target, true );
	}

	public void Undetected()
	{
		BroadcastOnEscape();

		TargetObject = null;
		TargetPosition = WorldPosition;
		
	}

	[Broadcast]
	private void BroadcastOnEscape()
	{
		if ( TargetObject is not null )
			OnEnemyEscaped?.Invoke( TargetObject );
	}
	private bool IsPlayerDetected( GameObject player, Vector3 position, float range )
	{
		return Vector3.DistanceBetween( player.WorldPosition, position ) <= range;
	}
	private const float DetectionRange = 150.0f;
	private const float MaxTeleportRange = 70.0f;
	/// <summary>
	/// Who should the NPC follow, set null to go back to manually setting the target position
	/// </summary>
	/// <param name="target"></param>
	/// <param name="escapeFrom"></param>
	public void SetTarget( GameObject target, bool escapeFrom = false )
	{
		if ( target == null )
		{
			TargetObject = null;
			FollowingTargetObject = false;

			TargetPosition = WorldPosition;
			
		}
		else
		{
			
			TargetObject = target;
			FollowingTargetObject = !escapeFrom;

			if ( HasWindAbility && (DateTime.Now - lastWindAbilityUse).TotalSeconds >= WindAbilityCooldown )
			{
				if ( IsPlayerDetected( target, WorldPosition, DetectionRange ) )
				{
					Random random = new Random();
					if ( random.NextDouble() <= WindAbilityChance )
					{
						Vector3 randomOffset;
						do
						{
							// Berechnung des Offsets hinter dem Spieler
							Vector3 playerForward = target.WorldRotation.Forward;
							float offsetDistance = (float)(random.NextDouble() * MaxTeleportRange);
							randomOffset = -playerForward * offsetDistance;

							// Zufällige Abweichung hinzufügen
							float offsetX = (float)(random.NextDouble() * 2 - 1) * MaxTeleportRange * 0.2f; // 20% der MaxTeleportRange
							float offsetY = (float)(random.NextDouble() * 2 - 1) * MaxTeleportRange * 0.2f; // 20% der MaxTeleportRange
							float offsetZ = (float)(random.NextDouble() * 2 - 1) * MaxTeleportRange * 0.2f; // 20% der MaxTeleportRange

							randomOffset += new Vector3( offsetX, offsetY, offsetZ );
						} while ( randomOffset.Length < 3 ); // Mindestens 10 Einheiten entfernt

						Vector3 targetPosition = target.WorldPosition + randomOffset;

						// Überprüfen, ob der Zielort innerhalb der maximalen Reichweite liegt
						if ( (targetPosition - target.WorldPosition).Length <= MaxTeleportRange )
						{
							WorldPosition = targetPosition;

							// Spiele den Sound an der neuen Position des NPCs ab
							Sound.Play( "/sounds/chargedattack.sound", WorldPosition );

							// Aktualisiere den letzten Aktivierungszeitpunkt
							lastWindAbilityUse = DateTime.Now;

							// Optional: Füge eine visuelle oder akustische Rückmeldung hinzu
							// z.B. einen Partikeleffekt oder einen Sound
						}
					}
				}
			}
			if ( HasFireAbility && (DateTime.Now - lastFireAbilityUse).TotalSeconds >= FireAbilityCooldown )
			{
				Random random = new Random();
				if ( random.NextDouble() <= FireAbilityChance )
				{
					// Erzeuge einen Schadensbereich um den NPC
					CreateFireDamageArea();

					// Spiele den Sound für die Feuerfähigkeit ab
					Sound.Play( "/sounds/fireattack.sound", WorldPosition );

					// Aktualisiere den letzten Aktivierungszeitpunkt
					lastFireAbilityUse = DateTime.Now;

					// Optional: Füge eine visuelle oder akustische Rückmeldung hinzu
					// z.B. einen Partikeleffekt oder einen Sound
				}
			}
		}
	}
	
	private void CreateFireDamageArea()
	{
		// Finde alle Spieler im Schadensbereich
		var playersInRange = FindPlayersInRange( WorldPosition, FireDamageRadius );

		// Füge allen Spielern im Bereich Schaden zu
		foreach ( var player in playersInRange )
		{
			var healthComponent = player.GetComponent<IHealthComponent>();
			if ( healthComponent != null )
			{
				healthComponent.TakeDamage( DamageType.fire, FireDamage, WorldPosition, Vector3.Zero, Guid.Empty, player.Id );
				
			}
		}
	}
	

	private IEnumerable<GameObject> FindPlayersInRange( Vector3 position, float radius )
	{
		var playersInRange = new List<GameObject>();

		// Verwende Scene.Trace, um alle Spieler im angegebenen Radius zu finden
		var traceResult = Scene.Trace.Sphere( radius, position, position + Vector3.One * radius )
			.WithTag( "Player" )
			.RunAll();

		foreach ( var hit in traceResult )
		{
			if ( hit.GameObject != null && hit.GameObject.Tags.Has( "Player" ) )
			{
				playersInRange.Add( hit.GameObject );
			}
		}

		return playersInRange;
	}

	/// <summary>
	/// Is the object within the npc's reach (AttackRange) 
	/// </summary>
	/// <param name="target"></param>
	/// <returns></returns>
	public bool IsWithinRange( GameObject target )
	{
		if ( !GameObject.IsValid() ) return false;

		return IsWithinRange( target, AttackRange  );
	}

	/// <summary>
	/// Is the object within the npc's range (Unscaled)
	/// </summary>
	/// <param name="target"></param>
	/// <param name="range"></param>
	/// <returns></returns>
	public bool IsWithinRange( GameObject target, float range = 60f )
	{
		if ( !GameObject.IsValid() ) return false;

		return target.WorldPosition.Distance( WorldPosition ) <= range;
	}
	public void SetHealthBasedOnLevel()
	{
		// Berechne das MaxHealth und Health basierend auf dem Level
		MaxHealth = (float)(100 * Math.Pow( 1.09, Level ));
		Health = MaxHealth;
	}

	/// <summary>
	/// Get a random position around the position (Horizonal)
	/// </summary>
	/// <param name="position"></param>
	/// <param name="minRange"></param>
	/// <param name="maxRange"></param>
	/// <returns></returns>
	public static Vector3 GetRandomPositionAround( Vector3 position, float minRange = 50f, float maxRange = 300f )
	{
		var tries = 0;
		var hitGround = false;
		var hitPosition = position;

		while ( hitGround == false && tries <= 10f )
		{
			var randomDirection = Rotation.FromYaw( Game.Random.Float( 360f ) ).Forward;
			var randomDistance = Game.Random.Float( minRange, maxRange );
			var randomPoint = position + randomDirection * randomDistance;

			var groundTrace = Game.ActiveScene.Trace.Ray( randomPoint + Vector3.Up * 64f, randomPoint + Vector3.Down * 64f )
				.Size( 5f )
				.WithoutTags( "player", "npc", "trigger" )
				.Run();

			if ( groundTrace.Hit && !groundTrace.StartedSolid )
			{
				hitGround = true;
				hitPosition = groundTrace.HitPosition;
			}

			tries++;
		}

		return hitPosition;
	}



	/// <summary>
	/// Where the NPC should find themselves when near the target
	/// </summary>
	/// <param name="target"></param>
	/// <returns></returns>
	public Vector3 GetPreferredTargetPosition( GameObject target )
	{
		if ( !target.IsValid() )
			return TargetPosition;

		var targetPosition = target.WorldPosition;

		var direction = (WorldPosition - targetPosition).Normal;
		var offset = FollowingTargetObject ? direction * AttackRange  / 2f : direction * VisionRange ;
		var wishPos = targetPosition + offset;

		var groundTrace = Scene.Trace.Ray( wishPos + Vector3.Up * 64f, wishPos + Vector3.Down * 64f )
			.Size( 5f )
			.IgnoreGameObjectHierarchy( GameObject )
			.WithoutTags( "player", "npc", "trigger" )
			.Run();

		return groundTrace.Hit && !groundTrace.StartedSolid ? groundTrace.HitPosition : (FollowingTargetObject ? targetPosition : targetPosition + offset);
	}
	public static Player Host { get; set; }
	[Property] public bool PogMode { get; private set; }

	public event Action OnTakeDamage;
	private bool RecentlyDamaged { get; set; }
	private float DamageCooldown = 5.0f; // Zeit in Sekunden, wie lange der NPC nach Schaden den Spieler verfolgt
	private float LastDamageTime;
	private int CalculateVyndaliumReward( int npcLevel )
	{
		if ( npcLevel <= 10 )
		{
			return new Random().Next( 5, 8 ); // 5-15 Vyndalium für Level 1-10
		}
		else if ( npcLevel <= 20 )
		{
			return new Random().Next( 10, 15 ); // 15-30 Vyndalium für Level 11-20
		}
		else if ( npcLevel <= 30 )
		{
			return new Random().Next( 15, 30 ); // 30-50 Vyndalium für Level 21-30
		}
		else if ( npcLevel <= 40 )
		{
			return new Random().Next( 25, 51 ); // 50-70 Vyndalium für Level 31-40
		}
		else if ( npcLevel <= 50 )
		{
			return new Random().Next( 70, 91 ); // 70-90 Vyndalium für Level 41-50
		}
		else if ( npcLevel <= 60 )
		{
			return new Random().Next( 90, 111 ); // 90-110 Vyndalium für Level 51-60
		}
		else if ( npcLevel <= 70 )
		{
			return new Random().Next( 110, 131 ); // 110-130 Vyndalium für Level 61-70
		}
		else if ( npcLevel <= 80 )
		{
			return new Random().Next( 130, 151 ); // 130-150 Vyndalium für Level 71-80
		}
		else if ( npcLevel <= 90 )
		{
			return new Random().Next( 150, 171 ); // 150-170 Vyndalium für Level 81-90
		}
		else
		{
			return new Random().Next( 171, 201 ); // 170-200 Vyndalium für Level 91-100
		}
	}
	private int CalculateXpReward( int npcLevel )
	{
		int halfNpcLevel = npcLevel / 2;

		if ( npcLevel <= 10 )
		{
			return new Random().Next( 2, 16) * halfNpcLevel; // 5-15 XP pro halbes Level für Level 1-10
		}
		else if ( npcLevel <= 20 )
		{
			return new Random().Next( 16, 32) * halfNpcLevel; // 15-30 XP pro halbes Level für Level 11-20
		}
		else if ( npcLevel <= 30 )
		{
			return new Random().Next( 32, 450 ) * halfNpcLevel; // 30-50 XP pro halbes Level für Level 21-30
		}
		else if ( npcLevel <= 40 )
		{
			return new Random().Next( 45, 7000 ) * halfNpcLevel; // 50-70 XP pro halbes Level für Level 31-40
		}
		else if ( npcLevel <= 50 )
		{
			return new Random().Next( 48, 16500 ) * halfNpcLevel; // 70-90 XP pro halbes Level für Level 41-50
		}
		else if ( npcLevel <= 60 )
		{
			return new Random().Next( 65, 35000 ) * halfNpcLevel; // 90-110 XP pro halbes Level für Level 51-60
		}
		else if ( npcLevel <= 70 )
		{
			return new Random().Next( 81, 45000) * halfNpcLevel; // 110-130 XP pro halbes Level für Level 61-70
		}
		else if ( npcLevel <= 80 )
		{
			return new Random().Next( 100, 56151 ) * halfNpcLevel; // 130-150 XP pro halbes Level für Level 71-80
		}
		else if ( npcLevel <= 90 )
		{
			return new Random().Next( 150, 86171 ) * halfNpcLevel; // 150-170 XP pro halbes Level für Level 81-90
		}
		else
		{
			return new Random().Next( 170, 101201 ) * halfNpcLevel; // 170-200 XP pro halbes Level für Level 91-100
		}
	}
	
	[Broadcast]
	public void TakeDamage( DamageType type, float amount, Vector3 hitPosition, Vector3 hitDirection, Guid attackerId, Guid playerId )
	{
		if ( LifeState == LifeState.Dead )
			return;

		if (Armor > 0)
		{
			amount *= 0.75f; // Reduzieren Sie den Schaden um 25%
			Armor -= (int)amount; // Verringern Sie die Armor um den reduzierten Schaden
			if (Armor < 0)
			{
				Armor = 0; // Stellen Sie sicher, dass Armor nicht negativ wird
			}
		}

		if ( type == DamageType.Bullet || type == DamageType.Serious )
		{

			var p = new SceneParticles( Scene.SceneWorld, "particles/impact.flesh.bloodpuff.vpcf" );
			p.SetControlPoint( 0, hitPosition );
			p.SetControlPoint( 0, Rotation.LookAt( hitDirection.Normal * -1f ) );
			p.SetControlPoint( 1, new Vector3( 0.5f, 0.1f, 0.1f ) );
			p.PlayUntilFinished( Task );
		}
		if ( Model != null && isSlime )
		{
			Model.Set( "slime_damage", true );
		} 
		else if ( Model != null && isChibi )
		{
			Model.Set( "chibi_damage", true );
		}
		
		
		
		

		if ( Network.IsProxy )
			return;

		Health = Math.Clamp( Health - amount, 0f, MaxHealth );

		

		OnTakeDamage?.Invoke();

		RecentlyDamaged = true;
		LastDamageTime = Time.Now;

		if ( Health <= 0f ) // checks if zombie is dead
		{

			LifeState = LifeState.Dead;
			
			var zombie = ZombieRagedol.Clone( this.GameObject.WorldPosition, this.GameObject.WorldRotation );
			zombie.NetworkSpawn();
			
			SpawnItemAtPosition( this.GameObject.WorldPosition );
			

			KillerId = attackerId;


			GameObject.Destroy();

			var killer = Scene.Directory.FindByGuid( attackerId );

			if ( killer == null )
			{
				// yes
				return;
			}



			var killerPlayer = killer.Components.Get<Player>( FindMode.EverythingInSelfAndAncestors );
			if ( killerPlayer == null )
			{
				// Logge oder handle den Fehler
				return;
			}
			if ( this == null )
			{
				// Logge oder handle den Fehler
				return;
			}

			int npcLevel = this.Level;

			// Skalieren der Punkte basierend auf dem Level des NPC
			int vyndaliumPointsToAdd = CalculateVyndaliumReward( npcLevel );
			int xpPointsToAdd = CalculateXpReward( npcLevel );


			if ( DeathSounds != null )
			{
				
				Sound.Play( DeathSounds, Player.Local.Head.WorldPosition );
			}
			killerPlayer.GiveVyndalium( vyndaliumPointsToAdd );
			killerPlayer.AddVyndalium( vyndaliumPointsToAdd );
			killerPlayer.GiveXp( xpPointsToAdd );
			CreateXpOrbEffect( this.GameObject.WorldPosition, killerPlayer.GameObject.WorldPosition );

			if ( Hitprefab != null && this.GameObject != null )
			{
				GameObject vyndaliumHitInfo = Hitprefab.Clone( this.GameObject.WorldPosition + new Vector3( 30, 0, 25 ) );
				if ( vyndaliumHitInfo != null )
				{
					FaceThing vyndaliumFaceThing = vyndaliumHitInfo.Components.Get<FaceThing>();
					if ( vyndaliumFaceThing != null )
					{
						vyndaliumFaceThing.Thing = killerPlayer.GameObject;
					}

					TextRenderer vyndaliumTextRenderer = vyndaliumHitInfo.Components.Get<TextRenderer>();
					if ( vyndaliumTextRenderer != null )
					{
						vyndaliumTextRenderer.Color = Color.Yellow;
						
						vyndaliumTextRenderer.Text = $"+{vyndaliumPointsToAdd} $";
					}

					ScaleTextWithDistance vyndaliumScaleText = vyndaliumHitInfo.Components.Get<ScaleTextWithDistance>();
					if ( vyndaliumScaleText != null )
					{
						vyndaliumScaleText.Thing = killerPlayer.GameObject;
						
					}

					// Start coroutine to move and destroy the hit info
					
				}

				GameObject xpHitInfo = Hitprefab.Clone( this.GameObject.WorldPosition + new Vector3( 0, 0, 50 ) );
				if ( xpHitInfo != null )
				{
					FaceThing xpFaceThing = xpHitInfo.Components.Get<FaceThing>();
					if ( xpFaceThing != null )
					{
						xpFaceThing.Thing = killerPlayer.GameObject;
					}

					TextRenderer xpTextRenderer = xpHitInfo.Components.Get<TextRenderer>();
					if ( xpTextRenderer != null )
					{
						xpTextRenderer.Color = Color.Blue;
						xpTextRenderer.Text = $"+{xpPointsToAdd} XP";
						
					}

					ScaleTextWithDistance xpScaleText = xpHitInfo.Components.Get<ScaleTextWithDistance>();
					if ( xpScaleText != null )
					{
						xpScaleText.Thing = killerPlayer.GameObject;
					}
					
					// Start coroutine to move and destroy the hit info

				}

			
			}





		
			GameObject.Destroy();
		  
			killerPlayer.OnZombieKilled();


		};

	}

	public void SpawnItemAtPosition( Vector3 position )
	{
		SpawnRandomPrefab( position );
	}
	public void CreateXpOrbEffect( Vector3 npcPosition, Vector3 playerPosition )
	{
		// Erhöhe die z-Koordinate der Positionen, um den Partikeleffekt nach oben zu verschieben
		Vector3 adjustedNpcPosition = new Vector3( npcPosition.x, npcPosition.y, npcPosition.z + 100.0f );
		Vector3 adjustedPlayerPosition = new Vector3( playerPosition.x, playerPosition.y, playerPosition.z + 100.0f );

		var p = new SceneParticles( Scene.SceneWorld, "particles/bleed.vpcf" );
		p.SetControlPoint( 0, adjustedNpcPosition );
		p.SetControlPoint( 1, adjustedPlayerPosition ); // Endposition des Strahls
		p.SetControlPoint( 2, (adjustedPlayerPosition - adjustedNpcPosition).Length ); // Distanz zwischen NPC und Spieler
		p.PlayUntilFinished( Task );
	}




	public event Action<int> VyndaliumAdded; // Declare the event "VyndaliumAdded"

	public bool GiveVyndalium(int baseAmount)
	{
		var player = Player.Local;
		int bonusVyndalium = (int)(baseAmount * player.BonusVyndalium);
		int totalVyndalium = baseAmount + bonusVyndalium;
		VyndaliumPoints += totalVyndalium;
		VyndaliumPointsChanged?.Invoke(VyndaliumPoints);
		VyndaliumAdded?.Invoke(totalVyndalium); // Benachrichtige alle Abonnenten über die Änderung der Vyndalium-Punkte
		return true;
	}
	public bool GiveXp(int baseAmount)
	{
		var player = Player.Local;
		int bonusXp = (int)(baseAmount * player.BonusEXPGain);
		int totalXp = baseAmount + bonusXp;
		Experience += totalXp;
		ExperienceChanged?.Invoke(Experience);
		return true;
	}
	public SceneWorld SceneWorld { get; set; }

	public void CreateParticleEffect( Vector3 position, Rotation rotation )
	{
		// Erhöhe die z-Koordinate der Position, um den Partikeleffekt nach oben zu verschieben
		Vector3 adjustedPosition = new Vector3( position.x, position.y, position.z + 100.0f ); // Erhöhe die z-Koordinate um 20.0f


		var p = new SceneParticles( Scene.SceneWorld, "particles/fire.vpcf" );
		p.SetControlPoint( 0, adjustedPosition );
		p.SetControlPoint( 1, rotation.Forward * -5f );
		p.SetControlPoint( 2, new Vector3( 0f, 0f, 0f ) );
		p.PlayUntilFinished( Task );

	
	}
	public void CreateParticleEffectBleed( Vector3 npcPosition, Vector3 playerPosition, Rotation rotation )
	{
		// Erhöhe die z-Koordinate der Positionen, um den Partikeleffekt nach oben zu verschieben
		Vector3 adjustedNpcPosition = new Vector3( npcPosition.x, npcPosition.y, npcPosition.z + 100.0f );
		Vector3 adjustedPlayerPosition = new Vector3( playerPosition.x, playerPosition.y, playerPosition.z + 100.0f );

		var p = new SceneParticles( Scene.SceneWorld, "particles/bleed.vpcf" );
		p.SetControlPoint( 0, adjustedNpcPosition );
		p.SetControlPoint( 1, adjustedPlayerPosition ); // Endposition des Strahls
		p.SetControlPoint( 2, (adjustedPlayerPosition - adjustedNpcPosition).Length ); // Distanz zwischen NPC und Spieler
		p.PlayUntilFinished( Task );
	}



}
public abstract class StatusEffect
{
	public float Duration { get; set; }
	public abstract void Apply( Npc npc , Player attacker);
}
public class BurnEffectNpc : StatusEffect
    {
        public BurnEffectNpc(float duration)
        {
            Duration = duration;
        }
		

	public override async void Apply( Npc npc , Player attacker)
	{
		int damagePerSecond = 10; // Schaden pro Sekunde
		int totalDuration = (int)Duration;  // Gesamtdauer des Brenneffekts in Sekunden

		for ( int i = 0; i < totalDuration; i++ )
		{
			await Task.Delay( 1000 ); // Verzögerung um 1 Sekunde

			// Überprüfen, ob der NPC noch lebt
			if ( npc.Health > 0 )
			{
				// Fügen Sie dem NPC Schaden zu
				npc.Health -= damagePerSecond;
				npc.CreateParticleEffect( npc.WorldPosition, Rotation.Identity );


			}
		}
	}
}
public class BleedEffect : StatusEffect
{
	
	public BleedEffect( float duration )
	{
		Duration = duration;
	}

	public override async void Apply( Npc npc, Player attacker )
	{
		int damagePerSecond = 5; // Schaden pro Sekunde
		int totalDuration = (int)Duration;  // Gesamtdauer des Bluteffekts in Sekunden
		

		npc.IsBleeding = true;

		for ( int i = 0; i < totalDuration; i++ )
		{
			await Task.Delay( 1000 ); // Verzögerung um 1 Sekunde

			// Überprüfen, ob der NPC noch lebt
			if ( npc.Health > 0 )
			{
				// Heile den Angreifer um 1% seines maximalen Lebens

				// Fügen Sie dem NPC Schaden zu
				npc.Health -= damagePerSecond;
				
				// Erstelle Partikeleffekt
				

				
			}
		}

		npc.IsBleeding = false;
	}
}

public class SlowEffect : StatusEffect
{


	public override void Apply( Npc npc , Player attacker)
	{
		// Setze das IsSlowed-Flag auf true
		npc.IsSlowed = true;

		// Implementiere die Logik für den Verlangsamungseffekt
		//npc.MoveSpeed *= 0.5f; // Beispiel: Reduziere die Bewegungsgeschwindigkeit um 50%
		

		// Setze einen Timer, um den Effekt nach der Dauer zu entfernen
		Task.Delay( (int)(Duration * 1000) ).ContinueWith( _ =>
		{
			// Setze die Bewegungsgeschwindigkeit zurück
			npc.MoveSpeed /= 0.5f;
			// Setze das IsSlowed-Flag auf false
			npc.IsSlowed = false;
		} );
	}


}
public class KnockbackEffect : StatusEffect
{
	public Vector3 KnockbackDirection { get; set; }
	public float KnockbackForce { get; set; }
	private bool isKnockedBack = false;

	public KnockbackEffect(float duration, Vector3 knockbackDirection, float knockbackForce)
	{
		Duration = duration;
		KnockbackDirection = knockbackDirection;
		KnockbackForce = knockbackForce;
	}

	public override async void Apply(Npc npc, Player attacker)
	{
		Log.Info("Applying knockback effect");
		if (isKnockedBack)
		{
			Log.Info("Knockback effect already active, ignoring new effect");
			return;
		}

		if (npc == null)
		{
			Log.Error("NPC is null, cannot apply knockback effect");
			return;
		}

		if (npc.Position == null)
		{
			Log.Error("NPC position is null, cannot apply knockback effect");
			return;
		}

		isKnockedBack = true;
		Log.Info("Knockback effect applied");
		// Speichere die ursprüngliche Position des NPCs
		Vector3 originalPosition = npc.Position;

		// Katapultiere den NPC um 40 Einheiten nach oben
		Vector3 knockbackPosition = originalPosition;
		//knockbackPosition.z += 40;
		npc.Position = knockbackPosition;

		// Warte für die Dauer des Effekts
		await Task.Delay((int)(Duration * 1000));

		// Setze die Position des NPCs auf den Boden zurück
		//npc.Position = new Vector3(npc.Position.x, npc.Position.y, originalPosition.z);

		// Markiere den NPC als "grounded"
		isKnockedBack = false;
	}
}