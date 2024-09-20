using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace GeneralGame;


public partial class WeaponContainer : Component
{
	[Property] public PrefabScene StartingWeapon { get; set; }
	[Property] public GameObject WeaponBone { get; set; }
	[Property] public AmmoContainer Ammo { get; set; }
	[Property] public Player PlayrControl { get; set; }
	[Property] public Inventory Inventory { get; set; }
	public BaseGun Equipped { get; set; }


	private WeaponComponent _deployed;

	public WeaponComponent Deployed => _deployed;
	

	protected override void OnUpdate()
	{
		UpdateDeployedStatus();
	}

	private void UpdateDeployedStatus()
	{
		_deployed = Components.GetAll<WeaponComponent>( FindMode.EverythingInSelfAndDescendants ).FirstOrDefault( w => w.IsDeployed );
							  
	}

	public IEnumerable<WeaponComponent> All => Components.GetAll<WeaponComponent>( FindMode.EverythingInSelfAndDescendants );
	public bool HasAny => All.Any();

	public bool Has( GameObject prefab )
	{

		return All.Any( w => w.GameObject.Components.GetInDescendantsOrSelf<WeaponComponent>( true ).DisplayName == prefab.Components.GetInDescendantsOrSelf<WeaponComponent>( true ).DisplayName );
	}



	protected override void OnAwake()
	{
		var player = Player.Local;
		var weapon = Equipped;

		if ( weapon != null && player != null )
		{
			// Initialisieren Sie die Ammo-Eigenschaft des Players
			player.InitializeAmmo();

			var ammoToGive = player.Ammo.Get( weapon.AmmoType );
			if ( ammoToGive > 0 )
			{
				var ammoToAdd = Math.Min( ammoToGive, weapon.MaxAmmo - weapon.DefaultAmmo );
				if ( weapon.DefaultAmmo < weapon.MaxAmmo )
				{
					weapon.DefaultAmmo += ammoToAdd;
					player.Ammo.TryTake( weapon.AmmoType, ammoToAdd, out var taken );
				}
			}

			if ( weapon.AmmoInClip < weapon.ClipSize )
			{
				weapon.AmmoInClip = weapon.ClipSize;
			}
		}
		
	}



	public void Clear()
	{
		if ( IsProxy ) return;

		foreach ( var weapon in All )
		{
			weapon.GameObject.Destroy();
		}
	}

	public void GiveDefault()
	{
		if ( IsProxy ) return;

		// Überprüfen Sie, ob ein Item im EquipSlot vorhanden ist
		var equippedItem = Inventory.GetItemInSlot( EquipSlot.Hand ); // Ersetzen Sie Primary durch den gewünschten Slot
		if ( equippedItem != null )
		{
			// Wenn ja, geben Sie das ausgerüstete Item
			Give( equippedItem.GameObject, true );

			// Rüsten Sie das Item automatisch aus
			Inventory.EquipItemFromBackpack( equippedItem );
		}
		else if ( StartingWeapon.IsValid() )
		{
			// Wenn kein Item ausgerüstet ist, geben Sie das StartingWeapon
			Give( StartingWeapon, true );
		}
	}

