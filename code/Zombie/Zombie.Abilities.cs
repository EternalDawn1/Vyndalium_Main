namespace GeneralGame;

public class Abilities : Component
{
    [Order( 100 ),Property,Group("General Settings")] bool HasSpecialAbility { get; set; }

    [Order( 20 ),Feature("SpecialAbility"),Property,Group("Abilities"),ShowIf( "HasSpecialAbility", true )] bool FireAbility { get; set; }
    [Order( 100 ),Feature( "SpecialAbility" ),Property,Group("FireGroup"),ShowIf( "FireAbility", true )] bool FireAttackEnabled  { get; set; }

    [Order( 20 ),Feature("SpecialAbility"),Property,Group("Abilities"),ShowIf( "HasSpecialAbility", true )] PrefabFile FireBallPrefab { get; set; }

    [Order( 20 ), Feature( "SpecialAbility" ), Property, Group( "Abilities" ), ShowIf( "HasSpecialAbility", true )] SoundEvent FireBallChargeSound { get; set; }
    [Order( 20 ), Feature( "SpecialAbility" ), Property, Group( "Abilities" ), ShowIf( "HasSpecialAbility", true )] SoundEvent FireBallAttackSound { get; set; }

    public Vector3 PlayerProximityDistance { get; set; } = new Vector3( 500f, 500f, 500f );

    [Property]private RealTimeSince FireBallAttackTime;
    [Property]private float FireBallCooldown = 15.0f;

    protected override void OnUpdate()
    {
        var players = Scene.GetAllComponents<Player>();
        var targetPlayer = players.FirstOrDefault();
        if ( targetPlayer == null )
        {
            return;
        }

        if ( FireAbility ) // Überprüfen Sie, ob FireAbility auf true gesetzt ist
        {
            if ( IsPlayerInProximity() ) // Überprüfen Sie, ob diese Methode true zurückgibt
            {
                if ( targetPlayer != null )
                {
                    float distanceToPlayer = (targetPlayer.WorldPosition - this.WorldPosition).Length;
                    if ( distanceToPlayer <= PlayerProximityDistance.Length ) // Überprüfen Sie die Berechnung der Distanz
                    {
                        if ( FireBallAttackTime > FireBallCooldown ) // Überprüfen Sie den Wert von FireBallAttackTime
                        {
                           FireBallAttack( targetPlayer ); // Diese Methode sollte aufgerufen werden
                            FireBallAttackTime = 0.0f;
                        }
                    }
                }
            }
        }
    }
    private bool IsPlayerInProximity()
    {
        if ( Network.IsProxy )
            return false;

        var players = Scene.GetAllComponents<Player>();
        if ( players == null )
        {
            return false;
        }
        foreach ( var player in players )
        {
            if ( (player.WorldPosition - this.WorldPosition).Length < PlayerProximityDistance.Length )
                return true;
        }
        return false;
    }
    private async void FireBallAttack( Player targetPlayer )
    {
        if ( targetPlayer == null )
        {
            return;
        }
        if ( !FireAttackEnabled)
        {
            return;
        }

        var prefab = FireBallPrefab;
        if ( prefab == null )
        {
            return;
        }


        var fireBallObjects = new List<GameObject>();

        // Erstelle vier Feuerkugeln
        for ( int i = 0; i < 4; i++ )
        {
            var fireBallObject = GameObject.Clone( prefab );
            
            fireBallObject.WorldPosition = WorldPosition + new Vector3( 0, 0, 150 ); // 50 Einheiten über dem NPC
            fireBallObject.WorldRotation = Rotation.Identity;
            fireBallObject.NetworkSpawn();
            fireBallObjects.Add( fireBallObject );

            if ( FireBallChargeSound != null )
            {
                Sound.Play( FireBallChargeSound );
            }

            // Verzögerung zwischen den Spawns
            await Task.Delay( 250 ); // 500ms Verzögerung
        }

        if ( FireBallAttackSound != null )
        {
            Sound.Play( FireBallAttackSound );
        }
        // Bewege die Feuerkugeln in einem Bogen
        var directions = new Vector3[]
        {
        Vector3.Up, // Oben
        Vector3.Down, // Unten
        Vector3.Right, // Rechts
        Vector3.Left // Links
      
        };

        for ( int i = 0; i < fireBallObjects.Count; i++ )
        {
            var fireBallObject = fireBallObjects[i];
            var direction = directions[i];
            _ = MoveFireBallObjectAvatarMode( fireBallObject, targetPlayer, direction );
        }
         FireBallAttackTime = 0.0f;
    }
    private async Task MoveFireBallObjectAvatarMode( GameObject fireBallObject, Player targetPlayer, Vector3 initialDirection )
    {
        const float speed = 400.0f; // Geschwindigkeit des Feuerballs
        const float updateInterval = 0.01f; // Update alle 10ms für eine flüssigere Bewegung
        const float homingDuration = 1.0f; // Dauer des Homing-Effekts in Sekunden
        const float straightFlightDuration = 5.0f; // Dauer des geraden Flugs in Sekunden
        const float increasedSpeed = 600.0f; // Erhöhte Geschwindigkeit nach dem Homing-Effekt

        // Schieße die Kugel in die angegebene Richtung
        fireBallObject.WorldPosition += initialDirection * 200.0f;



        float elapsedTime = 0.0f;

        while ( elapsedTime < homingDuration )
        {
            await Task.Delay( (int)(updateInterval * 1000) ); // Update alle 10ms
            var targetPosition = targetPlayer.WorldPosition;
            var direction = (targetPosition - fireBallObject.WorldPosition).Normal;
            fireBallObject.WorldPosition += direction * speed * updateInterval; // Bewege das Objekt

            // Kollisionserkennung mit Spielern
            var players = Scene.GetAllComponents<Player>();
            foreach ( var player in players )
            {
                if ( (player.WorldPosition - fireBallObject.WorldPosition).Length < 1.0f )
                {
                    // Füge dem Spieler Schaden zu
                    var playerHealthComponent = player.GetComponent<IHealthComponent>();
                    if ( playerHealthComponent != null )
                    {
                        playerHealthComponent.TakeDamage( DamageType.fire, 50, fireBallObject.WorldPosition, Vector3.Zero, Guid.Empty, fireBallObject.Id );
                    }

                    // Lösche das Feuerballobjekt nach einer Sekunde Verzögerung
                    fireBallObject.Destroy();
                    return;
                }
            }

            elapsedTime += updateInterval;
        }

        // Fliege für eine Sekunde geradeaus und erhöhe die Geschwindigkeit
        var straightFlightDirection = (targetPlayer.WorldPosition - fireBallObject.WorldPosition).Normal;
        elapsedTime = 0.0f;

        while ( elapsedTime < straightFlightDuration )
        {
            await Task.Delay( (int)(updateInterval * 1000) ); // Update alle 10ms
            fireBallObject.WorldPosition += straightFlightDirection * increasedSpeed * updateInterval; // Bewege das Objekt

            elapsedTime += updateInterval;
        }



        fireBallObject.Destroy();
    }




}