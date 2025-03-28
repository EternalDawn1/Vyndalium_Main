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
		Random,
		FullCircle,
		
		Cross,
		Diagonal,


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
	private Vector3 BoxPosition { get; set; }

	[Property, Group( "Attack Pattern Settings" )]
	private PrefabFile AttackPrefab { get; set; }

	[Property, Group( "Attack Pattern Settings" )]
	private AttackPattern SelectedAttackPattern { get; set; } = AttackPattern.Box;

	private List<GameObject> activeAttackObjects = new();
	[Property, Group( "Attack Pattern Settings" )] private float attackCooldown = 10.0f; // Cooldown in Sekunden
	[Property, Group( "Attack Pattern Settings" )] private float timeSinceLastAttack = 0.0f;

	[Property, Group( "Attack Pattern Settings" )] private float patternChangeInterval = 10f; // Intervall in Sekunden für den Musterwechsel
	private float timeSinceLastPatternChange = 0.0f;

	protected override void OnUpdate()
	{
		base.OnUpdate();


		timeSinceLastAttack += Time.Delta;
		timeSinceLastPatternChange += Time.Delta;

		if ( timeSinceLastPatternChange >= patternChangeInterval )
		{
			// Wechseln Sie das Angriffsmuster
			SelectedAttackPattern = (AttackPattern)new Random().Next( 0, 5 ); // Aktualisiert, um die neuen Muster einzuschließen
			timeSinceLastPatternChange = 0.0f;
		}

		if ( timeSinceLastAttack >= attackCooldown )
		{
			switch ( SelectedAttackPattern )
			{
				case AttackPattern.Box:
					ExecuteBoxAttackPattern();
					break;
				case AttackPattern.Random:
					ExecuteRandomAttackPattern();
					break;
				case AttackPattern.FullCircle:
					Execute360AttackPattern();
					break;
				case AttackPattern.Cross:
					ExecuteCrossAttackPattern();
					break;
				case AttackPattern.Diagonal:
					ExecuteDiagonalAttackPattern();
					break;
			
				
				
				
			}
			timeSinceLastAttack = 0.0f;
		}
	}
	



	private async void ExecuteCrossAttackPattern()
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
			SpawnObjectAtBoxSide( prefab, BoxSize.x, Vector3.Left );
			SpawnObjectAtBoxSide( prefab, BoxSize.x, Vector3.Right );
			

			await Task.Delay( (int)(SpawnInterval * 1000) );
		}
	}

	private async void ExecuteDiagonalAttackPattern()
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
			SpawnObjectAtBoxSide( prefab, BoxSize.Length, Normalize( new Vector3( 1, 1, 0 ) ) );
			SpawnObjectAtBoxSide( prefab, BoxSize.Length, Normalize( new Vector3( -1, 1, 0 ) ) );
			SpawnObjectAtBoxSide( prefab, BoxSize.Length, Normalize( new Vector3( 1, -1, 0 ) ) );
			SpawnObjectAtBoxSide( prefab, BoxSize.Length, Normalize( new Vector3( -1, -1, 0 ) ) );

			await Task.Delay( (int)(SpawnInterval * 1000) );
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
			

			SpawnObjectAtBoxSide( prefab, BoxSize.z, Vector3.Forward );
			SpawnObjectAtBoxSide( prefab, BoxSize.z, Vector3.Backward );
			SpawnObjectAtBoxSide( prefab, BoxSize.x, Vector3.Left );
			SpawnObjectAtBoxSide( prefab, BoxSize.x, Vector3.Right );

			await Task.Delay( (int)(SpawnInterval * 1000) );
		}
	}
	private async void Execute360AttackPattern()
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

		int numberOfObjects = 36; // Anzahl der Objekte, die in einem Kreis gespawnt werden
		float angleStep = 360.0f / numberOfObjects;

		for ( int i = 0; i < numberOfObjects; i++ )
		{
			float angle = i * angleStep;
			Vector3 direction = new Vector3(
				(float)Math.Cos( DegreesToRadians( angle ) ),
				(float)Math.Sin( DegreesToRadians( angle ) ),
				0
			);

			SpawnObjectAtBoxSide( prefab, BoxSize.Length, direction );

			await Task.Delay( (int)(SpawnInterval * 500) );
		}
	}
	private float DegreesToRadians( float degrees )
	{
		return (float)(degrees * Math.PI / 180.0);
	}
	private Vector3 Normalize( Vector3 vector )
	{
		float length = vector.Length;
		if ( length > 0 )
		{
			return vector / length;
		}
		return Vector3.Zero;
	}
	private async void ExecuteRandomAttackPattern()
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
			Vector3 randomDirection = new Vector3(
				(float)(new System.Random().NextDouble() * 2 - 1),
				(float)(new System.Random().NextDouble() * 2 - 1),
				(float)(new System.Random().NextDouble() * 2 - 1)
			);
			randomDirection = Normalize( randomDirection );

			SpawnObjectAtBoxSide( prefab, BoxSize.Length, randomDirection );

			await Task.Delay( (int)(SpawnInterval * 1000) );
		}
	}

	private void SpawnObjectAtBoxSide( PrefabFile prefab, float boxSideLength, Vector3 direction )
	{
		var spawnPosition = LocalPosition; // Setze die Spawn-Position auf den Mittelpunkt der Box

		var attackObject = GameObject.Clone( prefab );
		attackObject.LocalPosition = spawnPosition;
		attackObject.WorldRotation = Rotation.Identity;
		attackObject.NetworkSpawn();
		attackObject.Network.DropOwnership();

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

	protected override void DrawGizmos()
	{
		base.DrawGizmos();
		var draw = Gizmo.Draw;
		Gizmo.Draw.Color = Color.Red; // Farbe des Gizmos

		// Zeichne die Box an der festen Position
		Vector3 center = BoxPosition; // Verwende die BoxPosition anstelle von LocalPosition
		Gizmo.Draw.LineBBox( new BBox( center - BoxSize / 2, center + BoxSize / 2 ) );
	}
}