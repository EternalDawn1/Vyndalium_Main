#nullable enable

using Sandbox;
using Sandbox.Utility;
using GeneralGame;
using static Npc;
using static Sandbox.PhysicsContact;

namespace GeneralGame;

public static partial class NpcNodes
{
	
	


	/// <summary>
	/// Ragdolls/Unragdolls the NPC, signal will fire once it unragdolls
	/// </summary>
	[ActionGraphNode( "npc.ragdoll" )]
	[Title( "Ragdoll NPC" ), Group( "NPC" ), Icon( "accessibility_new" )]
	public static async Task Ragdoll( Npc npc, bool enabled = true, float duration = 1f, float spin = 0f )
	{
		if ( npc == null ) return;

		npc.SetRagdoll( enabled, duration, spin );

		while ( npc.Ragdoll != null )
			await GameTask.DelaySeconds( Time.Delta );

		return;
	}

	public delegate Task Body();

	/// <summary>
	/// Move to a position
	/// </summary>
	[ActionGraphNode( "npc.moveto" )]
	[Title( "Move To" ), Group( "NPC" ), Icon( "turn_right" )]
	public static async Task<Task?> MoveTo( Npc npc, Vector3 position, Body? reachedDestination, Body? failedToReachDestination )
	{
		if ( npc == null ) return Task.CompletedTask;

		npc.MoveTo( position );
		var currentPosition = npc.TargetPosition;

		while ( npc.IsValid() && !npc.ReachedDestination && currentPosition == npc.TargetPosition )
			await GameTask.DelaySeconds( Time.Delta );

		var success = npc.IsValid() && npc.ReachedDestination && currentPosition == npc.TargetPosition;

		return success ? reachedDestination?.Invoke() : failedToReachDestination?.Invoke();
	}

	/// <summary>
	/// Start following a gameobject to attack it
	/// </summary>
	[ActionGraphNode( "npc.startfollowing" )]
	[Title( "Start Following" ), Group( "NPC" ), Icon( "follow_the_signs" )]
	public static async Task<Task?> StartFollowing( Npc npc, GameObject target, Body? failedToReachTarget )
	{
		if ( npc == null ) return Task.CompletedTask;

		npc.SetTarget( target );

		while ( npc.IsValid() && target.IsValid() && !npc.IsWithinRange( target ) && npc.FollowingTargetObject )
			await GameTask.DelaySeconds( Time.Delta );

		var success = npc.IsValid() && target.IsValid() && npc.IsWithinRange( target ) && npc.FollowingTargetObject;

		return success ? Task.CompletedTask : failedToReachTarget?.Invoke();
	}

	/// <summary>
	/// Stop following whatever target you had
	/// </summary>
	[ActionGraphNode( "npc.stopfollow" )]
	[Title( "Stop Following" ), Group( "NPC" ), Icon( "dangerous" )]
	public static void StopFollowing( Npc npc )
	{
		if ( npc == null ) return;

		npc.SetTarget( null );
	}

	/// <summary>
	/// Start escaping from whatever target you had
	/// </summary>
	[ActionGraphNode( "npc.startescaping" )]
	[Title( "Start Escaping" ), Group( "NPC" ), Icon( "directions_run" )]
	public static async Task<Task?> StartEscaping( Npc npc, GameObject target, Body? succesfullyEscaped )
	{
		if ( npc == null ) return Task.CompletedTask;

		npc.SetTarget( target, true );

		while ( npc.IsValid() && target.IsValid() && npc.IsWithinRange( target, npc.VisionRange ) && !npc.FollowingTargetObject )
			await GameTask.DelaySeconds( Time.Delta );

		var success = npc.IsValid() && target.IsValid() && !npc.IsWithinRange( target, npc.VisionRange ) && !npc.FollowingTargetObject;

		return success ? succesfullyEscaped?.Invoke() : Task.CompletedTask;
	}

	

	/// <summary>
	/// Stop moving
	/// </summary>
	[ActionGraphNode( "npc.stopmoving" )]
	[Title( "Stop Moving" ), Group( "NPC" ), Icon( "hail" )]
	public static void StopMoving( Npc npc )
	{
		if ( npc == null ) return;

		npc.TargetPosition = npc.Transform.Position;
		npc.ReachedDestination = true;
	}

	/// <summary>
	/// Deal damage to whatever, depending on damage type and force it will also ragdoll and punch
	/// </summary>
	[ActionGraphNode( "npc.damage" )]
	[Title( "Damage" ), Group( "NPC" ), Icon( "whatshot" )]
	public static void Damage( HealthComponent? healthComponent, int amount, DamageType type = DamageType.Mild, GameObject? attacker = null, Vector3 worldHurtPosition = default, Vector3 forceDirection = default, float force = 0 )
	{
		if ( healthComponent == null ) 
			return;

		healthComponent.Damage( amount, type, attacker, worldHurtPosition, forceDirection, force );
	}

	/// <summary>
	/// Get a random position around the position (Horizonal)
	/// </summary>
	/// <param name="position"></param>
	/// <param name="minRange"></param>
	/// <param name="maxRange"></param>
	[ActionGraphNode( "npc.getrandomposaround" ), Pure]
	[Title( "Get Random Position Around" ), Group( "NPC" ), Icon( "photo_size_select_small" )]
	public static Vector3 GetRandomPosAround( Vector3 position, float minRange = 50f, float maxRange = 300f )
	{
		return Npc.GetRandomPositionAround( position, minRange, maxRange );
	}
}
