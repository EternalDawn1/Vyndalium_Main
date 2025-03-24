using Sandbox;
using GeneralGame;

[Icon("category")]
public sealed class ItemSpawnArea : Component
{
    public struct ItemAmount
    {
        [Property]
        [JsonInclude]
        public GameObject Item { get; set; }

        [Property]
        [JsonInclude]
        [Range(1, 100, 1)]
        public float AmountToSpawn { get; set; } = 5;

        [Property]
        [JsonInclude]
        public bool StartFrozen { get; set; } = true;

        [Property]
        [JsonInclude]
        [Range(0.01f, 1f, 0.01f)]
        public float SpawnChance { get; set; } = 0.2f;

        public ItemAmount() { }
    }

    [Property]
    public float Radius { get; set; }

    [Property]
    public float Height { get; set; }

    [Property]
    [JsonInclude]
    public List<ItemAmount> ItemPool { get; set; }

    public struct ItemPosition
    {
        public Vector3 SpawnPosition;
        public GameObject Item;

        public ItemPosition() { }
        public ItemPosition(Vector3 position, GameObject item)
        {
            SpawnPosition = position;
            Item = item;
        }
    }

    [JsonIgnore]
    [Sync]
    public NetList<ItemPosition> SpawnedItems { get; set; } = new();

    protected override void DrawGizmos()
    {
        base.DrawGizmos();

        var draw = Gizmo.Draw;

        draw.LineCylinder(Vector3.Down * Height / 2f, Vector3.Up * Height / 2f, Radius, Radius, 15);
    }

    protected override void OnStart()
    {
        SpawnItems();
    }
    [Rpc.Broadcast( NetFlags.SendImmediate )]
    public void SpawnItems()
    {
        RemoveItems();

        Log.Info($"Spawning items in area: {WorldPosition}, Radius: {Radius}, Height: {Height}");

        foreach (var itemAmount in ItemPool)
        {
            var random = Game.Random.Float(0f, 1f);
            var shouldSpawn = random <= itemAmount.SpawnChance;

            Log.Info($"Trying to spawn item: {itemAmount.Item}, SpawnChance: {itemAmount.SpawnChance}, Random: {random}, ShouldSpawn: {shouldSpawn}");

            if (shouldSpawn)
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

                    Log.Info($"Try {tries}: StartPos = {startPos}, EndPos = {endPos}, Hit = {groundTrace.Hit}, StartedSolid = {groundTrace.StartedSolid}");

                    if (groundTrace.Hit && !groundTrace.StartedSolid)
                    {
                        if (Vector3.GetAngle(Vector3.Up, groundTrace.Normal) <= 60f)
                        {
                            var clone = itemAmount.Item.Clone(groundTrace.HitPosition, Rotation.FromYaw(Game.Random.Float(360f)));
                            clone.NetworkMode = NetworkMode.Object;
                            clone.NetworkSpawn();

                            if (clone != null)
                            {
                                SpawnedItems.Add(new ItemPosition(groundTrace.HitPosition, clone));
                                Log.Info($"Successfully spawned item: {clone} at position {groundTrace.HitPosition}");
                            }
                            else
                            {
                                Log.Warning($"Failed to spawn item: {itemAmount.Item}");
                            }

                            break;
                        }
                    }

                    tries++;
                }
            }
        }
    }

    [Rpc.Broadcast( NetFlags.SendImmediate )]
    public void RemoveItems()
    {
        foreach (var item in SpawnedItems)
        {
            item.Item?.Destroy();
        }
    }
}