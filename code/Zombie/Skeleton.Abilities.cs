namespace GeneralGame;

[Flags]
public enum EffectFlags
{
    None = 0,
    ApplyFreeze = 1 << 0,
    ApplyPoison = 1 << 1,
    // Weitere Effekte hier hinzufügen
}

public class SkeletonAbilities : Abilities
{
    [Property] public bool HasIceAbility { get; set; } = false; // Boolean zum Aktivieren/Deaktivieren der Eisfähigkeit
    [Property, Group( "IceBall" ), Feature( "Ice" ), ShowIf( "HasIceAbility", true )] public float IceBallAttackCooldown { get; set; } = 10.0f; // Abklingzeit des Eisangriffs
    [Property, Group( "IceBall" ), Feature( "Ice" ), ShowIf( "HasIceAbility", true )] public PrefabFile IceBallPrefab { get; set; } // Prefab für den Eisball
    [Property, Group( "IceBall" ), Order( 0 ), Feature( "Ice" ), ShowIf( "HasIceAbility", true )] public bool CanUseIceBallAttack { get; set; } = true; // Boolean zum Aktivieren/
    [Property, Group( "IceBall" ), Feature( "Ice" ), ShowIf( "HasIceAbility", true )] public SoundEvent IceBallAttackSound { get; set; }
    [Property, Group( "IceBall" ), Feature( "Ice" ), ShowIf( "HasIceAbility", true )] public SoundEvent IceUnfreezeSound { get; set; } // Sound für das Auftauen

    

    [Property, Group( "IcePillar" ), Feature( "Ice" ), ShowIf( "HasIceAbility", true )] public PrefabFile IcePillarPrefab { get; set; } // Prefab für den IcePillar
    [Property, Group( "IcePillar" ), Feature( "Ice" ), ShowIf( "HasIceAbility", true )] public float IcePillarCooldown { get; set; } = 20.0f; // Cooldown für den IcePillar-Angriff
    [Property, Group( "IcePillar" ), Feature( "Ice" ), ShowIf( "HasIceAbility", true )] public bool CanUseIcePillar { get; set; } = true; // Boolean zum Aktivieren/Deaktivieren des IcePillar-Angriffs
    [Property, Group( "IcePillar" ), Feature( "Ice" ), ShowIf( "HasIceAbility", true )] public SoundEvent IcePillarAttackSound { get; set; } // Sound für den IcePillar-Angriff
    [Property, Group( "IcePillar" ), Feature( "Ice" ), ShowIf( "HasIceAbility", true )] public EffectFlags Effects { get; set; } = EffectFlags.None;

    [Property, Group( "IcePillar" ), Feature( "Ice" ), ShowIf( "HasIceAbility", true )] int pillarCount = 5;
    [Property, Group( "IcePillar" ), Feature( "Ice" ), ShowIf( "HasIceAbility", true )]float radius = 150.0f; 
    [Property, Group( "IcePillar" ), Feature( "Ice" ), ShowIf( "HasIceAbility", true )]  float freezeDuration = 2.0f;
    [Property, Group( "IcePillar" ), Feature( "Ice" ), ShowIf( "HasIceAbility", true )]  float poisonDuration = 5.0f;
    [Property, Group( "IcePillar" ), Feature( "Ice" ), ShowIf( "HasIceAbility", true )]  int spawnDelay = 1000; 



    [Property, Group( "IceGround" ), Feature( "Ice" ), ShowIf( "HasIceAbility", true )] public PrefabFile IceGroundPrefab { get; set; } // Prefab für den IceGround
    [Property, Group( "IceGround" ), Feature( "Ice" ), ShowIf( "HasIceAbility", true )] public float IceGroundCooldown { get; set; } = 15.0f; // Cooldown für den IceGround-Angriff
    [Property, Group( "IceGround" ), Feature( "Ice" ), ShowIf( "HasIceAbility", true )] public bool CanUseIceGround { get; set; } = true; // Boolean zum Aktivieren/Deaktivieren des IceGround-Angriffs
    [Property, Group( "IceGround" ), Feature( "Ice" ), ShowIf( "HasIceAbility", true )] public SoundEvent IceGroundAttackSound { get; set; } // Sound für den 
                                                                                                                                             // 
                                                                                                                                             // IceGround-Angriff

