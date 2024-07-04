using GeneralGame;
using GeneralGame.HUD;
using Sandbox;
using System;
using Sandbox.UI;
using System.Collections.Generic;

namespace GeneralGame;

public sealed class MasterInteraction : Component
{
    [Property] public ItemInteractable chest { get; set; }
    [Property] public float InteractionRange { get; set; } = 100.0f;

    public BaseInteractionHint Interaction { get; set; }
    public MasterInteraction()
    {
        // Initialisieren Sie Interaction hier, wenn es immer benötigt wird
        Interaction = new BaseInteractionHint();
    }

    protected override void OnUpdate()
    {
        if ( IsPlayerNearby() )
        {
            chest?.Highlight( true );

            if ( chest != null && chest.IsHighlighted && Input.Pressed( "use" ) )
            {
                chest.Open();
                if ( Interaction != null )
                {
                    Interaction.TogglePanel();

                }
                else
                {
                    Log.Error( "Interaction ist null. Stellen Sie sicher, dass es initialisiert wurde." );
                }
            }
        }
        else
        {
            chest?.Highlight( false );
        }
    }


    private bool IsPlayerNearby()
    {


        var players = Scene.GetAllComponents<Player>();
        if ( players == null || !players.Any() )
        {

            return false;
        }

        // Anzahl der erkannten Spieler loggen

        foreach ( var player in players )
        {


            var distance = (player.Transform.Position - this.Transform.Position).Length;



            if ( distance < InteractionRange )
            {

                return true;


            }
        }
        return false;
    }


}