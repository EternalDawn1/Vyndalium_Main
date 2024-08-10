using System.Collections.Generic;
using Sandbox;

namespace GeneralGame;

[Group( "Arena" )]
[Title( "Weapon Manager")]
public class WeaponManager : Component
{
	public static WeaponManager Instance { get; private set; }
	public List<BaseGun> Weapons { get; set; } = new();
	
	
	[Property] public List<PrefabScene> Prefabs { get; set; }


	protected override void OnAwake()
	{
		Instance = this;

		var player = Player.Local; // Annahme: Player.Local gibt den lokalen Spieler zurück
		var ammoContainer = player?.Components.Get<AmmoContainer>();

		foreach ( var prefab in Prefabs )
		{
			var weapon = prefab.Components.Get<BaseGun>();
			weapon.InitializeAmmo( ammoContainer );
			Weapons.Add( weapon );
			Components.GetOrCreate<Interactions>();
		}

		base.OnAwake();
	}

	protected override void OnDestroy()
	{
		Instance = null;
		base.OnDestroy();
	}
}