    [Property, Group( "IceWormhole" ), Feature( "Ice" ), ShowIf( "HasIceAbility", true )] public PrefabFile IceWormholePrefab { get; set; } // Prefab für das Ice Wormhole
    [Property, Group( "IceWormhole" ), Feature( "Ice" ), ShowIf( "HasIceAbility", true )] public float IceWormholeCooldown { get; set; } = 60.0f; // Cooldown für das Ice Wormhole
    [Property, Group( "IceWormhole" ), Feature( "Ice" ), ShowIf( "HasIceAbility", true )] public bool CanUseIceWormhole { get; set; } = true; // Boolean zum Aktivieren/Deaktivieren des Ice Wormhole
    [Property, Group( "IceWormhole" ), Feature( "Ice" ), ShowIf( "HasIceAbility", true )] public SoundEvent IceWormholeSound { get; set; } // Sound für das Ice Wormhole

    private RealTimeSince timeSinceIceBallAttack;
    private RealTimeSince timeSinceIcePillarAttack;
    private RealTimeSince timeSinceIceGroundAttack;
    private RealTimeSince timeSinceIceWormhole;
    protected override void OnUpdate()
    {
        base.OnUpdate();

        

        var players = Scene.GetAllComponents<Player>();
        var targetPlayer = players.FirstOrDefault();
        if ( targetPlayer == null )
        {
            return;
        }

        if ( timeSinceIceBallAttack > IceBallAttackCooldown && CanUseIceBallAttack )
        {
            IceBallAttack( targetPlayer );
            timeSinceIceBallAttack = 0.0f;
        }

        if ( CanUseIcePillar && timeSinceIcePillarAttack > IcePillarCooldown  )
        {
           
            IcePillarAttack();
            timeSinceIcePillarAttack = 0.0f;
        }
        if ( CanUseIceGround && timeSinceIceGroundAttack > IceGroundCooldown )
        {
            IceGroundAttack();
            timeSinceIceGroundAttack = 0.0f;
        }
        if ( CanUseIceWormhole && timeSinceIceWormhole > IceWormholeCooldown )
        {
            IceWormholeUltimate( targetPlayer );
            timeSinceIceWormhole = 0.0f;
        }
    }


    private async void IceBallAttack( Player targetPlayer )
    {
        if ( targetPlayer == null )
        {
            return;
        }

        var prefab = ResourceLibrary.Get<PrefabFile>( IceBallPrefab.ResourcePath );
        if ( prefab == null )
        {
            return;
        }

        // Logik für den Eisangriff
         // Dauer des Einfrierens in Sekunden
        const float iceBallSpeed = 200.0f; // Geschwindigkeit des Eisballs

        var iceBallObject = GameObject.Clone( prefab ); // Erstellen Sie das Eisball-Objekt aus dem Prefab
        iceBallObject.WorldPosition = this.WorldPosition;
        var direction = (targetPlayer.WorldPosition - this.WorldPosition).Normal;
        iceBallObject.WorldPosition += direction * iceBallSpeed * Time.Delta;

        // Bewege den Eisball zum Spieler
        await MoveIceBallObject( iceBallObject, targetPlayer, direction );

        // Bewege den Eisball nach oben
        await MoveIceBallUpwards( iceBallObject );

        // Explodiere und verursache Schaden
        ExplodeAndDamage( iceBallObject, targetPlayer );

       
    }

    private async Task MoveIceBallObject( GameObject iceBallObject, Player targetPlayer, Vector3 initialDirection )
    {
        const float speed = 300.0f; // Geschwindigkeit des Eisballs
        const float updateInterval = 0.01f; // Update alle 10ms für eine flüssigere Bewegung
        const float homingDuration = 2.0f; // Dauer des Homing-Effekts in Sekunden

        float elapsedTime = 0.0f;

        while ( elapsedTime < homingDuration )
        {
            await Task.Delay( (int)(updateInterval * 1000) ); // Update alle 10ms
            var targetPosition = targetPlayer.WorldPosition;
            var direction = (targetPosition - iceBallObject.WorldPosition).Normal;
            iceBallObject.WorldPosition += direction * speed * updateInterval; // Bewege das Objekt

            elapsedTime += updateInterval;
        }
    }

    private async Task MoveIceBallUpwards( GameObject iceBallObject )
    {
        const float speed = 200.0f; // Geschwindigkeit des Eisballs
        const float updateInterval = 0.01f; // Update alle 10ms für eine flüssigere Bewegung
        const float flightDuration = 0.4f; // Dauer des Flugs nach oben in Sekunden

        float elapsedTime = 0.0f;
        var direction = Vector3.Up; // Richtung nach oben

        while ( elapsedTime < flightDuration )
        {
            await Task.Delay( (int)(updateInterval * 1000) ); // Update alle 10ms
            iceBallObject.WorldPosition += direction * speed * updateInterval; // Bewege das Objekt

            elapsedTime += updateInterval;
        }
    }

