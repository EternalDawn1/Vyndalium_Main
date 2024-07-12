

using GeneralGame.HUD;

namespace GeneralGame;

public enum InputMode
{
	[Icon( "mouse" )]
	Pressed,

	[Icon( "front_hand" )]
	Released,

	[Icon( "ads_click" )]
	Down
}

public enum InteractAnimations
{
	None,
	Interact,
	Action,
	Shoot,
	Reload
}

[Flags]
public enum AccessibleFrom
{
	None = 0,
	World = 1 << 0,
	Hands = 1 << 1,
	All = World | Hands,
}

public class Interaction
{
	/// <summary>
	/// Unique identifier for this interaction
	/// </summary>
	[Property, Category( "Required" )]
	public Signal Identifier { get; set; }

	/// <summary>
	/// The keybind used to trigger this interaction
	/// </summary>
	[Property, Category( "Required" )]
	[InputAction]
	public string Keybind { get; set; } = HUD.InputAction.Use;
	

	/// <summary>
	/// The UI description displayed when interacting
	/// </summary>
	[Property, Category( "Required" )]
	public string Description { get; set; } = "";

	[Property, Category( "Required" )]
	public string Stats { get; set; }



	/// <summary>
	/// The action that is performed when interacted with
	/// </summary>
	[Property, Category( "Required" )]
	public InteractionEvent Action { get; set; }
	public delegate void InteractionEvent( Player interactor, GameObject obj );

	/// <summary>
	/// The max distance you can use this interaction from
	/// </summary>
	[Property, Category( "Optional" )]
	public float InteractDistance { get; set; } = 125f;

	/// <summary>
	/// Where this interaction is accessible from
	/// </summary>
	[Property, Category( "Optional" )]
	public AccessibleFrom Accessibility { get; set; } = AccessibleFrom.All;

	/// <summary>
	/// Whether or not the interaction can be performed.
	/// </summary>
	[Property, Category( "Optional" )]
	public Func<bool> Disabled { get; set; } = () => false;

	/// <summary>
	/// If the interaction is disabled, we still show it in the list but with a disabled look.
	/// </summary>
	[Property, Category( "Optional" )]
	public Func<bool> ShowWhenDisabled { get; set; } = () => false;

	/// <summary>
	/// What the text should be displayed as
	/// </summary>
	[Property, Category( "Optional" )]
	public Func<string> DynamicText { get; set; }


	/// <summary>
	/// The color the text will use
	/// </summary>
	[Property, Category( "Optional" )]
	public Func<Color> DynamicColor { get; set; }

	/// <summary>
	/// Does this interaction have a cooldown
	/// </summary>
	[Property, Category( "Optional" )]
	public bool Cooldown { get; set; } = false;

	/// <summary>
	/// How much the cooldown lasts
	/// </summary>
	[Property, Category( "Optional" ), ShowIf( "Cooldown", true )]
	[Range( 0.1f, 5f, 0.1f )]
	public float CooldownTime { get; set; } = 0.5f;

	/// <summary>
	/// Does this interaction use bounds?
	/// </summary>
	[Property]
	public bool HasBounds { get; set; } = false;

	/// <summary>
	/// The position of this interaction, if it even has them
	/// </summary>
	[Property, ShowIf( "HasBounds", true )]
	public Vector3 Position { get; set; }

	/// <summary>
	/// The extents of this interaction, if it even has them
	/// </summary>
	[Property, ShowIf( "HasBounds", true )]
	public Vector3 Extents { get; set; }

	[Property, Category( "Optional" )]
	public InteractAnimations Animation { get; set; } = InteractAnimations.Interact;

	/// <summary>
	/// The sound to play when this interaction occurs.
	/// </summary>
	[Property, Category( "Optional" )]
	public Func<SoundEvent> Sound { get; set; }

	/// <summary>
	/// The sound will play from the player instead of the interaction item.
	/// </summary>
	[Property, Category( "Optional" )]
	public bool PlaySoundFromPlayer { get; set; }

	/// <summary>
	/// What context should the input be called in
	/// </summary>
	[Property, Category( "Optional" )]
	public InputMode InputMode { get; set; } = InputMode.Pressed;

	/// <summary>
	/// The text that should actually be displayed.
	/// </summary>
	[Hide, JsonIgnore]
	public string Text => DynamicText?.Invoke() ?? Description;

	/// <summary>
	/// The color that should actually be displayed.
	/// </summary>
	[Hide, JsonIgnore]
	public Color Color => DynamicColor?.Invoke() ?? Color.White;

	[Hide, JsonIgnore]
	public TimeUntil CooldownTimer;

