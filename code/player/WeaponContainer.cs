using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace GeneralGame;


public  partial class WeaponContainer : Component
{
	[Property] public PrefabScene StartingWeapon { get; set; }
	[Property] public GameObject WeaponBone { get; set; }
	[Property] public AmmoContainer Ammo { get; set; }
	[Property] public Player PlayrControl { get; set; }
	[Property] public Inventory Inventory { get; set; }

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
		if ( !StartingWeapon.IsValid() ) return;
		
		Give( StartingWeapon, true );
	}
	
	public void Give( GameObject prefab, bool shouldDeploy = false )
	{
		if ( IsProxy ) return;

		
		var weaponGo = prefab.Clone();
		var weapon = weaponGo.Components.GetInDescendantsOrSelf<WeaponComponent>( true );
		weapon.Owner = PlayrControl;
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
			
			var ammoToGive = nextWeponGo.DefaultAmmo - Ammo.Get( nextWeponGo.AmmoType );

			if ( ammoToGive > 0 )
			{
				Ammo.Give( nextWeponGo.AmmoType, ammoToGive );
			}
		}

		weaponGo.NetworkSpawn();
		weaponGo.Components.Get<ModelCollider>().Destroy();
		weaponGo.Components.Get<Rigidbody>().Destroy();
	}
	
	public void Next()
	{
		ScrollThroughEquippedItems(1);
	}
	
	public void Previous()
	{
		ScrollThroughEquippedItems(-1);
	}
	public void ScrollThroughEquippedItems(int direction)
	{
		if ( !HasAny ) return;

		var weapons = All.ToList(); // Nehmen Sie an, dass 'All' alle Waffen zurückgibt
		var currentIndex = -1;
		var deployed = Deployed; // Sie müssen diese Eigenschaft entsprechend Ihrer Anforderungen implementieren

		if ( deployed != null )
		{
			currentIndex = weapons.IndexOf( deployed );
		}

		currentIndex = (currentIndex + direction + weapons.Count) % weapons.Count; // Stellen Sie sicher, dass 'weapons.Count' die Anzahl der Elemente in der Liste zurückgibt

		var nextWeapon = weapons[currentIndex];
		if ( nextWeapon == deployed )
			return;

		foreach ( var weapon in weapons.Where( weapon => weapon != nextWeapon ) )
		{
			weapon.Holster(); // Sie müssen diese Methode entsprechend Ihrer Anforderungen implementieren
		}

		nextWeapon.Deploy(); // Sie müssen diese Methode entsprechend Ihrer Anforderungen implementieren
	}
	
}
