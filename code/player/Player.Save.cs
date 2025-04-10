

namespace GeneralGame;

public struct ItemSave
{
	[JsonInclude] public string Path;
	[JsonInclude] public bool IsFavorite{ get; set; }
	[JsonInclude] public bool IsBackpack{ get; set; }
	[JsonInclude] public bool IsWeapon{ get; set; }
	[JsonInclude] public bool IsPotion{ get; set; }
	[JsonInclude] public bool IsAccessory{ get; set; }
	

	[JsonInclude] public Dictionary<string, string> Data;
	[JsonInclude] public string Description;
	[JsonInclude] public ItemState State;
	[JsonInclude] public int Index;
	[JsonInclude] public int IndexStorage;
	[JsonInclude] public int IndexBackpackBag;
	[JsonInclude] public float SellPrice { get; set; }
	[JsonInclude] public float BuyPrice { get; set; }
	[JsonInclude] public int DMG { get; set; }
	[JsonInclude] public int STG { get; set; }
	[JsonInclude] public int HE { get; set; }
	[JsonInclude] public int DEX { get; set; }
	[JsonInclude]public int PER { get; set; }
	[JsonInclude] public int INT { get; set; }
	[JsonInclude] public int Mana { get; set; }
	[JsonInclude] public int MaxStack { get; set; }
	[JsonInclude] public int Count { get; set; }
	[JsonInclude] public int Health { get; set; }
	[JsonInclude] public int ItemLevel { get; set; }
	[JsonInclude]public int CritHitDamage { get; set; }
	[JsonInclude] public int FireRate { get; set; }
	[JsonInclude] public int BulletSpeed { get; set; }
	[JsonInclude] public int CritHitChance { get; set; }
	[JsonInclude]public int AbilityHaste { get; set; }
	[JsonInclude] public int AttackPower { get; set; }
	[JsonInclude] public int MagicPower { get; set; }
	[JsonInclude] public GeneralGame.Tier Tier { get; set; }
	[JsonInclude] public AspectType Aspect { get; set; }
	
	[JsonInclude] public int RequiredLevel { get; set; }
	[JsonInclude] public int DamageBalance { get; set; }
	[JsonInclude] public int MinArmorValue { get; set; }
	[JsonInclude] public int MaxArmorValue { get; set; }
	[JsonInclude] public int MinAttackValue { get; set; }
	[JsonInclude] public int MaxAttackValue { get; set; }
	[JsonInclude] public int Durability { get; set; }
	[JsonInclude] public int AttackSpeed { get; set; }
	[JsonInclude] public int MoveSpeed { get; set; }
	[JsonInclude] public int Armor { get; set; }
	[JsonInclude] public int MagicDefense { get; set; }
	[JsonInclude] public int Evasion { get; set; }
	[JsonInclude] public int Cover { get; set; }
	[JsonInclude] public int BonusEXP { get; set; }
	[JsonInclude] public int BonusScore { get; set; }
	[JsonInclude] public int BonusVyndalium { get; set; }
	[JsonInclude] public int Tenacity { get; set; }
	[JsonInclude] public int StunResistance { get; set; }
	[JsonInclude] public int BlindResistance { get; set; }
	[JsonInclude] public int SlowResistence { get; set; }
	[JsonInclude] public int FireResistence { get; set; }
	[JsonInclude] public int BleedResistance { get; set; }
	[JsonInclude] public int PoisonResistence { get; set; }
	[JsonInclude] public int IceResistence { get; set; }
	[JsonInclude] public int LightningResistence { get; set; }
	[JsonInclude] public int HolyResistence { get; set; }
	[JsonInclude] public int ShadowResistence { get; set; }
	
}



public struct PlayerSave
{
	public const string FILE_PATH = "viwis.json";
	