	public async void Give( GameObject prefab, bool shouldDeploy = false )
	{
		if(IsProxy)
		return;
		
		await Task.Delay( 1 );

		if ( Player.Local == null )
		{
			Log.Error( "Player.Local is null in WeaponContainer.Give" );
			return;
		}

		if ( prefab == null )
		{
			Log.Error( "Prefab is null in WeaponContainer.Give" );
			return;
		}

		var weaponComponent = prefab.Components.Get<WeaponComponent>();
		if ( weaponComponent != null )
		{
			weaponComponent.Owner = Player.Local; // Stellen Sie sicher, dass der Player zugewiesen wird
		}

		var itemComponents = prefab.Components.GetInDescendantsOrSelf<ItemComponent>( true );
		if ( itemComponents == null )
		{
			Log.Error( "ItemComponents is null in WeaponContainer.Give" );
			return;
		}

		if ( itemComponents.IsEquipment )
		{
			return;
		}

		if ( WeaponBone == null )
		{
			Log.Error( "WeaponBone is null in WeaponContainer.Give" );
			return;
		}

		var modelCollider = prefab.Components.Get<ModelCollider>();
		if ( modelCollider != null )
		{
			modelCollider.Destroy();
		}

		var rigidBody = prefab.Components.Get<Rigidbody>();
		if ( rigidBody != null )
		{
			rigidBody.Destroy();
		}

		var weaponGo = prefab.Clone();
		var weapon = weaponGo.Components?.GetInDescendantsOrSelf<WeaponComponent>( true );
		if ( weapon == null || !weapon.IsValid() )
		{
			
			weaponGo.Destroy();
			return;
		}

		weapon.Owner = PlayrControl;

		if ( shouldDeploy )
		{
			foreach ( var w in All )
			{
				w.Holster();
			}
		}

		weaponGo.SetParent( WeaponBone );
		weaponGo.Transform.Position = WeaponBone.Transform.Position;
		weaponGo.Transform.Rotation = WeaponBone.Transform.Rotation;

		var nextWeaponGo = weaponGo.Components.GetInDescendantsOrSelf<BaseGun>( true );
		if ( nextWeaponGo.IsValid() )
		{
			var player = Player.Local as Player;
			if ( player != null )
			{
				var ammoToGive = player.Ammo.Get( nextWeaponGo.AmmoType );
				if ( ammoToGive > 0 )
				{
					var ammoToAdd = Math.Min( ammoToGive, nextWeaponGo.MaxAmmo - nextWeaponGo.DefaultAmmo );
					if ( nextWeaponGo.DefaultAmmo < nextWeaponGo.MaxAmmo )
					{
						nextWeaponGo.DefaultAmmo += ammoToAdd;
						player.Ammo.TryTake( nextWeaponGo.AmmoType, ammoToAdd, out var taken );
					}
				}
				if ( nextWeaponGo.AmmoInClip < nextWeaponGo.ClipSize )
				{
					nextWeaponGo.AmmoInClip = nextWeaponGo.ClipSize;
				}
			}
			nextWeaponGo.AmmoInClip = nextWeaponGo.ClipSize;
			nextWeaponGo.IsDeployed = !Deployed.IsValid();
		}

		weaponGo.NetworkSpawn();
	}
	public void RemoveWeapon( GameObject prefab, bool shouldDeploy = false )
	{
		if ( WeaponBone == null )
		{
			Log.Error( "WeaponBone is null." );
			return;
		}

		
			prefab.SetParent( null );
			ClearWeaponBone();
		
	}

	private void ClearWeaponBone()
	{
		if ( WeaponBone == null )
		{
			Log.Error( "WeaponBone is null." );
			return;
		}

		foreach ( var child in WeaponBone.Children.ToList() )
		{
			child.Destroy();
		}
	}


	public void Next()
	{
		ScrollThroughEquippedItems( 1 );
	}

	public void Previous()
	{
		ScrollThroughEquippedItems( -1 );
	}
	public void ScrollThroughEquippedItems( int direction )
	{
		if ( !HasAny ) return;
		EquipSlot[] slots = { EquipSlot.Hand, EquipSlot.Back };
		var weapons = GetEquippedItems( slots );
		if ( !weapons.Any() ) return;

		var currentIndex = 0;
		var deployed = Deployed;
		var equipped = Inventory.EquipItemFromBackpack; // Angenommen, dies ist die Methode, die die ausgerüstete Waffe zurückgibt

		if ( deployed != null )
		{
			currentIndex = weapons.IndexOf( deployed );
		}
		if ( equipped != null )
		{

			if ( currentIndex != -1 )
			{
				// Deploy the equipped item
				weapons[currentIndex].Deploy();
			}
		}

		Log.Info( $"Aktueller Index: {currentIndex}" );

		currentIndex = (currentIndex + direction + weapons.Count) % weapons.Count;

		var nextWeapon = weapons[currentIndex];
		if ( nextWeapon == deployed )
			return;

		foreach ( var weapon in weapons.Where( weapon => weapon != nextWeapon ) )
		{
			weapon.Holster();
		}

		nextWeapon.Deploy();
	}
	public List<WeaponComponent> GetEquippedItems( params EquipSlot[] slots )
	{

		return All.Where( w => slots.Contains( w.Slot ) ).ToList();

	}


}
