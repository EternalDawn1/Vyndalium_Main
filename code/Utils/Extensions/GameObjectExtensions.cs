namespace GeneralGame;

public static class GameObjectExtensions
{
	public static void SetupNetworking(
    this GameObject obj,
    OwnerTransfer transfer = OwnerTransfer.Takeover,
	NetworkOrphaned orphaned = NetworkOrphaned.ClearOwner )
	{
		obj.NetworkMode = NetworkMode.Object;

		if ( !obj.Network.Active )
			obj.NetworkSpawn();

		obj.Network.SetOwnerTransfer( transfer );
		obj.Network.SetOrphanedMode( orphaned );
	}
	public static IEnumerable<Interaction> GetInteractions( this GameObject obj )
	{
		return obj.Components.Get<Interactions>( FindMode.EverythingInSelf )?.AllInteractions;
	}
}
