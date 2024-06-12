using GeneralGame;
using GeneralGame.HUD;
using Sandbox.Citizen;
using Sandbox.UI;
using System.Linq;
using Sandbox;
using static GeneralGame.NpcNodes;
using System;



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



public partial class Npc : Component ,IHealthComponent
{
	[Property]
	public string Name { get; set; } = "Default";

	[Property]
	public MoveHelper MoveHelper { get; set; }
	[Property] public GameObject ZombieRagedol { get; set; }
	[Property]public SkinnedModelRenderer Model { get; set; }
	[Sync, Property] public float MaxHealth { get; private set; } = 100f;
	[Sync,Property] public float Health { get; private set; } = 100f;
	[Property] public HealthComponent Healthone { get; set; }

	[Property]
	public NavigationType WalkingType { get; set; } = NavigationType.Dumb;

	[Property]
	public NavigationType RunningType { get; set; } = NavigationType.Smart;

	public NavigationType NavigationType => IsRunning ? RunningType : WalkingType;
	public Guid LastAttackerId {get ; set;}

	

	/// <summary>
	/// How much this creature weights (To handle ragdol force amount and duration)
	/// </summary>
	[Property]
	[Category( "Stats" )]
	public WeightType Weight { get; set; } = WeightType.Middle;

	/// <summary>
	/// Should the stats scale linearly with the scale of the object
	/// </summary>
	[Property]
	[Category( "Stats" )]
	public bool ScaleStats { get; set; } = true;
	public float Scale => ScaleStats ? MathF.Max( MathF.Max( GameObject.Transform.Scale.x, GameObject.Transform.Scale.y ), GameObject.Transform.Scale.z ) : 1f;

	/// <summary>
	/// Doesn't move (Don't add a MoveHelper if this is on)
	/// </summary>
	[Property]
	[Category( "Stats" )]
	public bool Static { get; set; } = false;

	/// <summary>
	/// For animations. How many units per second the run animation is tuned to (This is automatically scaled by the scale)
	/// </summary>
	[Property]
	[Category( "Stats" )]
	[HideIf( "Static", true )]
	public float MaxRunAnimationSpeed { get; set; } = 150f;

	/// <summary>
	/// How fast this NPC can walk
	/// </summary>
	[Property]
	[Category( "Stats" )]
	[HideIf( "Static", true )]
	[Range( 0f, 600f, 10f, false )]
	public float WalkSpeed { get; set; } = 90f;

	/// <summary>
	/// How fast this NPC can run
	/// </summary>
	[Property]
	[Category( "Stats" )]
	[HideIf( "Static", true )]
	[Range( 0f, 600f, 10f, false )]
	public float RunSpeed { get; set; } = 180f;

	/// <summary>
	/// Should we automatically make the NPC look towards its target
	/// </summary>
	[Property]
	[Category( "Stats" )]
	[HideIf( "Static", true )]
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
	public float DetectRange { get; set; } = 256f;

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
	
	[Property] private float PlayerProximityDistance { get; set; } = 400f;
	public Guid KillerId { get; set; } // Fügen Sie diese Eigenschaft hinzu
	public float ForceMultiplier
	{
		get
		{
			return Weight switch
			{
				WeightType.Feather => 2f,
				WeightType.Light => 1.5f,
				WeightType.Middle => 1f,
				WeightType.Heavy => 0.75f,
				WeightType.Massive => 0.5f,
				_ => 1f
			};
		}
	}
	
	

	protected override void OnStart()
	{
		Tags.Set( "npc", true );


		NpcId = Scene.GetAllComponents<Npc>().OrderByDescending( x => x.NpcId ).First().NpcId + 1;

		if ( MoveHelper != null )
			MoveHelper.AirFriction = 100f;

		Collider = Components.Get<Collider>();
	}

	protected override void OnAwake()
	{
		var spawnTrace = Scene.Trace.Ray( Transform.Position + Vector3.Up * 30f, Transform.Position - Vector3.Up * 200f )
			.Size( 5f )
			.IgnoreGameObjectHierarchy( GameObject )
			.WithoutTags( "player", "npc", "trigger" )
			.Run();

		player = Scene.GetAllComponents<Player>().FirstOrDefault();
		agent = Components.Get<NavMeshAgent>();

		SpawnPosition = spawnTrace.Hit ? spawnTrace.HitPosition : Transform.Position;

		if ( MoveHelper != null )
		{
			MoveHelper.StepHeight *= Scale;
			MoveHelper.TraceRadius *= Scale;
			MoveHelper.TraceHeight *= Scale;
			MoveHelper.StopSpeed *= Scale;
		}

		BroadcastOnSpawn();
	}


