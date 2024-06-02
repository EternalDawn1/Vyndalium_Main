namespace GeneralGame;

[Group( "Interactables" )]
[Title( "Interactable" )]
public partial class Interactable : Component
{
	public delegate void InteractionDelegate( Player player, GameObject gameObject );
	[Property, Category( "Actions" )] public InteractionDelegate OnInteraction { get; set; }

	protected override void OnStart()
	{
		Tags.Add( "interactable" );
	}

	public void Interact( Player player )
	{
		OnInteraction?.Invoke( player, GameObject );
	}
}
