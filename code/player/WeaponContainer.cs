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
		if (IsProxy) return;

		// Überprüfen Sie, ob ein Item im EquipSlot vorhanden ist
		var equippedItem = Inventory.GetItemInSlot(EquipSlot.Hand); // Ersetzen Sie Primary durch den gewünschten Slot
		if (equippedItem != null)
		{
			// Wenn ja, geben Sie das ausgerüstete Item
			Give(equippedItem.GameObject, true);

			// Rüsten Sie das Item automatisch aus
			Inventory.EquipItemFromBackpack(equippedItem);
		}
		else if (StartingWeapon.IsValid())
		{
			// Wenn kein Item ausgerüstet ist, geben Sie das StartingWeapon
			Give(StartingWeapon, true);
		}
	}
	
	public void Give( GameObject prefab, bool shouldDeploy = false )
	{
		if ( IsProxy ) return;
		if (prefab == null)
    {
        Log.Error("Prefab is null in WeaponContainer.Give");
        return;
		
    }
	if (WeaponBone == null)
	{
		Log.Error("WeaponBone is null in WeaponContainer.Give");
		return;
	}
	

    prefab.SetParent(WeaponBone);
    
    if (WeaponBone != null)
    {
        prefab.Transform.Position = WeaponBone.Transform.Position;
        prefab.Transform.Rotation = WeaponBone.Transform.Rotation;
    }
    else
    {
        Log.Error("WeaponBone is null in WeaponContainer.Give");
    }

    var modelCollider = prefab.Components.Get<ModelCollider>();
    if (modelCollider != null)
    {
        modelCollider.Destroy();
    }
    else
    {
        Log.Error("ModelCollider is null in WeaponContainer.Give");
    }

    var rigidBody = prefab.Components.Get<Rigidbody>();
	if (rigidBody != null)
	{
		rigidBody.Destroy();
	}
	else
	{
		Log.Error("RigidBody is null in WeaponContainer.Give");
	}
		
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
	private List<ItemComponent> weaponList = new List<ItemComponent>();

    // Methode, um eine Waffe zur Liste hinzuzufügen
    public void AddWeapon(ItemComponent weapon)
    {
        weaponList.Add(weapon);
    }

    // Methode, um zu überprüfen, ob eine bestimmte Waffe in der Liste ist
    public bool Contains(ItemComponent item)
    {
        return weaponList.Contains(item);
    }

    // Methode, um durch alle Waffen in der Liste zu iterieren und eine Aktion auszuführen
    public void CheckAllWeapons(Action<ItemComponent> action)
    {
        foreach (var weapon in weaponList)
        {
            action(weapon);
        }
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
		Log.Info("ScrollThroughEquippedItems wurde aufgerufen");

		if (!HasAny) return;

		var weapons = GetEquippedItems(EquipSlot.Back, EquipSlot.Hand);
		if (!weapons.Any()) return;

		Log.Info($"Anzahl der Waffen: {weapons.Count}");

		var currentIndex = 0;
		var deployed = Deployed;

		if (deployed != null)
		{
			currentIndex = weapons.IndexOf(deployed);
		}

		Log.Info($"Aktueller Index: {currentIndex}");

		currentIndex = (currentIndex + direction + weapons.Count) % weapons.Count;

		var nextWeapon = weapons[currentIndex];
		if (nextWeapon == deployed)
			return;

		foreach (var weapon in weapons.Where(weapon => weapon != nextWeapon))
		{
			weapon.Holster();
		}

		nextWeapon.Deploy();
	}
	public List<WeaponComponent> GetEquippedItems(params EquipSlot[] slots)
	{
		
		return All.Where(w => slots.Contains(w.Slot)).ToList();
		
	}
	
	
}
