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

	public int ExpPerLevel => (int)Math.Floor( Math.Pow( 1.1, Level ) * 100 ) + 150;

	public static List<(int MinLevel, string Name)> Ranks = new()
	{
		(99, "Eclipse"),
		(80, "Zen"),
		(60, "Striker"),
		(40, "blaster"),
		(20, "leader"),
		(10, "champion"),
		(0, "adventurer")
	};

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
			StatsPoints++;

			if ( LevelUp is null )
				return;

			Sound.Play( LevelUp, Transform.Position );
		}

		OnExperienceEarned?.Invoke( exp );
		if ( oldLevel != Level )
		{
			OnLevelUp?.Invoke( Level );

		}
	}
	
}
