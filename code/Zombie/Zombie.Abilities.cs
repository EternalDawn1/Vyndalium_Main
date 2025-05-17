using System.Threading.Tasks;
namespace GeneralGame;

public class Abilities : Component
{
    [Order( 100 ),Property,Group("General Settings")] bool HasSpecialAbility { get; set; }
    [Order( 100 ), Feature( "SpecialAbility" ), Property, Group( "FireGroup" ), ShowIf( "FireAbility", true )] bool FireRingEnabled { get; set; }
    [Order( 100 ), Feature( "SpecialAbility" ), Property, Group( "FireGroup" ), ShowIf( "FireAbility", true )] bool FireCannonEnabled { get; set; }
    [Order( 100 ), Feature( "SpecialAbility" ), Property, Group( "FireGroup" ), ShowIf( "FireAbility", true )] bool FireAttackEnabled { get; set; }
    [Order( 100 ), Feature( "SpecialAbility" ), Property, Group( "FireGroup" ), ShowIf( "FireAbility", true )] bool FireWallEnabled{ get; set; }
    [Order( 20 ),Feature("SpecialAbility"),Property,Group("Abilities"),ShowIf( "HasSpecialAbility", true )] bool FireAbility { get; set; }
    [Order( 20 ), Feature( "SpecialAbility" ), Property, Group( "Abilities" ), ShowIf( "HasSpecialAbility", true )] bool FireUltimateEnabled { get; set; }



    [Order( 20 ),Feature("SpecialAbility"),Property,Group( "FireBall" ),ShowIf( "HasSpecialAbility", true )] PrefabFile FireBallPrefab { get; set; }


    [Order( 20 ), Feature( "SpecialAbility" ), Property, Group( "FireBall" ), ShowIf( "HasSpecialAbility", true )] SoundEvent FireBallChargeSound { get; set; }
    [Order( 20 ), Feature( "SpecialAbility" ), Property, Group( "FireBall" ), ShowIf( "HasSpecialAbility", true )] SoundEvent FireBallAttackSound { get; set; }

    [Order( 20 ), Feature( "SpecialAbility" ), Property, Group( "FireRing" ), ShowIf( "HasSpecialAbility", true )] PrefabFile FireRingPrefab { get; set; }
    [Order( 20 ), Feature( "SpecialAbility" ), Property, Group( "FireRing" ), ShowIf( "HasSpecialAbility", true )] SoundEvent FireRingSound { get; set; }


    [Order( 20 ), Feature( "SpecialAbility" ), Property, Group( "FireCannon" ), ShowIf( "HasSpecialAbility", true )] PrefabFile FireCannonPrefab { get; set; }
    [Order( 20 ), Feature( "SpecialAbility" ), Property, Group( "FireCannon" ), ShowIf( "HasSpecialAbility", true )] SoundEvent FireCannonChargeSound { get; set; }
    [Order( 20 ), Feature( "SpecialAbility" ), Property, Group( "FireCannon" ), ShowIf( "HasSpecialAbility", true )] SoundEvent FireCannonAttackSound { get; set; }

    [Order( 20 ), Feature( "SpecialAbility" ), Property, Group( "FireWall" ), ShowIf( "HasSpecialAbility", true )] PrefabFile FireWallPrefab { get; set; }
    [Order( 20 ), Feature( "SpecialAbility" ), Property, Group( "FireWall" ), ShowIf( "HasSpecialAbility", true )] SoundEvent FireWallSound { get; set; }







    // Bereits existierende Liste
    private List<GameObject> activeFireRingObjects = new();
    // Neue Listen für andere Angriffe
    private List<GameObject> activeFireBallObjects = new();
    private List<GameObject> activeFireCannonObjects = new();
    private List<GameObject> activeFlameWallObjects = new();
    [Property]private Npc npc { get; set; } = new Npc();

    [Property] public Vector3 PlayerProximityDistance { get; set; } = new Vector3( 500f, 500f, 500f );

    [Property]private RealTimeSince FireBallAttackTime;
    [Property]private float FireBallCooldown = 15.0f;

    [Property] private RealTimeSince FireRingAttackTime;
    [Property] private float FireRingCooldown = 10.0f;