	[Broadcast]
	private void BroadcastOnSpawn()
	{
		OnSpawn?.Invoke();
	}
	private bool IsPlayerNearby()
        {
            if (Network.IsProxy)
        return false;

		var players = Scene.GetAllComponents<Player>();
		foreach (var player in players)
		{
			// Überprüfe, ob der Spieler in der Nähe ist
			if ((player.Transform.Position - this.Transform.Position).Length < PlayerProximityDistance)
            return true;
		}
            return false; 
        }

	protected override void OnUpdate()
	{
		// Überprüfe auf Vorbedingungen, um eine ungültige Ausführung zu vermeiden
		if ( Model == null || Static || (Healthone != null && !Healthone.Alive) )
			return;

		bool isPlayerNearby = IsPlayerNearby();	

		if ( Model != null)
		{
			
			if ( Spawner != null && Spawner.IsSpawning )
			{
				Model.Set("slime_spawn", true);
			}
			else if (!isPlayerNearby)
			{
				Model.Set("slime_idle", true);
			}
			else if (IsAttacking)
			{
				Model.Set("slime_attack", true);
			}
			else if (IsDamaged)
			{
				Model.Set("slime_damage", true);
			}
			else if (IsRunning)
			{
				Model.Set("slime_run", true);
			}
		}
			
		

		// Suchen Sie nach allen Spielern in der Szene
		var players = Scene.GetAllComponents<Player>();

		// Finden Sie den Spieler, der dem NPC am nächsten ist
		Player closestPlayer = null;
		var closestDistanceSquared = float.MaxValue;

		foreach ( var player in players )
		{
			Vector3 direction = player.Transform.Position - Transform.Position;
			var distanceSquared = direction.LengthSquared;

			if ( distanceSquared < closestDistanceSquared )
			{
				closestDistanceSquared = distanceSquared;
				closestPlayer = player;
			}
		}

		if ( closestPlayer != null )
		{
			var closestDistance = MathF.Sqrt( closestDistanceSquared );
			SetTarget( closestPlayer.GameObject );

			// Richte den NPC auf die Bewegungsrichtung aus, falls erforderlich
			if ( Ragdoll == null && FaceTowardsVelocity )
			{
				if ( !MoveHelper.Velocity.IsNearlyZero( 1f ) )
				{
					Transform.Rotation = Rotation.Lerp( Transform.Rotation, Rotation.LookAt( MoveHelper.Velocity.WithZ( 0f ), Vector3.Up ), Time.Delta * (IsRunning ? 10f : 5f) );
				}
			}

			UpdateAnimations( closestPlayer );

			// Überprüfe die Entfernung zum nächsten Spieler und passe die Bewegungsart entsprechend an
			if ( closestDistance < 80f )
			{
				AnimationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Walk;
				agent.Stop();
				NormalTrace();
			}
			else
			{
				AnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.Swing;
				agent.MoveTo( closestPlayer.Transform.Position );
				if(!isPlayerNearby)
				{
					AnimationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Run;
				}
				
			}
		}
		else
		{
			if ( TargetObject != null )
			{
				// Überprüfen, ob das Ziel immer noch gültig ist, oder es außerhalb der Reichweite ist
				if ( !IsWithinRange( TargetObject ) )
				{
					// Ziel außerhalb der Reichweite, verfolge weiterhin das letzte Ziel
					agent.MoveTo( TargetObject.Transform.Position );
				}
				else
				{
					DetectAround();

				}
			}
			


		}
		UpdateFootAnimations();
	}

