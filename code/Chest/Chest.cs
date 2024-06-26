namespace GeneralGame;
using GeneralGame.HUD;
using Sandbox;



public class Chest : BaseChest
{
    public Vector3 Position { get; set; }
    public float InteractionDistance { get; set; } = 50.0f;
    public ChestPanel chestPanel;
    public ChestInteraction chestInteraction;
    public Material HighlightMaterial;
    protected override void OnAwake()
    {
        base.OnAwake();

        chestInteraction = new ChestInteraction( this );
    }
    protected override void OnStart()
    {
        base.OnStart();

    }

    public void Highlight( bool shouldHighlight )
    {
        var chestObject = GameObject.Components.Get<Chest>();
        if ( chestObject != null )
        {
            var outline = chestObject.GameObject.Components.Get<HighlightOutline>(); // Korrigiert von GameObject.Components.Get<HighlightOutline>();
            if ( outline == null )
            {
                outline = chestObject.GameObject.Components.Create<HighlightOutline>(); // Korrigiert von GameObject<HighlightOutline>();

                outline.Color = Color.White;
                outline.Width = 2f;
            }
            outline.Enabled = shouldHighlight;
        }
    }



}