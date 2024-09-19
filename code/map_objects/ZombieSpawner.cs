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
	[Property] public bool Level1To15 { get; set; }
	[Property] public bool Level15To30 { get; set; }
	[Property] public bool Level30To55 { get; set; }
	[Property] public bool Level55To70 { get; set; }
	[Property] public bool Level70To90 { get; set; }
	[Property] public bool Level90To100 { get; set; }
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
			
			itemComponent.GenerateRandomStats();
			itemComponent.CalculateSellPrice();
		
		}
		zombie.NetworkSpawn();


		// Setze das Level des Zombies basierend auf den Properties
		var npcComponent = zombie.Components.Get<Npc>();
		if ( npcComponent != null )
		{
			npcComponent.Level = GetRandomLevel();
			npcComponent.SetHealthBasedOnLevel();
			npcComponent.HasIceAbility = DetermineFreezeAbility( npcComponent.Level );

			if ( npcComponent is Slime )
			{
				npcComponent.Model.Set( "slime_spawn", true );
				
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
			zombie.Transform.LocalScale = randomScale;
		}

		IsSpawning = true;

		CreateSpawnParticle( zombie.Transform.Position );

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
		if ( Level1To15 )
			return random.Next( 1, 16 );
		if ( Level15To30 )
			return random.Next( 15, 31 );
		if ( Level30To55 )
			return random.Next( 30, 56 );
		if ( Level55To70 )
			return random.Next( 55, 71 );
		if ( Level70To90 )
			return random.Next( 70, 91 );
		if ( Level90To100 )
			return random.Next( 90, 101 );

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