    [Property] private RealTimeSince FireCannonAttackTime;
    [Property] private float FireCannonCooldown = 5.0f;

    [Property] private RealTimeSince FlameWallAttackTime;
    [Property] private float FlameWallCooldown = 20.0f;

    [Property] private RealTimeSince UltimateJumpAttackTime;
    [Property] private float UltimateJumpAttackCooldown = 30.0f;

    [Property, Group( "Ultimate Jump Attack Settings" )] private float HoverDuration { get; set; } = 2f;
    [Property, Group( "Ultimate Jump Attack Settings" )] private Vector3 MidPointOffset { get; set; } = new Vector3( 0, 0, 300 );

    protected override void OnStart()
    {
        this.npc = this.GetComponent<Npc>();
        if ( this.npc != null )
        {
            this.npc.NavMeshAgent = this.npc.GetComponent<NavMeshAgent>();
            if ( this.npc.NavMeshAgent == null )
            {
                Log.Error( "NavMeshAgent is null." );
            }
        }
        else
        {
            Log.Error( "NPC is null." );
        }
    }

    protected override void OnUpdate()
    {
        if ( GameObject == null || !GameObject.IsValid() || npc == null || !npc.IsValid() )
        {
            CleanupAllActiveObjects();
            return;
        }
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

        if ( FireRingEnabled && IsPlayerTooClose( targetPlayer ) && FireRingAttackTime > FireRingCooldown )
        {
            ExecuteFireRingAttack();
            FireRingAttackTime = 0.0f;
        }
        if ( FireCannonEnabled && IsPlayerInProximity() && !IsPlayerTooClose( targetPlayer ) && FireCannonAttackTime > FireCannonCooldown )
        {
            FireCannonAttack( targetPlayer );
            FireCannonAttackTime = 0.0f;
        }
        if ( FireWallEnabled && FlameWallAttackTime > FlameWallCooldown )
        {
            FlameWallAttack( targetPlayer );
            FlameWallAttackTime = 0.0f;
        }
        if ( FireUltimateEnabled && UltimateJumpAttackTime > UltimateJumpAttackCooldown )
        {
            UltimateJumpAttack( targetPlayer );
            UltimateJumpAttackTime = 0.0f; // Setzen Sie die UltimateJumpAttackTime zurück
        }


    }
    
