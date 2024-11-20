using Sandbox;
using GeneralGame;

[Icon("groups")]
public sealed class NpcSpawnArea : Component
{
	public enum GizmoType
	{
		Sphere,
		Box,
		Cylinder,

		Capsule,
		Cone,
		Arrow,
		Line,
		

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

		public NpcChance() { }
	}

	/// <summary>
	/// Gizmo
	/// </summary>

	[Property , Group("Gizmo")]
	public float PlayerProximityDistance { get; set; } = 1000f;

	[Property , Group("Gizmo")]
	public bool DrawProximityRangeGizmo { get; set; }

	[Property , Group("Gizmo")]
	public GizmoType GizmoShape { get; set; } = GizmoType.Sphere;

	[Property , Group("Gizmo")]
	public Color GizmoColor { get; set; } = Color.Red.WithAlpha(0.3f);
	[Property, Group("Gizmo")]
	public bool FillGizmo { get; set; } = false;

	[Property]
	private bool hasSpawnedNPCs = false;

	/// <summary>
	/// SpawnRange
	/// </summary>

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
	public List<NpcChance> NpcPool { get; set; }

	[JsonIgnore]
	[Sync]
	public NetList<GameObject> SpawnedNpcs { get; set; } = new();

	protected override void DrawGizmos()
	{
		base.DrawGizmos();
		var draw = Gizmo.Draw;
		draw.LineCylinder(Vector3.Down * Height / 2f, Vector3.Up * Height / 2f, Radius, 15);
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
						Gizmo.Draw.LineCylinder(Vector3.Zero, Vector3.Up * PlayerProximityDistance, PlayerProximityDistance, 15);
					break;
				case GizmoType.Capsule:
					if (FillGizmo)
						Gizmo.Draw.SolidCapsule(Vector3.Zero, Vector3.Up * PlayerProximityDistance, PlayerProximityDistance, 15, 15);
					else
						Gizmo.Draw.LineCapsule(Vector3.Zero, Vector3.Up * PlayerProximityDistance, PlayerProximityDistance, 15);
					break;
				case GizmoType.Cone:
					if (FillGizmo)
						Gizmo.Draw.SolidCone(Vector3.Zero, Vector3.Up, PlayerProximityDistance, PlayerProximityDistance, 15);
					else
						Gizmo.Draw.LineCone(Vector3.Zero, Vector3.Up, PlayerProximityDistance, PlayerProximityDistance);
					break;
				case GizmoType.Arrow:
					Gizmo.Draw.Arrow(Vector3.Zero, Vector3.Up * PlayerProximityDistance, PlayerProximityDistance);
					break;
				case GizmoType.Line:
					Gizmo.Draw.Line(Vector3.Zero, Vector3.Up * PlayerProximityDistance);
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
		foreach (var npcChance in NpcPool)
		{
			var random = Game.Random.Float(0f, 1f);
			var shouldSpawn = random <= npcChance.SpawnChance;
			if (shouldSpawn)
			{
				for (int i = 0; i < npcChance.SpawnCount; i++)
				{
					var tries = 0;
					while (tries <= 20) // Erhöhe die Anzahl der Versuche
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
								var clone = npcChance.Npc.Clone(groundTrace.HitPosition, Rotation.FromYaw(Game.Random.Float(360f)));
								clone.NetworkMode = NetworkMode.Object;
								clone.NetworkSpawn();
								if (clone != null)
								{
									SpawnedNpcs.Add(clone);
								}
								break;
							}
						}
						tries++;
					}
				}
			}
		}
	}

	public void RemoveNPCs()
	{
		foreach (var npc in SpawnedNpcs)
		{
			npc?.Destroy();
		}
	}
}