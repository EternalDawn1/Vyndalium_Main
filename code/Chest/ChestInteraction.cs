using GeneralGame;
using GeneralGame.HUD;
using Sandbox;
using System;
using Sandbox.UI;
using System.Collections.Generic;



public sealed class ChestInteraction : Component
{
    [Property] public Chest Chest { get; set; }

    [Property] public float InteractionRange { get; set; } = 100.0f;

    protected override void OnUpdate()
    {
        // Überprüfen, ob OnUpdate aufgerufen wird

        if (IsPlayerNearby())
        {
            Chest?.Highlight(true);

        }
        else
        {
            Chest?.Highlight(false);


        }
    }
    private bool IsPlayerNearby()
    {
        var players = Scene.GetAllComponents<Player>();
        if (players == null || !players.Any())
        {

            return false;
        }

        // Anzahl der erkannten Spieler loggen

        foreach (var player in players)
        {


            var distance = (player.Transform.Position - this.Transform.Position).Length;


            if (distance < InteractionRange)
            {
                return true;
            }
        }
        return false;
    }


}