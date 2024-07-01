namespace GeneralGame;
using GeneralGame.HUD;
using Sandbox;



public class Chest : BaseChest
{




    protected override void OnAwake()
    {
        base.OnAwake();
        OnOpen += HandleOpen; // Ereignisabonnent hinzufügen
        OnClose += HandleClose;

    }
    protected override void OnStart()
    {
        base.OnStart();


    }


    private void HandleOpen()
    {
        // Logik für das Öffnen der Truhe, z.B. visuelles Feedback
        Highlight( true );
    }

    private void HandleClose()
    {
        // Logik für das Schließen der Truhe, z.B. visuelles Feedback entfernen
        Highlight( false );
    }

    public void Highlight( bool shouldHighlight )
    {
        isHighlighted = shouldHighlight;
        var chestObject = this; // Direkte Nutzung des aktuellen Objekts
        var outline = chestObject.GameObject.Components.Get<HighlightOutline>();
        if ( shouldHighlight )
        {
            if ( outline == null )
            {
                outline = chestObject.GameObject.Components.Create<HighlightOutline>();
                outline.Color = Color.White;
                outline.Width = 1.3f;

            }
        }
        else
        {
            if ( outline != null )
            {
                outline.Destroy();

            }
        }
    }





}