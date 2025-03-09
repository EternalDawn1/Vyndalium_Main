using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GeneralGame;

public sealed class ActiveArea : Component
{
	public enum AttackPattern
	{
		Box,
		Tornado,
		Vortex
	}

	[Property, Group( "Attack Pattern Settings" )]
	private float SpawnInterval { get; set; } = 1.0f;

	[Property, Group( "Attack Pattern Settings" )]
	private float ObjectLifetime { get; set; } = 5.0f;

	[Property, Group( "Attack Pattern Settings" )]
	private float ObjectSpeed { get; set; } = 200.0f;

	[Property, Group( "Attack Pattern Settings" )]
	private int ObjectsPerSide { get; set; } = 5;

	[Property, Group( "Attack Pattern Settings" )]
	private Vector3 BoxSize { get; set; }

	[Property, Group( "Attack Pattern Settings" )]
	private PrefabFile AttackPrefab { get; set; }

	[Property, Group( "Attack Pattern Settings" )]
	private AttackPattern SelectedAttackPattern { get; set; } = AttackPattern.Box;

	private List<GameObject> activeAttackObjects = new();
	private float attackCooldown = 10.0f; // Cooldown in Sekunden
	private float timeSinceLastAttack = 0.0f;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		timeSinceLastAttack += Time.Delta;

