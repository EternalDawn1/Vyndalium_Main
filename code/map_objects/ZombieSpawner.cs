using System;
using System.Linq;
using Sandbox;
namespace GeneralGame;
public sealed class ZombieSpawner : Component
{
	[Property] public GameObject ZombiePrefab { get; set; }
	[Property] public bool IsSpawning { get; set; }
	[Property] public float RespawnTime { get; set; } = 50f;
	private TimeUntil? TimeUntilRespawn { get; set; }
	[Property] public float PlayerProximityDistance { get; set; } = 1000f;
	[Property] public int MaxSpawns { get; set; } = 10; // Neue Eigenschaft für maximale Anzahl von Spawns
    private int SpawnCount { get; set; } // Zähler für die Anzahl der Spawns

	protected override void DrawGizmos()
	{
		const float boxSize = 4f;
		var bounds = new BBox( Vector3.One * -boxSize, Vector3.One * boxSize );

		Gizmo.Hitbox.BBox( bounds );

		Gizmo.Draw.Color = Color.Cyan.WithAlpha( (Gizmo.IsHovered || Gizmo.IsSelected) ? 0.5f : 0.2f );
		Gizmo.Draw.LineBBox( bounds );
		Gizmo.Draw.SolidBox( bounds );

		Gizmo.Draw.Color = Color.Cyan.WithAlpha( (Gizmo.IsHovered || Gizmo.IsSelected) ? 0.8f : 0.6f );
	}

	protected override void OnStart()
	{
		TimeUntilRespawn = 5f;
		base.OnStart();
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( !Networking.IsHost )
			return;
			
		if (!IsPlayerNearby()) // Überprüfen, ob ein Spieler in der Nähe ist
                return;

		if (SpawnCount >= MaxSpawns) // Überprüfen, ob die maximale Anzahl von Spawns erreicht wurde
            {
                // Zerstöre das Spawner-Objekt, wenn die maximale Anzahl von Spawns erreicht wurde
                GameObject.Destroy();
                return;
            }

		if ( !TimeUntilRespawn.HasValue )
		{
			TimeUntilRespawn = RespawnTime;
			return;
		}

		if ( !TimeUntilRespawn.Value )
			return;

		var zombie = ZombiePrefab.Clone( this.Transform.World );
		zombie.NetworkSpawn();
		
		IsSpawning = true;

		CreateSpawnParticle(zombie.Transform.Position);

		TimeUntilRespawn = null;

		SpawnCount++;
	}
	private void CreateSpawnParticle(Vector3 position)
        {
            // Erstelle und spiele ein Partikelsystem beim Spawnen
            var p = new SceneParticles( Scene.SceneWorld, "particles/impact.flesh.bloodpuff.vpcf" );
			p.SetControlPoint( 0, position );
			p.SetControlPoint( 0,  -1f ) ;
			p.SetControlPoint(1, new Vector3(5.5f, 0.1f, 0.1f));
			p.PlayUntilFinished( Task );
        }
	private bool IsPlayerNearby()
        {
            if (Network.IsProxy && !ZombiePrefab.IsValid())
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

}
