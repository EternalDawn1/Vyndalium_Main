using System;
using Sandbox;

namespace GeneralGame
{
	public interface IHealthComponent
	{
		public LifeState LifeState { get; }

		public float MaxHealth { get; }
		public float Health { get; }


		public void TakeDamage(DamageType type, float amount, Vector3 hitPosition, Vector3 hitDirection, Guid attackerId, Guid playerId);
	}
}
