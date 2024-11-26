using System;
using System.Linq;
using System.Threading.Tasks;
using Sandbox;

namespace GeneralGame;

public static class Extensions
{
	public static async void PlayUntilFinished( this SceneParticles particles, TaskSource source )
	{
		try
		{
			while ( !particles.Finished )
			{
				await source.Frame();
				particles.Simulate( Time.Delta );
			}
		}
		catch ( TaskCanceledException )
		{
			// Do nothing.
		}
		particles.Delete();
	}
	

}
public static class FloatExtensions
{
	public static string Timer( this float value )
	{
		var timeSpan = TimeSpan.FromSeconds( value );
		return string.Format( "{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds );
	}
}

