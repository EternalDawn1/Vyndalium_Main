using GeneralGame.HUD;
using Sandbox.ui.Hud;

namespace GeneralGame;

public partial class Player : Component, Component.ExecuteInEditor
{
    public bool HasShirt
    {
        get
        {
            if ( !Inventory.IsValid() || Inventory == null ) return false;

            foreach ( var item in Inventory.EquippedItems )
            {
                if ( item is ItemEquipment equipped )
                {
                    if ( equipped.Slot == EquipSlot.Body )
                        return true;
                }
            }

            return false;
        }
    }
    public void BlackScreen( float startingTransition = 2f, float blackTransition = 2f, float endingTransition = 1f )
    {
        if ( IsProxy ) return;

        var gameObject = Hudmaster.Instance.GameObject;

        if ( gameObject == null ) return;

        var blackScreen = gameObject.Components.Create<Blackscreen>();
        blackScreen.StartingTransition = startingTransition;
        blackScreen.BlackTransition = blackTransition;
        blackScreen.EndingTransition = endingTransition;
        blackScreen.Start();
    }
    public void PlaySuccessSoundFromPath( string soundEventPath, float volume )
    {
        // SoundEvent anhand des Pfads laden
        var soundEvent = ResourceLibrary.Get<SoundEvent>( soundEventPath );
        if ( soundEvent != null )
        {
            // SoundEvent abspielen
            var soundHandle = Sound.Play( soundEvent );
            if ( soundHandle.IsValid() )
            {
                // Lautstärke und Position einstellen
                soundHandle.Volume = volume; // Lautstärke einstellen
                soundHandle.Position = Vector3.Zero; // Position auf (0,0,0) setzen
                soundHandle.ListenLocal = true; // Sound lokal abspielen
            }
            else
            {
                Log.Warning( $"SoundHandle für '{soundEventPath}' ist ungültig." );
            }
        }
        else
        {
            // Fehlerbehandlung, falls das SoundEvent nicht gefunden wird
            Log.Warning( $"SoundEvent '{soundEventPath}' konnte nicht gefunden werden." );
        }
    }



    public AmmoContainer AmmoContainer { get; set; }
    public Inventory Inventory { get; private set; }
    






}
