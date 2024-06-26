using GeneralGame;
using GeneralGame.HUD;
using Sandbox;
using System;
using Sandbox.UI;
using System.Collections.Generic;
namespace GeneralGame.HUD;






public sealed class ChestInteraction : Component
{
    public Chest chest;
    public ChestPanel chestPanel;
    private bool isPlayerNear = true;

    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        // Aktualisiere den Zustand von isPlayerNear
        isPlayerNear = IsPlayerNearby();
        chest.Highlight( isPlayerNear );
    }

    public ChestInteraction( Chest chest )
    {
        this.chest = chest;
    }

    private bool IsPlayerNearby()
    {
        var players = Scene.GetAllComponents<Player>();
        foreach ( var player in players )
        {
            if ( (player.Transform.Position - this.Transform.Position).Length < chest.InteractionDistance )
            {
                return true;
            }
        }
        return false;
    }

    public void OnInteract( Player player )
    {
        if ( isPlayerNear )
        {
            if ( !chest.isOpen )
            {
                chest.Open();
                chestPanel.Show();
            }
        }
    }
}