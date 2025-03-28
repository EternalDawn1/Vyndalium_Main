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
	[Property] public bool DrawProximityRangeGizmo { get; set; }
	[Property] public bool Level1To5 { get; set; }
	[Property] public bool Level5To10 { get; set; }
	[Property] public bool Level10To15 { get; set; }
	[Property] public bool Level15To20 { get; set; }
	[Property] public bool Level20To25 { get; set; }
	[Property] public bool Level25To30 { get; set; }
	[Property] public bool Level30To35 { get; set; }
	[Property] public bool Level35To40 { get; set; }
	[Property] public bool Level40To45 { get; set; }
	[Property] public bool Level45To50 { get; set; }
	[Property] public bool Level50To55 { get; set; }
	[Property] public bool Level55To60 { get; set; }
	[Property] public bool Level60To65 { get; set; }
	[Property] public bool Level65To70 { get; set; }
	[Property] public bool Level70To75 { get; set; }
	[Property] public bool Level75To80 { get; set; }
	[Property] public bool Level80To85 { get; set; }
	[Property] public bool Level85To90 { get; set; }
	[Property] public bool Level90To95 { get; set; }
	[Property] public bool Level95To100 { get; set; }
	[Property] public bool RandomizePropertiesOnRain { get; set; }
	[Property] public bool RandomizeTierOnSpawn { get; set; }

	
	

	protected override void DrawGizmos()
	{
		const float boxSize = 4f;
		var bounds = new BBox( Vector3.One * -boxSize, Vector3.One * boxSize );

		Gizmo.Hitbox.BBox( bounds );

		Gizmo.Draw.Color = Color.Cyan.WithAlpha( (Gizmo.IsHovered || Gizmo.IsSelected) ? 0.5f : 0.2f );
		Gizmo.Draw.LineBBox( bounds );
		Gizmo.Draw.SolidBox( bounds );

		Gizmo.Draw.Color = Color.Cyan.WithAlpha( (Gizmo.IsHovered || Gizmo.IsSelected) ? 0.8f : 0.6f );

		// Zeichne den PlayerProximityDistance-Gizmo, wenn aktiviert
		if ( DrawProximityRangeGizmo )
		{
			Gizmo.Draw.Color = Color.Red.WithAlpha( 0.3f );
			Gizmo.Draw.LineSphere( Vector3.Zero, PlayerProximityDistance );
		}
	}

	protected override void OnStart()
	{
		TimeUntilRespawn = 5f;
		base.OnStart();
	}
	

	[Property]public bool Randomized { get; set; } = false;
	[Property] public float destroyChance { get; set; } = 0.01f;
	[Property]
	public GameObject Ragdoll { get; set; }

	[Property]
	public bool EnableRagdollEffect { get; set; } = false;

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( !Networking.IsHost )
			return;

		if ( !IsPlayerNearby() ) // Überprüfen, ob ein Spieler in der Nähe ist
			return;

		if(Randomized)
		{
			// 10% Wahrscheinlichkeit
			Random random = new Random();
			if ( random.NextDouble() < destroyChance )
			{
				GameObject.Destroy();
				return;
			}
		}
		


		if ( SpawnCount >= MaxSpawns ) // Überprüfen, ob die maximale Anzahl von Spawns erreicht wurde
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
		
		if ( zombie == null || zombie.Components == null )
		{
			// Log error or handle the null case
			return;
		}
		// Führe einen Raycast nach unten durch, um die Bodenhöhe zu ermitteln
		var spawnPosition = zombie.WorldPosition;
		var groundTrace = Scene.Trace.Ray(spawnPosition + Vector3.Up * 100f, spawnPosition + Vector3.Down * 200f)
			.Size(5f)
			.IgnoreGameObjectHierarchy(zombie)
			.WithoutTags("player", "npc", "trigger")
			.Run();

		if (groundTrace.Hit)
		{
			// Setze die Position des Zombies auf die Bodenhöhe
			zombie.WorldPosition = groundTrace.HitPosition;
		}

		if ( RandomizeTierOnSpawn )
		{
			var itemInteractable = zombie.Components.Get<ItemInteractable>();
			if ( itemInteractable != null )
			{
				itemInteractable.Tier = (GeneralGame.Tier)GetRandomTier();
			}
		}
		var itemComponent = zombie.Components.Get<ItemComponent>();
		if ( itemComponent != null )
		{
			
			
			itemComponent.CalculateSellPrice();
		
		}
		if ( EnableRagdollEffect && Ragdoll != null )
		{
			var ragdoll = Ragdoll.Clone( WorldPosition );
			if ( ragdoll != null )
			{
				ragdoll.WorldRotation = WorldRotation;
				ragdoll.WorldPosition = WorldPosition;
				ragdoll.NetworkSpawn();
				ragdoll.Network.DropOwnership();
			}
		}
		Sound.Play( "sounds/levelup/levelup.sound", zombie.WorldPosition );
		zombie.NetworkSpawn();
		zombie.Network.DropOwnership();


		// Setze das Level des Zombies basierend auf den Properties
		var npcComponent = zombie.Components.Get<Npc>();
		if ( npcComponent != null )
		{
			npcComponent.Level = GetRandomLevel();
			npcComponent.SetHealthBasedOnLevel();
			npcComponent.HasIceAbility = DetermineFreezeAbility( npcComponent.Level );
			npcComponent.HasWindAbility = DetermineWindAbility( npcComponent.Level );
			npcComponent.HasFireAbility = DetermineFireAbility( npcComponent.Level );

			if ( npcComponent.HasFireAbility )
			{
				// Laden Sie das Prefab über die ResourceLibrary
				var firePrefab = ResourceLibrary.Get<PrefabFile>( "prefabs/npc/slime_variants/fire.prefab" );

				if ( firePrefab != null )
				{
					// Erstellen Sie eine Instanz des Prefabs auf dem NPC-GameObject
					var fireInstance = GameObject.Clone( firePrefab );
					if ( fireInstance != null )
					{
						fireInstance.Parent = GameObject; // Explizite Konvertierung zu GameObject
						fireInstance.WorldPosition = npcComponent.WorldPosition; // Setzen Sie die Position relativ zum NPC
						fireInstance.NetworkSpawn();
						fireInstance.Network.DropOwnership();

						var fireNpcComponent = fireInstance.GetComponent<Npc>();
						if ( fireNpcComponent != null )
						{
							fireNpcComponent.Level = GetRandomLevel();
							fireNpcComponent.SetHealthBasedOnLevel();
						}
					}
				}
				else
				{
					Log.Error( "Fire prefab could not be loaded." );
				}
				
			}

			if ( npcComponent is Slime )
			{
				npcComponent.Model.Set( "slime_spawn", true );
				
			}
			else if ( npcComponent is Npc && npcComponent.Model != null )
			{
				npcComponent.Model.Set( "chibi_spawn", true );
			}

		}
		
		


		if ( RandomizePropertiesOnRain )
		{
			// Zufällige Farbe generieren

			var prop = GameObject.Components.Get<Prop>();
			if ( prop != null )
			{
				// Erstelle ein neues Random-Objekt
				var random = new Random();

				// Generiere eine zufällige Farbe
				var randomColor = new Color(
					(float)random.NextDouble(),
					(float)random.NextDouble(),
					(float)random.NextDouble()
				);

				// Setze die zufällige Farbe auf das Prop-Objekt
				prop.Tint = randomColor;
			}

			// Zufällige Skalierung generieren
			var randomScale = new Vector3(
			(float)Random.Shared.NextDouble() * 1.9f + 0.1f,
			(float)Random.Shared.NextDouble() * 1.9f + 0.1f,
			(float)Random.Shared.NextDouble() * 1.9f + 0.1f
			);
			zombie.LocalScale = randomScale;
		}

		IsSpawning = true;

		CreateSpawnParticle( zombie.WorldPosition );

		TimeUntilRespawn = null;

		SpawnCount++;
	}
	
	private Tier GetRandomTier()
	{
		var values = Enum.GetValues( typeof( Tier ) );
		return (Tier)values.GetValue( new Random().Next( values.Length ) );
	}
	public enum Tier
	{
		C = 0,
		B = 1,
		A = 2,
		S = 3,
		SS = 4,
		SSS = 5
	}




	private int GetRandomLevel()
	{
		var random = new Random();
		if ( Level1To5 )
			return random.Next( 1, 6 );
		if ( Level5To10 )
			return random.Next( 5, 11 );
		if ( Level10To15 )
			return random.Next( 10, 16 );
		if ( Level15To20 )
			return random.Next( 15, 21 );
		if ( Level20To25 )
			return random.Next( 20, 26 );
		if ( Level25To30 )
			return random.Next( 25, 31 );
		if ( Level30To35 )
			return random.Next( 30, 36 );
		if ( Level35To40 )
			return random.Next( 35, 41 );
		if ( Level40To45 )
			return random.Next( 40, 46 );
		if ( Level45To50 )
			return random.Next( 45, 51 );
		if ( Level50To55 )
			return random.Next( 50, 56 );
		if ( Level55To60 )
			return random.Next( 55, 61 );
		if ( Level60To65 )
			return random.Next( 60, 66 );
		if ( Level65To70 )
			return random.Next( 65, 71 );
		if ( Level70To75 )
			return random.Next( 70, 76 );
		if ( Level75To80 )
			return random.Next( 75, 81 );
		if ( Level80To85 )
			return random.Next( 80, 86 );
		if ( Level85To90 )
			return random.Next( 85, 91 );
		if ( Level90To95 )
			return random.Next( 90, 96 );
		if ( Level95To100 )
			return random.Next( 95, 101 );

		return 1; // Standardlevel, falls keine Property gesetzt ist
	}
	private bool DetermineFreezeAbility( int level )
	{
		var random = new Random();
		if ( level >= 1 && level <= 15 )
			return random.Next( 100 ) < 15;
		if ( level >= 16 && level <= 30 )
			return random.Next( 100 ) < 30;
		if ( level >= 31 && level <= 55 )
			return random.Next( 100 ) < 55;
		if ( level >= 56 && level <= 70 )
			return random.Next( 100 ) < 70;
		if ( level >= 71 && level <= 90 )
			return random.Next( 100 ) < 90;
		if ( level >= 91 && level <= 100 )
			return random.Next( 100 ) < 100;

		return false;
	}
	private bool DetermineWindAbility(int level)
	{
		var random = new Random();
		if (level >= 1 && level <= 15)
			return random.Next(100) < 15;
		if (level >= 16 && level <= 30)
			return random.Next(100) < 30;
		if (level >= 31 && level <= 55)
			return random.Next(100) < 55;
		if (level >= 56 && level <= 70)
			return random.Next(100) < 70;
		if (level >= 71 && level <= 90)
			return random.Next(100) < 90;
		if (level >= 91 && level <= 100)
			return random.Next(100) < 100;

		return false;
	}
	private bool DetermineFireAbility(int level)
	{
		var random = new Random();
		if (level >= 1 && level <= 15)
			return random.Next(100) < 15;
		if (level >= 16 && level <= 30)
			return random.Next(100) < 30;
		if (level >= 31 && level <= 55)
			return random.Next(100) < 55;
		if (level >= 56 && level <= 70)
			return random.Next(100) < 70;
		if (level >= 71 && level <= 90)
			return random.Next(100) < 90;
		if (level >= 91 && level <= 100)
			return random.Next(100) < 100;

		return false;
	}
	private void CreateSpawnParticle(Vector3 position)
        {
            // Erstelle und spiele ein Partikelsystem beim Spawnen
          /*   var p = new SceneParticles( Scene.SceneWorld, "particles/impact.flesh.bloodpuff.vpcf" );
			p.SetControlPoint( 0, position );
			p.SetControlPoint( 0,  -1f ) ;
			p.SetControlPoint(1, new Vector3(5.5f, 0.1f, 0.1f));
			p.PlayUntilFinished( Task ); */
        }
	private bool IsPlayerNearby()
        {
            if (Network.IsProxy && !ZombiePrefab.IsValid())
        return false;

		var players = Scene.GetAllComponents<Player>();
		foreach (var player in players)
		{
			// Überprüfe, ob der Spieler in der Nähe ist
			if ((player.WorldPosition - this.WorldPosition).Length < PlayerProximityDistance)
            return true;
		}
            return false; 
        }

}
