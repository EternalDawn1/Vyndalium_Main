using System.Net.Mail;
using GeneralGame;
using Sandbox;

public sealed class Fists : Component
{
	[Property] public SkinnedModelRenderer fists { get; set; }
	[Property] public GameObject ViewModelCamera { get; set; }
	public Player playerController { get; set; }
	[Property] public int Damage { get; set; } = 10;
    [Property] public float Spread { get; set; } = 0.01f;
    [Property, Category( "Parameters" )] public float HitForce { get; set; } = 300;
    [Property] public float FireRate { get; set; } = 3f;
    [Property, Category( "Parameters" )] public DamageType DamageType { get; set; } = DamageType.Serious;
    public TimeUntil NextAttackTime { get; set; }
    [Property] public ParticleSystem ImpactEffect { get; set; }
    [Property] public float DamageForce { get; set; } = 5f;

	protected override void OnStart()
	{
		fists.Set("b_attack", false);
		fists.Set("b_deploy", true);
		playerController = Scene.GetAllComponents<Player>().FirstOrDefault( x => !x.IsProxy );
	}

	protected override void OnUpdate()
	{
		if (!IsProxy)
		{
			if (Input.Pressed("attack1"))
			{
				Attack();
			}
			UpdateAnimations();
		}
		if (IsProxy)
		{
			ViewModelCamera.Enabled = false;
		}
	}
	protected override void OnEnabled()
	{
		if (!IsProxy)
		{
			fists.Set("b_deploy", true);
		}
	}
	protected override void OnDisabled()
	{
		if (!IsProxy)
		{
			fists.Set("b_attack", false);
		}
	}
	void Attack()
{
    fists.Set("b_attack", true);

    var startPos = playerController.PlyCamera.Transform.Position;
    var direction = playerController.PlyCamera.Transform.Rotation.Forward;
    direction += Vector3.Random * Spread;

    var endPos = startPos + direction * 1000f;
    var trace = Scene.Trace.Ray(startPos, endPos)
        .IgnoreGameObjectHierarchy(GameObject.Root)

        .UseHitboxes()
        .Run();

    var damage = Damage;
 

    IHealthComponent damageable = null;

    if (trace.Component.IsValid())
        damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();

    if (damageable is not null)
    {
        damageable.TakeDamage(DamageType.Bullet, damage, trace.EndPosition, trace.Direction * DamageForce , GameObject.Id, GameObject.Id);
    }
    else if ( trace.Hit )
		{
			SendImpactMessage( trace.EndPosition, trace.Normal );
		}
    

    var target = trace.GameObject;
    if (target != null)
    {
        if (target.Components.TryGet<Rigidbody>(out var body))
            body.ApplyImpulseAt(trace.HitPosition, trace.Direction * HitForce);

        if (target.Components.TryGet<HealthComponent>(out var health))
            health.Damage(Damage, DamageType, playerController.GameObject, trace.HitPosition, trace.Direction, HitForce);
    }

    NextAttackTime = 1f / FireRate;
    

    
}
[Broadcast]
	private void SendImpactMessage( Vector3 position, Vector3 normal )
	{
		if ( ImpactEffect is null ) return;

		var p = new SceneParticles( Scene.SceneWorld, ImpactEffect );
		p.SetControlPoint( 0, position );
		p.SetControlPoint( 0, Rotation.LookAt( normal ) );
		p.PlayUntilFinished( Task );
	}

	void UpdateAnimations()
{
    if (fists == null || playerController?.CharacterController == null)
    {
        return;
    }

    if (Input.Pressed("jump") && !IsProxy)
    {
        fists.Set("b_jump", true);
    }

    if (!playerController.CharacterController.IsOnGround && !IsProxy)
    {
        fists.Set("b_grounded", false);
    }
    else
    {
        fists.Set("b_grounded", true);
    }
    fists.Set("move_groundspeed", playerController.CharacterController.Velocity.Length);
}

	
}