	[JsonInclude] public string Firstname;
	[JsonInclude] public string Lastname;
	[JsonInclude] public string AuthToken {get ; set;}
	[JsonInclude] public float MouseSensitivity;
	[JsonInclude] public int MAX_BACKPACK_SLOTS;
	[JsonInclude] public Dictionary<AmmoType, int> AmmoCount { get; set; }
	[JsonInclude] public float DefaultFOV;
	[JsonInclude] public float Life;
	[JsonInclude] public int DefaultAmmo;
	[JsonInclude] public int Vyndalium;
	[JsonInclude] public int Experience;
	[JsonInclude] public int Level;
	[JsonInclude] public float MaxHealth;
	[JsonInclude] public float MaxMana;
	[JsonInclude] public float Stamina;
	[JsonInclude] public float MinArmorValue;
	[JsonInclude] public float MaxArmorValue;
	[JsonInclude] public float MinAttackValue;
	[JsonInclude] public float MaxAttackValue;
	[JsonInclude] public float ArmorPenetration;
	[JsonInclude] public float AttackRange;
	[JsonInclude] public float BonusVyndalium;
	[JsonInclude] public float MagicPenetration;
	[JsonInclude] public float AbilityHaste;
	[JsonInclude] public float PlayerWalkSpeed;
	[JsonInclude] public float PlayerRunSpeed;
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
	[JsonInclude] public int StrengthCost;
	[JsonInclude] public int AttackPowerCost;
	[JsonInclude] public int ArmorPenetrationCost;
	[JsonInclude] public int AttackRangeCost;
	[JsonInclude] public int AttackSpeedCost;
	[JsonInclude] public int CriticalChanceCost;
	[JsonInclude] public int CriticalDamageCost;
	[JsonInclude] public int DexterityCost;
	[JsonInclude] public int EvasionCost;
	[JsonInclude] public int AbilityHasteCost;
	[JsonInclude] public int StaminaCost;
	[JsonInclude] public int PlayerWalkSpeedCost;
	[JsonInclude] public int PlayerRunSpeedCost;
	[JsonInclude] public int ManaCost;
	[JsonInclude] public int IntelligenceCost;
	[JsonInclude] public int MagicPowerCost;
	[JsonInclude] public int MagicPenetrationCost;
	[JsonInclude] public int BonusEXPGainCost;
	[JsonInclude] public int BonusVyndaliumGainCost;
	[JsonInclude] public int PrestigeLevel;
	[JsonInclude] public int StatsPoints;

	[JsonInclude] public float Height;
	[JsonInclude] public float Fatness;

	[JsonInclude] public Color SkinColor;

	[JsonInclude] public ItemSave[] Clothes;
	[JsonInclude] public ItemSave[] Inventory;
	[JsonInclude] public ItemSave[] StorageItems;
	
	[JsonInclude] public ItemSave[] BackpackBagItems;

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
			var components = item.Components?.GetAll();
			if ( components == null ) return default;
			foreach ( var component in components )
			{
				var type = GlobalGameNamespace.TypeLibrary?.GetType( component.GetType() );
				if ( type == null ) continue;
				var properties = type.Properties?.Where( property =>
				{
					var attribute = property.GetCustomAttribute<TargetSaveAttribute>();
					if ( attribute == null ) return false;
					var ignore = attribute?.IgnoreIf?.Equals( property.GetValue( component ) ) ?? false;
					return !ignore;
				} );
				if ( properties == null ) continue;
				foreach ( var property in properties )
				{
					data[property.Name] = property.GetValue( component )?.ToString();
				}
			}
			item.SellPrice = item.SellPrice;
			item.BuyPrice = item.BuyPrice;
			item.MaxStack = item.MaxStack;
			item.Count = item.Count;
			item.Description = item.Description;
			item.IsFavorite = item.IsFavorite;
			item.Aspect = item.Aspect;
			item.IsAccessory = item.IsAccessory;
			item.IsBackpack = item.IsBackpack;
			item.RequiredLevel = item.RequiredLevel;
			item.State = item.State;
			item.IsWeapon = item.IsWeapon;
			item.IsPotion = item.IsPotion;
			item.IsAccessory = item.IsAccessory;
		
		

			

