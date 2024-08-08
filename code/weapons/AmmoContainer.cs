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
		return Get( type );
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

	// Serialisierungsmethoden
	public string Serialize()
	{
		return JsonSerializer.Serialize( this );
	}

	public static AmmoContainer Deserialize( string jsonString )
	{
		return JsonSerializer.Deserialize<AmmoContainer>( jsonString );
	}

	// Fehlerbehandlung für Nullreferenzen
	
}
