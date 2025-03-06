namespace GeneralGame;

public class SkeletonAbilities : Abilities
{
    [Property] public bool HasIceAbility { get; set; } = false; // Boolean zum Aktivieren/Deaktivieren der Eisfähigkeit
    [Property,Group("IceBall"),Feature("Ice"), ShowIf( "HasIceAbility", true )] public float IceBallAttackCooldown { get; set; } = 10.0f; // Abklingzeit des Eisangriffs
    [Property, Group( "IceBall" ), Feature( "Ice" ), ShowIf( "HasIceAbility", true )] public PrefabFile IceBallPrefab { get; set; } // Prefab für den Eisball
    [Property, Group( "IceBall" ),Order(0), Feature( "Ice" ), ShowIf( "HasIceAbility", true )] public bool CanUseIceBallAttack { get; set; } = true; // Boolean zum Aktivieren/

    [Property, Group( "IceBall" ), Feature( "Ice" ), ShowIf( "HasIceAbility", true )] public SoundEvent IceBallAttackSound { get; set; } 
    
    [Property, Group( "IceBall" ), Feature( "Ice" ), ShowIf( "HasIceAbility", true )] public SoundEvent IceUnfreezeSound { get; set; } // Sound für das Auftauen

    private RealTimeSince timeSinceIceBallAttack;

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if ( !CanUseIceBallAttack || IceBallPrefab == null )
            return;

        var players = Scene.GetAllComponents<Player>();
        var targetPlayer = players.FirstOrDefault();
        if ( targetPlayer == null )
        {
            return;
        }

        if ( timeSinceIceBallAttack > IceBallAttackCooldown )
        {
            IceBallAttack( targetPlayer );
            timeSinceIceBallAttack = 0.0f;
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
}