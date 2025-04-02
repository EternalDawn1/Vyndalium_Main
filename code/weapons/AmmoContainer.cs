using System.Collections.Generic;
using System.Dynamic;
using Sandbox;

namespace GeneralGame;

[Group( "Arena" )]
[Title( "Ammo Container" )]
public class AmmoContainer : Component
{
	[Property] public Dictionary<AmmoType, int> AmmoCount { get; set; } = new Dictionary<AmmoType, int>();


	public int GetAmmoCount( AmmoType ammoType )
	{
		if ( AmmoCount.TryGetValue( ammoType, out int count ) )
		{
			return count;
		}
		return 0;
	}
	public void ResetAmmo()
	{
		foreach ( var ammoType in AmmoCount.Keys.ToList() )
		{
			AmmoCount[ammoType] = 0;
		}
	}
	public void RemoveAmmo( AmmoType ammoType, int count )
	{
		if ( AmmoCount.ContainsKey( ammoType ) )
		{
			AmmoCount[ammoType] = Math.Max( 0, AmmoCount[ammoType] - count );
		}
	}

	public void SetAmmoCount( AmmoType ammoType, int count )
	{
		AmmoCount[ammoType] = count;
	}

	public void Give( AmmoType type, int ammo )
	{
		if ( AmmoCount.TryAdd( type, ammo ) )
			return;
		AmmoCount[type] += ammo;
	}


	public bool TryTake( AmmoType type, int amount, out int taken )
	{
		var ammo = GetAmmoCount( type );
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

	// Neue Methoden für DefaultAmmo


	

	// Serialisierungsmethoden
	public string Serialize()
	{
		return JsonSerializer.Serialize( this );
	}

	public static AmmoContainer Deserialize( string jsonString )
	{
		return JsonSerializer.Deserialize<AmmoContainer>( jsonString );
	}
}
