using Sandbox;
using System.Linq;

namespace GeneralGame;
public sealed class Fists : BaseMeleeWeapon
{
	[Property] public SkinnedModelRenderer fists { get; set; }
	[Property] public GameObject ViewModelCamera { get; set; }
	public Player playerController { get; set; }
	

	protected override void OnStart()
	{
		fists.Set( "b_attack", false );
		fists.Set( "b_deploy", true );
		playerController = Scene.GetAllComponents<Player>().FirstOrDefault( x => !x.IsProxy );
	}

	protected override void OnUpdate()
	{
		if ( !IsProxy )
		{
			if ( Input.Pressed( "attack1" ) )
			{
				PrimaryAction();
			}
			UpdateAnimations();
		}
		if ( IsProxy )
		{
			ViewModelCamera.Enabled = false;
		}
	}

	protected override void OnEnabled()
	{
		if ( !IsProxy )
		{
			fists.Set( "b_deploy", true );
		}
	}

	protected override void OnDisabled()
	{
		// Implementiere die Logik für das Deaktivieren der Fäuste
	}

	void UpdateAnimations()
	{
		// Implementiere die Logik für das Aktualisieren der Animationen
	}
}