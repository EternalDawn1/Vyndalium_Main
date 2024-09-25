namespace GeneralGame;

public enum EquipSlot : byte
{
	Head,
	Face,
	Body,
	Legs,
	Feet,
	Hand,
	Back,
	Bracer,
	Belt
}

public enum HoldType : byte
{
	Idle,
	Rifle,
	FishingRod,
	Item,
	Flashlight,
	Melee
}

public class ItemEquipment : ItemComponent
{

	[Property, Category( "Equipment" )] public bool IsBackable { get; set; }
	[Property, Category( "Equipment" )] public EquipSlot Slot { get; set; } = EquipSlot.Hand;
	[Property, Category( "Equipment" )] public HiddenBodyGroup HideBodygroups { get; set; }
	[Property, Category( "Equipment" )] public bool UseSkinTint { get; set; }

	[Property, Category( "Holding" ), ShowIf( "Slot", EquipSlot.Hand )] public HoldType HoldType { get; set; } = HoldType.Item;
	[Property, Category( "Holding" )] public bool UpdatePosition { get; set; }
	[Property, Category( "Holding" ), ShowIf( "UpdatePosition", true )] public string Attachment { get; set; } = "hand_R";
	[Property, Category( "Holding" ), ShowIf( "UpdatePosition", true )] public Transform AttachmentTransform { get; set; } = global::Transform.Zero;

	public ModelRenderer Renderer { get; private set; }
	public WeaponComponent Weapon { get; private set; }

	
	

	

	private readonly SoundEvent _equipSound = ResourceLibrary.Get<SoundEvent>( "sounds/misc/pickup.sound" );

	public bool IsClothing => Slot != EquipSlot.Hand;
	public bool Equipped => State == ItemState.Equipped;



	

	

	

	protected override void OnStart()
	{
		base.OnStart();

		var interactions = Components.GetOrCreate<Interactions>();
		interactions.AddInteraction( new Interaction()
		{
			Identifier = $"item.equipped.{Name}",
			Action = ( Player interactor, GameObject obj ) => interactor.Inventory.EquipItemFromWorld( this ),
			Keybind = "use2",
			Description = "Equip",
			Stats = "Stats",
			Disabled = () => Player.Local.Inventory.IsSlotOccupied( Slot ),
			ShowWhenDisabled = () => true,
			Accessibility = AccessibleFrom.World,
			Sound = () => _equipSound,
		} );

		
		if ( Renderer != null ) Renderer.RenderType = ModelRenderer.ShadowRenderType.On;
	}

	protected override void OnPreRender()
	{
		if ( !Equipped || !UpdatePosition || !Game.IsPlaying || GameObject == Scene )
			return;

		var player = GameObject.Parent.Components.Get<Player>( true );
		if ( player == null )
			return;

		var obj = Renderer?.SceneObject;
		if ( !obj.IsValid() )
			return;

		var transform = player.GetAttachment( Attachment, true ).ToWorld( AttachmentTransform );
		obj.Transform = transform;
		(obj as SceneModel)?.Update( RealTime.Delta );
	}

	#region GIZMO STUFF
	private SceneModel _model;
	private SceneObject GetModel()
	{
		var world = Game.ActiveScene?.SceneWorld;
		if ( world == null )
			return null;

		_model ??= new SceneModel( world, "models/citizen/citizen.vmdl", global::Transform.Zero );
		_model.RenderingEnabled = false;
		return _model;
	}

	protected override void DrawGizmos()
	{
		var ignore = false;
		if ( !UpdatePosition || Attachment == string.Empty )
			ignore = true;

		if ( ignore || GameObject != Game.ActiveScene )
			ignore = true;

		if ( ignore || !Gizmo.HasSelected )
		{
			if ( _model != null )
				_model.RenderingEnabled = false;

			return;
		}

		var model = GetModel();
		if ( model == null )
			return;

		var renderer = Components.Get<ModelRenderer>( FindMode.EverythingInSelfAndDescendants );
		if ( renderer == null || renderer.Model == null )
			return;

		var attachment = _model.GetAttachment( Attachment ) ?? global::Transform.Zero;
		Gizmo.Draw.Model( renderer.Model, model.Transform );

		Gizmo.Draw.IgnoreDepth = true;
		Gizmo.Draw.SolidSphere( attachment.Position, 0.1f );
		Gizmo.Draw.IgnoreDepth = false;

		model.Transform = attachment.ToWorld( AttachmentTransform );

		using ( Gizmo.Scope( $"{Name}", new Transform( model.Position, model.Rotation ) ) )
		{
			Gizmo.Hitbox.DepthBias = 0.01f;

			if ( Gizmo.IsShiftPressed )
			{
				if ( Gizmo.Control.Rotate( "rotate", out var rotate ) )
					AttachmentTransform = AttachmentTransform.WithRotation( AttachmentTransform.Rotation * rotate.ToRotation() );

				return;
			}

			if ( Gizmo.Control.Position( "position", Vector3.Zero, out var pos ) )
				AttachmentTransform = AttachmentTransform.WithPosition( AttachmentTransform.Position + pos * AttachmentTransform.Rotation );
		}
	}
	#endregion
}
