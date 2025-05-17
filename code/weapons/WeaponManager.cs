using System.Collections.Generic;
using Sandbox;

namespace GeneralGame;

[Group( "Arena" )]
[Title( "Weapon Manager")]
public class WeaponManager : Component
{
	public static WeaponManager Instance { get; private set; }
	public List<BaseGun> Weapons { get; set; } = new();
	public List<BaseMelee> MeleeWeapons { get; set; } = new();
	
	
	[Property] public List<PrefabScene> Prefabs { get; set; }


	protected override void OnAwake()
{
    Instance = this;

    var player = Player.Local;
    var ammoContainer = player?.Components.Get<AmmoContainer>();

    foreach ( var prefab in Prefabs )
    {
        var weapon = prefab.Components.Get<BaseGun>();
        // Munitionszustand nur initialisieren, wenn die Waffe neu ist oder keine Munition hat
        if (weapon != null && !Weapons.Contains(weapon))
        {
            weapon.InitializeAmmo( ammoContainer );
            Weapons.Add( weapon );
            Components.GetOrCreate<Interactions>();
        }
        
        var melee = prefab.Components.Get<BaseMelee>();
        if (melee != null && !MeleeWeapons.Contains(melee))
        {
            MeleeWeapons.Add( melee );
            Components.GetOrCreate<Interactions>();
        }
    }

    base.OnAwake();
}

	protected override void OnDestroy()
	{
		Instance = null;
		base.OnDestroy();
	}
}
