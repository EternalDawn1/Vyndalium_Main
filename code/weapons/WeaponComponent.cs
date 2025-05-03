using Sandbox;
using Sandbox.Citizen;
using System;
using System.Numerics;
namespace GeneralGame;





public class WeaponComponent : Component
{

	

	
	[Property] public string DisplayName { get; set; }
	[Property] public bool IsMelee { get; set; } = false;
	[Property, Category( "Weapon Properties" )] public float DeployTime { get; set; } = 0.5f;
	[Property, Category( "Weapon Properties" )] public float DamageForce { get; set; } = 5f;
	[Property, Category( "Weapon Properties" ), Feature( "Weapon Properties" )] public int Damage { get; set; } = 5;
	[Property, Category( "Weapon Properties" ) ,Feature("Weapon Properties")] public float FireRate { get; set; } = 18f;
	[Property] public GameObject ViewModelPrefab { get; set; }

	[Property] public SoundEvent DeploySound { get; set; }
	[Property] public SoundEvent HolsterSound { get; set; }
	[Property] public bool IsDeployed { get; set; } = false;
	[Property] public Vector3 IdlePos { get; set; }
	[Property] public Vector3 AimPos { get; set; }
	[Property]public Rotation CurRot { get; set; }
	[Property] public Rotation AimRotation { get; set; }
	[Property] public Rotation RunRotation { get; set; }
	[Property] public Rotation AimRotationOffset { get; set; }

	public bool HasViewModel => ViewModel.IsValid() ;
	public Player Owner { get; set; }
	public SkinnedModelRenderer ModelRenderer { get; set; }
	public ViewModel ViewModel { get; set; }
	public TimeUntil NextAttackTime { get; set; }
	public SkinnedModelRenderer EffectRenderer => ViewModel.IsValid() ? ViewModel.ModelRenderer : ModelRenderer;
	public EquipSlot Slot { get; set; }
	

	public bool IsInitialized { get; private set; }

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( !Owner.IsValid() || !ModelRenderer.IsValid() )
			return;

		
	}
	protected override void OnStart()
	{
		if ( Player.Local?.LifeState == LifeState.Dead )
		{
			return;
		}

		// Cache ModelRenderer nur einmal
		ModelRenderer ??= Components.GetInDescendantsOrSelf<SkinnedModelRenderer>( true );

		if ( !Owner.IsValid() ) return;

		// Hier die Änderung - automatisch deployen, wenn die Waffe aktiviert wird
		if ( GameObject.Active && !IsDeployed )
		{
			IsDeployed = true;
			OnDeployed();
		}
		else if ( IsDeployed )
		{
			OnDeployed();
		}
		else
		{
			OnHolstered();
		}

		base.OnStart();
	}

	protected override void OnAwake()
	{
		// Cache ModelRenderer und Owner nur einmal
		ModelRenderer ??= Components.GetInDescendantsOrSelf<SkinnedModelRenderer>(true);
		Owner ??= Components.GetInAncestors<Player>();

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
		Log.Info( "OnDeployed called" );
		
		if ( ModelRenderer == null || Owner == null )
		{
			Log.Error( "ModelRenderer or Owner is null in OnDeployed" );
			return;
		}

		Owner.Components.Get<PlayerDresser>()?.RemoveClothing();

		 if (Owner.IsValid() && Owner.Animators != null)
		{
			foreach ( var animator in Owner.Animators )
			{
				animator.TriggerDeploy();
				
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
		player?.Components.Get<PlayerDresser>()?.RemoveClothing();

		DestroyViewModel();
	}

	public void DestroyViewModel()
	{
		if ( ViewModel?.GameObject != null && ViewModel.IsValid() )
		{
			ViewModel.GameObject.Destroy();
			ViewModel = null;
		}
	}

	public void CreateViewModel()
	{
		
		if ( IsProxy || !ViewModelPrefab.IsValid() )
		{

			return;
		}
		

		var player = Components.GetInAncestors<Player>();
		if ( player == null ) return;

		if ( player.CameraMode != 0 ) // 0 = First-Person
		{
			
			return;
		}

		player.Components.Get<Character>()?.CreatePreviewClothing( null );

		var viewModelGameObject = ViewModelPrefab.Clone();
		if ( viewModelGameObject == null )
		{
			
			return;
		}

		viewModelGameObject.SetParent( player.ViewModelRoot, false );

		ViewModel = viewModelGameObject.Components.Get<ViewModel>();
		if ( ViewModel == null )
		{
			return;
		}

		ViewModel.SetWeaponComponent( this );
		ViewModel.SetCamera( player.PlyCamera );

		ModelRenderer.Enabled = false;
	}


}
