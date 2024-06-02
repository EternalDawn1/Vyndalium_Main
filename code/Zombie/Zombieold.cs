using System;
using System.Linq;
using Sandbox;
using Sandbox.Citizen;

namespace GeneralGame;

public sealed class Zombie : Component, IHealthComponent
{
	[Property] public GameObject Body { get; set; }
	[Property] public GameObject Eye { get; set; }
	[Property] public CitizenAnimationHelper AnimationHelper { get; set; }
	[Property] public SoundEvent HitSounds { get; set; }
	[Sync, Property] public float MaxHealth { get; private set; } = 100f;
	[Sync] public LifeState LifeState { get; private set; } = LifeState.Alive;
	[Sync] public float Health { get; private set; } = 100f;
	[Property] public GameObject ZombieRagedol { get; set; }
	private NavMeshAgent agent;
	private Player player;
	private TimeSince timeSinceHit = 0;

	protected override void OnAwake()
	{
		agent = Components.Get<NavMeshAgent>();
		player = Scene.GetAllComponents<Player>().FirstOrDefault();
		
	}
	protected override void OnUpdate()
	{
		AnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.Swing;
		AnimationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Run;
		var target = player.Transform.Position;
		player = Scene.GetAllComponents<Player>().FirstOrDefault();
		
		UpdateAnimtions();
		if (Vector3.DistanceBetween(target, GameObject.Transform.Position ) < 80f)
		{
			agent.Stop();
			NormalTrace();
		}
		else
		{
			agent.MoveTo(player.Transform.Position);
		}
	}
	

	void UpdateAnimtions()
	{
		AnimationHelper.WithWishVelocity(agent.WishVelocity);
		AnimationHelper.WithVelocity(agent.Velocity);
		var targetRot = Rotation.LookAt(player.GameObject.Transform.Position.WithZ(Transform.Position.z) - Body.Transform.Position);
		Body.Transform.Rotation = Rotation.Slerp(Body.Transform.Rotation, targetRot, Time.Delta * 5.0f);
	}
	void NormalTrace()
	{
		var tr = Scene.Trace.Ray(Body.Transform.Position, Body.Transform.Position + Body.Transform.Rotation.Forward * 100).Run();

		if (tr.Hit && tr.GameObject.Tags.Has("player") && timeSinceHit > 1.0f && GameObject is not null)
		{
			IHealthComponent damageable;
			damageable = tr.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();

			damageable.TakeDamage( DamageType.Bullet, 10, tr.EndPosition, tr.Direction * 5, GameObject.Id, GameObject.Id );
			
			AnimationHelper.Target.Set("b_attack", true);
			timeSinceHit = 0;

			Sound.Play( HitSounds, Transform.Position );
		}

	}

	[Broadcast]
	public void TakeDamage( DamageType type, float damage, Vector3 position, Vector3 force, Guid attackerId )
	{
		if ( LifeState == LifeState.Dead )
			return;

		if ( type == DamageType.Bullet )
		{
			var p = new SceneParticles( Scene.SceneWorld, "particles/impact.flesh.bloodpuff.vpcf" );
			p.SetControlPoint( 0, position );
			p.SetControlPoint( 0, Rotation.LookAt( force.Normal * -1f ) );
			p.PlayUntilFinished( Task );
		}

		if ( IsProxy )
			return;


		Health = MathF.Max( Health - damage, 0f );

		if ( Health <= 0f )
		{
			LifeState = LifeState.Dead;
			var zombie = ZombieRagedol.Clone( this.GameObject.Transform.Position, this.GameObject.Transform.Rotation );
			zombie.NetworkSpawn();
			GameObject.Destroy();
		}

	}

	public void TakeDamage( DamageType type, float amount, Vector3 hitPosition, Vector3 hitDirection, Guid attackerId, Guid playerId )
	{
		throw new NotImplementedException();
	}
}
