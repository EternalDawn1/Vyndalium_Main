using Sandbox;
using GeneralGame;

[Icon("groups")]
public sealed class NpcSpawnArea : Component
{
	[Property] public Checkpoint Checkpoint { get; set; }
	[Property] public ChallengeDoor ChallengeDoor { get; set; }
	[Property] public SoundEvent BattleMusic { get; set; }

	[Property] public ActiveArea ActiveArena { get; set; }


	[Property] public List<Light> Lights { get; set; } = new();
	[Property] public Color BaseColor { get; set; } = Color.White;
	[Property] public Color FadingToColor { get; set; } = Color.Red;
	[Property] public float ColorChangeDuration { get; set; } = 2.0f;



	public enum GizmoType
	{
		Sphere,
		Box,
		Cylinder
	}
	public struct SpawnCountRange
	{
		[Property]
		[JsonInclude]
		public int MinCount { get; set; } = 1;

		[Property]
		[JsonInclude]
		public int MaxCount { get; set; } = 10;

		[Property]
		[JsonInclude]
		[Range( 0.01f, 1f, 0.01f )]
		public float SpawnChance { get; set; } = 0.2f;

		public SpawnCountRange() { }
	}

	public struct NpcChance
	{
		[Property]
		[JsonInclude]
		public GameObject Npc { get; set; }

		[Property]
		[JsonInclude]
		public SpawnCountRange SpawnCountRange { get; set; } = new();

	

		[Property]
		[JsonInclude]
		public int SpawnCount { get; set; }

		[Property]
		[JsonInclude]
		
		public bool EnableSubNpcPool { get; set; } = false;

		[Property]
		[JsonInclude]
		[ShowIf("EnableSubNpcPool", true)]
		public List<SubNpcChance> SubNpcPool { get; set; } = new();

		[Property]
		[JsonInclude]
		[ShowIf("EnableSubNpcPool", true)]
		public float SubNpcSpawnDelay { get; set; } = 5f;

		[Property]
		[JsonInclude]
		public int MinLevel { get; set; } = 1;
		[Property]
		[JsonInclude]

		public int MaxLevel { get; set; } = 1;

		[Property]
		[JsonInclude]
		[Range(0f, 1f)]
		public float IceAbilityChance { get; set; } = 0f;
		[Property]
		[JsonInclude]
		[Range(0f, 1f)]
		public float WindAbilityChance { get; set; } = 0f;
		[Property]
		[JsonInclude]
		[Range(0f, 1f)]
		public float FireAbilityChance { get; set; } = 0f;

		[Property]
		[JsonInclude]
		public bool SequentialSpawn { get; set; } = false;

		[Property]
		[JsonInclude]
		[ShowIf("SequentialSpawn", true)]
		public float SpawnInterval { get; set; } = 1f;

		public NpcChance() { }
	}
	public struct SubNpcChance
	{
		[Property]
		[JsonInclude]
		public GameObject Npc { get; set; }


		[Property]
		[JsonInclude]
		[Range(0.01f, 1f, 0.01f)]
		public float SpawnChance { get; set; } = 0.2f;

		[Property]
		[JsonInclude]
		public int SpawnCount { get; set; }
		[Property]
		[JsonInclude]
		public float SubNpcSpawnDelayPerNpc { get; set; }

		[Property]
		[JsonInclude]
		[Range(0f, 1f)]
		public float IceAbilityChance { get; set; } = 0f;
		[Property]
		[JsonInclude]
		[Range(0f, 1f)]
		public float WindAbilityChance { get; set; } = 0f;
		[Property]
		[JsonInclude]
		[Range(0f, 1f)]
		public float FireAbilityChance { get; set; } = 0f;
		[Property]
		[JsonInclude]
		public int MinLevel { get; set; } = 1;
		[Property]
		[JsonInclude]
		public int MaxLevel { get; set; } = 100;

		[Property]
		[JsonInclude]
		public bool SequentialSpawn { get; set; } = false;

