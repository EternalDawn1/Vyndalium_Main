using GeneralGame.HUD;

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


    public AmmoContainer AmmoContainer { get; set; }
    public Inventory Inventory { get; private set; }



}
