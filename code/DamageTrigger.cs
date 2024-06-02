using Sandbox;
using System;

namespace GeneralGame
{
    public sealed class DamageTrigger : Component, Component.ITriggerListener
    {
        [Property] public SoundEvent TriggerSoundPath { get; set; } 
        
        [Property] float Amount { get; set; } = 10f;

        private float timer = 0f;
        private bool isPlayerInside = false;
        private Player playerInside;

		protected override void OnUpdate()
		{
			base.OnUpdate();
			timer += Time.Delta;

            if (timer >= 1f && isPlayerInside && playerInside != null)
            {
                playerInside.TakeDamage(DamageType.Bullet, Amount, new Vector3(), new Vector3(), new Guid(), GameObject.Id);
                Sound.Play(TriggerSoundPath, Transform.Position);
                timer = 0f;
            }
		}
        

        public void OnTriggerEnter(Collider other)
        {
            var player = other.Components.Get<Player>();
            if (player != null)
            {
                isPlayerInside = true;
                playerInside = player;
            }
        }

        public void OnTriggerExit(Collider other)
        {
            var player = other.Components.Get<Player>();
            if (player != null)
            {
                isPlayerInside = false;
                playerInside = null;
            }
        }
    }
}