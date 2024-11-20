#nullable enable

using Sandbox;
using Sandbox.Utility;
using GeneralGame;
using static Npc;
using static Sandbox.PhysicsContact;

public static partial class NpcNodes
{
    public static async Task<Task> Escape(Npc Npc, GameObject target, Func<Task>? succesfullyEscaped = null)
    {
        if (Npc == null || target == null) return Task.CompletedTask;

        Npc.SetTarget(target, true);

        while (Npc.IsValid() && target.IsValid() && Npc.IsWithinRange(target, Npc.VisionRange) && !Npc.FollowingTargetObject)
            await GameTask.DelaySeconds(Time.Delta);

        var success = Npc.IsValid() && target.IsValid() && !Npc.IsWithinRange(target, Npc.VisionRange) && !Npc.FollowingTargetObject;

        return success ? (succesfullyEscaped?.Invoke() ?? Task.CompletedTask) : Task.CompletedTask;
    }

    [ActionGraphNode("Npc.stopescaping")]
    [Title("Stop Escaping"), Group("Npc"), Icon("hail")]
    public static void StopEscaping(Npc Npc)
    {
        if (Npc == null) return;

        Npc.SetTarget(null);
    }

    [ActionGraphNode("Npc.stopmoving")]
    [Title("Stop Moving"), Group("Npc"), Icon("hail")]
    public static void StopMoving(Npc Npc)
    {
        if (Npc == null) return;

        Npc.TargetPosition = Npc.WorldPosition;
        Npc.ReachedDestination = true;
    }

    [ActionGraphNode("Npc.damage")]
    [Title("Damage"), Group("Npc"), Icon("whatshot")]
    public static void Damage(HealthComponent healthComponent, int amount, DamageType type = DamageType.Mild, GameObject? attacker = null, Vector3 worldHurtPosition = default, Vector3 forceDirection = default)
    {
        // Implement damage logic here
    }
}