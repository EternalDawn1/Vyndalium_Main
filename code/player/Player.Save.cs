

namespace GeneralGame;

public struct ItemSave
{
	[JsonInclude] public string Path;
	[JsonInclude] public Dictionary<string, string> Data;
	[JsonInclude] public int Index;
}


public struct PlayerSave
{
	public const string FILE_PATH = "viwis.json";

	[JsonInclude] public string Firstname;
	[JsonInclude] public string Lastname;
	[JsonInclude] public string AuthToken {get ; set;}
	[JsonInclude] public AmmoContainer AmmoContainerData;
	[JsonInclude] public int AmmoCount;
	[JsonInclude] public int Vyndalium;
	[JsonInclude] public int Experience;
	[JsonInclude] public int Level;
	[JsonInclude] public float MaxHealth;
	[JsonInclude] public float MaxMana;
	[JsonInclude] public float MaxStamina;
	[JsonInclude] public float AttackPower;
	[JsonInclude] public float MagicPower;
	[JsonInclude] public float Armor;
	[JsonInclude] public float MagicDefense;
	[JsonInclude] public float MovementSpeed;
	[JsonInclude] public float AttackSpeed;
	[JsonInclude] public float Evasion;
	[JsonInclude] public float Block;
	[JsonInclude] public float CritHitChance;
	[JsonInclude] public float CritHitDamage;
	[JsonInclude] public float BonusEXPGain;
	[JsonInclude] public float Tenacity;
	[JsonInclude] public float StunResist;
	[JsonInclude] public float BlindResist;
	[JsonInclude] public float SlowResist;
	[JsonInclude] public float FireResist;
	[JsonInclude] public float PoisonResist;
	[JsonInclude] public float BleedResist;
	[JsonInclude] public float FreezeResist;
	[JsonInclude] public float IceResist;
	[JsonInclude] public float LightningResist;
	[JsonInclude] public float ShadowResist;
	[JsonInclude] public float LightResist;
	[JsonInclude] public int STG;
	[JsonInclude] public int INT;
	[JsonInclude] public int DEX;
	[JsonInclude] public int PER;

	[JsonInclude] public int StatsPoints;

	[JsonInclude] public float Height;
	[JsonInclude] public float Fatness;

	[JsonInclude] public Color SkinColor;

	[JsonInclude] public ItemSave[] Clothes;
	[JsonInclude] public ItemSave[] Inventory;

}


public class ValidateAuthTokenResponse
{
    public long SteamId { get; set; }
    public string Status { get; set; }
}

[AttributeUsage( AttributeTargets.Property )]
public class TargetSaveAttribute : Attribute
{
	public object IgnoreIf { get; set; }
}


partial class Player
{
	public AmmoContainer AmmoContainerData { get; set; }
	private static readonly JsonSerializerOptions options = new JsonSerializerOptions()
	{
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
	};
	
	private static PlayerSave? _saveData;

	/// <summary>
	/// Gets current local save data.
	/// </summary>
	/// <returns></returns>
	public static (PlayerSave Save, bool Has) GetSave()
	{
		if ( _saveData.HasValue )
			return (_saveData.Value, true);

		if ( !FileSystem.OrganizationData.FileExists( PlayerSave.FILE_PATH ) )
			return (default, false);

		_saveData = FileSystem.OrganizationData.ReadJson<PlayerSave>( PlayerSave.FILE_PATH );

		return (_saveData.Value, true);
	}

	/// <summary>
	/// Writes a pure PlayerSave struct into a local save.
	/// </summary>
	/// <param name="save"></param>
	public static void WriteSave( PlayerSave save )
		=> FileSystem.OrganizationData.WriteJson( PlayerSave.FILE_PATH, save );

