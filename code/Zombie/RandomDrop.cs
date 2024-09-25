namespace GeneralGame;


public class RandomItemDrop : GameResource
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
           
            item.NetworkSpawn();
			Log.Info("hello");
            DroppedItems.Add(item);
        }
    }
}