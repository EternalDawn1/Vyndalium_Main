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

	
	public WeaponComponent Deployed => Components.GetAll<WeaponComponent>( FindMode.EverythingInSelfAndDescendants ).FirstOrDefault( c => c.IsDeployed );
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

	public void Give( GameObject prefab, bool shouldDeploy = false )
	{
		if ( WeaponBone == null )
		{
			Log.Error( "WeaponBone is null in WeaponContainer.Give" );
			return;
		}

		prefab.SetParent( WeaponBone );
		prefab.Transform.Position = WeaponBone.Transform.Position;
		prefab.Transform.Rotation = WeaponBone.Transform.Rotation;

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
		var weapon = weaponGo.Components.GetInDescendantsOrSelf<WeaponComponent>( true );
		weapon.Owner = PlayrControl;
		if ( weapon == null || !weapon.IsValid() )
		{
			weaponGo.DestroyImmediate();
			return;
		}

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
					player.Ammo.TryTake( nextWeaponGo.AmmoType, ammoToGive, out var taken );
					nextWeaponGo.DefaultAmmo = Math.Min( nextWeaponGo.DefaultAmmo + taken, nextWeaponGo.MaxAmmo );
				}
				else
				{
					// Setze DefaultAmmo nur, wenn sie noch nicht initialisiert wurde
					if ( nextWeaponGo.DefaultAmmo == 0 )
					{
						nextWeaponGo.DefaultAmmo = nextWeaponGo.ClipSize;
					}
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

		if ( WeaponBone.Children.Contains( prefab ) )
		{
			prefab.SetParent( null );
			ClearWeaponBone();
			//RemoveUnnecessaryComponents( prefab );
		}
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