    private async void ExplodeAndDamage( GameObject iceBallObject, Player targetPlayer )
    {
        const float explosionRadius = 200.0f; // Radius der Explosion
        const float damageAmount = 25.0f; // Schaden der Explosion
        const float freezeDuration = 5.0f;

        var explosionObject = new GameObject();
        explosionObject.WorldPosition = iceBallObject.WorldPosition;
        if ( IceBallAttackSound != null )
        {
            Sound.Play( IceBallAttackSound, explosionObject.WorldPosition );
        }
        // Erstellen und konfigurieren Sie den SpriteRenderer für die Explosion
        var spriteRenderer = explosionObject.Components.GetOrCreate<SpriteRenderer>();
        spriteRenderer.Texture = ResourceLibrary.Get<Texture>( "particles/explosion/explosion001.vtex_c" );
        spriteRenderer.Size = new Vector2( explosionRadius * 2, explosionRadius * 2 );
        spriteRenderer.Color = Color.Blue;
        spriteRenderer.RenderOptions.AfterUI = true;
        spriteRenderer.Enabled = true;

        // Erstellen Sie ein neues GameObject aus einem Prefab
        var explosionObjectv2 = ResourceLibrary.Get<PrefabFile>( "prefabs/hit/skeleton/explosion_v2.prefab" );
        var freezeEffectObjectv2 = GameObject.Clone( explosionObjectv2 );
        freezeEffectObjectv2.WorldPosition = explosionObject.WorldPosition;

        // Logik für die Explosion und den Schaden
        var players = Scene.GetAllComponents<Player>();
        foreach ( var player in players )
        {
            if ( (player.WorldPosition - explosionObject.WorldPosition).Length <= explosionRadius )
            {
                player.TakeDamage( DamageType.ice, damageAmount, explosionObject.WorldPosition, Vector3.Zero, Guid.Empty, Guid.Empty );

                // Erstellen Sie ein neues GameObject aus einem Prefab
                var freezeEffectPrefab = ResourceLibrary.Get<PrefabFile>( "prefabs/hit/skeleton/ice_game.prefab" );
                var freezeEffectObject = GameObject.Clone( freezeEffectPrefab );
                freezeEffectObject.WorldPosition = targetPlayer.WorldPosition;

                targetPlayer.ApplyFreeze( freezeDuration );

                // Überprüfen Sie die Zeit und zerstören Sie das Freeze-Effekt-Objekt nach Ablauf der Freeze-Dauer
                _ = DestroyFreezeEffectAfterDuration( freezeEffectObject, freezeDuration );
            }
        }

        // Zerstören Sie das ursprüngliche Eisball-Objekt
        iceBallObject.Destroy();

        // Warten Sie eine Sekunde, bevor Sie das Explosion-Objekt zerstören
        await Task.Delay( 100 );
        explosionObject.Destroy();

        // Zerstören Sie das Freeze-Effekt-Objekt nach Ablauf der Freeze-Dauer
        _ = DestroyFreezeEffectAfterDuration( freezeEffectObjectv2, freezeDuration );
    }

    private async Task DestroyFreezeEffectAfterDuration( GameObject freezeEffectObject, float duration )
    {
        RealTimeSince timeSinceFreeze = 0.0f;

        while ( timeSinceFreeze < duration )
        {
            await Task.Delay( 100 ); // Überprüfen Sie alle 100ms
        }
        if ( IceUnfreezeSound != null )
        {
            Sound.Play( IceUnfreezeSound, freezeEffectObject.WorldPosition );
        }

        freezeEffectObject.Destroy();
    }
    private async void IcePillarAttack()
    {
        

  

        if ( IcePillarAttackSound != null )
        {
            Sound.Play( IcePillarAttackSound, this.WorldPosition );
        }

        for ( int i = 0; i < pillarCount; i++ )
        {
            var angle = i * (360.0f / pillarCount);
            var position = this.WorldPosition + new Vector3( MathF.Cos( angle ), MathF.Sin( angle ), 0 ) * radius;

            var prefab = ResourceLibrary.Get<PrefabFile>( IcePillarPrefab.ResourcePath );
            if ( prefab == null )
            {
            
                continue;
            }

            var icePillarObject = GameObject.Clone( prefab );
            icePillarObject.WorldPosition = position;

            var boxCollider = icePillarObject.Components?.Get<BoxCollider>();
           
            boxCollider.IsTrigger = true; // Als Trigger festlegen
          

            boxCollider.OnTriggerEnter += ( Collider other ) =>
            {
                var player = other.GameObject.GetComponent<Player>();
                if ( player != null )
                {
                    if ( Effects.HasFlag( EffectFlags.ApplyFreeze ) )
                    {
                        player.ApplyFreeze( freezeDuration );
                    }

                    if ( Effects.HasFlag( EffectFlags.ApplyPoison ) )
                    {
                        player.ApplyPoison( poisonDuration );
                    }

                    // Weitere Effekte hier hinzufügen
                }
            };
           

            //_ = CheckPlayerProximityAndFreeze( icePillarObject, freezeDistance, freezeDuration );

            // Zerstören Sie das IcePillar-Objekt nach 5 Sekunden
            _ = DestroyIcePillarAfterDelay( icePillarObject, 5000 );

            // Verzögerung zwischen den Spawns
            await Task.Delay( spawnDelay );
        }

   
    }
    private async Task CheckPlayerProximityAndFreeze( GameObject icePillarObject, float freezeDistance, float freezeDuration )
    {
        while ( icePillarObject != null )
        {
            var players = Scene.GetAllComponents<Player>();
            foreach ( var player in players )
            {
                var distance = Vector3.DistanceBetween( player.WorldPosition, icePillarObject.WorldPosition );
                if ( distance <= freezeDistance )
                {
                    player.ApplyFreeze( freezeDuration );
                }
            }
            await Task.Delay( 1000 ); // Überprüfen Sie jede Sekunde
        }
    }


