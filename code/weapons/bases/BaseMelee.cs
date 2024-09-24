using Sandbox;
using System;
using System.Numerics;

namespace GeneralGame
{
	public class BaseMelee : BaseGun
	{
		[Property, Category( "Parameters" )] public float MeleeRange { get; set; } = 1.5f;
		[Property, Category( "Parameters" )] public float MeleeDamage { get; set; } = 10f;
		[Property, Category( "Parameters" )] public float MeleeCooldown { get; set; } = 1f;
		
		public TimeUntil NextMeleeAttackTime { get; set; }
		
		GameObject Hitprefab;
		
		protected override void OnStart()
		{
			// Standardmunition setzen, wenn sie nicht bereits gesetzt ist
			
			Hitprefab = SceneUtility.GetPrefabScene( ResourceLibrary.Get<PrefabFile>( "prefabs/hitinfo.prefab" ) );

			Components.GetOrCreate<Interactions>();

			base.OnStart();
			
			
		}
		
		

		protected override void OnDeployed()
		{
			base.OnDeployed();
			
		}

		protected override void OnHolstered()
		{
			base.OnHolstered();

			// Die Waffe wird nicht mehr gehalten
			

			EffectRenderer.Set( "b_empty", false );
		}

		public override void PrimaryAction()
		{
			if ( NextMeleeAttackTime > 0 ) return;

			var player = Owner as Player;
			if ( player == null ) return;

			var startPos = player.PlyCamera.Transform.Position;
			var direction = player.PlyCamera.Transform.Rotation.Forward;
			var endPos = startPos + direction * MeleeRange;

			var trace = Scene.Trace.Ray( startPos, endPos )
				.IgnoreGameObject( player.GameObject )
				.WithTag( "enemy" )
				.Run();

			if ( trace.Hit )
			{
				var damageable = trace.Component.Components.Get<IHealthComponent>();
				if ( damageable != null )
				{
					damageable.TakeDamage( DamageType.Serious, MeleeDamage, trace.EndPosition, trace.Direction * HitForce, GameObject.Id, GameObject.Id );
				}
			}

			NextMeleeAttackTime = MeleeCooldown;
		}

		public override void SecondaryAction()
		{
			// Implementiere hier eine sekundäre Nahkampfaktion, falls erforderlich
		}

		
		protected override void OnUpdate()
		{
			


			base.OnUpdate();
		}
	}
}