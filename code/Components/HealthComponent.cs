using Sandbox;
using GeneralGame;
using static Npc;
using Sandbox.Citizen;
using System;
using System.Linq;
using System.Collections.Generic;



public enum DamageType
{

	Mild = 1,

	Average = 2,

	Serious = 3,
	
	Nothing = 4,

	Bullet = 5,

	ice = 6,
	fire = 7,
	water = 8,
	 blunt = 8,	

	 bleed = 9,

	 air = 10,

}


[Icon( "medication" )]
[Title( "Health" )]
public sealed class HealthComponent : Component
{
	/// <summary>
	/// What type to damage is able to ragdoll this
	/// </summary>
	[Property]
	public DamageType StunnedBy { get; set; } = DamageType.Nothing;

	/// <summary>
	/// Can this get damaged at all
	/// </summary>
	[Property]
	public bool Immortal { get; set; } = false;

	/// <summary>
	/// What type of damage is able to hurt this
	/// </summary>
	[Property]
	[HideIf( "Immortal", true )]
	public DamageType DamagedBy { get; set; } = DamageType.Mild;

	/// <summary>
	/// Should this get ragdolled as well when the damage is greater or equal than both DamagedBy and StunnedBy
	/// </summary>
	[Property]
	[HideIf( "Immortal", true )]
	public bool StunWhenDamaged { get; set; } = false;

	/// <summary>
	/// How many hit points this has, usually 1 hit point comes from DamageType.Mild
	/// </summary>
	[Property]
	[HideIf( "Immortal", true )]
	[Range( 1f, 100f, 1f, false )]
	public int MaxHealth { get; set; } = 100;

	[Sync,Property]
	public int Health { get; set; } 

	[Sync]
	public bool Alive { get; set; } = true;

	/// <summary>
	/// Can this thing regenerate health over time
	/// </summary>
	[Property]
	[HideIf( "Immortal", true )]
	public bool CanRegenerate { get; set; } = true;

	/// <summary>
	/// How much time passed since the last time its been damaged before it starts regenerating health
	/// </summary>
	[Property]
	[ShowIf( "CanRegenerate", true )]
	[HideIf( "Immortal", true )]
	[Range( 0f, 10f, 0.1f )]
	public float RegenerationTimer { get; set; } = 5f;

	/// <summary>
	/// How many seconds it takes to regenerate 1 hit point
	/// </summary>
	[Property]
	[ShowIf( "CanRegenerate", true )]
	[HideIf( "Immortal", true )]
	[Range( 0f, 5f, 0.1f )]
	public float RegenerationCooldown { get; set; } = 2f;

	/// <summary>
	/// What gets spawned (Preferably an item) when this dies (Not when it gets destroyed)
	/// </summary>
	[Property]
	public List<GameObject> DropOnDeath { get; set; }

	public delegate void AttackerInfo( int damage, DamageType type, GameObject attacker = null, Vector3 localHurtPosition = default, Vector3 forceDirection = default, float force = 0 );

	[Property]
	public AttackerInfo OnDamaged { get; set; }

	public TimeSince LastDamaged { get; set; }
	TimeUntil _nextHeal { get; set; }
	[Property] public CitizenAnimationHelper AnimationHelper { get; set; }




	protected override void OnStart()
	{
		LastDamaged = 0;
		_nextHeal = 0;

		Health = MaxHealth;

		

	}

	/// <summary>
	/// How many hitpoints to remove (Negative will heal instead and not call the OnAttacked event)
	/// </summary>
	/// <param name="amount">The amount of damage dealth</param>
	/// <param name="type">The type of damage dealth</param>
	/// <param name="attacker">The person that attacked, null if not set</param>
	/// <param name="worldHurtPosition">The world position of where the damage happened, (0,0,0) if not set</param>
	/// <param name="forceDirection">The direction which the force will be applied, (0,0,0) if not set</param>
	/// <param name="force">How much force was behind that damage, 0 by default</param>
	public void Damage(int amount, DamageType type, GameObject attacker = null, Vector3 worldHurtPosition = default, Vector3 forceDirection = default, float force = 0)
    {
        if (!Alive) return;
        if (amount == 0) return;

        var stunned = type >= StunnedBy && type != DamageType.Nothing; // Don't ragdoll if we're healing
        var damaged = type >= DamagedBy;


        if (!Immortal)
        {
            if (damaged)
            {
                Health = Math.Clamp(Health - amount, 0, MaxHealth);

                if (amount > 0)
                {
                    LastDamaged = 0; // We were just attacked
                    _nextHeal = RegenerationTimer + RegenerationCooldown; // Reset the health timer

                    if (Components.TryGet<Npc>(out var npc))
                        npc.Damaged(attacker);
                }
            }
        }

        if (stunned)
        {
            if (damaged && !StunWhenDamaged && !Immortal) return;

            var damageFrac = Math.Min((float)amount / (float)MaxHealth, 1f);
            // Add code here if necessary for stunned state
        }

        if (Health <= 0)
        {
            Kill(attacker?.Id ?? Guid.Empty);
            return;
        }
    }

    private void InternalKill(GameObject attacker = null)
    {
        Alive = false;

        if (attacker != null)
            if (attacker.Components.TryGet<Player>(out var killer))
                killer.AddExperience(MaxHealth * 10);

        if (Components.TryGet<Npc>(out var npc))
        {
            npc.OnKilled?.Invoke(attacker);
        }

        if (DropOnDeath != null)
        {
            foreach (var item in DropOnDeath)
            {
                var droppedItem = item.Clone(WorldPosition + Vector3.Up * 30f, WorldRotation);
                droppedItem?.SetupNetworking();
            }
        }
    }

    [Broadcast]
    public void Kill(Guid attackerId)
    {
        var attackerObj = Game.ActiveScene.GetAllObjects(true)
            .Where(x => x.Id == attackerId)
            .FirstOrDefault();

        InternalKill(attackerObj);
    }

    // Function to allow other objects to inflict damage on this NPC
    public void InflictDamage(int damage, DamageType type, GameObject attacker = null, Vector3 position = default, Vector3 force = default, Guid? attackerId = null)
{
    Guid attackerGuid = attackerId ?? Guid.Empty; // Use the provided attackerId if not null, otherwise use Guid.Empty
    Damage(damage, type, attacker, position, force, (float)attackerGuid.GetHashCode()); // Convert Guid to float and pass it to the Damage method
}
}