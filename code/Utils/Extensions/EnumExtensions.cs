namespace GeneralGame;

public static class EnumExtensions
{
	public static string GetIcon( this EquipSlot slot )
	{
		var path = "/ui/hud/" + slot switch
		{
			EquipSlot.Head => "clothes_hat.png",
			EquipSlot.Face => "clothes_face.png",
			EquipSlot.Body => "clothes_torso.png",
			EquipSlot.Legs => "clothes_legs.png",
			EquipSlot.Feet => "clothes_boots.png",
			EquipSlot.Hand => "hand_slot.png",
			EquipSlot.Bracer => "clothes_bracer.png",
			EquipSlot.Belt => "clothes_belt.png", 
			EquipSlot.Back => "clothes_back.png", 
			_ => ""
		};

		return path;
	}
}
