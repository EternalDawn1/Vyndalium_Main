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

    
  

    protected override void OnUpdate()
    {
        if ( IsPlayerNearby() )
        {
            chest?.Highlight( true );
            


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