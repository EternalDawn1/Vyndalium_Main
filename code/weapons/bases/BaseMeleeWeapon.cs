using Sandbox;
using System;
using System.Numerics;

namespace GeneralGame
{
    public class BaseMeleeWeapon : BaseGun
    {
        [Property, Category( "Parameters" )] public float MeleeRange { get; set; } = 1.5f;
        [Property, Category( "Parameters" )] public float MeleeDamage { get; set; } = 10f;
        [Property, Category( "Parameters" )] public float MeleeCooldown { get; set; } = 1f;
        public TimeUntil NextMeleeAttackTime { get; set; }

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

        public override void OnEquip( Player player )
        {
            base.OnEquip( player );
            // Zusätzliche Logik für das Ausrüsten der Nahkampfwaffe
        }

        
    }
}