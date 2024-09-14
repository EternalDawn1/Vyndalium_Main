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



    public AmmoContainer AmmoContainer { get; set; }
    public Inventory Inventory { get; private set; }
    






}