	/// <summary>
	/// Uses the interactions input mode and gets the key state by input action
	/// </summary>
	/// <param name="action"></param>
	/// <returns></returns>
	public bool InputFunction( string action )
	{
		switch ( InputMode )
		{
			case InputMode.Pressed:
				return Input.Pressed( action );
			case InputMode.Down:
				return Input.Down( action );
			case InputMode.Released:
				return Input.Pressed( action );
		};

		return false;
	}
}

public class Interactions : Component
{
	public float InteractDistance { get; set; } = 125f;
	[Property]
	public List<Interaction> ObjectInteractions { get; set; }

	[Property]
	public bool HideOnEmpty { get; set; } = false;

	public IEnumerable<Interaction> AllInteractions => ObjectInteractions.Concat( programmedInteractions ?? new List<Interaction>() );

	private List<Interaction> programmedInteractions;

	public void AddInteraction( Interaction interaction )
	{
		programmedInteractions ??= new();
		programmedInteractions.Add( interaction );
	}

	public void AddInteractions( List<Interaction> interactions )
	{
		programmedInteractions ??= new();
		programmedInteractions.AddRange( interactions );
	}

	protected override void OnAwake()
	{
		ObjectInteractions ??= new();
	}

	protected override void DrawGizmos()
	{
		var interactions = ObjectInteractions;
		if ( interactions == null )
			return;

		for ( int i = 0; i < interactions.Count; i++ )
		{
			var interaction = interactions[i];
			if ( !interaction.HasBounds )
				continue;

			var bbox = new BBox( interaction.Position - interaction.Extents / 2, interaction.Position + interaction.Extents / 2 );
			Gizmo.Draw.Color = Color.Yellow;
			Gizmo.Draw.LineThickness = 0.5f;
			Gizmo.Draw.LineBBox( bbox );

			Gizmo.Draw.Color = Color.White;
			Gizmo.Draw.Text( $"{interaction.Description}", new Transform( interaction.Position ), "Consolas", 12 );

			if ( !Gizmo.HasSelected || GameObject != Game.ActiveScene )
				continue;

			using ( Gizmo.Scope( $"{interaction.Description}", new Transform( interaction.Position ) ) )
			{
				Gizmo.Hitbox.BBox( bbox );
				Gizmo.Hitbox.DepthBias = 0.01f;

				if ( Gizmo.IsShiftPressed )
				{
					if ( Gizmo.Control.Scale( "scale", Vector3.Zero, out var scale ) )
					{
						interaction.Extents += scale * 50;
						ObjectInteractions[ObjectInteractions.IndexOf( interaction )] = interaction;
					}
					continue;
				}

				if ( Gizmo.Control.Position( "position", Vector3.Zero, out var pos ) )
				{
					interaction.Position += pos;
					ObjectInteractions[ObjectInteractions.IndexOf( interaction )] = interaction;
				}
			}
		}
	}

	public void Highlight( bool shouldHighlight )
	{
		if ( IsProxy )
			return;

		var chestObject = this; // Direkte Nutzung des aktuellen Objekts
		var outline = chestObject.GameObject.Components.Get<HighlightOutline>();

		// Überprüfen, ob der aktuelle Highlight-Zustand sich vom gewünschten Zustand unterscheidet
		bool isCurrentlyHighlighted = outline != null;
		if ( shouldHighlight == isCurrentlyHighlighted )
		{
			// Zusätzliche Überprüfung, ob die Eigenschaften des Outline-Objekts bereits den gewünschten Werten entsprechen
			if ( outline != null && outline.Color == Color.White && outline.Width == 0.5f && outline.ObscuredColor == Color.White )
			{
				// Keine Änderung notwendig, da der gewünschte Zustand bereits erreicht ist
				return;
			}
		}

		if ( shouldHighlight )
		{
			if ( outline == null )
			{
				outline = chestObject.GameObject.Components.Create<HighlightOutline>();
			}
			// Setzen oder Aktualisieren der Eigenschaften des Outline-Objekts
			outline.Color = Color.White;
			outline.Width = 0.5f;
			outline.ObscuredColor = Color.White;
		}
		else
		{
			if ( outline != null && shouldHighlight == false)
			{
				outline.Destroy();
			}
		}
	}
	public bool IsHighlighted()
	{
		var outline = this.GameObject.Components.Get<HighlightOutline>();
		return outline != null;
	}
	protected override void OnUpdate()
	{
		
		if (  IsProxy )
		{
			Highlight( true );

		}
		else
		{
			Highlight( false );
			
		}
	}

	
	private bool IsPlayerNearby()
	{
		if(IsProxy)
		{
			return false;
		}
		

		var players = Scene.GetAllComponents<Player>();
		if ( players == null || !players.Any() )
		{

			return false;
		}

		// Anzahl der erkannten Spieler loggen

		foreach ( var player in players )
		{


			var distance = (player.Transform.Position - this.Transform.Position).Length;



			if ( distance < InteractDistance )
			{

				return true;


			}
		}
		return false;
	}

}