	 void UpdateAnimations( Player player )
	{
		AnimationHelper.WithWishVelocity( agent.WishVelocity );
		AnimationHelper.WithVelocity( MoveHelper.Velocity );

		var playerPosition = player.GameObject.Transform.Position.WithZ( Transform.Position.z );
		var targetRotation = Rotation.LookAt( playerPosition - Body.Transform.Position );
		Body.Transform.Rotation = Rotation.Slerp( Body.Transform.Rotation, targetRotation, Time.Delta * 5.0f );

		// Setzen Sie die Bewegungsart nur, wenn sich die Geschwindigkeit ändert
		var moveStyle = IsRunning ? CitizenAnimationHelper.MoveStyles.Run : CitizenAnimationHelper.MoveStyles.Walk;
		if ( AnimationHelper.MoveStyle != moveStyle )
		{
			AnimationHelper.MoveStyle = moveStyle;
			
		}
	}

	void UpdateFootAnimations()
	{
		// Holen Sie die Geschwindigkeit des NPCs
		var scaledSpeed = MaxRunAnimationSpeed * Scale;
		var forwardVelocity = Vector3.Dot( MoveHelper.Velocity, Model.Transform.Rotation.Forward ) / scaledSpeed;
		var rightVelocity = Vector3.Dot( MoveHelper.Velocity, Model.Transform.Rotation.Right ) / scaledSpeed;

		// Lerp nur, wenn sich die Geschwindigkeit ändert
		var oldX = Model.GetFloat( "move_x" );
		var oldY = Model.GetFloat( "move_y" );
		var newX = MathX.Lerp( oldX, forwardVelocity, Time.Delta * 5f );
		var newY = MathX.Lerp( oldY, rightVelocity, Time.Delta * 5f );

		// Batchen Sie die Set-Operationen
		Model.Set( "move_x", newX );
		Model.Set( "move_y", newY );
	}
	public void NormalTrace()
    {
        var tr = Scene.Trace.Ray( Body.Transform.Position, Body.Transform.Position + Body.Transform.Rotation.Forward * 100 ).Run();

        if ( tr.Hit && timeSinceHit > 1.5f && GameObject != null )
        {
            IHealthComponent damageable =  tr.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();

            if(tr.GameObject.Tags.Has("player") || tr.GameObject.Tags.Has("npc"))
            {
                // Fügen Sie die GameObject.Id des angreifenden Spielers hinzu
                damageable.TakeDamage( DamageType.Bullet, 10, tr.EndPosition, tr.Direction * 5, GameObject.Id, GameObject.Id );

                AnimationHelper.Target.Set( "b_attack", true );
                timeSinceHit = 0;

                Sound.Play( HitSounds, Transform.Position );
            }
        }
    }


