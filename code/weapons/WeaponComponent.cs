using Sandbox;
using Sandbox.Citizen;
using System;
using System.Numerics;
namespace GeneralGame;


public enum WeaponType
{
	Melee,
	Ranged
}


public class WeaponComponent : Component
{


	[Property] public string DisplayName { get; set; }
	[Property, Category( "Weapon Properties" )] public float DeployTime { get; set; } = 0.5f;
	[Property, Category( "Weapon Properties" )] public float DamageForce { get; set; } = 5f;
	[Property, Category( "Weapon Properties" )] public int Damage { get; set; } = 5;
	[Property, Category( "Weapon Properties" )] public float FireRate { get; set; } = 3f;
	[Property] public GameObject ViewModelPrefab { get; set; }
	[Property] public CitizenAnimationHelper.HoldTypes HoldType { get; set; } = CitizenAnimationHelper.HoldTypes.Pistol;
	[Property] public SoundEvent DeploySound { get; set; }
	[Property] public SoundEvent HolsterSound { get; set; }
	[Property] public bool IsDeployed { get; set; }
	[Property] public Vector3 IdlePos { get; set; }
	[Property] public Vector3 AimPos { get; set; }
	[Property] public Rotation AimRotation { get; set; }
	[Property] public Rotation RunRotation { get; set; }
	public bool HasViewModel => ViewModel.IsValid();
	public Player Owner { get; set; }
	public SkinnedModelRenderer ModelRenderer { get; set; }
	public ViewModel ViewModel { get; set; }
	public TimeUntil NextAttackTime { get; set; }
	public SkinnedModelRenderer EffectRenderer => ViewModel.IsValid() ? ViewModel.ModelRenderer : ModelRenderer;
	public EquipSlot Slot { get; set; }

	public bool IsInitialized { get; private set; }

	// Methode zum Initialisieren der Waffe, die auch IsInitialized setzt
	public void Initialize()
	{
		// Initialisierungslogik hier...

		// Nach erfolgreicher Initialisierung
		IsInitialized = true;
	}

	protected override void OnStart()
	{
		if ( !Owner.IsValid() ) return;
		if ( IsDeployed )
			OnDeployed();
		else
			OnHolstered();


		base.OnStart();


	}


	protected override void OnAwake()
	{
		ModelRenderer = Components.GetInDescendantsOrSelf<SkinnedModelRenderer>( true );
		base.OnAwake();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
	}

	protected override void OnDestroy()
	{
		if ( IsDeployed )
		{
			if ( ViewModel != null )
			{
				OnHolstered();
			}
			IsDeployed = false;
		}

		base.OnDestroy();
	}



	[Broadcast]
	public virtual void Deploy()
	{
		if ( !IsDeployed )
		{
			IsDeployed = true;

			OnDeployed();



		}
	}

	[Broadcast]
	public virtual void Holster()
	{
		if ( IsDeployed )
		{
			OnHolstered();

			IsDeployed = false;


		}


	}

	public virtual void PrimaryAction()
	{



	}
	public virtual void PrimaryActionRelease()
	{


	}

	public virtual void SecondaryAction()
	{
		var weapon = Player.Local.Weapons.Deployed;
		if ( weapon != null && weapon.IsValid() )
		{
			weapon.Holster();
		}

	}
	public virtual void SeccondaryActionRelease()
	{

	}


	public virtual void ReloadAction()
	{

	}


	protected virtual void OnDeployed()
	{
		var player = Components.GetInAncestors<Player>();
		var playerDresser = player.Components.Get<PlayerDresser>();
		if ( playerDresser != null )
		{
			playerDresser.RemoveClothing();
		}


		if ( player.IsValid() )
		{
			foreach ( var animator in player.Animators )
			{
				animator.TriggerDeploy();
			}
		}

		ModelRenderer.Enabled = !HasViewModel;

		if ( DeploySound is not null )
		{
			Sound.Play( DeploySound, Transform.Position );
		}

		if ( !IsProxy )
		{
			CreateViewModel();
		}

		NextAttackTime = DeployTime;
	}

	protected virtual void OnHolstered()
	{
		ModelRenderer.Enabled = false;
		var player = Components.GetInAncestors<Player>();
		if ( player != null )
		{
			var playerDresser = player.Components.Get<PlayerDresser>();
			if ( playerDresser != null )
			{
				playerDresser.RemoveClothing();
			}
		}
		else
		{
			Log.Error( "Spieler ist null in OnHolstered" );
		}

		DestroyViewModel();
	}

	private void DestroyViewModel()
	{
		if (ViewModel != null && ViewModel.GameObject != null)
    {
        ViewModel.GameObject.Destroy();
        ViewModel = null;
    }
	}

	public void CreateViewModel()
	{
		if ( !ViewModelPrefab.IsValid() )
			return;

		var player = Components.GetInAncestors<Player>();
		var character = player.Components.Get<Character>();
		if ( character != null )
		{
			character.CreatePreviewClothing( null );
		}

		var playerDresser = player.Components.Get<PlayerDresser>();
		if ( playerDresser != null )
		{
			playerDresser.RemoveClothing();
		}

		var viewModelGameObject = ViewModelPrefab.Clone();
		viewModelGameObject.SetParent( player.ViewModelRoot, false );

		ViewModel = viewModelGameObject.Components.Get<ViewModel>();
		ViewModel.SetWeaponComponent( this );
		ViewModel.SetCamera( player.PlyCamera );

		ModelRenderer.Enabled = false;
	}


}
