using Sandbox;
namespace GeneralGame.HUD;


public static class InputAction
{
	public const string LeftClick = "Mouse1";
	public const string RightClick = "Mouse2";
	public const string Jump = "Jump";
	public const string Duck = "Duck";
	public const string Walk = "Walk";
	public const string Use = "Use";
	public const string Sprint = "Sprint";

	public const string Voice = "Voice";
	public const string Inventory = "Inventory";
	public const string Character = "Character";
	public const string Abilities = "Abilities";
	
	
	public const string Shop = "Shop";
	public static bool Pressed( string action )
	{
		return Sandbox.Input.Pressed( action );
	}
	public const string Interaction = "Interaction";
}