		if ( timeSinceLastAttack >= attackCooldown )
		{
			switch ( SelectedAttackPattern )
			{
				case AttackPattern.Box:
					ExecuteBoxAttackPattern();
					break;
				case AttackPattern.Tornado:
					ExecuteTornadoAttackPattern();
					break;
				case AttackPattern.Vortex:
					ExecuteVortexAttackPattern();
					break;
			}
			timeSinceLastAttack = 0.0f;
		}
	}

	private async void ExecuteBoxAttackPattern()
	{
		if ( AttackPrefab == null )
		{
			return;
		}

		var prefab = ResourceLibrary.Get<PrefabFile>( AttackPrefab.ResourcePath );
		if ( prefab == null )
		{
			return;
		}

		for ( int i = 0; i < ObjectsPerSide; i++ )
		{
			SpawnObjectAtBoxSide( prefab, BoxSize.y, Vector3.Up );

			SpawnObjectAtBoxSide( prefab, BoxSize.z, Vector3.Forward );
			SpawnObjectAtBoxSide( prefab, BoxSize.z, Vector3.Backward );
			SpawnObjectAtBoxSide( prefab, BoxSize.x, Vector3.Left );
			SpawnObjectAtBoxSide( prefab, BoxSize.x, Vector3.Right );

			await Task.Delay( (int)(SpawnInterval * 1000) );
		}
	}

	private void SpawnObjectAtBoxSide( PrefabFile prefab, float boxSideLength, Vector3 direction )
	{
		var spawnPosition = LocalPosition; // Setze die Spawn-Position auf den Mittelpunkt der Box

		var attackObject = GameObject.Clone( prefab );
		attackObject.WorldPosition = spawnPosition;
		attackObject.WorldRotation = Rotation.Identity;
		attackObject.NetworkSpawn();

		activeAttackObjects.Add( attackObject );
		_ = MoveAttackObject( attackObject, direction );
	}

	private async Task MoveAttackObject( GameObject attackObject, Vector3 direction )
	{
		float elapsedTime = 0.0f;
		Vector3 boxMin = LocalPosition - BoxSize / 2;
		Vector3 boxMax = LocalPosition + BoxSize / 2;

		while ( elapsedTime < ObjectLifetime )
		{
			await Task.Delay( 10 ); // Update alle 10ms
			attackObject.WorldPosition += direction * ObjectSpeed * 0.01f; // Bewege das Objekt
			elapsedTime += 0.01f;

			// Überprüfe, ob das Objekt den Rand der Box erreicht hat
			if ( attackObject.WorldPosition.x < boxMin.x || attackObject.WorldPosition.x > boxMax.x ||
				attackObject.WorldPosition.y < boxMin.y || attackObject.WorldPosition.y > boxMax.y ||
				attackObject.WorldPosition.z < boxMin.z || attackObject.WorldPosition.z > boxMax.z )
			{
				break;
			}
		}

		attackObject.Destroy();
		activeAttackObjects.Remove( attackObject );
	}

	private async void ExecuteTornadoAttackPattern()
	{
		if ( AttackPrefab == null )
		{
			return;
		}

		var prefab = ResourceLibrary.Get<PrefabFile>( AttackPrefab.ResourcePath );
		if ( prefab == null )
		{
			return;
		}

		for ( int i = 0; i < ObjectsPerSide; i++ )
		{
			SpawnObjectInTornadoPattern( prefab, BoxSize.y, Vector3.Up );
			await Task.Delay( (int)(SpawnInterval * 1000) );
		}
	}

	private void SpawnObjectInTornadoPattern( PrefabFile prefab, float boxSideLength, Vector3 direction )
	{
		var spawnPosition = LocalPosition; // Setze die Spawn-Position auf den Mittelpunkt der Box

		var attackObject = GameObject.Clone( prefab );
		attackObject.WorldPosition = spawnPosition;
		attackObject.WorldRotation = Rotation.Identity;
		attackObject.NetworkSpawn();

		activeAttackObjects.Add( attackObject );
		_ = MoveAttackObjectInTornado( attackObject, direction );
	}

	private async Task MoveAttackObjectInTornado( GameObject attackObject, Vector3 direction )
	{
		float elapsedTime = 0.0f;
		Vector3 boxMin = LocalPosition - BoxSize / 2;
		Vector3 boxMax = LocalPosition + BoxSize / 2;
		float angle = 0.0f;

		while ( elapsedTime < ObjectLifetime )
		{
			await Task.Delay( 10 ); // Update alle 10ms
			angle += 0.1f; // Winkel erhöhen, um die Drehung zu simulieren
			float radius = 0.5f * BoxSize.Length; // Radius des Tornados
			Vector3 offset = new Vector3( MathF.Cos( angle ), MathF.Sin( angle ), 0 ) * radius;
			attackObject.WorldPosition = LocalPosition + offset + direction * ObjectSpeed * elapsedTime; // Bewege das Objekt in einem Bogen
			elapsedTime += 0.01f;

			// Überprüfe, ob das Objekt den Rand der Box erreicht hat
			if ( attackObject.WorldPosition.x < boxMin.x || attackObject.WorldPosition.x > boxMax.x ||
				attackObject.WorldPosition.y < boxMin.y || attackObject.WorldPosition.y > boxMax.y ||
				attackObject.WorldPosition.z < boxMin.z || attackObject.WorldPosition.z > boxMax.z )
			{
				break;
			}
		}

		attackObject.Destroy();
		activeAttackObjects.Remove( attackObject );
	}

	private async void ExecuteVortexAttackPattern()
	{
		if ( AttackPrefab == null )
		{
			return;
		}

		var prefab = ResourceLibrary.Get<PrefabFile>( AttackPrefab.ResourcePath );
		if ( prefab == null )
		{
			return;
		}

		for ( int i = 0; i < ObjectsPerSide; i++ )
		{
			SpawnObjectInVortexPattern( prefab, BoxSize.y, Vector3.Up );
			await Task.Delay( (int)(SpawnInterval * 1000) );
		}
	}

	private void SpawnObjectInVortexPattern( PrefabFile prefab, float boxSideLength, Vector3 direction )
	{
		var spawnPosition = LocalPosition; // Setze die Spawn-Position auf den Mittelpunkt der Box

		var attackObject = GameObject.Clone( prefab );
		attackObject.WorldPosition = spawnPosition;
		attackObject.WorldRotation = Rotation.Identity;
		attackObject.NetworkSpawn();

		activeAttackObjects.Add( attackObject );
		_ = MoveAttackObjectInVortex( attackObject, direction );
	}

	private async Task MoveAttackObjectInVortex( GameObject attackObject, Vector3 direction )
	{
		float elapsedTime = 0.0f;
		Vector3 boxMin = LocalPosition - BoxSize / 2;
		Vector3 boxMax = LocalPosition + BoxSize / 2;
		float angle = 0.0f;

		while ( elapsedTime < ObjectLifetime )
		{
			await Task.Delay( 10 ); // Update alle 10ms
			angle += 0.1f; // Winkel erhöhen, um die Drehung zu simulieren
			float radius = 0.5f * BoxSize.Length; // Radius des Vortex
			Vector3 offset = new Vector3( MathF.Cos( angle ), MathF.Sin( angle ), 0 ) * radius;
			attackObject.WorldPosition = LocalPosition + offset + direction * ObjectSpeed * elapsedTime; // Bewege das Objekt in einem Bogen
			elapsedTime += 0.01f;

			// Überprüfe, ob das Objekt den Rand der Box erreicht hat
			if ( attackObject.WorldPosition.x < boxMin.x || attackObject.WorldPosition.x > boxMax.x ||
				attackObject.WorldPosition.y < boxMin.y || attackObject.WorldPosition.y > boxMax.y ||
				attackObject.WorldPosition.z < boxMin.z || attackObject.WorldPosition.z > boxMax.z )
			{
				break;
			}
		}

		attackObject.Destroy();
		activeAttackObjects.Remove( attackObject );
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();
		var draw = Gizmo.Draw;
		Gizmo.Draw.Color = Color.Red; // Farbe des Gizmos

		// Zeichne die Box an der festen Position
		Vector3 center = LocalPosition;
		Gizmo.Draw.LineBBox( new BBox( center - BoxSize / 2, center + BoxSize / 2 ) );
	}
	// ...existing code...
}