using GeneralGame.HUD;
using Sandbox;
using System;

namespace GeneralGame
{
	public sealed class VyndaliumTrigger : Component, Component.ITriggerListener
	{
		[Property] public SoundEvent TriggerSoundPath { get; set; } 
		[Property] private int MinVyndaliumAmount { get; set; }
		[Property] private int MaxVyndaliumAmount { get; set; }
		
		public void OnTriggerEnter(Collider other)
		{
			var player = other.Components.Get<Player>();
			if (player != null)
			{
				int addvydalium = new Random().Next(MinVyndaliumAmount, MaxVyndaliumAmount);
				player.GiveVyndalium(addvydalium);
				int addXP = new Random().Next(50, 525);
				player.GiveXp(addXP);

				
				

				Sound.Play(TriggerSoundPath, Transform.Position);

				GameObject.Destroy();
			}
		}

		public void OnTriggerExit( Collider other )
		{
			// Hier k�nnen Sie ggf. eine Aktion hinzuf�gen, die ausgef�hrt wird,
			// wenn der Spieler den Triggerbereich verl�sst.
		}
	}
	
}
