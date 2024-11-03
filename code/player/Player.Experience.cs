using GeneralGame.HUD;
using GeneralGame;

namespace GeneralGame;

public partial class Player
{
	[Property, Sync, Category( "Parameters" )]
	public int Experience
	{
		get => _experience;
		set => _experience = value.Clamp( 0, int.MaxValue );
	}

	[Property, Sync, Category( "Parameters" )]
	public int Level
	{
		get => _level;
		set => _level = value.Clamp( 0, 105 );
	}

	private int _level;
	private int _experience;
	[Property] public SoundEvent LevelUp { get; set; }

	public int ExpPerLevel => (int)Math.Floor( Math.Pow( 1.2, Level ) * 100 ) + 150;

	public static List<(int MinLevel, string Name, string Color)> Ranks = new()
	{
		(105, "Legend", "rgb(255, 0, 0)"), // Rot
		(100, "Mythic", "rgb(255, 140, 0)"), // Dunkelorange
		(95, "Eternal", "rgb(255, 215, 0)"), // Gold
		(90, "Immortal", "rgb(255, 69, 0)"), // Orangerot
		(85, "Divine", "rgb(255, 20, 147)"), // Tiefes Pink
		(80, "Eclipse", "rgb(255, 255, 255)"), // Weiß
		(75, "Zen", "rgb(173, 216, 230)"), // Hellblau
		(70, "Ascendant", "rgb(0, 191, 255)"), // Himmelblau
		(65, "Celestial", "rgb(0, 255, 255)"), // Cyan
		(60, "Paragon", "rgb(0, 255, 127)"), // Frühlingsgrün
		(55, "Vanguard", "rgb(0, 255, 0)"), // Grün
		(50, "Sentinel", "rgb(127, 255, 0)"), // Gelbgrün
		(45, "Guardian", "rgb(255, 255, 0)"), // Gelb
		(40, "Warden", "rgb(255, 165, 0)"), // Orange
		(35, "Protector", "rgb(255, 105, 180)"), // Hotpink
		(30, "Champion", "rgb(255, 20, 147)"), // Tiefes Pink
		(25, "Hero", "rgb(255, 0, 255)"), // Magenta
		(20, "Conqueror", "rgb(138, 43, 226)"), // Blauviolett
		(19, "Vanquisher", "rgb(75, 0, 130)"), // Indigo
		(18, "Slayer", "rgb(0, 0, 255)"), // Blau
		(17, "Destroyer", "rgb(0, 0, 139)"), // Dunkelblau
		(16, "Ravager", "rgb(0, 100, 0)"), // Dunkelgrün
		(15, "Berserker", "rgb(34, 139, 34)"), // Waldgrün
		(14, "Warrior", "rgb(0, 128, 128)"), // Teal
		(13, "Fighter", "rgb(0, 255, 255)"), // Cyan
		(12, "Striker", "rgb(0, 191, 255)"), // Himmelblau
		(11, "Blaster", "rgb(173, 216, 230)"), // Hellblau
		(10, "Leader", "rgb(255, 255, 255)"), // Weiß
		(9, "Commander", "rgb(255, 228, 196)"), // Bisque
		(8, "Captain", "rgb(255, 218, 185)"), // Pfirsichpuff
		(7, "Lieutenant", "rgb(255, 222, 173)"), // Navajoweiß
		(6, "Sergeant", "rgb(255, 250, 205)"), // Zitronencreme
		(5, "Corporal", "rgb(255, 255, 224)"), // Hellgelb
		(4, "Private", "rgb(255, 255, 240)"), // Elfenbein
		(3, "Recruit", "rgb(240, 255, 240)"), // Honigtau
		(2, "Novice", "rgb(245, 255, 250)"), // Minzcreme
		(1, "Apprentice", "rgb(240, 255, 255)"), // Azur
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
	
}