		[Property]
		[JsonInclude]
		[ShowIf("SequentialSpawn", true)]
		public float SpawnInterval { get; set; } = 1f;

		[Property]
		[JsonInclude]
		public bool EnableDestroyAfterTime { get; set; } = false;

		[Property]
		[JsonInclude]
		[ShowIf( "EnableDestroyAfterTime", true )]
		public float DestroyAfterTime { get; set; } = 10f;

		public SubNpcChance() { }
	}


	[Property, Group( "Gizmo" )]
	public Vector3 PlayerProximityDistance { get; set; } = new Vector3( 1000f, 1000f, 1000f );

	[Property, Group( "Gizmo" )]
	public Vector3 PositionOffset { get; set; } = Vector3.Zero;

	[Property, Group("Gizmo")]
	public bool DrawProximityRangeGizmo { get; set; }

	[Property, Group("Gizmo")]
	public GizmoType GizmoShape { get; set; } = GizmoType.Sphere;

	[Property, Group("Gizmo")]
	public Color GizmoColor { get; set; } = Color.Red.WithAlpha(0.3f);

	[Property, Group("Gizmo")]
	public bool FillGizmo { get; set; } = false;

	/// <summary>
	/// Debug-Properties
	/// </summary>

	[Property , Hide]
	private bool hasSpawnedNPCs = false;

	
	

	[Property , Hide]
	private bool hasSpawnedBoss = false;
	[Property, Hide]
	private bool allSubNpcsKilled = false;

	[Property , Group("General")]public bool LoopSpawning { get; set; }

	[Property, Group( "General" )] public bool InfiniteLoops { get; set; } = false;
	[Property, Group("General")] public float LoopSpawnInterval { get; set; } = 300f;
	[Property,Group("General")] public RealTimeSince lastSpawnTime = 0f;

	[Property, Group("General")]public bool DestroyAfterSpawning { get; set; }

	[Property, Group("SpawnRange")]
	public bool DrawSpawnAreaGizmo { get; set; }

	

	[Property, Group("SpawnRange")]
	public Color SpawnAreaGizmoColor { get; set; } = Color.Blue.WithAlpha(0.3f);
	[Property, Group("SpawnRange")]
	public Vector3 Radius { get; set; } = new Vector3(1000f, 1000f, 1000f);

	[Property, Group("SpawnRange")]
	public float Height { get; set; }

	[Property, Group("SpawnRange")]
	public float StopLogicDistance { get; set; } = 4000f;

	[Property, Group("SpawnRange")]
	public float StopDrawingDistance { get; set; } = 7000f;

	[Property]
	[JsonInclude]
	[Group("NpcSettings")]

	public List<NpcChance> NpcPool { get; set; }

	[Property]
	[JsonInclude]
	[Group("BossSettings")]
	public List<NpcChance> BossNpcPool { get; set; }

	[Property, Group("BossSettings")]
	public float BossSpawnDelay { get; set; } = 10f;
	

	private TimeUntil? TimeUntilBossSpawn { get; set; }

	[JsonIgnore]
	[Sync]
	public NetList<GameObject> SpawnedNpcs { get; set; } = new();

	private void CheckAllNpcsKilled()
	{
		if ( SpawnedNpcs.All( npc => npc == null || !npc.IsValid ) )
		{
			Checkpoint?.Activate();
		}
	}
	private void OnNpcKilled( GameObject npc )
	{
		SpawnedNpcs.Remove( npc );
		CheckAllNpcsKilled();
	}

	