	/// <summary>
	/// Writes a local save based on player or local.
	/// </summary>
	/// <param name="player"></param>
	public static void Save( Player player = null )
	{
		player ??= Local;
		
		// Get the data that sticks.
		var data = GetSave();
		var save = data.Has
			? data.Save
			: new PlayerSave()
			{

			};

		var items = PrefabLibrary.FindByComponent<ItemComponent>();
		
		// Save dynamic data.
		ItemSave Serialize( ItemComponent item )
		{
			if ( item == null )
				return default;

			if ( !ResourceLibrary.TryGet<PrefabFile>( item.Prefab, out var resource ) )
				return default;

			var data = new Dictionary<string, string>();
			foreach ( var component in item.Components.GetAll() )
			{
				var properties = GlobalGameNamespace.TypeLibrary
					?.GetType( component.GetType() )
					?.Properties
					?.Where( property =>
					{
						var attribute = property.GetCustomAttribute<TargetSaveAttribute>();
						if ( attribute == null ) return false;

						var ignore = attribute?.IgnoreIf?.Equals( property.GetValue( component ) ) ?? false;
						return !ignore;
					} );

				foreach ( var property in properties )
				{
					var serialized = JsonSerializer.Serialize( property.GetValue( component ), property.PropertyType, options );
					if ( data.ContainsKey( property.Name ) )
						data[property.Name] = serialized;
					else
						data.Add( property.Name, serialized );
				}
			}
			data.Add("Tier", JsonSerializer.Serialize(item.Tier));
  			data.Add("ItemLevel", JsonSerializer.Serialize(item.ItemLevel));
    
			return new ItemSave
			{
				Path = item.Prefab,
				Data = data.Count > 0 ? data : null,
				Index = player.Inventory.IndexOf( item )
			};
		}
		
		_saveData = save with
		{
			
			Vyndalium = (int)player.Vyndalium,
			Experience = (int)player.Experience,
			Level = (int)player.Level,
			MaxHealth = player.MaxHealth,
			MaxMana = player.MaxMana,
			MaxStamina = player.MaxStamina,
			AttackPower = player.AttackPower,
			MagicPower = player.MagicPower,
			Armor = player.Armor,
			MagicDefense = player.MagicDefense,
			MovementSpeed = player.PlayerRunSpeed,
			AttackSpeed = (float)player.AttackSpeed,
			Evasion = (float)player.Evasion,
			Block = (float)player.Block,
			CritHitChance = player.CritHitChance,
			CritHitDamage = player.CritHitDamage,
			BonusEXPGain = (float)player.BonusEXPGain,
			Tenacity = (float)player.Tenacity,
			StunResist = (float)player.StunResist,
			BlindResist = player.BlindResist,
			SlowResist = player.SlowResist,
			FireResist = player.FireResist,
			PoisonResist = player.PoisonResist,
			BleedResist = player.BleedResist,
			FreezeResist = player.FreezeResist,
			IceResist = player.IceResist,
			LightningResist = player.LightningResist,
			ShadowResist = player.ShadowResist,
			LightResist = player.LightResist,
			StatsPoints = (int)player.StatsPoints,
			STG = (int)player.STG,
			INT = (int)player.INT,
			DEX = (int)player.DEX,
			PER = (int)player.PER,
			
			
			Clothes = player.Inventory.EquippedItems
			
			
				.Where( x => x != null )
				.Select( Serialize )
				.ToArray(),
			Inventory = player.Inventory.BackpackItems
				.Where( x => x != null )
				.Select( Serialize )
				.ToArray(),
			
		};

		if ( player.AmmoContainer == null )
		{
			player.AmmoContainer = new AmmoContainer();
		}

		save.AmmoContainerData = player.AmmoContainer;

		Log.Info( $"Speichere Daten: {_saveData.Value}" );

		// Write save.
		WriteSave( _saveData.Value );
		Log.Info( "Spielerdaten erfolgreich gespeichert." );
	}

	[ConCmd("newgame_save")]
	public static void SavePlayer()
	{
		Save();
	}