	protected override void OnFixedUpdate()
	{
		if ( Healthone != null && Healthone.Alive ) // If we are still alive
		{
			if ( Ragdoll == null ) // If we are not ragdolled
			{
				if (!IsPlayerNearby())
				
			
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
					if ( !Static )
					{
						ComputeNavigation();
						MoveHelper.Move();
					}
				}

				
			}
		}
		else
		{
			MoveHelper.WishVelocity = 0;

		}
		
		
		
	}

	
	private void StopAnimations()
	{
		
    	
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
		if ( TargetObject == null ) // Check if there is a target object
		{
			var currentTick = (int)(Time.Now / Time.Delta);
			if ( currentTick % 20 != NpcId % 20 ) return; // Check every 20 ticks

			var foundAround = Scene.FindInPhysics( new Sphere( Transform.Position, DetectRange * Scale ) ) // Find gameobjects nearby
				.Where( x => x.Enabled )
				.Where( x => EnemyTags != null && x.Tags.HasAny( EnemyTags ) ) // Do they have any of our enemy tags
				.Where( x => x.Components.Get<HealthComponent>()?.Alive ?? true ); // Are they dead or undead

			if ( foundAround.Any() )
				Detected( foundAround.First(), true ); // If we don't have any target yet, pick the first one around us
		}
		else // There is a target object
		{
			if ( IsWithinRange( TargetObject ) ) // Is the target within reach
			{
				if ( NextAttack ) // Is it time to attack
				{
					BroadcastOnAttack();
					NextAttack = AttackCooldown;
				}
			}
			else // Target is out of range
			{
				Undetected();
			}
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
				.Where( x => x.Transform.Position.Distance( Transform.Position ) <= x.VisionRange * x.Scale )
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
		TargetPosition = Transform.Position;
		ReachedDestination = true;
	}

	[Broadcast]
	private void BroadcastOnEscape()
	{
		if ( TargetObject is not null )
			OnEnemyEscaped?.Invoke( TargetObject );
	}

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
			ReachedDestination = true;
			TargetPosition = Transform.Position;
		}
		else
		{
			TargetObject = target;
			FollowingTargetObject = !escapeFrom;
			MoveTo( GetPreferredTargetPosition( TargetObject ) );
			ReachedDestination = false;
		}
	}

	/// <summary>
	/// Is the object within the npc's reach (AttackRange) 
	/// </summary>
	/// <param name="target"></param>
	/// <returns></returns>
	public bool IsWithinRange( GameObject target )
	{
		if ( !GameObject.IsValid() ) return false;

		return IsWithinRange( target, AttackRange * Scale );
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
		
		return target.Transform.Position.Distance( Transform.Position ) <= range;
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

		var targetPosition = target.Transform.Position;

		var direction = (Transform.Position - targetPosition).Normal;
		var offset = FollowingTargetObject ? direction * AttackRange * Scale / 2f : direction * VisionRange * Scale;
		var wishPos = targetPosition + offset;

		var groundTrace = Scene.Trace.Ray( wishPos + Vector3.Up * 64f, wishPos + Vector3.Down * 64f )
			.Size( 5f )
			.IgnoreGameObjectHierarchy( GameObject )
			.WithoutTags( "player", "npc", "trigger" )
			.Run();
		
		return groundTrace.Hit && !groundTrace.StartedSolid ? groundTrace.HitPosition : (FollowingTargetObject ? targetPosition : targetPosition + offset);
	}
	public static Player Host { get; set; }
	
	
	[Broadcast]
	public void TakeDamage(DamageType type, float amount, Vector3 hitPosition, Vector3 hitDirection, Guid attackerId, Guid playerId)
	{
		if (LifeState == LifeState.Dead)
			return;
		

		if (type == DamageType.Bullet || type == DamageType.Serious)
		{
			var p = new SceneParticles(Scene.SceneWorld, "particles/impact.flesh.bloodpuff.vpcf");
			p.SetControlPoint(0, hitPosition);
			p.SetControlPoint(0, Rotation.LookAt(hitDirection.Normal * -1f));
			p.SetControlPoint(1, new Vector3(0.5f, 0.1f, 0.1f));
			p.PlayUntilFinished(Task);
		}

		if (Network.IsProxy)
			return;

		Health = Math.Clamp(Health - amount, 0f, MaxHealth);
		

		if (Health <= 0f) // checks if zombie is dead
		{	
			
			LifeState = LifeState.Dead;
			var zombie = ZombieRagedol.Clone(this.GameObject.Transform.Position, this.GameObject.Transform.Rotation);
			zombie.NetworkSpawn();

			Log.Info($"Killer attacker + {attackerId}");
			KillerId = attackerId;
			Log.Info($"Zombie killed by: {KillerId}"); // yes
			
			GameObject.Destroy();

			var killer = Scene.Directory.FindByGuid(attackerId);

			if (killer == null)
			{
				Log.Info($"Killer with the id {KillerId} not found");  // yes
				return;
			}

			Log.Info("Killer found" + killer);

			var killerPlayer = killer.Components.Get<Player>(FindMode.EverythingInSelfAndAncestors);

			Log.Info($"Killer with the id {KillerId} is found"); // no
			int vyndaliumPointsToAdd = new Random().Next(1, 500);
			int xpPointsToAdd = new Random().Next(75, 125);

			// Geben Sie dem Killer Vyndalium und XP
			killerPlayer.GiveVyndalium(vyndaliumPointsToAdd);
			killerPlayer.GiveXp(xpPointsToAdd);
						

				};

	}
	
	
			
				
		
	public event Action<int> VyndaliumAdded; // Declare the event "VyndaliumAdded"

	public bool GiveVyndalium( int amount )
	{
		Log.Info($"Vyndalium-Punkte hinzugefügt: {amount}");
		VyndaliumPoints += amount;
		VyndaliumPointsChanged?.Invoke(VyndaliumPoints);
		VyndaliumAdded?.Invoke(amount); // Benachrichtige alle Abonnenten über die Änderung der Vyndalium-Punkte
		return true;
	}
	public bool GiveXp( int amount )
	{
		Experience += amount;
		ExperienceChanged?.Invoke( Experience );
		return true;
	}
	
		
}