    private bool IsPlayerTooClose( Player targetPlayer )
    {
        float distanceToPlayer = (targetPlayer.WorldPosition - this.WorldPosition).Length;
        return distanceToPlayer < 400.0f; // Beispielwert für zu nahe Distanz
    }
    private bool IsPlayerTooCloseUltimate( Player targetPlayer )
    {
        float distanceToPlayer = (targetPlayer.WorldPosition - this.WorldPosition).Length;
        return distanceToPlayer < 250.0f; // Beispielwert für zu nahe Distanz
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

    /// <summary>
    /// Erstellt Feuerbälle asynchron
    /// </summary>
    /// <param name="fireBallObjects"></param>
    private void CreateFireBallsAsync( List<GameObject> fireBallObjects )
    {


        for ( int i = 0; i < 4; i++ )
        {

            var fireBallObject = GameObject.Clone( FireBallPrefab );
            fireBallObject.WorldPosition = WorldPosition + new Vector3( 0, 0, 150 ); // 50 Einheiten über dem NPC
            fireBallObject.WorldRotation = Rotation.Identity;
            fireBallObjects.Add( fireBallObject );
            activeFireBallObjects.Add( fireBallObject );
        }
    }

    private  void FireBallAttack( Player targetPlayer )
    {
        if ( targetPlayer == null )
        {
            return;
        }
        if ( !FireAttackEnabled )
        {
            return;
        }

        var fireBallObjects = new List<GameObject>();

        if ( FireBallChargeSound != null )
        {
            Sound.Play( FireBallChargeSound );
        }

         CreateFireBallsAsync( fireBallObjects );

        foreach ( var fireBallObject in fireBallObjects )
        {
            fireBallObject.NetworkSpawn();
            fireBallObject.Network.DropOwnership();
            activeFireBallObjects.Add( fireBallObject );
        }



        if ( FireBallAttackSound != null )
        {
            Sound.Play( FireBallAttackSound, WorldPosition );
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



        activeFireBallObjects.Remove( fireBallObject );
        fireBallObject.Destroy();
    }
    protected override void OnDestroy()
    {
        CleanupAllActiveObjects();
        base.OnDestroy();
    }
    private void CleanupAllActiveObjects()
    {
        // Feuerringe bereinigen
        foreach ( var fireRingObject in activeFireRingObjects.ToList() )
        {
            if ( fireRingObject != null && fireRingObject.IsValid() )
            {
                fireRingObject.Destroy();
            }
        }
        activeFireRingObjects.Clear();

        // Feuerbälle bereinigen
        foreach ( var fireBallObject in activeFireBallObjects.ToList() )
        {
            if ( fireBallObject != null && fireBallObject.IsValid() )
            {
                fireBallObject.Destroy();
            }
        }
        activeFireBallObjects.Clear();

        // Feuerkanonen bereinigen
        foreach ( var fireCannonObject in activeFireCannonObjects.ToList() )
        {
            if ( fireCannonObject != null && fireCannonObject.IsValid() )
            {
                fireCannonObject.Destroy();
            }
        }
        activeFireCannonObjects.Clear();

        // Feuerwände bereinigen
        foreach ( var flameWallObject in activeFlameWallObjects.ToList() )
        {
            if ( flameWallObject != null && flameWallObject.IsValid() )
            {
                flameWallObject.Destroy();
            }
        }
        activeFlameWallObjects.Clear();
    }


    private async void ExecuteFireRingAttack()
    {
        if ( FireRingPrefab == null )
        {
            return;
        }

        var prefab = ResourceLibrary.Get<PrefabFile>( FireRingPrefab.ResourcePath );
        if ( prefab == null )
        {
            return;
        }

        const int numberOfWaves = 5;
        const int objectsPerWave = 12;
        const float waveInterval = 1.0f; // Interval between waves in seconds
        const float objectSpeed = 100.0f; // Speed at which objects move outward
        const float maxDistance = 500.0f; // Maximum distance before objects are destroyed

        for ( int wave = 0; wave < numberOfWaves; wave++ )
        {
            for ( int i = 0; i < objectsPerWave; i++ )
            {
                float angle = (360.0f / objectsPerWave) * i;
                Vector3 direction = new Vector3( MathF.Cos( angle ), MathF.Sin( angle ), 0 );
                var fireObject = GameObject.Clone( prefab );
                fireObject.WorldPosition = WorldPosition;
                fireObject.WorldRotation = Rotation.Identity;
                fireObject.NetworkSpawn();
                fireObject.Network.DropOwnership();

                activeFireRingObjects.Add( fireObject );
                if ( FireRingSound != null )
                {
                    Sound.Play( FireRingSound, WorldPosition );
                }

                _ = MoveFireObject( fireObject, direction, objectSpeed, maxDistance );
            }

            await Task.Delay( (int)(waveInterval * 1000) );
        }
    }

    private async Task MoveFireObject( GameObject fireObject, Vector3 direction, float speed, float maxDistance )
    {
        float distanceTraveled = 0.0f;

        while ( distanceTraveled < maxDistance )
        {
            await Task.Delay( 100 ); // Update alle 100ms
            fireObject.WorldPosition += direction * speed * 0.1f; // Bewege das Objekt
            distanceTraveled += speed * 0.1f;
        }

        fireObject.Destroy();
        activeFireRingObjects.Remove( fireObject );
    }

    private void FireCannonAttack( Player targetPlayer )
    {
        if ( targetPlayer == null )
        {
            return;
        }
        if ( !FireCannonEnabled )
        {
            return;
        }

        var fireCannonObjects = new List<GameObject>();

        if ( FireCannonChargeSound != null )
        {
            Sound.Play( FireCannonChargeSound );
        }

        CreateFireCannonsAsync( fireCannonObjects );

        foreach ( var fireCannonObject in fireCannonObjects )
        {
            fireCannonObject.NetworkSpawn();
            fireCannonObject.Network.DropOwnership();
        }

        if ( FireCannonAttackSound != null )
        {
            Sound.Play( FireCannonAttackSound, WorldPosition );
        }

        var directions = new Vector3[]
        {
        Vector3.Left, // Links
        Vector3.Zero, // Mitte
        Vector3.Right // Rechts
        };

        bool shouldUseAllColors = npc.Health < npc.MaxHealth * 0.5f; // Überprüfen Sie, ob der NPC unter 50% Leben ist

        if ( shouldUseAllColors )
        {
            for ( int i = 0; i < fireCannonObjects.Count; i++ )
            {
                var fireCannonObject = fireCannonObjects[i];
                var direction = directions[i];
                bool shouldCurve = true;
                bool shouldZigZag = true;

                _ = MoveFireCannonObject( fireCannonObject, targetPlayer, direction, shouldCurve, shouldZigZag );
            }
        }
        else
        {
            var random = new Random();
            int colorChoice = random.Next( 3 ); // Zufällige Auswahl der Farbe für die Welle

            bool shouldCurve = colorChoice == 1;
            bool shouldZigZag = colorChoice == 0;

            for ( int i = 0; i < fireCannonObjects.Count; i++ )
            {
                var fireCannonObject = fireCannonObjects[i];
                var direction = directions[i];

                _ = MoveFireCannonObject( fireCannonObject, targetPlayer, direction, shouldCurve, shouldZigZag );
            }
        }

        FireCannonAttackTime = 0.0f;
    }
    private void CreateFireCannonsAsync( List<GameObject> fireCannonObjects )
    {
        for ( int i = 0; i < 3; i++ )
        {
            var fireCannonObject = GameObject.Clone( FireCannonPrefab );
            fireCannonObject.WorldPosition = WorldPosition + new Vector3( 0, 0, 0 ); // 50 Einheiten unter dem NPC
            fireCannonObject.WorldRotation = Rotation.Identity;
            fireCannonObjects.Add( fireCannonObject );
        }
    }

    private async Task MoveFireCannonObject( GameObject fireCannonObject, Player targetPlayer, Vector3 initialDirection, bool shouldCurve, bool shouldZigZag )
    {
        const float speed = 200.0f; // Geschwindigkeit des Feuerballs
        const float updateInterval = 0.01f; // Update alle 10ms für eine flüssigere Bewegung
        const float zigZagDuration = 0.5f; // Dauer des Zick-Zack-Musters in Sekunden
        const float straightFlightDuration = 3.0f; // Dauer des geraden Flugs in Sekunden
        const float increasedSpeed = 600.0f; // Erhöhte Geschwindigkeit nach dem Homing-Effekt

        // Schieße die Kugel in die angegebene Richtung
        fireCannonObject.WorldPosition += initialDirection * 200.0f;

        if ( shouldZigZag )
        {
            SetSpriteColor( fireCannonObject, Color.Green );
        }
        else if ( shouldCurve )
        {
            SetSpriteColor( fireCannonObject, Color.Red );
        }
        else
        {
            SetSpriteColor( fireCannonObject, Color.Blue );
        }

        float elapsedTime = 0.0f;

        if ( shouldZigZag )
        {
            // Bewege das Objekt sofort in einem verstärkten Zick-Zack-Muster
            while ( elapsedTime < zigZagDuration )
            {
                await Task.Delay( (int)(updateInterval * 1000) ); // Update alle 10ms
                var targetPosition = targetPlayer.WorldPosition;
                var direction = (targetPosition - fireCannonObject.WorldPosition).Normal;
                var zigZagDirection = MathF.Sin( elapsedTime * 20.0f ) * initialDirection.Cross( Vector3.Up ).Normal; // Zick-Zack-Richtung
                fireCannonObject.WorldPosition += (direction + zigZagDirection * 1.0f) * increasedSpeed * updateInterval;

                elapsedTime += updateInterval;
            }

            // Fliege für eine Sekunde geradeaus und erhöhe die Geschwindigkeit
            var straightFlightDirection = (targetPlayer.WorldPosition - fireCannonObject.WorldPosition).Normal;
            elapsedTime = 0.0f;

            while ( elapsedTime < straightFlightDuration )
            {
                await Task.Delay( (int)(updateInterval * 1000) ); // Update alle 10ms
                fireCannonObject.WorldPosition += straightFlightDirection * increasedSpeed * updateInterval; // Bewege das Objekt

                elapsedTime += updateInterval;
            }
        }
        else
        {
            // Homing-Phase für Kurven und normale Bewegung
            float homingDuration = shouldCurve ? 0.5f : 0.25f;

            while ( elapsedTime < homingDuration )
            {
                await Task.Delay( (int)(updateInterval * 1000) ); // Update alle 10ms
                var targetPosition = targetPlayer.WorldPosition;
                var direction = (targetPosition - fireCannonObject.WorldPosition).Normal;
                fireCannonObject.WorldPosition += direction * speed * updateInterval; // Bewege das Objekt

                elapsedTime += updateInterval;
            }

            // Fliege für eine Sekunde geradeaus und erhöhe die Geschwindigkeit
            var straightFlightDirection = (targetPlayer.WorldPosition - fireCannonObject.WorldPosition).Normal;
            elapsedTime = 0.0f;

            while ( elapsedTime < straightFlightDuration )
            {
                await Task.Delay( (int)(updateInterval * 1000) ); // Update alle 10ms
                if ( shouldCurve )
                {
                    // Bewege das Objekt in einem Bogen
                    var curveDirection = initialDirection.Cross( Vector3.Up ).Normal; // Bogenrichtung
                    fireCannonObject.WorldPosition += (straightFlightDirection + curveDirection * 0.5f) * increasedSpeed * updateInterval;
                }
                else
                {
                    fireCannonObject.WorldPosition += straightFlightDirection * increasedSpeed * updateInterval; // Bewege das Objekt
                }

                elapsedTime += updateInterval;
            }
        }

        fireCannonObject.Destroy();
    }
    private void SetSpriteColor( GameObject gameObject, Color color )
    {
        var spriteRenderer = gameObject.Components.Get<SpriteRenderer>();
        if ( spriteRenderer != null )
        {
            spriteRenderer.Color = color;
        }
    }

    private void FlameWallAttack( Player targetPlayer )
    {
        if ( targetPlayer == null )
        {
            return;
        }
        if ( !FireWallEnabled )
        {
            return;
        }

        var flameWallObjects = new List<GameObject>();

        if ( FireWallSound != null )
        {
            Sound.Play( FireWallSound );
        }

        bool shouldIncreaseSizeAndSpeed = npc.Health < npc.MaxHealth * 0.5f; // Überprüfen Sie, ob der NPC unter 50% Leben ist

        CreateFlameWallAsync( flameWallObjects, shouldIncreaseSizeAndSpeed, targetPlayer );

        foreach ( var flameWallObject in flameWallObjects )
        {
            flameWallObject.NetworkSpawn();
            flameWallObject.Network.DropOwnership();
        }

        var direction = (targetPlayer.WorldPosition - WorldPosition).Normal;
        float speed = shouldIncreaseSizeAndSpeed ? 400.0f : 200.0f; // Verdoppeln Sie die Geschwindigkeit, wenn der NPC unter 50% Leben ist

        for ( int i = 0; i < flameWallObjects.Count; i++ )
        {
            var flameWallObject = flameWallObjects[i];
            _ = MoveFlameWallObject( flameWallObject, direction, speed );
        }

        FireCannonAttackTime = 0.0f;
    }

    private void CreateFlameWallAsync( List<GameObject> flameWallObjects, bool shouldIncreaseSizeAndSpeed, Player targetPlayer )
    {
        int wallSize = shouldIncreaseSizeAndSpeed ? 9 : 3; // Verdoppeln Sie die Größe der Wand, wenn der NPC unter 50% Leben ist
        const float spacing = 50.0f; // Abstand zwischen den Flammenobjekten

        var direction = (targetPlayer.WorldPosition - WorldPosition).Normal;
        var perpendicularDirection = new Vector3( -direction.y, direction.x, direction.z ); // Berechne die Richtung, die senkrecht zum Spieler ist

        for ( int i = -wallSize / 2; i <= wallSize / 2; i++ )
        {
            var flameWallObject = GameObject.Clone( FireWallPrefab );
            flameWallObject.WorldPosition = WorldPosition + perpendicularDirection * i * spacing;
            flameWallObject.WorldRotation = Rotation.Identity;
            flameWallObjects.Add( flameWallObject );
            activeFlameWallObjects.Add( flameWallObject );
        }
    }
    private async Task MoveFlameWallObject( GameObject flameWallObject, Vector3 direction, float speed )
    {
        const float updateInterval = 0.01f; // Update alle 10ms für eine flüssigere Bewegung
        const float maxDistance = 2000.0f; // Maximale Distanz, bevor die Flammenobjekte zerstört werden

        float distanceTraveled = 0.0f;

        while ( distanceTraveled < maxDistance )
        {
            await Task.Delay( (int)(updateInterval * 1000) ); // Update alle 10ms
            flameWallObject.WorldPosition += direction * speed * updateInterval; // Bewege das Objekt
            distanceTraveled += speed * updateInterval;
        }

        flameWallObject.Destroy();
    }
    private async void UltimateJumpAttack( Player targetPlayer )
    {
        
        if ( targetPlayer == null )
        {
            
            return;
        }

        if ( this.npc == null )
        {
          
            return;
        }

        if ( this.npc.NavMeshAgent == null )
        {
           
            return;
        }
        if(!IsPlayerTooCloseUltimate(targetPlayer))
        {
            return;
        }
        SkinnedModelRenderer skinnedModelRenderer = this.GetComponent<SkinnedModelRenderer>();
        if ( skinnedModelRenderer != null )
        {
            skinnedModelRenderer.SceneObject.Attributes.Set( "BossMode", true );
        }

        // Speichern Sie die letzte bekannte Position des Spielers
        var targetPosition = GetRandomPositionAround( targetPlayer.WorldPosition, 50f, 100f ); // Generiere eine zufällige Position im Radius von 50 bis 100 Einheiten um den Spieler

        // Verzögerung vor dem Sprung
        const float jumpDelay = 0.1f; // 1 Sekunde Verzögerung
        await Task.Delay( (int)(jumpDelay * 1000) );

        // Berechnen Sie die Zwischenpunkte für die Bogenbewegung
        var startPosition = this.WorldPosition;
        var midPoint = (startPosition + targetPosition) / 2 + MidPointOffset; // Verwenden Sie die Property MidPointOffset
        const float updateInterval = 0.01f; // Update alle 10ms für eine flüssigere Bewegung

        float elapsedTime = 0.0f;

    

        // Deaktivieren Sie die automatische Positionsaktualisierung
        this.npc.NavMeshAgent.UpdatePosition = false;

        // Bewege den NPC in einem Bogen zur Zielposition
        while ( elapsedTime < HoverDuration )
        {
            await Task.Delay( (int)(updateInterval * 1000) ); // Update alle 10ms
            float t = elapsedTime / HoverDuration;
            this.WorldPosition = Vector3.Lerp( Vector3.Lerp( startPosition, midPoint, t ), Vector3.Lerp( midPoint, targetPosition, t ), t );
            elapsedTime += updateInterval;
        }

        // Setze die Position des NPCs auf die Zielposition
        this.WorldPosition = targetPosition;
        this.npc.NavMeshAgent.SetAgentPosition( targetPosition );

        // Reaktivieren Sie die automatische Positionsaktualisierung
        this.npc.NavMeshAgent.UpdatePosition = true;

       

        // Lösen Sie den FireRing-Angriff aus, wenn die Abklingzeit abgelaufen ist
        if ( FireRingAttackTime > FireRingCooldown )
        {
            ExecuteFireRingAttack();
            FireRingAttackTime = 0.0f;
        }

        // Setzen Sie die UltimateJumpAttackTime zurück
        UltimateJumpAttackTime = 0.0f;
    }

    private Vector3 GetRandomPositionAround( Vector3 position, float minRange, float maxRange )
    {
        var random = new Random();
        var angle = random.NextDouble() * Math.PI * 2;
        var radius = random.NextDouble() * (maxRange - minRange) + minRange;
        var offset = new Vector3( (float)(Math.Cos( angle ) * radius), (float)(Math.Sin( angle ) * radius), 0 );
        return position + offset;
    }
}