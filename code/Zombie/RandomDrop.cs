namespace GeneralGame;


public class RandomItemDrop : Component
{
    [Property] public List<GameObject> RandomItems { get; set; } = new();
    public List<GameObject> DroppedItems { get; private set; } = new();

    public void DropRandomItem()
    {
        if (RandomItems.Count == 0) return;
        var randomItem = Game.Random.FromList(RandomItems);

        if (randomItem != null)
        {
            var item = randomItem.Clone();
            item.Transform.Position = Transform.Position + Vector3.Up * 20;
            item.Transform.Rotation = Transform.Rotation;
            item.NetworkSpawn();
			Log.Info("hello");
            DroppedItems.Add(item);
        }
    }
}