    private async Task DestroyIcePillarAfterDelay( GameObject icePillarObject, int delay )
    {
        await Task.Delay( delay );
        icePillarObject.Destroy();
       
    }
    private async void IceGroundAttack()
    {
        const int groundCount = 16;
        const int outerGroundCount = groundCount * 2; // Doppelte Anzahl für die äußeren Bögen
        const float spawnDistance = 90.0f; // Abstand zwischen den GameObjects
        const float arcSpawnDistance = spawnDistance / 2; // Abstand zwischen den GameObjects in den Bögen
        const float freezeDuration = 3.0f; // Dauer des Einfrierens
        const float minAngle = 15.0f; // Minimaler Winkel
        const float maxAngle = 45f; // Maximaler Winkel

        if ( IceGroundAttackSound != null )
        {
            Sound.Play( IceGroundAttackSound, this.WorldPosition );
        }

        var players = Scene.GetAllComponents<Player>();
        var targetPlayer = players.FirstOrDefault();
        if ( targetPlayer == null )
        {
            return;
        }

        var direction = (targetPlayer.WorldPosition - this.WorldPosition).Normal;

        // Gerade auf den Spieler zu
        await SpawnIceGroundPath( direction, groundCount, spawnDistance, freezeDuration );

        // Links herum in einem Bogen
        await SpawnIceGroundArc( direction, outerGroundCount, arcSpawnDistance, freezeDuration, true, 30000 ); // 30 Sekunden

        // Rechts herum in einem Bogen
        await SpawnIceGroundArc( direction, outerGroundCount, arcSpawnDistance, freezeDuration, false, 30000 ); // 30 Sekunden

        // Gemischte Variante
        for ( int i = 0; i < groundCount; i++ )
        {
            var offset = direction * spawnDistance * i;
            var angle = (i % 2 == 0) ? minAngle : maxAngle;
            var radians = MathF.PI / 180.0f * angle;
            var mixedDirection = new Vector3(
                direction.x * MathF.Cos( radians ) - direction.y * MathF.Sin( radians ),
                direction.x * MathF.Sin( radians ) + direction.y * MathF.Cos( radians ),
                direction.z
            );

            await SpawnIceGroundObject( this.WorldPosition + offset, mixedDirection, freezeDuration, 5000 ); // 5 Sekunden
        }
    }

    private async Task SpawnIceGroundPath( Vector3 direction, int groundCount, float spawnDistance, float freezeDuration )
    {
        for ( int i = 0; i < groundCount; i++ )
        {
            var offset = direction * spawnDistance * i;
            await SpawnIceGroundObject( this.WorldPosition + offset, direction, freezeDuration, 5000 ); // 5 Sekunden
        }
    }

    private async Task SpawnIceGroundArc( Vector3 direction, int groundCount, float spawnDistance, float freezeDuration, bool left, int delay )
    {
        for ( int i = 0; i < groundCount; i++ )
        {
            var angle = (left ? -1 : 1) * (i * 10.0f); // Winkel für den Bogen
            var radians = MathF.PI / 180.0f * angle;
            var arcDirection = new Vector3(
                direction.x * MathF.Cos( radians ) - direction.y * MathF.Sin( radians ),
                direction.x * MathF.Sin( radians ) + direction.y * MathF.Cos( radians ),
                direction.z
            );

            // Spiegeln der Richtung um 180 Grad
            arcDirection = -arcDirection;

            var offset = arcDirection * spawnDistance * i;
            await SpawnIceGroundObject( this.WorldPosition + offset, arcDirection, freezeDuration, delay );
        }
    }