	protected override void DrawGizmos()
	{
		base.DrawGizmos();
		var draw = Gizmo.Draw;
		if ( DrawSpawnAreaGizmo )
		{
			Gizmo.Draw.Color = SpawnAreaGizmoColor;
			switch ( GizmoShape )
			{
				case GizmoType.Sphere:
					if ( FillGizmo )
						Gizmo.Draw.SolidSphere( PositionOffset, Radius.x ); // Verwenden Sie die X-Komponente des Radius
					else
						Gizmo.Draw.LineSphere( PositionOffset, Radius.x ); // Verwenden Sie die X-Komponente des Radius
					break;
				case GizmoType.Box:
					if ( FillGizmo )
						Gizmo.Draw.SolidBox( new BBox( new Vector3( -Radius.x, -Height / 2, -Radius.z ) + PositionOffset, new Vector3( Radius.x, Height / 2, Radius.z ) + PositionOffset ) );
					else
						Gizmo.Draw.LineBBox( new BBox( new Vector3( -Radius.x, -Height / 2, -Radius.z ) + PositionOffset, new Vector3( Radius.x, Height / 2, Radius.z ) + PositionOffset ) );
					break;
				case GizmoType.Cylinder:
					if ( FillGizmo )
						Gizmo.Draw.SolidCylinder( PositionOffset, Vector3.Up * Height, Radius.x, 15 ); // Verwenden Sie die X-Komponente des Radius
					else
						Gizmo.Draw.LineCylinder( PositionOffset, Vector3.Up * Height, Radius.x, Radius.x, 15 ); // Verwenden Sie die X-Komponente des Radius
					break;
			}
		}

		if ( DrawProximityRangeGizmo )
		{
			Gizmo.Draw.Color = GizmoColor;
			switch ( GizmoShape )
			{
				case GizmoType.Sphere:
					if ( FillGizmo )
						Gizmo.Draw.SolidSphere( PositionOffset, PlayerProximityDistance.Length ); // Verwenden Sie die Länge des Vektors
					else
						Gizmo.Draw.LineSphere( PositionOffset, PlayerProximityDistance.Length ); // Verwenden Sie die Länge des Vektors
					break;
				case GizmoType.Box:
					if ( FillGizmo )
						Gizmo.Draw.SolidBox( new BBox( -PlayerProximityDistance + PositionOffset, PlayerProximityDistance + PositionOffset ) );
					else
						Gizmo.Draw.LineBBox( new BBox( -PlayerProximityDistance + PositionOffset, PlayerProximityDistance + PositionOffset ) );
					break;
				case GizmoType.Cylinder:
					if ( FillGizmo )
						Gizmo.Draw.SolidCylinder( PositionOffset, Vector3.Up * PlayerProximityDistance.z, PlayerProximityDistance.x, 15 ); // Verwenden Sie die X- und Z-Distanzen
					else
						Gizmo.Draw.LineCylinder( PositionOffset, Vector3.Up * PlayerProximityDistance.z, PlayerProximityDistance.x, PlayerProximityDistance.x, 15 ); // Verwenden Sie die X- und Z-Distanzen
					break;
			}
		}
	}
	private float playerProximityDuration = 3.0f; // Zeit in Sekunden, die der Spieler in der Nähe sein muss
	private float playerProximityTimer = 0.0f;
	protected override void OnFixedUpdate()
	{
		if ( !IsPlayerInRoom() )
		{
			playerProximityTimer = 0.0f; // Timer zurücksetzen, wenn der Spieler nicht im Raum ist
			return;
		}

		playerProximityTimer += Time.Delta; // Timer erhöhen, wenn der Spieler im Raum ist

		if ( playerProximityTimer >= playerProximityDuration )
		{
			if ( ChallengeDoor != null && ChallengeDoor.GameObject != null )
			{
				ChallengeDoor.GameObject.Enabled = true;
			}

			if ( !hasSpawnedNPCs )
			{
				SpawnNPCs();
				hasSpawnedNPCs = SpawnedNpcs.Count > 0; // Setze auf true, wenn NPCs erfolgreich gespawnt wurden
			}

			// Überprüfen, ob alle NPCs aus dem NpcPool tot sind
			if ( !hasSpawnedBoss )
			{
				if ( TimeUntilBossSpawn == null || TimeUntilBossSpawn <= 0 )
				{
					SpawnBossNPCs();
					hasSpawnedBoss = SpawnedNpcs.Count > 0; // Setze auf true, wenn Boss-NPCs erfolgreich gespawnt wurden
				}
			}
		}

		// Loop-Spawning-Logik
		if ( LoopSpawning && lastSpawnTime >= LoopSpawnInterval )
		{
			SpawnNPCs();
			lastSpawnTime = 0f; // Timer zurücksetzen
			if ( !InfiniteLoops )
			{
				LoopSpawning = false; // Deaktiviere LoopSpawning nach einmaligem Ausführen, wenn InfiniteLoops nicht aktiviert ist
			}
		}

		// Setze LoopSpawning nach Ablauf des Intervalls wieder auf true, wenn InfiniteLoops aktiviert ist
		if ( InfiniteLoops && !LoopSpawning && lastSpawnTime >= LoopSpawnInterval )
		{
			LoopSpawning = true;
		}
	}
	private bool IsPlayerInDoor( Player player )
	{
		if ( ChallengeDoor == null || ChallengeDoor.GameObject == null )
		{
			return false;
		}

		// Implementieren Sie die Logik, um zu überprüfen, ob der Spieler sich in der Tür befindet
		// Beispiel: Überprüfen Sie die Position des Spielers relativ zur Tür
		var doorPosition = ChallengeDoor.GameObject.WorldPosition;
		var playerPosition = player.WorldPosition;
		var distanceToDoor = (playerPosition - doorPosition).Length;

		// Beispielwert für die Türbreite, anpassen nach Bedarf
		float doorWidth = 2.0f;

		return distanceToDoor < doorWidth;
	}

