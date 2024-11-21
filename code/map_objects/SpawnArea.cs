using Sandbox;
using GeneralGame;

[Icon("groups")]
public sealed class NpcSpawnArea : Component
{
	public enum GizmoType
	{
		Sphere,
		Box,
		Cylinder
	}

	public struct NpcChance
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

		public SubNpcChance() { }
	}


	[Property, Group("Gizmo")]
	public float PlayerProximityDistance { get; set; } = 1000f;

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
	[Property, Group("General")]public bool DestroyAfterSpawning { get; set; }

	[Property, Group("SpawnRange")]
	public bool DrawSpawnAreaGizmo { get; set; }

	[Property, Group("SpawnRange")]
	public Color SpawnAreaGizmoColor { get; set; } = Color.Blue.WithAlpha(0.3f);
	[Property, Group("SpawnRange")]
	public float Radius { get; set; }

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

	

	protected override void DrawGizmos()
	{
		base.DrawGizmos();
		var draw = Gizmo.Draw;
		if (DrawSpawnAreaGizmo)
		{
			Gizmo.Draw.Color = SpawnAreaGizmoColor;
			switch (GizmoShape)
			{
				case GizmoType.Sphere:
					if (FillGizmo)
						Gizmo.Draw.SolidSphere(Vector3.Zero, Radius);
					else
						Gizmo.Draw.LineSphere(Vector3.Zero, Radius);
					break;
				case GizmoType.Box:
					if (FillGizmo)
						Gizmo.Draw.SolidBox(new BBox(new Vector3(-Radius, -Height / 2, -Radius), new Vector3(Radius, Height / 2, Radius)));
					else
						Gizmo.Draw.LineBBox(new BBox(new Vector3(-Radius, -Height / 2, -Radius), new Vector3(Radius, Height / 2, Radius)));
					break;
				case GizmoType.Cylinder:
					if (FillGizmo)
						Gizmo.Draw.SolidCylinder(Vector3.Zero, Vector3.Up * Height, Radius, 15);
					else
						Gizmo.Draw.LineCylinder(Vector3.Zero, Vector3.Up * Height, Radius, Radius, 15);
					break;
			}
		}
		if (DrawProximityRangeGizmo)
		{
			Gizmo.Draw.Color = GizmoColor;
			switch (GizmoShape)
			{
				case GizmoType.Sphere:
					if (FillGizmo)
						Gizmo.Draw.SolidSphere(Vector3.Zero, PlayerProximityDistance);
					else
						Gizmo.Draw.LineSphere(Vector3.Zero, PlayerProximityDistance);
					break;
				case GizmoType.Box:
					if (FillGizmo)
						Gizmo.Draw.SolidBox(new BBox(Vector3.One * -PlayerProximityDistance, Vector3.One * PlayerProximityDistance));
					else
						Gizmo.Draw.LineBBox(new BBox(Vector3.One * -PlayerProximityDistance, Vector3.One * PlayerProximityDistance));
					break;
				case GizmoType.Cylinder:
					if (FillGizmo)
						Gizmo.Draw.SolidCylinder(Vector3.Zero, Vector3.Up * PlayerProximityDistance, PlayerProximityDistance, 15);
					else
						Gizmo.Draw.LineCylinder(Vector3.Zero, Vector3.Up * PlayerProximityDistance, PlayerProximityDistance, PlayerProximityDistance, 15);
					break;
			}
		}
	}

	protected override void OnFixedUpdate()
	{
		if (!IsPlayerNearby())
		{
			return;
		}
		
		if (!hasSpawnedNPCs)
		{
			SpawnNPCs();
			hasSpawnedNPCs = SpawnedNpcs.Count > 0; // Setze auf true, wenn NPCs erfolgreich gespawnt wurden
		}

		// Überprüfen, ob alle NPCs aus dem NpcPool tot sind
		if (!hasSpawnedBoss)
		{
			if (TimeUntilBossSpawn == null || TimeUntilBossSpawn <= 0)
			{
				SpawnBossNPCs();
				hasSpawnedBoss = SpawnedNpcs.Count > 0; // Setze auf true, wenn Boss-NPCs erfolgreich gespawnt wurden
			}
		}
	}

	private bool IsPlayerNearby()
	{
		if (Network.IsProxy || NpcPool == null || NpcPool.Count == 0)
			return false;

		var players = Scene.GetAllComponents<Player>();
		if (players == null)
		{
			return false;
		}
		foreach (var player in players)
		{
			if ((player.WorldPosition - this.WorldPosition).Length < PlayerProximityDistance)
				return true;
		}
		return false;
	}

	public void SpawnNPCs()
	{
		RemoveNPCs();

		// Spawne die normalen NPCs
		foreach (var npcChance in NpcPool)
		{
			var random = Game.Random.Float(0f, 1f);
			var shouldSpawn = random <= npcChance.SpawnChance;
			if (shouldSpawn)
			{
				for (int i = 0; i < npcChance.SpawnCount; i++)
				{
					var npc = SpawnNpc(npcChance.Npc);
					if (npc != null)
					{
						var npcComponent = npc.GetComponent<Npc>();
						if (npcComponent != null)
						{
							npcComponent.Level = new Random().Next(npcChance.MinLevel, npcChance.MaxLevel + 1);
							npcComponent.SetHealthBasedOnLevel();
							var abilityRandom = new Random();
							npcComponent.HasIceAbility = npcChance.IceAbilityChance >= 1.0 || new Random().NextDouble() <= npcChance.IceAbilityChance;
							npcComponent.HasWindAbility = npcChance.WindAbilityChance >= 1.0 || new Random().NextDouble() <= npcChance.WindAbilityChance;
							npcComponent.HasFireAbility = npcChance.FireAbilityChance >= 1.0 || new Random().NextDouble() <= npcChance.FireAbilityChance;
							if (npcComponent.HasFireAbility)
							{
								// Laden Sie das Prefab über die ResourceLibrary
								var firePrefab = ResourceLibrary.Get<PrefabFile>("prefabs/npc/slime_variants/fire.prefab");

								if (firePrefab != null)
								{
									// Erstellen Sie eine Instanz des Prefabs auf dem NPC-GameObject
									var fireInstance = GameObject.Clone(firePrefab);
									if (fireInstance != null)
									{
										fireInstance.Parent = GameObject; // Explizite Konvertierung zu GameObject
										fireInstance.WorldPosition = npcComponent.WorldPosition; // Setzen Sie die Position relativ zum NPC
										fireInstance.NetworkSpawn();

										var fireNpcComponent = fireInstance.GetComponent<Npc>();
										if (fireNpcComponent != null)
										{
										
											fireNpcComponent.SetHealthBasedOnLevel();
										}
									}
								}
								else
								{
									Log.Error("Fire prefab could not be loaded.");
								}

							}

							if (npcComponent is Slime)
							{
								npcComponent.Model.Set("slime_spawn", true);

							}
							else if (npcComponent is Npc && npcComponent.Model != null)
							{
								npcComponent.Model.Set("chibi_spawn", true);
							}
						}
						
						
						
						CreateSpawnParticle(npc.WorldPosition);

						SpawnedNpcs.Add(npc);

						_ = SpawnSubNpcsWithDelay(npcChance.SubNpcPool, npcChance.SubNpcSpawnDelay);
					}
				}
			}
		}
	}

	private void SpawnBossNPCs()
	{
		// Spawne die Boss-NPCs
		foreach (var bossChance in BossNpcPool)
		{
			var random = Game.Random.Float(0f, 1f);
			var shouldSpawn = random <= bossChance.SpawnChance;
			if (shouldSpawn)
			{
				for (int i = 0; i < bossChance.SpawnCount; i++)
				{
					var boss = SpawnNpc(bossChance.Npc);
					if (boss != null)
					{
						var npcComponent = boss.GetComponent<Npc>();
						if (npcComponent != null)
						{
							npcComponent.Level = new Random().Next(bossChance.MinLevel, bossChance.MaxLevel + 1);
							npcComponent.SetHealthBasedOnLevel();
							var abilityRandom = new Random();
							npcComponent.HasIceAbility = bossChance.IceAbilityChance >= 1.0 || new Random().NextDouble() <= bossChance.IceAbilityChance;
							npcComponent.HasWindAbility = bossChance.WindAbilityChance >= 1.0 || new Random().NextDouble() <= bossChance.WindAbilityChance;
							npcComponent.HasFireAbility = bossChance.FireAbilityChance >= 1.0 || new Random().NextDouble() <= bossChance.FireAbilityChance;
						}
						SpawnedNpcs.Add(boss);

						// Setze die Variable zurück, wenn ein neuer Boss gespawnt wird
						allSubNpcsKilled = false;
						// Spawne die Sub-NPCs des Bosses mit einer Verzögerung
						_ = SpawnSubNpcsWithDelay(bossChance.SubNpcPool, bossChance.SubNpcSpawnDelay);
					}
				}
			}
			if (allSubNpcsKilled)
			{
				break;
			}
		}
	}

	private async Task SpawnSubNpcsWithDelay(List<SubNpcChance> subNpcPool, float delay)
	{
		do
		{
			await Task.Delay((int)TimeSpan.FromSeconds(delay).TotalMilliseconds);
			if (allSubNpcsKilled)
			{
				return;
			}
			foreach (var npcChance in subNpcPool)
			{
				var random = Game.Random.Float(0f, 1f);
				var shouldSpawn = random <= npcChance.SpawnChance;
				if (shouldSpawn)
				{
					for (int i = 0; i < npcChance.SpawnCount; i++)
					{
						var npc = SpawnNpc(npcChance.Npc);
						if (npc != null)
						{
							var npcComponent = npc.GetComponent<Npc>();
							if (npcComponent != null)
							{
								npcComponent.Level = new Random().Next(npcChance.MinLevel, npcChance.MaxLevel + 1);
								npcComponent.SetHealthBasedOnLevel();

								var abilityRandom = new Random();
								npcComponent.HasIceAbility = npcChance.IceAbilityChance >= 1.0 || new Random().NextDouble() <= npcChance.IceAbilityChance;
								npcComponent.HasWindAbility = npcChance.WindAbilityChance >= 1.0 || new Random().NextDouble() <= npcChance.WindAbilityChance;
								npcComponent.HasFireAbility = npcChance.FireAbilityChance >= 1.0 || new Random().NextDouble() <= npcChance.FireAbilityChance;
							}
							SpawnedNpcs.Add(npc);
							await Task.Delay((int)TimeSpan.FromSeconds(npcChance.SubNpcSpawnDelayPerNpc).TotalMilliseconds);
						}
					}
				}
			}
		} while (LoopSpawning);

		if (DestroyAfterSpawning)
		{
			GameObject.Destroy();
		}
	}



	private GameObject SpawnNpc(GameObject npcPrefab)
	{
		var tries = 0;
		while (tries <= 20)
		{
			var randomDirection = Rotation.FromYaw(Game.Random.Float(360f)).Forward;
			var randomPosition = WorldPosition + randomDirection * Game.Random.Float(Radius);
			var startPos = randomPosition.WithZ(WorldPosition.z + Height / 2f);
			var endPos = randomPosition.WithZ(WorldPosition.z - Height / 2f);
			var groundTrace = Game.ActiveScene.Trace.Ray(startPos, endPos)
				.Size(5f)
				.WithoutTags("player", "npc", "trigger")
				.Run();
			if (groundTrace.Hit && !groundTrace.StartedSolid)
			{
				if (Vector3.GetAngle(Vector3.Up, groundTrace.Normal) <= 60f)
				{
					var clone = npcPrefab.Clone(groundTrace.HitPosition, Rotation.FromYaw(Game.Random.Float(360f)));
					clone.NetworkMode = NetworkMode.Object;
					clone.NetworkSpawn();
					return clone;
				}
			}
			tries++;
		}
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
		var p = new SceneParticles(Scene.SceneWorld, "particles/impact.flesh.bloodpuff.vpcf");
		p.SetControlPoint(0, position);
		p.SetControlPoint(0, -1f);
		p.SetControlPoint(1, new Vector3(5.5f, 0.1f, 0.1f));
		p.PlayUntilFinished(Task);
	}
	



}