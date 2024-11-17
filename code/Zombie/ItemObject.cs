using GeneralGame;
using GeneralGame.HUD;
using Sandbox;
using System;

namespace GeneralGame
{
    public partial class ItemObject : Component, IHealthComponent
    {
        [Property]public float Health {get; set;} = 100f;
        
        [Property] public float MaxHealth { get; set; } = 100f;
        [Property] public int XpReward { get; set; } = 50;
        [Property] public int VyndaliumReward { get; set; } = 10;
        [Property] public GameObject Ragdoll { get; set; }
        [Property] public string Name { get; set; } 
        
      
        private static readonly Random random = new Random();
       
        [Property] public SoundEvent DeathSound { get; set; }
        public event Action OnTakeDamage;
        public event Action OnDeath;
        public GameObject Hitprefab { get; set; }
        public LifeState LifeState { get; set; } = LifeState.Alive;
       
        [Property]
        private readonly List<string> prefabPaths = new List<string>
        {
            "prefabs/potions/potion_small.prefab",
            "prebabs/pickupammo.prefab",
            "prefabs/items/wood_log.prefab",       // 30% Wahrscheinlichkeit
            "prefabs/entitys/chestsystem/example1.prefab",
             // 5% Wahrscheinlichkeit
            null // Restliche Wahrscheinlichkeit (45%) für nichts
        };

        private readonly List<float> probabilities = new List<float>
        {
            0.4f, // 10% Wahrscheinlichkeit für Munition
            0.3f, // 20% Wahrscheinlichkeit für Tränke
            0.1f, // 30% Wahrscheinlichkeit für Holz
            0.15f, // 5% Wahrscheinlichkeit für eine Truhe
            0.25f  // 45% Wahrscheinlichkeit für nichts
        };


        // Methode zum Spawnen eines zufälligen Prefabs
        private void SpawnRandomPrefab( Vector3 position )
        {
            float totalProbability = 0f;
            foreach ( var probability in probabilities )
            {
                totalProbability += probability;
            }

            float randomValue = (float)random.NextDouble() * totalProbability;
            float cumulativeProbability = 0f;

            for ( int i = 0; i < prefabPaths.Count; i++ )
            {
                cumulativeProbability += probabilities[i];
                if ( randomValue <= cumulativeProbability )
                {
                    var prefab = ResourceLibrary.Get<PrefabFile>( prefabPaths[i] );
                    if ( prefab != null )
                    {
                        var gameObject = GameObject.Clone( prefab );
                        if ( gameObject != null )
                        {
                            // Spawnen des Items in der Luft
                            gameObject.WorldPosition = position + new Vector3( 0, 0, 50 );
                            gameObject.NetworkSpawn();

                            
                           

                            // Zerstören des Partikelemitters nach einer kurzen Zeit
                            // 2 Sekunden Verzögerung
                        }
                    }
                    break;
                }
                
            
            }
        }

        // Asynchrone Methode zum Zerstören des Partikelemitters nach einer Verzögerung
       
        public void OnBoxDestroyed()
        {
            Vector3 position = this.GameObject.WorldPosition;
            SpawnRandomPrefab( position );

        }

        [Broadcast]
        public void TakeDamage( DamageType type, float amount, Vector3 hitPosition, Vector3 hitDirection, Guid attackerId, Guid playerId )
        {
            
            if ( LifeState == LifeState.Dead )
                return;

            if ( type == DamageType.Bullet || type == DamageType.Serious )
            {
                var p = new SceneParticles( Scene.SceneWorld, "particles/impact.flesh.bloodpuff.vpcf" );
                p.SetControlPoint( 0, hitPosition );
                p.SetControlPoint( 0, Rotation.LookAt( hitDirection.Normal * -1f ) );
                p.SetControlPoint( 1, new Vector3( 0.5f, 0.1f, 0.1f ) );
                p.PlayUntilFinished( Task );
            }

            if ( Network.IsProxy )
                return;

            Health = Math.Clamp( Health - amount, 0f, MaxHealth );
            OnTakeDamage?.Invoke();

            if ( Health <= 0f )
            {
                LifeState = LifeState.Dead;
                OnDeath?.Invoke();

                if ( DeathSound != null )
                {
                    Sound.Play( DeathSound, WorldPosition );
                }

                var killer = Scene.Directory.FindByGuid( attackerId );
                if ( killer == null )
                {
                    return;
                }

                var killerPlayer = killer.Components.Get<Player>( FindMode.EverythingInSelfAndAncestors );
                if ( killerPlayer == null )
                {
                    return;
                }

                int npcLevel = 1; // Beispielwert, ersetzen Sie dies durch die tatsächliche Logik zur Bestimmung des Levels
                int vyndaliumPointsToAdd = CalculateVyndaliumReward( npcLevel );
                int xpPointsToAdd = CalculateXpReward( npcLevel );

                killerPlayer.GiveVyndalium( vyndaliumPointsToAdd );
                killerPlayer.GiveXp( xpPointsToAdd );

                if ( Hitprefab != null && this.GameObject != null )
                {
                    GameObject vyndaliumHitInfo = Hitprefab.Clone( this.GameObject.WorldPosition + new Vector3( 50, 0, 25 ) );
                    if ( vyndaliumHitInfo != null )
                    {
                        FaceThing vyndaliumFaceThing = vyndaliumHitInfo.Components.Get<FaceThing>();
                        if ( vyndaliumFaceThing != null )
                        {
                            vyndaliumFaceThing.Thing = killerPlayer.GameObject;
                        }

                        TextRenderer vyndaliumTextRenderer = vyndaliumHitInfo.Components.Get<TextRenderer>();
                        if ( vyndaliumTextRenderer != null )
                        {
                            vyndaliumTextRenderer.Color = Color.Yellow;
                            vyndaliumTextRenderer.Text = $"+{vyndaliumPointsToAdd} $";
                        }

                        ScaleTextWithDistance vyndaliumScaleText = vyndaliumHitInfo.Components.Get<ScaleTextWithDistance>();
                        if ( vyndaliumScaleText != null )
                        {
                            vyndaliumScaleText.Thing = killerPlayer.GameObject;
                        }
                    }

                    GameObject xpHitInfo = Hitprefab.Clone( this.GameObject.WorldPosition + new Vector3( 0, 0, 50 ) );
                    if ( xpHitInfo != null )
                    {
                        FaceThing xpFaceThing = xpHitInfo.Components.Get<FaceThing>();
                        if ( xpFaceThing != null )
                        {
                            xpFaceThing.Thing = killerPlayer.GameObject;
                        }
                    }
                    
                }
                Vector3 position = this.GameObject.WorldPosition;
                SpawnRandomPrefab( position );

                var ragdoll = Ragdoll.Clone( WorldPosition );
                if ( ragdoll != null )
                {
                    ragdoll.WorldRotation = WorldRotation;
                    ragdoll.WorldPosition = WorldPosition;
                    ragdoll.NetworkSpawn();
                }
               
                GameObject.Destroy();
            }
        }

        private int CalculateVyndaliumReward( int level )
        {
            // Beispielhafte Berechnung der Vyndalium-Belohnung basierend auf dem Level
            return VyndaliumReward * level;
        }

        private int CalculateXpReward( int level )
        {
            // Beispielhafte Berechnung der XP-Belohnung basierend auf dem Level
            return XpReward * level;
        }
    }
}