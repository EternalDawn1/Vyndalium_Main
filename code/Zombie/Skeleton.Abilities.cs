namespace GeneralGame;

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

    private RealTimeSince timeSinceIceBallAttack;
    private RealTimeSince timeSinceIcePillarAttack;

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
        const int pillarCount = 5;
        const float radius = 150.0f; // Radius um den Skeleton-Boss
        const float freezeDistance = 100.0f; // Distanz zum Einfrieren der Spieler
        const float freezeDuration = 2.0f; // Dauer des Einfrierens
        const int spawnDelay = 1000; // Verzögerung zwischen den Spawns in Millisekunden

  

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
                    
                    player.ApplyFreeze( freezeDuration );
                }
            };
           

            _ = CheckPlayerProximityAndFreeze( icePillarObject, freezeDistance, freezeDuration );

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

}
