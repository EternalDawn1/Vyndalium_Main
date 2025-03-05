namespace GeneralGame;

public class SkeletonAbilities : Abilities
{
    [Property] public float IceBallAttackCooldown { get; set; } = 10.0f; // Abklingzeit des Eisangriffs
    [Property] public PrefabFile IceBallPrefab { get; set; } // Prefab für den Eisball
    [Property] public bool CanUseIceBallAttack { get; set; } = true; // Boolean zum Aktivieren/

    [Property] public SoundEvent IceBallAttackSound { get; set; } // Sound für den Eisangriff

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
        const float freezeDuration = 5.0f; // Dauer des Einfrierens in Sekunden
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

        // Spieler einfrieren
        targetPlayer.ApplyFreeze( freezeDuration );
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

    private void ExplodeAndDamage( GameObject iceBallObject, Player targetPlayer )
    {
        const float explosionRadius = 300.0f; // Radius der Explosion
        const float damageAmount = 25.0f; // Schaden der Explosion

        // Erstellen und konfigurieren Sie den SpriteRenderer für die Explosion
        var spriteRenderer = iceBallObject.AddComponent<SpriteRenderer>();
        spriteRenderer.Texture = ResourceLibrary.Get<Texture>( "particles/explosion/explosion001.vtex_c" );
        spriteRenderer.Enabled = true;

        if ( IceBallAttackSound != null )
        {
            Sound.Play( IceBallAttackSound, iceBallObject.WorldPosition );
        }

        // Logik für die Explosion und den Schaden
        var players = Scene.GetAllComponents<Player>();
        foreach ( var player in players )
        {
            if ( (player.WorldPosition - iceBallObject.WorldPosition).Length <= explosionRadius )
            {
                player.TakeDamage( DamageType.ice, damageAmount, iceBallObject.WorldPosition, Vector3.Zero, Guid.Empty, Guid.Empty );
            }
        }

        iceBallObject.Destroy();
    }
}