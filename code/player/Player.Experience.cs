using GeneralGame.HUD;
using GeneralGame;

namespace GeneralGame;

public partial class Player
{
	[Property, Sync, Category("Parameters")]
	public long Experience
	{
		get => _experience;
		set => _experience = value.Clamp(0, long.MaxValue);
	}

	[Property, Sync, Category( "Parameters" )]
	public int Level
	{
		get => _level;
		set => _level = value.Clamp(0, GetMaxLevel());
	}
	[Sync, Property , Category("Parameters")]public int PrestigeLevel { get; set; } = 0;
	private int _level;
	private long _experience;
	[Property] public SoundEvent LevelUp { get; set; }
	public int GetMaxLevel()
	{
		return 105 + (PrestigeLevel * 100);
	}

	public long ExpPerLevel => (long)Math.Floor(Math.Pow(1.1, Level) * 100) + 150;

	public static List<(int MinLevel, string Name, string Color)> Ranks = new()
	{
		(105, "God", "rgb(255, 0, 0)"), // Rot
		(100, "Grand Sage", "rgb(255, 140, 0)"), // Dunkelorange
		(95, "Celestial", "rgb(255, 215, 0)"), // Gold
		(90, "Eternal", "rgb(255, 69, 0)"), // Orangerot
		(85, "Divine", "rgb(255, 20, 147)"), // Tiefes Pink
		(80, "Immortal", "rgb(255, 255, 255)"), // Weiß
		(75, "Empyrean", "rgb(173, 216, 230)"), // Hellblau
		(70, "Demi-God", "rgb(0, 191, 255)"), // Himmelblau
		(65, "Demon", "rgb(0, 255, 255)"), // Cyan
		(60, "Emporer", "rgb(0, 255, 127)"), // Frühlingsgrün
		(55, "Lord", "rgb(0, 255, 0)"), // Grün
		(50, "Hero", "rgb(127, 255, 0)"), // Gelbgrün
		(45, "Paladin", "rgb(255, 255, 0)"), // Gelb
		(40, "Champion", "rgb(255, 165, 0)"), // Orange
		(35, "Beserker", "rgb(255, 105, 180)"), // Hotpink
		(30, "Warrior", "rgb(255, 20, 147)"), // Tiefes Pink
		(25, "Traveler", "rgb(255, 0, 255)"), // Magenta
		(20, "Novize", "rgb(138, 43, 226)"), // Blauviolett
		(15, "Adventurer", "rgb(34, 139, 34)"), // Waldgrün
		(10, "Apprentice", "rgb(255, 255, 255)"), // Weiß
		(5, "Recrute", "rgb(255, 255, 224)"), // Hellgelb
		(1, "Beginner", "rgb(240, 255, 255)"), // Azur
		(0, "Newbie", "rgb(255, 255, 255)") // Weiß
	};
	public string GetRankColor()
	{
		var rank = Ranks.FirstOrDefault( r => r.MinLevel <= Level );
		return rank.Color;
	}

	public event Action<int> OnExperienceEarned;
	public event Action<int> OnLevelUp;

	public string GetRankName() => Ranks.First( rank => rank.MinLevel <= Level ).Name;

	public void AddExperience( int exp )
	{
		Experience += exp;

		var oldLevel = Level;
		while ( Experience >= ExpPerLevel )
		{
			if ( Level == 105 )
				break;
			Experience -= ExpPerLevel;
			Level++;
			StatsPoints += 3;

			if ( LevelUp is null )
				return;

			Sound.Play( LevelUp, WorldPosition );
		}

		OnExperienceEarned?.Invoke( exp );
		if ( oldLevel != Level )
		{
			OnLevelUp?.Invoke( Level );

		}
	}
	public void PrestigeRankUp()
	{
		if (Level == 105)
		{
			Level = 1;
			Experience = 0;
			PrestigeLevel++;
			MaxHealth = 50;
			Health = 50;
			MaxMana = 50;
			Mana = 50;
			StatsPoints = 0;
			MaxStamina = 50;
			Stamina = 50;
			AttackPower = 0;
			Tenacity = 0;
			Block = 0;
			MagicDefense = 0;
			Armor = 0;
			STG = 0;
			ArmorPenetration = 0;
			MagicPenetration = 0;
			AttackRange = 0;
			AttackSpeed = 0;
			CritHitChance = 0;
			CritHitDamage = 0;
			DEX = 0;
			Evasion = 0;
			AbilityHaste = 0;
			PlayerWalkSpeed = 125;
			PlayerRunSpeed = 150;
			INT = 0;
			MagicPower = 0;
			MagicPenetration = 0;
			BonusVyndalium = 50 + (PrestigeLevel * 10); // Erhöht um 10 pro Prestige-Level
			BonusEXPGain = 100 + (PrestigeLevel * 20);





			Log.Info($"Player has prestiged to Prestige Level {PrestigeLevel}");
		}
	}
	public void ResetRankUp()
	{
		
		{
		
		
		
			MaxHealth = 50;
			MinArmorValue = 0;
			MaxArmorValue = 0;
			MinAttackValue = 0;
			MaxAttackValue = 0;
			Health = 50;
			MaxMana = 50;
			Mana = 50;
			StatsPoints = 0;
			MaxStamina = 50;
			Stamina = 50;
			AttackPower = 0;
			Tenacity = 0;
			Block = 0;
			MagicDefense = 0;
			Armor = 0;
			STG = 0;
			ArmorPenetration = 0;
			MagicPenetration = 0;
			AttackRange = 0;
			AttackSpeed = 0;
			CritHitChance = 0;
			CritHitDamage = 0;
			DEX = 0;
			Evasion = 0;
			AbilityHaste = 0;
			PlayerWalkSpeed = 125;
			PlayerRunSpeed = 170;
			INT = 0;
			MagicPower = 0;
			MagicPenetration = 0;
			BonusVyndalium = 50 + (PrestigeLevel * 10); // Erhöht um 10 pro Prestige-Level
			BonusEXPGain = 100 + (PrestigeLevel * 20);





			Log.Info($"Player has prestiged to Prestige Level {PrestigeLevel}");
		}
	}


}
