using System.Collections.Generic;
using System.Dynamic;
using Sandbox;

namespace GeneralGame;

[Group( "Arena" )]
[Title( "Ammo Container" )]
public sealed class AmmoContainer : Component
{
	private Dictionary<AmmoType, int> AmmoCount { get; set; } = new();
	public int GetAmmoCount( AmmoType type )
	{
		return Get( type ); // Nutzt die vorhandene Get-Methode, um die Munitionsanzahl zurückzugeben
	}
	public void RemoveAmmo( AmmoType type, int amount )
	{
		if ( AmmoCount.ContainsKey( type ) && AmmoCount[type] >= amount )
		{
			AmmoCount[type] -= amount;
			if ( AmmoCount[type] < 0 )
			{
				AmmoCount[type] = 0; // Stellen Sie sicher, dass der Munitionszähler nicht negativ wird
			}
		}
		else
		{
			// Optional: Behandeln Sie den Fall, wenn nicht genug Munition vorhanden ist
			// Dies könnte eine Warnung ausgeben oder einfach nichts tun
		}
	}

	public void Give( AmmoType type, int ammo )
	{
		if ( AmmoCount.TryAdd( type, ammo ) )
			return;

		AmmoCount[type] += ammo;
	}

	public bool TryTake( AmmoType type, int amount, out int taken )
	{
		var ammo = Get( type );
		if ( ammo == 0 )
		{
			taken = 0;
			return false;
		}

		if ( ammo >= amount )
		{
			taken = amount;
			AmmoCount[type] -= taken;
			return true;
		}

		taken = ammo;
		AmmoCount[type] = 0;
		return true;
	}

	public bool CanTake( AmmoType type, int amount, out int taken )
	{
		var ammo = Get( type );
		if ( ammo == 0 )
		{
			taken = 0;
			return false;
		}

		if ( ammo >= amount )
		{
			taken = amount;
			return true;
		}

		taken = ammo;
		return true;
	}

	public int Get( AmmoType type )
	{
		return CollectionExtensions.GetValueOrDefault( AmmoCount, type, 0 );
	}
}
