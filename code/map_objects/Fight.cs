using Sandbox;
namespace GeneralGame


{
	public sealed class Fight : Component, Component.ITriggerListener
	{
		public void OnTriggerEnter( Component other )
		{
			var player = other.Components.Get<Player>();
			if ( player != null )
			{
				Log.Info( "Player entered the fight zone" );
				
			}
		}

		public void OnExit( Component other )
		{
			var player = other.Components.Get<Player>();
			if ( player != null )
			{
				Log.Info( "Player entered the fight zone" );
				GameObject.Destroy();
			}
		}

	}
	}