	public bool IsPlayerInRoom()
	{
		if ( Network.IsProxy || NpcPool == null || NpcPool.Count == 0 )
			return false;

		var players = Scene.GetAllComponents<Player>();
		if ( players == null )
		{
			return false;
		}

		foreach ( var player in players )
		{
			if ( (player.WorldPosition - this.WorldPosition).Length < PlayerProximityDistance.Length )
			{
				// Überprüfen, ob der Spieler sich nicht in der Tür befindet
				if ( !IsPlayerInDoor( player ) )
				{
					return true;
				}
			}
		}
		return false;
	}
	private bool IsPlayerNearby()
	{
		if ( Network.IsProxy || NpcPool == null || NpcPool.Count == 0 )
			return false;

		var players = Scene.GetAllComponents<Player>();
		if ( players == null )
		{
			return false;
		}
		foreach ( var player in players )
		{
			if ( (player.WorldPosition - this.WorldPosition).Length < PlayerProximityDistance.Length )
				return true;
		}
		return false;
	}
	

	public async void SpawnNPCs()
	{
		
		// Spawne die normalen NPCs
		foreach ( var npcChance in NpcPool )
		{
			var random = Game.Random.Float( 0f, 1f );
			var shouldSpawn = random <= npcChance.SpawnCountRange.SpawnChance;
			if ( shouldSpawn )
			{
				var spawnCount = Game.Random.Int( npcChance.SpawnCountRange.MinCount, npcChance.SpawnCountRange.MaxCount );
				for ( int i = 0; i < spawnCount; i++ )
				{
					var npc = SpawnNpc( npcChance.Npc );
					if ( npc != null )
					{
						var npcComponent = npc.GetComponent<Npc>();
						if ( npcComponent != null )
						{
							npcComponent.Level = new Random().Next( npcChance.MinLevel, npcChance.MaxLevel + 1 );
							npcComponent.SetHealthBasedOnLevel();
							var abilityRandom = new Random();
							npcComponent.HasIceAbility = npcChance.IceAbilityChance >= 1.0 || new Random().NextDouble() <= npcChance.IceAbilityChance;
							npcComponent.HasWindAbility = npcChance.WindAbilityChance >= 1.0 || new Random().NextDouble() <= npcChance.WindAbilityChance;
							npcComponent.HasFireAbility = npcChance.FireAbilityChance >= 1.0 || new Random().NextDouble() <= npcChance.FireAbilityChance;
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

										var fireNpcComponent = fireInstance.GetComponent<Npc>();
										if ( fireNpcComponent != null )
										{
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

						CreateSpawnParticle( npc.WorldPosition );
						SpawnedNpcs.Add( npc );

						if ( npcChance.SequentialSpawn )
						{
							await Task.Delay( (int)(npcChance.SpawnInterval * 1000) );
						}
					}
				}

				if ( npcChance.EnableSubNpcPool )
				{
					_ = SpawnSubNpcsWithDelay( npcChance.SubNpcPool, npcChance.SubNpcSpawnDelay );
				}
			}
		}
	}


	private void SpawnBossNPCs()
	{
		// Spawne die Boss-NPCs
		foreach ( var bossChance in BossNpcPool )
		{
			var random = Game.Random.Float( 0f, 1f );
			var shouldSpawn = random <= bossChance.SpawnCountRange.SpawnChance;
			if ( shouldSpawn )

				if ( shouldSpawn )
			{
					var spawnCount = Game.Random.Int( bossChance.SpawnCountRange.MinCount, bossChance.SpawnCountRange.MaxCount );
					for ( int i = 0; i < spawnCount; i++ )
					{
					var boss = SpawnNpc( bossChance.Npc );
					if ( boss != null )
					{
						var npcComponent = boss.GetComponent<Npc>();
						if ( npcComponent != null )
						{
							npcComponent.Level = new Random().Next( bossChance.MinLevel, bossChance.MaxLevel + 1 );
							npcComponent.SetHealthBasedOnLevelBoss( boss.GetComponent<Npc>().MaxHealth ); // Verwende die neue Methode
							var abilityRandom = new Random();
							npcComponent.HasIceAbility = bossChance.IceAbilityChance >= 1.0 || new Random().NextDouble() <= bossChance.IceAbilityChance;
							npcComponent.HasWindAbility = bossChance.WindAbilityChance >= 1.0 || new Random().NextDouble() <= bossChance.WindAbilityChance;
							npcComponent.HasFireAbility = bossChance.FireAbilityChance >= 1.0 || new Random().NextDouble() <= bossChance.FireAbilityChance;
						}
						SpawnedNpcs.Add( boss );

						foreach ( var light in Lights )
						{
							_ = LerpLightColor( light, BaseColor, FadingToColor, ColorChangeDuration );
						}

						if ( BattleMusic != null )
						{
							Sound.Play( BattleMusic, boss.WorldPosition );
						}
						if( ActiveArena != null )
						{
							ActiveArena.Enabled = true;
						}

						// Setze die Variable zurück, wenn ein neuer Boss gespawnt wird
						allSubNpcsKilled = false;
						// Spawne die Sub-NPCs des Bosses mit einer Verzögerung
						_ = SpawnSubNpcsWithDelay( bossChance.SubNpcPool, bossChance.SubNpcSpawnDelay );
					}
					else
					{
						Log.Error( "Boss NPC konnte nicht gespawnt werden." );
					}
				}
			}
			if ( allSubNpcsKilled )
			{
				break;
			}
		}
		
	}
	

	private async Task SpawnSubNpcsWithDelay( List<SubNpcChance> subNpcPool, float delay )
	{
		do
		{
			await Task.Delay( (int)(delay * 1000) );
			if ( allSubNpcsKilled )
			{
				return;
			}

			foreach ( var npcChance in subNpcPool )
			{
				var random = Game.Random.Float( 0f, 1f );
				var shouldSpawn = random <= npcChance.SpawnChance;
				if ( shouldSpawn )
				{
					for ( int i = 0; i < npcChance.SpawnCount; i++ )
					{
						var npc = SpawnNpc( npcChance.Npc );
						if ( npc != null )
						{
							var npcComponent = npc.GetComponent<Npc>();
							if ( npcComponent != null )
							{
								npcComponent.Level = new Random().Next( npcChance.MinLevel, npcChance.MaxLevel + 1 );
								npcComponent.SetHealthBasedOnLevel();

								var abilityRandom = new Random();
								npcComponent.HasIceAbility = npcChance.IceAbilityChance >= 1.0 || new Random().NextDouble() <= npcChance.IceAbilityChance;
								npcComponent.HasWindAbility = npcChance.WindAbilityChance >= 1.0 || new Random().NextDouble() <= npcChance.WindAbilityChance;
								npcComponent.HasFireAbility = npcChance.FireAbilityChance >= 1.0 || new Random().NextDouble() <= npcChance.FireAbilityChance;
							}
							SpawnedNpcs.Add( npc );

							if ( npcChance.EnableDestroyAfterTime )
							{
								_ = DestroyAfterTime( npc, npcChance.DestroyAfterTime );
							}
						}
						if ( npcChance.SequentialSpawn )
						{
							await Task.Delay( (int)(npcChance.SpawnInterval * 1000) );
						}
					}
				}
			}

			// Füge eine Verzögerung hinzu, um den Cooldown zu berücksichtigen
			await Task.Delay( (int)(LoopSpawnInterval * 1000) );

		} while ( LoopSpawning && lastSpawnTime >= LoopSpawnInterval );

		if ( DestroyAfterSpawning && ChallengeDoor != null && ChallengeDoor.IsTimerExpired() )
		{
			GameObject.Destroy();
			
		}
	}

	private async Task DestroyAfterTime( GameObject npc, float time )
	{
		await Task.Delay( (int)(time * 1000) );
		npc?.Destroy();
	}


	private GameObject SpawnNpc( GameObject npcPrefab )
	{
		if ( npcPrefab == null )
		{
			Log.Warning( "npcPrefab is null." );
			return null;
		}

		var tries = 0;
		while ( tries <= 20 )
		{
			var randomDirection = Rotation.FromYaw( Game.Random.Float( 360f ) ).Forward;
			var randomPosition = WorldPosition + PositionOffset + randomDirection * Game.Random.Float( Radius.x ); // Verwenden Sie die X-Komponente des Radius
			var startPos = randomPosition.WithZ( WorldPosition.z + Height / 2f );
			var endPos = randomPosition.WithZ( WorldPosition.z - Height / 2f );
			var groundTrace = Game.ActiveScene.Trace.Ray( startPos, endPos )
				.Size( 5f )
				.WithoutTags( "player", "npc", "trigger" )
				.Run();

			if ( groundTrace.Hit && !groundTrace.StartedSolid )
			{
				if ( Vector3.GetAngle( Vector3.Up, groundTrace.Normal ) <= 60f )
				{
					var clone = npcPrefab.Clone( groundTrace.HitPosition, Rotation.FromYaw( Game.Random.Float( 360f ) ) );
					if ( clone == null )
					{
						Log.Warning( "Failed to clone npcPrefab." );
						return null;
					}

					clone.NetworkMode = NetworkMode.Object;
					clone.NetworkSpawn();

					return clone;
				}
			}
			tries++;
		}
		Log.Warning( "Failed to spawn NPC after 20 tries." );
		return null;
	}

	public void RemoveNPCs()
	{
		foreach (var npc in SpawnedNpcs)
		{
			npc?.Destroy();
		}
	}
	private bool DetermineFreezeAbility(int level)
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
		/* var p = new SceneParticles(Scene.SceneWorld, "particles/impact.flesh.bloodpuff.vpcf");
		p.SetControlPoint(0, position);
		p.SetControlPoint(0, -1f);
		p.SetControlPoint(1, new Vector3(5.5f, 0.1f, 0.1f));
		p.PlayUntilFinished(Task); */
	}
	private async Task LerpLightColor( Light light, Color startColor, Color endColor, float duration )
	{
		float time = 0;
		while ( time < duration )
		{
			light.LightColor = Color.Lerp( startColor, endColor, time / duration );
			time += Time.Delta;
			await Task.Yield();
		}
		light.LightColor = endColor;
	}



}