	/// <summary>
	/// Sets up everything for a player or local from local save.
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	public static bool Setup( Player player = null )
	{
		player ??= Local;

		var tuple = GetSave();
		if ( !tuple.Has )
			return false;

		// Setup basic player information.
		var save = tuple.Save;

		// Stellen Sie sicher, dass save.AmmoContainerData initialisiert wurde
		if ( save.AmmoContainerData == null )
		{
			Log.Error( "save.AmmoContainerData ist null. Initialisierung erforderlich." );
			save.AmmoContainerData = new AmmoContainer(); // Initialisierung
		}

		if ( player == null )
		{
			// Logik, um den Spieler zu initialisieren, falls er null ist
			player = new Player();
		}

		// Initialisiere den AmmoContainer, falls er noch nicht initialisiert ist
		if ( player.AmmoContainer == null )
		{
			player.AmmoContainer = new AmmoContainer();
		}

		// Wenn beide nicht null sind, führen Sie die Zuweisung durch
		if ( player.AmmoContainer != null && save.AmmoContainerData != null )
		{
			player.AmmoContainer = save.AmmoContainerData;
		}
		else
		{
			Log.Error( "Die Zuweisung von AmmoContainerData kann nicht durchgeführt werden, da eines der Objekte null ist." );
			player.AmmoContainer = new AmmoContainer();
		}

		player.MaxHealth = save.MaxHealth;
		player.MaxMana = save.MaxMana;
		player.MaxStamina = save.MaxStamina;
		player.Vyndalium = save.Vyndalium;
		player.Experience = save.Experience;
		
		player.Level = save.Level;
		player.StatsPoints = save.StatsPoints;
		player.MaxStamina = save.MaxStamina;
		player.AttackPower = save.AttackPower;
		player.MagicPower = save.MagicPower;
		player.Armor = save.Armor;
		player.MagicDefense = save.MagicDefense;
		player.PlayerRunSpeed = save.MovementSpeed;
		player.AttackSpeed = save.AttackSpeed;
		player.Evasion = save.Evasion;
		player.Block = save.Block;
		player.CritHitChance = save.CritHitChance;
		player.CritHitDamage = save.CritHitDamage;
		player.BonusEXPGain = save.BonusEXPGain;
		player.Tenacity = save.Tenacity;
		player.StunResist = save.StunResist;
		player.BlindResist = save.BlindResist;
		player.SlowResist = save.SlowResist;
		player.FireResist = save.FireResist;
		player.PoisonResist = save.PoisonResist;
		player.BleedResist = save.BleedResist;
		player.FreezeResist = save.FreezeResist;
		player.IceResist = save.IceResist;
		player.LightningResist = save.LightningResist;
		player.ShadowResist = save.ShadowResist;
		player.LightResist = save.LightResist;
		player.STG = save.STG;
		player.INT = save.INT;
		player.DEX = save.DEX;
		player.PER = save.PER;

		

		void ReadData( ItemSave data, GameObject obj )
		{
			var components = obj.Components.GetAll();
			foreach ( var component in components )
			{
				var properties = GlobalGameNamespace.TypeLibrary
					?.GetType( component.GetType() )
					?.Properties
					?.Where( x => x.HasAttribute<TargetSaveAttribute>() );

				foreach ( var property in properties )
				{
					if ( data.Data == null || !data.Data.TryGetValue( property.Name, out var serialized ) )
						continue;

					var deserialized = JsonSerializer.Deserialize( serialized, property.PropertyType, options );
					property.SetValue( component, deserialized );
				}
			}
		}

		// Go through all clothes.
		if ( save.Clothes != null )
			foreach ( var data in save.Clothes )
			{
				if ( !ResourceLibrary.TryGet<PrefabFile>( data.Path, out var prefab ) )
					continue;

				var o = SceneUtility.GetPrefabScene( prefab ).Clone();
				o.NetworkMode = NetworkMode.Object;
				if ( !o.Network.Active ) o.NetworkSpawn();
				var equipment = o.Components.Get<ItemEquipment>();
				if ( equipment == null )
					continue;

				player.Inventory.EquipItemFromWorld( equipment );
				ReadData( data, o );
			}

		// Go through all items.
		if ( save.Inventory != null )
			foreach ( var data in save.Inventory )
			{
				if ( !ResourceLibrary.TryGet<PrefabFile>( data.Path, out var prefab ) )
					continue;

				var o = SceneUtility.GetPrefabScene( prefab ).Clone();
				o.NetworkMode = NetworkMode.Object;
				if ( !o.Network.Active ) o.NetworkSpawn();
				var item = o.Components.Get<ItemComponent>();
				if ( item == null )
					continue;

				player.Inventory.SetItem( item, data.Index );
				ReadData( data, o );
			}

		

		return true;
	}
}