    private async Task SpawnIceGroundObject( Vector3 position, Vector3 direction, float freezeDuration, int delay )
    {
        var prefab = ResourceLibrary.Get<PrefabFile>( IceGroundPrefab.ResourcePath );
        if ( prefab == null )
        {
            return;
        }

        var iceGroundObject = GameObject.Clone( prefab );
        iceGroundObject.WorldPosition = position;

        var random = new Random();
        var randomDirection = random.Next( 0, 2 ) == 0 ? Vector3.Right : Vector3.Left;
        iceGroundObject.WorldRotation = Rotation.FromAxis( randomDirection, random.Next( 15, 45 ) );

        var randomScaleX = NextFloat( random, 0.3f, 2.5f );
        var randomScaleY = NextFloat( random, 0.3f, 2.5f );
        iceGroundObject.LocalScale = new Vector3( randomScaleX, randomScaleY, iceGroundObject.LocalScale.z );

        var boxCollider = iceGroundObject.Components?.GetOrCreate<BoxCollider>();
        boxCollider.IsTrigger = true; // Als Trigger festlegen

        boxCollider.OnTriggerEnter += ( Collider other ) =>
        {
            var player = other.GameObject.GetComponent<Player>();
            if ( player != null )
            {
                player.ApplyFreeze( freezeDuration );
            }
        };

        // Zerstören Sie das IceGround-Objekt nach der angegebenen Verzögerung
        _ = DestroyIceGroundAfterDelay( iceGroundObject, delay );

        await Task.Delay( 100 ); // Verzögerung zwischen den Spawns
    }

    private float NextFloat( Random random, float minValue, float maxValue )
    {
        return (float)(random.NextDouble() * (maxValue - minValue) + minValue);
    }

    private async Task DestroyIceGroundAfterDelay( GameObject iceGroundObject, int delay )
    {
        await Task.Delay( delay );
        iceGroundObject.Destroy();
    }

    private async void IceWormholeUltimate( Player targetPlayer )
    {
        const float pullRadius = 1300.0f; // Radius des Anziehungseffekts
        const float pullStrength = 750.0f; // Stärke des Anziehungseffekts
        const float pullDuration = 5.0f; // Dauer des Anziehungseffekts
        const float wormholeDuration = 10.0f; // Dauer des Wormholes
        float[] liftHeights = { 50.0f, 100.0f, 150.0f }; // Höhen, in die das Wormhole zieht
        int liftIndex = 0;

        var prefab = ResourceLibrary.Get<PrefabFile>( IceWormholePrefab.ResourcePath );
        if ( prefab == null )
        {
            return;
        }

        var wormholeObject = GameObject.Clone( prefab );
        wormholeObject.WorldPosition = this.WorldPosition;

        if ( IceWormholeSound != null )
        {
            Sound.Play( IceWormholeSound, wormholeObject.WorldPosition );
        }

        var spriteRenderer = wormholeObject.Components.GetOrCreate<ParticleSpriteRenderer>();

        var players = Scene.GetAllComponents<Player>();

        // Anziehungseffekt für eine bestimmte Dauer anwenden
        for ( float elapsedTime = 0; elapsedTime < pullDuration; elapsedTime += Time.Delta )
        {
            foreach ( var player in players )
            {
                var distance = Vector3.DistanceBetween( player.WorldPosition, wormholeObject.WorldPosition );
                if ( distance <= pullRadius )
                {
                    var direction = (wormholeObject.WorldPosition - player.WorldPosition).Normal;
                    player.WorldPosition += direction * pullStrength * Time.Delta;
                }
            }

            // Ändere den RotationOffset des spriteRenderer
            spriteRenderer.RotationOffset = liftHeights[liftIndex];
            liftIndex = (liftIndex + 1) % liftHeights.Length;

            // Zufällige Höhe zwischen den Werten in liftHeights
            wormholeObject.WorldPosition = this.WorldPosition + Vector3.Up * liftHeights[liftIndex];

            await Task.Delay( 100 ); // Wartezeit zwischen den Höhenänderungen
        }

        // Wurmloch für die restliche Dauer bestehen lassen, ohne Anziehungseffekt
        await Task.Delay( (int)((wormholeDuration - pullDuration) * 1000) );

        wormholeObject.Destroy();
    }
}