			return new ItemSave
			{
				IsFavorite = item.IsFavorite,
				IsBackpack = item.IsBackpack,
				IsWeapon = item.IsWeapon,
				IsPotion = item.IsPotion,
				IsAccessory = item.IsAccessory,
				Aspect = item.Aspect,
				RequiredLevel = item.RequiredLevel,
				Path = item.Prefab,
				State = item.State,
				Data = data.Count > 0 ? data : null,
				IndexStorage = player.Inventory._storageBoxItems.IndexOf( item ),
				Index = player.Inventory.IndexOf( item ),
				IndexBackpackBag = player.Inventory._backpackBagItems.IndexOf( item ),	
				
				SellPrice = item.SellPrice,
				BuyPrice = item.BuyPrice,
				MaxStack = item.MaxStack,
				Count = item.Count,
				Description = item.Description,
				DMG = item.DMG,
				STG = item.STG,
				HE = item.HE,
				DEX = item.DEX,
				PER = item.PER,
				INT = (int)item.INT,
				Mana = (int)item.Mana,
				Health = (int)item.Health,
				ItemLevel = item.ItemLevel,
				CritHitDamage = (int)item.CritHitDamage,
				FireRate = (int)item.FireRate,
				BulletSpeed = (int)item.BulletSpeed,
				CritHitChance = (int)item.CritHitChance,
				AbilityHaste = (int)item.AbilityHaste,
				AttackPower = (int)item.AttackPower,
				MagicPower = (int)item.MagicPower,
				Tier = (GeneralGame.Tier)item.Tier,
				DamageBalance = item.DamageBalance,
				Durability = item.Durability,
				AttackSpeed = (int)item.AttackSpeed,
				MoveSpeed = (int)item.MoveSpeed,
				Armor = (int)item.Armor,
				MagicDefense = (int)item.MagicDefense,
				Evasion = (int)item.Evasion,
				Cover = (int)item.Cover,
				BonusEXP = (int)item.BonusEXP,
				BonusScore = (int)item.BonusScore,
				BonusVyndalium = (int)item.BonusVyndalium,
				Tenacity = (int)item.Tenacity,
				StunResistance = (int)item.StunResistance,
				BlindResistance = (int)item.BlindResistance,
				SlowResistence = (int)item.SlowResistence,
				FireResistence = (int)item.FireResistence,
				BleedResistance = (int)item.BleedResistance,
				PoisonResistence = (int)item.PoisonResistence,
				IceResistence = (int)item.IceResistence,
				LightningResistence = (int)item.LightningResistence,
				HolyResistence = (int)item.HolyResistence,
				ShadowResistence = (int)item.ShadowResistence,
				MinArmorValue = (int)item.MinArmorValue,
				MaxArmorValue = (int)item.MaxArmorValue,
				MinAttackValue = (int)item.MinAttackValue,
				MaxAttackValue = (int)item.MaxAttackValue,

				


			};
		}
		if (player == null)
		{
			Log.Error("Player is null.");
			return;
		}

		if (player.Inventory == null)
		{
			Log.Error("Player inventory is null.");
			return;
		}


		_saveData = save with
		{
			Stamina = player.Stamina,
			ArmorPenetration = player.ArmorPenetration,
			AttackRange = player.AttackRange,
			BonusVyndalium = player.BonusVyndalium,
			MagicPenetration = player.MagicPenetration,
			AbilityHaste = player.AbilityHaste,
			PlayerWalkSpeed = player.PlayerWalkSpeed,
			PlayerRunSpeed = player.PlayerRunSpeed,
			DefaultAmmo = player.DefaultAmmo,
			Life  = player.RespawnAttempts,
			MouseSensitivity = player.MouseSensitivity,
			MinArmorValue = player.MinArmorValue,
			MaxArmorValue = player.MaxArmorValue,
			MinAttackValue = player.MinAttackValue,
			MaxAttackValue = player.MaxAttackValue,
		
			DefaultFOV = player.DefaultFov,
			PrestigeLevel = player.PrestigeLevel,

			AmmoCount = player.AmmoContainer.AmmoCount,
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

			StrengthCost = (int)player.StrengthCost,
			AttackPowerCost = (int)player.AttackPowerCost,
			ArmorPenetrationCost = (int)player.ArmorPenetrationCost,
			AttackRangeCost = (int)player.AttackRangeCost,
			AttackSpeedCost = (int)player.AttackSpeedCost,
			CriticalChanceCost = (int)player.CriticalChanceCost,
			CriticalDamageCost = (int)player.CriticalDamageCost,
			DexterityCost = (int)player.DexterityCost,
			EvasionCost = (int)player.EvasionCost,
			AbilityHasteCost = (int)player.AbilityHasteCost,
			StaminaCost = (int)player.StaminaCost,
			PlayerWalkSpeedCost = (int)player.PlayerWalkSpeedCost,
			PlayerRunSpeedCost = player.PlayerRunSpeedCost,
			ManaCost = (int)player.ManaCost,
			IntelligenceCost = (int)player.IntelligenceCost,
			MagicPowerCost = (int)player.MagicPowerCost,
			MagicPenetrationCost = (int)player.MagicPenetrationCost,
			BonusEXPGainCost = (int)player.BonusEXPGainCost,
			BonusVyndaliumGainCost = (int)player.BonusVyndaliumGainCost,


			Clothes = player.Inventory?.EquippedItems?
			.Where( x => x != null )
			.Select( Serialize )
			.ToArray() ?? Array.Empty<ItemSave>(),
					Inventory = player.Inventory?.BackpackItems?
			.Where( x => x != null )
			.Select( Serialize )
			.ToArray() ?? Array.Empty<ItemSave>(),
					StorageItems = player.Inventory?.StorageItems?
			.Where( x => x != null )
			.Select( Serialize )
			.ToArray() ?? Array.Empty<ItemSave>(),
					BackpackBagItems = player.Inventory?.BackpackBagItems?
			.Where( x => x != null )
			.Select( Serialize )
			.ToArray() ?? Array.Empty<ItemSave>(),


		};

		
		

		

		// Write save.
		WriteSave( _saveData.Value );
		
	}

	[ConCmd( "newgame_save" )]
	public static void SavePlayer()
	{
		Log.Info( "Saving player..." );
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

		var save = tuple.Save;

		if ( player.Inventory == null )
		{
			Log.Error( "Player.Inventory is null." );
			return false;
		}

		if ( player.Weapons == null )
		{
			Log.Error( "Player.Weapons is null." );
			return false;
		}
		// Stellen Sie sicher, dass save.AmmoContainerData initialisiert wurde


		if ( player.AmmoContainer == null )
		{
			player.AmmoContainer = new AmmoContainer();
		}

		if ( save.AmmoCount != null )
		{
			player.AmmoContainer.AmmoCount = save.AmmoCount;
			foreach ( var ammo in save.AmmoCount )
			{
				player.AmmoContainer.SetAmmoCount( ammo.Key, ammo.Value );
			}
		}
		else
		{
			player.AmmoContainer.AmmoCount = new Dictionary<AmmoType, int>();
		}
		// Setup basic player information.
		player.MouseSensitivity = save.MouseSensitivity;
		player.DefaultFov = save.DefaultFOV;
		player.Stamina = save.Stamina;
		player.ArmorPenetration = save.ArmorPenetration;
		player.AttackRange = save.AttackRange;
		player.BonusVyndalium = save.BonusVyndalium;
		player.MagicPenetration = save.MagicPenetration;
		player.AbilityHaste = save.AbilityHaste;
		player.PlayerWalkSpeed = save.PlayerWalkSpeed;
		player.PlayerRunSpeed = save.PlayerRunSpeed;
		player.MinArmorValue = save.MinArmorValue;
		player.MaxArmorValue = save.MaxArmorValue;
		player.MinAttackValue = save.MinAttackValue;
		player.MaxAttackValue = save.MaxAttackValue;
		player.DefaultAmmo = save.DefaultAmmo;
		player.RespawnAttempts = (int)save.Life;
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
		player.PrestigeLevel = save.PrestigeLevel;

		
		player.StrengthCost = save.StrengthCost;
        player.AttackPowerCost = save.AttackPowerCost;
        player.ArmorPenetrationCost = save.ArmorPenetrationCost;
		player.AttackRangeCost = save.AttackRangeCost;
		player.AttackSpeedCost = save.AttackSpeedCost;
		player.CriticalChanceCost = save.CriticalChanceCost;
		player.CriticalDamageCost = save.CriticalDamageCost;




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
				var item = obj.Components.Get<ItemComponent>();
				if ( item != null )
				{
					item.Aspect = data.Aspect;
					item.IsFavorite = data.IsFavorite;
					item.IsBackpack = data.IsBackpack;
					item.Description = data.Description;
					item.RequiredLevel = data.RequiredLevel;	
					item.State = data.State;
					item.SellPrice = (int)data.SellPrice;
					item.BuyPrice = (int)data.BuyPrice;
					item.MaxStack = data.MaxStack;
					item.Count = data.Count;
					item.DMG = data.DMG;
					item.STG = data.STG;
					item.MinArmorValue = data.MinArmorValue;
					item.MaxArmorValue = data.MaxArmorValue;
					item.MinAttackValue = data.MinAttackValue;
					item.MaxAttackValue = data.MaxAttackValue;
					item.HE = data.HE;
					item.DEX = data.DEX;
					item.PER = data.PER;
					item.INT = data.INT;
					item.Mana = data.Mana;
					item.Health = data.Health;
					item.ItemLevel = data.ItemLevel;
					item.CritHitDamage = data.CritHitDamage;
					item.FireRate = data.FireRate;
					item.BulletSpeed = data.BulletSpeed;
					item.CritHitChance = data.CritHitChance;
					item.AbilityHaste = data.AbilityHaste;
					item.AttackPower = data.AttackPower;
					item.MagicPower = data.MagicPower;
					item.Tier = (GeneralGame.Tier)data.Tier;
					item.DamageBalance = data.DamageBalance;
					item.Durability = data.Durability;
					item.AttackSpeed = data.AttackSpeed;
					item.MoveSpeed = data.MoveSpeed;
					item.Armor = data.Armor;
					item.MagicDefense = data.MagicDefense;
					item.Evasion = data.Evasion;
					item.Cover = data.Cover;
					item.BonusEXP = data.BonusEXP;
					item.BonusScore = data.BonusScore;
					item.BonusVyndalium = data.BonusVyndalium;
					item.Tenacity = data.Tenacity;
					item.StunResistance = data.StunResistance;
					item.BlindResistance = data.BlindResistance;
					item.SlowResistence = data.SlowResistence;
					item.FireResistence = data.FireResistence;
					item.BleedResistance = data.BleedResistance;
					item.PoisonResistence = data.PoisonResistence;
					item.IceResistence = data.IceResistence;
					item.LightningResistence = data.LightningResistence;
					item.HolyResistence = data.HolyResistence;
					item.ShadowResistence = data.ShadowResistence;

				}
				
			}
		}
		
		
		// Go through all clothes.
		if ( save.Clothes != null )
			foreach ( var data in save.Clothes )
			{
				if ( !ResourceLibrary.TryGet<PrefabFile>( data.Path, out var prefab ) )
					continue;

				var o = SceneUtility.GetPrefabScene(prefab).Clone();
				o.NetworkMode = NetworkMode.Object;
				if (!o.Network.Active) o.NetworkSpawn();
				var equipment = o.Components.Get<ItemEquipment>();
				if (equipment == null)
					continue;

				player?.Inventory?.EquipItemFromWorld( equipment );
				ReadData( data, o );

				equipment.Aspect = data.Aspect;
				equipment.IsBackpack = data.IsBackpack;
				equipment.IsFavorite = data.IsFavorite;
				equipment.Description = data.Description;
				equipment.RequiredLevel = data.RequiredLevel;
				equipment.MinArmorValue = data.MinArmorValue;
				equipment.MaxArmorValue = data.MaxArmorValue;
				equipment.MinAttackValue = data.MinAttackValue;
				equipment.MaxAttackValue = data.MaxAttackValue;
				equipment.FireRate = data.FireRate;
				equipment.State = data.State;
				equipment.MaxStack = data.MaxStack;
				equipment.Count = data.Count;
				equipment.SellPrice = (int)data.SellPrice;
				equipment.BuyPrice = (int)data.BuyPrice;
				equipment.DMG = data.DMG;
				equipment.STG = data.STG;
				equipment.BulletSpeed = data.BulletSpeed;
				equipment.HE = data.HE;
				equipment.DEX = data.DEX;
				equipment.PER = data.PER;
				equipment.INT = data.INT;
				equipment.Mana = data.Mana;
				equipment.Health = data.Health;
				equipment.ItemLevel = data.ItemLevel;
				equipment.CritHitDamage = data.CritHitDamage;
				equipment.CritHitChance = data.CritHitChance;
				equipment.AbilityHaste = data.AbilityHaste;
				equipment.AttackPower = data.AttackPower;
				equipment.MagicPower = data.MagicPower;
				equipment.Tier = (GeneralGame.Tier)data.Tier;
				equipment.DamageBalance = data.DamageBalance;
				equipment.Durability = data.Durability;
				equipment.AttackSpeed = data.AttackSpeed;
				equipment.MoveSpeed = data.MoveSpeed;
				equipment.Armor = data.Armor;
				equipment.MagicDefense = data.MagicDefense;
				equipment.Evasion = data.Evasion;
				equipment.Cover = data.Cover;
				equipment.BonusEXP = data.BonusEXP;
				equipment.BonusScore = data.BonusScore;
				equipment.BonusVyndalium = data.BonusVyndalium;
				equipment.Tenacity = data.Tenacity;
				equipment.StunResistance = data.StunResistance;
				equipment.BlindResistance = data.BlindResistance;
				equipment.SlowResistence = data.SlowResistence;
				equipment.FireResistence = data.FireResistence;
				equipment.BleedResistance = data.BleedResistance;
				equipment.PoisonResistence = data.PoisonResistence;
				equipment.IceResistence = data.IceResistence;
				equipment.LightningResistence = data.LightningResistence;
				equipment.HolyResistence = data.HolyResistence;
				equipment.ShadowResistence = data.ShadowResistence;

				// Debug-Ausgabe nach dem Setzen

			}



		// Go through all items.
		if ( save.Inventory != null )
		{
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
				player.Inventory?.SetItem( item, data.Index );
				ReadData( data, o );
				
				item.Aspect = data.Aspect;
				item.IsBackpack = data.IsBackpack;
				item.IsFavorite = data.IsFavorite;
				item.Description = data.Description;
				item.RequiredLevel = data.RequiredLevel;	
				item.MinArmorValue = data.MinArmorValue;
				item.MaxArmorValue = data.MaxArmorValue;
				item.MinAttackValue = data.MinAttackValue;
				item.MaxAttackValue = data.MaxAttackValue;
				item.FireRate = data.FireRate;
				item.MaxStack = data.MaxStack;
				item.Count = data.Count;
				item.SellPrice = (int)data.SellPrice;
				item.BuyPrice = (int)data.BuyPrice;
				item.BulletSpeed = data.BulletSpeed;
				item.DMG = data.DMG;
				item.STG = data.STG;
				item.HE = data.HE;
				item.DEX = data.DEX;
				item.PER = data.PER;
				item.INT = data.INT;
				item.Mana = data.Mana;
				item.Health = data.Health;
				item.ItemLevel = data.ItemLevel;
				item.CritHitDamage = data.CritHitDamage;
				item.CritHitChance = data.CritHitChance;
				item.AbilityHaste = data.AbilityHaste;
				item.AttackPower = data.AttackPower;
				item.MagicPower = data.MagicPower;
				item.Tier = (GeneralGame.Tier)data.Tier;
				item.DamageBalance = data.DamageBalance;
				item.Durability = data.Durability;
				item.AttackSpeed = data.AttackSpeed;
				item.MoveSpeed = data.MoveSpeed;
				item.Armor = data.Armor;
				item.MagicDefense = data.MagicDefense;
				item.Evasion = data.Evasion;
				item.Cover = data.Cover;
				item.BonusEXP = data.BonusEXP;
				item.BonusScore = data.BonusScore;
				item.BonusVyndalium = data.BonusVyndalium;
				item.Tenacity = data.Tenacity;
				item.StunResistance = data.StunResistance;
				item.BlindResistance = data.BlindResistance;
				item.SlowResistence = data.SlowResistence;
				item.FireResistence = data.FireResistence;
				item.BleedResistance = data.BleedResistance;
				item.PoisonResistence = data.PoisonResistence;
				item.IceResistence = data.IceResistence;
				item.LightningResistence = data.LightningResistence;
				item.HolyResistence = data.HolyResistence;
				item.ShadowResistence = data.ShadowResistence;
				
			}
		}
		if ( save.StorageItems != null )
		{
			foreach ( var data in save.StorageItems )
			{

				if ( !ResourceLibrary.TryGet<PrefabFile>( data.Path, out var prefab ) )
					continue;
				var o = SceneUtility.GetPrefabScene( prefab ).Clone();
				o.NetworkMode = NetworkMode.Object;
				if ( !o.Network.Active ) o.NetworkSpawn();
				var item = o.Components.Get<ItemComponent>();
				if ( item == null )
					continue;
				player.Inventory.GiveStorageItem( item, data.Index );
				ReadData( data, o );
				item.IsBackpack = data.IsBackpack;
				item.Aspect = data.Aspect;
				item.IsFavorite = data.IsFavorite;
				item.Description = data.Description;
				item.MaxStack = data.MaxStack;
				item.Count = data.Count;
				item.MinArmorValue = data.MinArmorValue;
				item.MaxArmorValue = data.MaxArmorValue;
				item.MinAttackValue = data.MinAttackValue;
				item.MaxAttackValue = data.MaxAttackValue;
				item.RequiredLevel = data.RequiredLevel;
				item.FireRate = data.FireRate;
				item.SellPrice = (int)data.SellPrice;
				item.BuyPrice = (int)data.BuyPrice;
				item.DMG = data.DMG;
				item.BulletSpeed = data.BulletSpeed;
				item.STG = data.STG;
				item.HE = data.HE;
				item.DEX = data.DEX;
				item.PER = data.PER;
				item.INT = data.INT;
				item.Mana = data.Mana;
				item.Health = data.Health;
				item.ItemLevel = data.ItemLevel;
				item.CritHitDamage = data.CritHitDamage;
				item.CritHitChance = data.CritHitChance;
				item.AbilityHaste = data.AbilityHaste;
				item.AttackPower = data.AttackPower;
				item.MagicPower = data.MagicPower;
				item.Tier = (GeneralGame.Tier)data.Tier;
				item.DamageBalance = data.DamageBalance;
				item.Durability = data.Durability;
				item.AttackSpeed = data.AttackSpeed;
				item.MoveSpeed = data.MoveSpeed;
				item.Armor = data.Armor;
				item.MagicDefense = data.MagicDefense;
				item.Evasion = data.Evasion;
				item.Cover = data.Cover;
				item.BonusEXP = data.BonusEXP;
				item.BonusScore = data.BonusScore;
				item.BonusVyndalium = data.BonusVyndalium;
				item.Tenacity = data.Tenacity;
				item.StunResistance = data.StunResistance;
				item.BlindResistance = data.BlindResistance;
				item.SlowResistence = data.SlowResistence;
				item.FireResistence = data.FireResistence;
				item.BleedResistance = data.BleedResistance;
				item.PoisonResistence = data.PoisonResistence;
				item.IceResistence = data.IceResistence;
				item.LightningResistence = data.LightningResistence;
				item.HolyResistence = data.HolyResistence;
				item.ShadowResistence = data.ShadowResistence;

			}
		}
		
		if ( save.BackpackBagItems != null )
		{
			foreach ( var data in save.BackpackBagItems )
			{
				if ( !ResourceLibrary.TryGet<PrefabFile>( data.Path, out var prefab ) )
					continue;
				var o = SceneUtility.GetPrefabScene( prefab ).Clone();
				o.NetworkMode = NetworkMode.Object;
				if ( !o.Network.Active ) o.NetworkSpawn();
				var item = o.Components.Get<ItemComponent>();
				if ( item == null )
					continue;
				player.Inventory?.GiveBackpackBagItem( item, data.Index );
				ReadData( data, o );
				item.IsBackpack = data.IsBackpack;
				item.Aspect = data.Aspect;
				item.IsFavorite = data.IsFavorite;
				item.Description = data.Description;
				item.MaxStack = data.MaxStack;
				item.Count = data.Count;
				item.MinArmorValue = data.MinArmorValue;
				item.MaxArmorValue = data.MaxArmorValue;
				item.MinAttackValue = data.MinAttackValue;
				item.MaxAttackValue = data.MaxAttackValue;
				item.RequiredLevel = data.RequiredLevel;
				item.FireRate = data.FireRate;
				item.SellPrice = (int)data.SellPrice;
				item.BuyPrice = (int)data.BuyPrice;
				item.DMG = data.DMG;
				item.STG = data.STG;
				item.HE = data.HE;
				item.DEX = data.DEX;
				item.PER = data.PER;
				item.INT = data.INT;
				item.BulletSpeed = data.BulletSpeed;
				item.Mana = data.Mana;
				item.Health = data.Health;
				item.ItemLevel = data.ItemLevel;
				item.CritHitDamage = data.CritHitDamage;
				item.CritHitChance = data.CritHitChance;
				item.AbilityHaste = data.AbilityHaste;
				item.AttackPower = data.AttackPower;
				item.MagicPower = data.MagicPower;
				item.Tier = (GeneralGame.Tier)data.Tier;
				item.DamageBalance = data.DamageBalance;
				item.Durability = data.Durability;
				item.AttackSpeed = data.AttackSpeed;
				item.MoveSpeed = data.MoveSpeed;
				item.Armor = data.Armor;
				item.MagicDefense = data.MagicDefense;
				item.Evasion = data.Evasion;
				item.Cover = data.Cover;
				item.BonusEXP = data.BonusEXP;
				item.BonusScore = data.BonusScore;
				item.BonusVyndalium = data.BonusVyndalium;
				item.Tenacity = data.Tenacity;
				item.StunResistance = data.StunResistance;
				item.BlindResistance = data.BlindResistance;
				item.SlowResistence = data.SlowResistence;
				item.FireResistence = data.FireResistence;
				item.BleedResistance = data.BleedResistance;
				item.PoisonResistence = data.PoisonResistence;
				item.IceResistence = data.IceResistence;
				item.LightningResistence = data.LightningResistence;
				item.HolyResistence = data.HolyResistence;
				item.ShadowResistence = data.ShadowResistence;
			}
		}
		return true;
	}
}
		
	

