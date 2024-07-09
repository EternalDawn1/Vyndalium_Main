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
		if ( IsProxy ) return;

		{
			if ( prefab == null )
			{
				// Behandeln Sie den Fall, dass das prefab null ist
				return;
			}


			prefab.SetParent( WeaponBone );

			if ( WeaponBone != null )
			{
				prefab.Transform.Position = WeaponBone.Transform.Position;
				prefab.Transform.Rotation = WeaponBone.Transform.Rotation;
			}
			else
			{
				Log.Error( "WeaponBone is null in WeaponContainer.Give" );
			}

			var modelCollider = prefab.Components.Get<ModelCollider>();
			if ( modelCollider != null )
			{
				modelCollider.Destroy();
			}
			else
			{
				Log.Error( "ModelCollider is null in WeaponContainer.Give" );
			}

			var rigidBody = prefab.Components.Get<Rigidbody>();
			if ( rigidBody != null )
			{
				rigidBody.Destroy();
			}
			else
			{
				Log.Error( "RigidBody is null in WeaponContainer.Give" );

			}

			var weaponGo = prefab.Clone();
			var weapon = weaponGo.Components.GetInDescendantsOrSelf<WeaponComponent>( true );
			weapon.Owner = PlayrControl;
			if ( weapon != null )
			{
				Log.Info( "Weapon is not null in WeaponContainer.Give" );
			}
			if ( !weapon.IsValid() )
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

			var nextWeponGo = weaponGo.Components.GetInDescendantsOrSelf<BaseGun>( true );
			if ( nextWeponGo.IsValid() )
			{

				nextWeponGo.AmmoInClip = nextWeponGo.ClipSize;
				nextWeponGo.IsDeployed = !Deployed.IsValid();
				var player = Player.Local as Player;
				var ammoToGive = player.Ammo.Get( nextWeponGo.AmmoType );
				if ( ammoToGive > 0 )
				{
					player.Ammo.TryTake( nextWeponGo.AmmoType, ammoToGive, out var taken );
					nextWeponGo.DefaultAmmo = Math.Min( nextWeponGo.DefaultAmmo + taken, nextWeponGo.MaxAmmo );
				}
			}

			weaponGo.NetworkSpawn();
			weaponGo.Components.Get<ModelCollider>().Destroy();
			weaponGo.Components.Get<Rigidbody>().Destroy();

		}
	}
	public List<WeaponComponent> GetInitializedEquippedWeapons()
	{
		// Erhalten Sie alle ausgerüsteten Waffen von dieser Instanz
		var equippedWeapons = this.GetEquippedItems( EquipSlot.Hand );

		// Filtern Sie die Liste, um nur initialisierte Waffen zu behalten
		var initializedEquippedWeapons = equippedWeapons.Where( weapon => weapon.IsInitialized ).ToList();

		return initializedEquippedWeapons;
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

		var weapons = All.ToList();
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
