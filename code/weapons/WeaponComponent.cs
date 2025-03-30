using Sandbox;
using Sandbox.Citizen;
using System;
using System.Numerics;
namespace GeneralGame;


public enum WeaponType
{
	Melee,
	Ranged,
	M4A1,
	AK,
	MP5,

}


public class WeaponComponent : Component
{

	
	public WeaponType WeaponType { get; set; }
	
	[Property] public string DisplayName { get; set; }
	[Property, Category( "Weapon Properties" )] public float DeployTime { get; set; } = 0.5f;
	[Property, Category( "Weapon Properties" )] public float DamageForce { get; set; } = 5f;
	[Property, Category( "Weapon Properties" ), Feature( "Weapon Properties" )] public int Damage { get; set; } = 5;
	[Property, Category( "Weapon Properties" ) ,Feature("Weapon Properties")] public float FireRate { get; set; } = 18f;
	[Property] public GameObject ViewModelPrefab { get; set; }
	[Property] public CitizenAnimationHelper.HoldTypes HoldType { get; set; } = CitizenAnimationHelper.HoldTypes.Pistol;
	[Property] public SoundEvent DeploySound { get; set; }
	[Property] public SoundEvent HolsterSound { get; set; }
	[Property] public bool IsDeployed { get; set; } = false;
	[Property] public Vector3 IdlePos { get; set; }
	[Property] public Vector3 AimPos { get; set; }
	[Property]public Rotation CurRot { get; set; }
	[Property] public Rotation AimRotation { get; set; }
	[Property] public Rotation RunRotation { get; set; }
	[Property] public Rotation AimRotationOffset { get; set; }

	public bool HasViewModel => ViewModel.IsValid();
	public Player Owner { get; set; }
	public SkinnedModelRenderer ModelRenderer { get; set; }
	public ViewModel ViewModel { get; set; }
	public TimeUntil NextAttackTime { get; set; }
	public SkinnedModelRenderer EffectRenderer => ViewModel.IsValid() ? ViewModel.ModelRenderer : ModelRenderer;
	public EquipSlot Slot { get; set; }
	

	public bool IsInitialized { get; private set; }

	// Methode zum Initialisieren der Waffe, die auch IsInitialized setzt
	


	protected override void OnStart()
	{
		if(Player.Local == null)
		{
			return;
		}
		if(Player.Local.LifeState == LifeState.Dead)
		{
			return;
		}
		ModelRenderer = Components.GetInDescendantsOrSelf<SkinnedModelRenderer>( true );

		if ( !Owner.IsValid() ) return;

		if ( IsDeployed )
		{
			OnDeployed();
		}
		else
		{
			OnHolstered();
			ModelRenderer?.Set( "b_holster", true );
		}

		base.OnStart();
	}


	protected override void OnAwake()
	{
		ModelRenderer = Components.GetInDescendantsOrSelf<SkinnedModelRenderer>( true );

		Owner = Components.GetInAncestors<Player>();
		

		base.OnAwake();
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



	[Rpc.Broadcast]
	public virtual void Deploy()
	{
		if ( !IsDeployed )
		{
			IsDeployed = true;

			OnDeployed();



		}
	}

	[Rpc.Broadcast]
	public virtual void Holster()
	{
		
		if ( IsDeployed )
		{
			
			OnHolstered();

			IsDeployed = false;
			

		}


	}
	public readonly WeaponContainer weaponcontainer;
	[Rpc.Broadcast]
	public virtual void RemoveWeaponComponents(ItemComponent item)
	{
		weaponcontainer.RemoveWeapon( item.GameObject, true );
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
		if ( ModelRenderer == null )
		{
			Log.Error( "ModelRenderer is null in OnDeployed" );
			return;
		}

		if ( Owner == null )
		{
			Log.Error( "Player is null in OnDeployed" );
			return;
		}

		var playerDresser = Owner.Components.Get<PlayerDresser>();
		if ( playerDresser != null )
		{
			playerDresser.RemoveClothing();
		}

		if ( Owner.IsValid() )
		{
			if ( Owner.Animators != null )
			{
				foreach ( var animator in Owner.Animators )
				{
					animator.TriggerDeploy();
				}
			}
			else
			{
				Log.Error( "Player animators are null in OnDeployed" );
			}
		}

		ModelRenderer.Enabled = !HasViewModel;

		if ( DeploySound != null )
		{
			Sound.Play( DeploySound, WorldPosition );
		}

		if ( !IsProxy )
		{
			CreateViewModel();
		}

		ModelRenderer.Set( "b_deploy", true );

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
			
		}

		DestroyViewModel();
	}

	public void DestroyViewModel()
	{
		if (ViewModel != null && ViewModel.GameObject != null && ViewModel.IsValid())
		{
			
			ViewModel.GameObject.Destroy();
			ViewModel = null;
		}
	}

	public void CreateViewModel()
	{
		if ( IsProxy )
		{
			return;
		}
		if ( !ViewModelPrefab.IsValid() )
		{
			Log.Error( "ViewModelPrefab is not valid in CreateViewModel" );
			return;
		}

		var player = Components.GetInAncestors<Player>();
		if ( player == null )
		{
			
			return;
		}

		var character = player.Components.Get<Character>();
		if ( character != null )
		{
			character.CreatePreviewClothing( null );
		}

		var playerDresser = player.Components.Get<PlayerDresser>();
		if ( playerDresser != null )
		{
			//playerDresser.RemoveClothing();
			//playerDresser.Destroy();
		}
		

		var viewModelGameObject = ViewModelPrefab.Clone();
		if ( viewModelGameObject == null )
		{
			Log.Error( "ViewModelPrefab.Clone() returned null in CreateViewModel" );
			return;
		}

		viewModelGameObject.SetParent( player.ViewModelRoot, false );

		ViewModel = viewModelGameObject.Components.Get<ViewModel>();
		if ( ViewModel == null )
		{
			Log.Error( "ViewModel is null in CreateViewModel" );
			return;
		}

		ViewModel.SetWeaponComponent( this );
		ViewModel.SetCamera( player.PlyCamera );

		ModelRenderer.Enabled = false;
	}


}
