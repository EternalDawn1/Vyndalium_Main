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
	
}
