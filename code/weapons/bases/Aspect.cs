using Sandbox;
using System;
using System.Numerics;
using Sandbox; // Für GameObject, Transform, etc.
using Sandbox.Physics;
using Sandbox.UI; // Für TextRenderer, FaceThing, etc.
using System.Threading.Tasks; // Für asynchrone Methoden
using System.Collections.Generic; // Für Listen
using System.Linq; // Für LINQ-Abfragen



namespace GeneralGame;

public partial class BaseGun : WeaponComponent, IUse
{

    /// <summary>
    /// Bleed Aspect
    /// </summary>
    /// <param name="damageable"></param>
    /// <param name="shooter"></param>
    private void ApplyBleedAspectPassive( IHealthComponent damageable, Player shooter )
    {

        if ( damageable is Npc npc )
        {
            Random random = new Random();
            random.Next( 0, 101 );
            if ( random.Next( 0, 101 ) <= 10 )
            {
                var bleedEffect = new BleedEffect( 5 ); // Dauer in Sekunden
                npc.ApplyStatusEffect( bleedEffect );

                // Erstelle das Prefab und lasse es auf den Spieler zufliegen
                CreateHomingBleedPrefab( npc, shooter );
            }



        }
    }
    private void CreateHomingBleedPrefab( Npc npc, Player shooter )
    {

        var prefabInstance = ResourceLibrary.Get<PrefabFile>( "particles/prefabs/homing_bleed.prefab" );
        if ( prefabInstance != null )
        {
            var prefabObject = GameObject.Clone( prefabInstance );
            if ( prefabObject != null )
            {
                prefabObject.WorldPosition = npc.WorldPosition; // Startposition auf den NPC setzen

                // Bewege das Prefab auf den Spieler zu
                MovePrefabToPlayer( prefabObject, shooter );
            }
        }
    }
    private void FireBulletWithBleedAspect( Player shooter )
    {
      
        Owner.ApplyRecoil( Recoil );
        EffectRenderer?.Set( "b_empty", AmmoInClip == 0 );
        EffectRenderer?.Set( "b_attack", true );
        EffectRenderer?.Set( "b_reload", false );
        NextAttackTime = 1f / FireRate;
        AmmoInClip--;

        var cameraPos = Owner.PlyCamera.WorldPosition;
        var cameraDir = Owner.PlyCamera.WorldRotation.Forward;

        // Zielstrahl vom Kamerazentrum durch das Fadenkreuz
        var targetRay = Scene.Trace.Ray( cameraPos, cameraPos + cameraDir * 5000f )
            .IgnoreGameObjectHierarchy( GameObject.Root )
            .WithoutTags( "player" )
            .UseHitboxes( true )
            .Run();

        Vector3 targetPoint = targetRay.Hit ? targetRay.EndPosition : cameraPos + cameraDir * 5000f;

        // 2. Mündungsposition abhängig vom Kameramodus bestimmen
        Vector3 muzzlePos;

        if ( Owner.CameraMode == 0 ) // First-Person
        {
            // In Ego-Perspektive: Mündungsposition aus dem Waffen-Attachment
            var attachment = EffectRenderer.GetAttachment( "muzzle" );
            muzzlePos = attachment?.Position ?? cameraPos;
        }
        else // Third-Person
        {
            // In Third-Person: Mündung aus dem sichtbaren Waffen-Model
            var muzzlePoint = GameObject.Components.GetInDescendantsOrSelf<MuzzlePoint>();
            if ( muzzlePoint != null )
            {
                muzzlePos = muzzlePoint.WorldPosition;
            }
            else
            {
                // Fallback auf die Position der Waffe + Offset in Blickrichtung
                muzzlePos = GameObject.Transform.World.Position + GameObject.Transform.World.Rotation.Forward * 20 + Vector3.Forward * 200;
            }
        }

        // 3. Schussrichtung vom Mündungspunkt zum Zielpunkt
        Vector3 shootDirection = (targetPoint - muzzlePos).Normal;

        // Korrektur für Third-Person: Versatz nach links hinzufügen
        // Korrektur für Third-Person: Versatz nach links und oben hinzufügen
        if ( Owner.CameraMode != 0 ) // Third-Person
        {

        }

        // 4. Jetzt erst den Spread hinzufügen
        shootDirection += Vector3.Random * Spread;

        // 5. Endpunkt berechnen
        var endPos = muzzlePos + shootDirection * 5000f;

        var trace = Scene.Trace.Ray( muzzlePos, endPos )
            .IgnoreGameObjectHierarchy( GameObject.Root )
            .WithoutTags( "player" )
            .UseHitboxes( true )
            .Run();

        // Setze endPos auf die Trefferposition, wenn etwas getroffen wird
        if ( trace.Hit )
        {
            endPos = trace.EndPosition;


        }

        if ( Trail != null )
        {
            var trailInstance = ResourceLibrary.Get<PrefabFile>( "particles/prefabs/aspects/firebullet_bleed.prefab" ); // Verwende das neue Prefab
            if ( trailInstance != null )
            {
                var trailobject = GameObject.Clone( trailInstance );
                if ( trailobject != null )
                {
                    trailobject.WorldPosition = muzzlePos; // Setze die Startposition auf die Mündung

                    var trailobjectRenderer = trailobject.Components.Get<ParticleEffect>();
                    if ( trailobjectRenderer != null )
                    {
                        trailobjectRenderer.Yaw = Rotation.LookAt( shootDirection ).Yaw();
                        trailobjectRenderer.Pitch = Rotation.LookAt( shootDirection ).Pitch();
                    }

                    var speed = BulletSpeed * 1000f; // Geschwindigkeit des Schusses basierend auf BulletSpeed
                    UpdateTrailObjectPosition( trailobject, shootDirection, speed, endPos, shooter );
                }
            }
        }
        if ( trace.Hit )
        {
            var damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();
            if ( damageable != null )
            {
                ApplyBleedAspectPassive( damageable, shooter );
            }
        }



        SendAttackMessage( muzzlePos, endPos, trace.Distance, trace );

        return;
    }


    /// <summary>
    /// Air Aspect
    /// </summary>
    /// <param name="shooter"></param>
    private void FireBulletWithAirAspect( Player shooter )
    {
      
        Owner.ApplyRecoil( Recoil );

        if ( EffectRenderer == null )
        {
            Log.Error( "EffectRenderer is null" );
            return;
        }

        EffectRenderer?.Set( "b_empty", AmmoInClip == 0 );
        EffectRenderer?.Set( "b_attack", true );
        EffectRenderer?.Set( "b_reload", false );
        NextAttackTime = 1f / FireRate;
        AmmoInClip--;

       

        var cameraPos = Owner.PlyCamera.WorldPosition;
        var cameraDir = Owner.PlyCamera.WorldRotation.Forward;

        // Zielstrahl vom Kamerazentrum durch das Fadenkreuz
        var targetRay = Scene.Trace.Ray( cameraPos, cameraPos + cameraDir * 5000f )
            .IgnoreGameObjectHierarchy( GameObject.Root )
            .WithoutTags( "player" )
            .UseHitboxes( true )
            .Run();

        Vector3 targetPoint = targetRay.Hit ? targetRay.EndPosition : cameraPos + cameraDir * 5000f;

        // 2. Mündungsposition abhängig vom Kameramodus bestimmen
        Vector3 muzzlePos;

        if ( Owner.CameraMode == 0 ) // First-Person
        {
            // In Ego-Perspektive: Mündungsposition aus dem Waffen-Attachment
            var attachment = EffectRenderer.GetAttachment( "muzzle" );
            muzzlePos = attachment?.Position ?? cameraPos;
        }
        else // Third-Person
        {
            // In Third-Person: Mündung aus dem sichtbaren Waffen-Model
            var muzzlePoint = GameObject.Components.GetInDescendantsOrSelf<MuzzlePoint>();
            if ( muzzlePoint != null )
            {
                muzzlePos = muzzlePoint.WorldPosition;
            }
            else
            {
                // Fallback auf die Position der Waffe + Offset in Blickrichtung
                muzzlePos = GameObject.Transform.World.Position + GameObject.Transform.World.Rotation.Forward * 20 + Vector3.Forward * 200;
            }
        }

        // 3. Schussrichtung vom Mündungspunkt zum Zielpunkt
        Vector3 shootDirection = (targetPoint - muzzlePos).Normal;

        // Korrektur für Third-Person: Versatz nach links hinzufügen
        // Korrektur für Third-Person: Versatz nach links und oben hinzufügen
        if ( Owner.CameraMode != 0 ) // Third-Person
        {

        }

        // 4. Jetzt erst den Spread hinzufügen
        shootDirection += Vector3.Random * Spread;

        // 5. Endpunkt berechnen
        var endPos = muzzlePos + shootDirection * 5000f;

        var trace = Scene.Trace.Ray( muzzlePos, endPos )
            .IgnoreGameObjectHierarchy( GameObject.Root )
            .WithoutTags( "player" )
            .UseHitboxes( true )
            .Run();

        // Setze endPos auf die Trefferposition, wenn etwas getroffen wird
        if ( trace.Hit )
        {
            endPos = trace.EndPosition;
        }

        if ( Trail != null )
        {
            var trailInstance = ResourceLibrary.Get<PrefabFile>( "particles/prefabs/aspects/firebullet_air.prefab" ); // Verwende das neue Prefab
            if ( trailInstance != null )
            {
                var trailobject = GameObject.Clone( trailInstance );
                if ( trailobject != null )
                {
                    trailobject.WorldPosition = muzzlePos; // Setze die Startposition auf die Mündung

                    var trailobjectRenderer = trailobject.Components.Get<ParticleEffect>();
                    if ( trailobjectRenderer != null )
                    {
                        trailobjectRenderer.Yaw = Rotation.LookAt( shootDirection ).Yaw();
                        trailobjectRenderer.Pitch = Rotation.LookAt( shootDirection ).Pitch();
                    }

                    var speed = BulletSpeed * 750f; // Geschwindigkeit des Schusses basierend auf BulletSpeed
                    UpdateTrailObjectPosition( trailobject, shootDirection, speed, endPos, shooter );
                }
            }
        }

        if ( trace.Component != null )
        {
            var damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();
            if ( damageable != null )
            {
                ApplyAirAspectPassive( damageable );
            }
        }

        SendAttackMessage( muzzlePos, endPos, trace.Distance, trace );

        return;
    }
    private void ApplyAirAspectPassive( IHealthComponent damageable )
    {
        // Implementiere die Logik für den Blitz-Aspekt
        if ( damageable is Npc npc )
        {
            Random random = new Random();
            int chance = random.Next( 0, 100 );
            if ( chance < 10 )
            {
                var stunEffect = new StunEffect( 3 ); // Dauer in Sekunden
                npc.ApplyStatusEffect( stunEffect );

                // Erzeuge einen Tornado
                var tornadoPrefab = ResourceLibrary.Get<PrefabFile>( "particles/prefabs/firebullet_air_extra.prefab" );
                if ( tornadoPrefab != null )
                {
                    for ( int i = 0; i < 2; i++ )
                    {
                        var tornadoObject = GameObject.Clone( tornadoPrefab );
                        if ( tornadoObject != null )
                        {
                            float heightOffset = random.Next( 5, 25 ); // Zufällige Höhe zwischen 5 und 20
                            float speed = random.Next( 30, 100 ); // Zufällige Geschwindigkeit zwischen 30 und 70


                            tornadoObject.WorldPosition = npc.WorldPosition + Vector3.Up * heightOffset; // Setze die Startposition auf den NPC und versetze sie
                            var direction = Vector3.Up; // Beispielhafte Richtung, anpassen nach Bedarf
                            var endPos = npc.WorldPosition + direction * 200.0f; // Beispielhafte Endposition, anpassen nach Bedarf
                            Tornado( tornadoObject, direction, speed, endPos, null ); // Spieler ist hier nicht relevant
                        }
                        //tornadoObject.Destroy();

                    }

                }

            }
        }


    }

    private async void Tornado( GameObject trailobject, Vector3 direction, float speed, Vector3 endPos, Player shooter )
    {
        var startTime = Time.Now;
        var duration = 2.5f; // Dauer der Bewegung in Sekunden, anpassen nach Bedarf
        var zigzagFrequency = 15.0f; // Frequenz der Zickzack-Bewegung
        var zigzagAmplitude = 15.0f; // Amplitude der Zickzack-Bewegung
        var pullRadius = 500.0f; // Radius, in dem NPCs angezogen werden
        var pullStrength = 100.0f; // Stärke des Anziehens
        var airDuration = 3.0f; // Dauer, die NPCs in der Luft bleiben sollen

        var affectedNpcs = new List<Npc>();
        var npcStartTimes = new Dictionary<Npc, float>();

        while ( Time.Now - startTime < duration )
        {
            var elapsedTime = Time.Now - startTime;
            var zigzagOffset = new Vector3(
                MathF.Sin( elapsedTime * zigzagFrequency ) * zigzagAmplitude,
                MathF.Cos( elapsedTime * zigzagFrequency ) * zigzagAmplitude,
                0
            );

            if ( trailobject != null )
            {
                trailobject.WorldPosition += (direction * speed * Time.Delta) + zigzagOffset;

                // Überprüfen, ob das Objekt die Endposition erreicht hat oder etwas trifft
                var trace = Scene.Trace.Ray( trailobject.WorldPosition, trailobject.WorldPosition + direction * 100f )
                    .IgnoreGameObjectHierarchy( GameObject.Root )
                    .WithoutTags( "player" )
                    .UseHitboxes( true )
                    .Run();

                if ( trace.Hit )
                {


                    if ( ImpactArea != null )
                    {
                        var impactInstance = ResourceLibrary.Get<PrefabFile>( ImpactArea.ResourcePath );
                        if ( impactInstance != null )
                        {
                            var impactObject = GameObject.Clone( impactInstance );
                            if ( impactObject != null )
                            {
                                impactObject.WorldPosition = trace.EndPosition;
                                impactObject.WorldRotation = Rotation.LookAt( trace.Normal );

                                var impactRenderer = impactObject.Components.Get<ParticleEffect>();
                                if ( impactRenderer != null )
                                {
                                    impactRenderer.Yaw = Rotation.LookAt( direction ).Yaw();
                                    impactRenderer.Pitch = Rotation.LookAt( direction ).Pitch();
                                }
                            }
                        }
                    }

                    trailobject.Destroy(); // Zerstöre das Objekt
                    return;
                }

                // Ziehe NPCs in der Nähe an und schleudere sie um den Tornado
                var npcs = FindNpcsInRadius( trailobject.WorldPosition, pullRadius );
                foreach ( var npc in npcs )
                {
                    if ( !affectedNpcs.Contains( npc ) )
                    {
                        if ( npc.NavMeshAgent != null )
                        {
                            npc.NavMeshAgent.UpdatePosition = false;
                            npc.NavMeshAgent.Enabled = false;
                            affectedNpcs.Add( npc );
                            npcStartTimes[npc] = Time.Now;
                        }
                    }

                    var toTornado = (trailobject.WorldPosition - npc.WorldPosition).Normal;
                    npc.WorldPosition += toTornado * pullStrength * Time.Delta;

                    // Hebe den NPC in die Luft
                    npc.WorldPosition += Vector3.Up * 10.0f * Time.Delta;

                    // Schleudere NPCs um den Tornado
                    var angle = elapsedTime * zigzagFrequency;
                    var offset = new Vector3(
                        MathF.Cos( angle ) * zigzagAmplitude,
                        MathF.Sin( angle ) * zigzagAmplitude,
                        0
                    );
                    npc.WorldPosition += offset * Time.Delta;

                    // Überprüfen, ob die NPCs 5 Sekunden in der Luft waren
                    if ( npcStartTimes.ContainsKey( npc ) && Time.Now - npcStartTimes[npc] >= airDuration )
                    {
                        npc.NavMeshAgent.UpdatePosition = true;
                        affectedNpcs.Remove( npc );
                        npcStartTimes.Remove( npc );
                    }
                }

                // Aktualisiere die Position der NPCs, um dem Trail zu folgen
                foreach ( var npc in affectedNpcs )
                {
                    if ( trailobject != null && npc != null && npc.IsValid() )
                    {
                        var toTrail = (trailobject.WorldPosition - npc.WorldPosition).Normal;
                        npc.WorldPosition += toTrail * speed * Time.Delta;
                    }
                }

                if ( trailobject != null && (trailobject.WorldPosition - endPos).Length < 1.0f ) // Überprüfen, ob das Objekt die Endposition erreicht hat
                {
                    trailobject.Destroy(); // Zerstöre das Objekt
                    break;
                }


            }

            await Task.Delay( 10 ); // Aktualisiere die Position alle 10 Millisekunden
        }

        // Reaktiviere NavMeshAgent.UpdatePosition für alle betroffenen NPCs
        foreach ( var npc in affectedNpcs )
        {
            if ( npc.NavMeshAgent != null && npc.IsValid() )
            {
                npc.NavMeshAgent.UpdatePosition = true;
                npc.NavMeshAgent.Enabled = true;

                // Setze die NPC-Position auf den Boden
                var groundTrace = Scene.Trace.Ray( npc.WorldPosition, npc.WorldPosition - Vector3.Up * 200f )
                    .IgnoreGameObjectHierarchy( npc.GameObject )
                    .WithoutTags( "player", "npc", "trigger" )
                    .Run();

                if ( groundTrace.Hit )
                {
                    npc.WorldPosition = groundTrace.EndPosition;
                }
            }
        }

        if ( trailobject != null )
        {
            trailobject.Destroy(); // Zerstöre das Objekt nach Ablauf der Dauer
        }
    }


/// <summary>
/// Fire Aspect
/// </summary>
/// <param name="shooter"></param>
    private void FireBulletWithFireAspect( Player shooter )
    {
      
        Owner.ApplyRecoil( Recoil );
        EffectRenderer?.Set( "b_empty", AmmoInClip == 0 );
        EffectRenderer?.Set( "b_attack", true );
        EffectRenderer?.Set( "b_reload", false );
        NextAttackTime = 1f / FireRate;
        AmmoInClip--;

        var cameraPos = Owner.PlyCamera.WorldPosition;
        var cameraDir = Owner.PlyCamera.WorldRotation.Forward;

        // Zielstrahl vom Kamerazentrum durch das Fadenkreuz
        var targetRay = Scene.Trace.Ray( cameraPos, cameraPos + cameraDir * 5000f )
            .IgnoreGameObjectHierarchy( GameObject.Root )
            .WithoutTags( "player" )
            .UseHitboxes( true )
            .Run();

        Vector3 targetPoint = targetRay.Hit ? targetRay.EndPosition : cameraPos + cameraDir * 5000f;

        // 2. Mündungsposition abhängig vom Kameramodus bestimmen
        Vector3 muzzlePos;

        if ( Owner.CameraMode == 0 ) // First-Person
        {
            // In Ego-Perspektive: Mündungsposition aus dem Waffen-Attachment
            var attachment = EffectRenderer.GetAttachment( "muzzle" );
            muzzlePos = attachment?.Position ?? cameraPos;
        }
        else // Third-Person
        {
            // In Third-Person: Mündung aus dem sichtbaren Waffen-Model
            var muzzlePoint = GameObject.Components.GetInDescendantsOrSelf<MuzzlePoint>();
            if ( muzzlePoint != null )
            {
                muzzlePos = muzzlePoint.WorldPosition;
            }
            else
            {
                // Fallback auf die Position der Waffe + Offset in Blickrichtung
                muzzlePos = GameObject.Transform.World.Position + GameObject.Transform.World.Rotation.Forward * 20 + Vector3.Forward * 200;
            }
        }

        // 3. Schussrichtung vom Mündungspunkt zum Zielpunkt
        Vector3 shootDirection = (targetPoint - muzzlePos).Normal;

        // Korrektur für Third-Person: Versatz nach links hinzufügen
        // Korrektur für Third-Person: Versatz nach links und oben hinzufügen
        if ( Owner.CameraMode != 0 ) // Third-Person
        {

        }

        // 4. Jetzt erst den Spread hinzufügen
        shootDirection += Vector3.Random * Spread;

        // 5. Endpunkt berechnen
        var endPos = muzzlePos + shootDirection * 5000f;

        var trace = Scene.Trace.Ray( muzzlePos, endPos )
            .IgnoreGameObjectHierarchy( GameObject.Root )
            .WithoutTags( "player" )
            .UseHitboxes( true )
            .Run();

        // Setze endPos auf die Trefferposition, wenn etwas getroffen wird
        if ( trace.Hit )
        {
            endPos = trace.EndPosition;
        }

        if ( Trail != null )
        {
            var trailInstance = ResourceLibrary.Get<PrefabFile>( "particles/prefabs/aspects/firebullet_fire.prefab" ); // Verwende das neue Prefab
            if ( trailInstance != null )
            {
                var trailobject = GameObject.Clone( trailInstance );
                if ( trailobject != null )
                {
                    trailobject.WorldPosition = muzzlePos; // Setze die Startposition auf die Mündung

                    var trailobjectRenderer = trailobject.Components.Get<ParticleEffect>();
                    if ( trailobjectRenderer != null )
                    {
                        trailobjectRenderer.Yaw = Rotation.LookAt( shootDirection ).Yaw();
                        trailobjectRenderer.Pitch = Rotation.LookAt( shootDirection ).Pitch();
                    }

                    var speed = BulletSpeed * 1000f; // Geschwindigkeit des Schusses basierend auf BulletSpeed
                    UpdateTrailObjectPosition( trailobject, shootDirection, speed, endPos, shooter );
                }
            }
        }

        if ( trace.Hit )
        {
            var damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();
            if ( damageable != null )
            {
                ApplyFireAspectPassive( damageable, shooter );
            }
        }

        SendAttackMessage( muzzlePos, endPos, trace.Distance, trace );
    }
    private void ApplyFireAspectPassive( IHealthComponent damageable, Player shooter )
    {
        
        if ( damageable is Npc npc )
        {
            var fireEffect = new BurnEffectNpc( 5 ); // Dauer in Sekunden
            npc.ApplyStatusEffect( fireEffect );

            // Übertrage den Feuereffekt auf andere NPCs in der Nähe
            var nearbyNpcs = FindNpcsInRadius( npc.WorldPosition, 30.0f ); // Radius in dem der Effekt übertragen wird
            foreach ( var nearbyNpc in nearbyNpcs )
            {
                if ( !nearbyNpc.HasStatusEffect<BurnEffectNpc>() )
                {
                    nearbyNpc.ApplyStatusEffect( fireEffect );
                }
            }
        }
    }



/// <summary>
/// // Water Aspect
/// </summary>
/// <param name="shooter"></param>
    private void FireBulletWithWaterAspect( Player shooter )
    {


     
        Owner.ApplyRecoil( Recoil );
        EffectRenderer?.Set( "b_empty", AmmoInClip == 0 );
        EffectRenderer?.Set( "b_attack", true );
        EffectRenderer?.Set( "b_reload", false );
        NextAttackTime = 1f / FireRate;
        AmmoInClip--;

        var cameraPos = Owner.PlyCamera.WorldPosition;
        var cameraDir = Owner.PlyCamera.WorldRotation.Forward;

        // Zielstrahl vom Kamerazentrum durch das Fadenkreuz
        var targetRay = Scene.Trace.Ray( cameraPos, cameraPos + cameraDir * 5000f )
            .IgnoreGameObjectHierarchy( GameObject.Root )
            .WithoutTags( "player" )
            .UseHitboxes( true )
            .Run();

        Vector3 targetPoint = targetRay.Hit ? targetRay.EndPosition : cameraPos + cameraDir * 5000f;

        // 2. Mündungsposition abhängig vom Kameramodus bestimmen
        Vector3 muzzlePos;

        if ( Owner.CameraMode == 0 ) // First-Person
        {
            // In Ego-Perspektive: Mündungsposition aus dem Waffen-Attachment
            var attachment = EffectRenderer.GetAttachment( "muzzle" );
            muzzlePos = attachment?.Position ?? cameraPos;
        }
        else // Third-Person
        {
            // In Third-Person: Mündung aus dem sichtbaren Waffen-Model
            var muzzlePoint = GameObject.Components.GetInDescendantsOrSelf<MuzzlePoint>();
            if ( muzzlePoint != null )
            {
                muzzlePos = muzzlePoint.WorldPosition;
            }
            else
            {
                // Fallback auf die Position der Waffe + Offset in Blickrichtung
                muzzlePos = GameObject.Transform.World.Position + GameObject.Transform.World.Rotation.Forward * 20 + Vector3.Forward * 200;
            }
        }

        // 3. Schussrichtung vom Mündungspunkt zum Zielpunkt
        Vector3 shootDirection = (targetPoint - muzzlePos).Normal;

        // Korrektur für Third-Person: Versatz nach links hinzufügen
        // Korrektur für Third-Person: Versatz nach links und oben hinzufügen
        if ( Owner.CameraMode != 0 ) // Third-Person
        {

        }

        // 4. Jetzt erst den Spread hinzufügen
        shootDirection += Vector3.Random * Spread;

        // 5. Endpunkt berechnen
        var endPos = muzzlePos + shootDirection * 5000f;

        var trace = Scene.Trace.Ray( muzzlePos, endPos )
            .IgnoreGameObjectHierarchy( GameObject.Root )
            .WithoutTags( "player" )
            .UseHitboxes( true )
            .Run();

        // Setze endPos auf die Trefferposition, wenn etwas getroffen wird
        if ( trace.Hit )
        {
            endPos = trace.EndPosition;
        }

        if ( Trail != null )
        {
            var trailInstance = ResourceLibrary.Get<PrefabFile>( "particles/prefabs/aspects/firebullet_water.prefab" ); // Verwende das neue Prefab
            if ( trailInstance != null )
            {
                var trailobject = GameObject.Clone( trailInstance );
                if ( trailobject != null )
                {
                    trailobject.WorldPosition = muzzlePos; // Setze die Startposition auf die Mündung


                    var trailobjectRenderer = trailobject.Components.Get<ParticleEffect>();
                    if ( trailobjectRenderer != null )
                    {
                        trailobjectRenderer.Yaw = Rotation.LookAt( shootDirection ).Yaw();
                        trailobjectRenderer.Pitch = Rotation.LookAt( shootDirection ).Pitch();

                    }

                    var speed = BulletSpeed * 750f; // Geschwindigkeit des Schusses basierend auf BulletSpeed
                    UpdateTrailObjectPosition( trailobject, shootDirection, speed, endPos, shooter );
                }
            }
        }
        if ( trace.Hit )
        {
            var damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();
            if ( damageable != null )
            {
                ApplyWaterAspectPassive( damageable );
            }
        }

        SendAttackMessage( muzzlePos, endPos, trace.Distance, trace );

        return;
    }
    private void ApplyWaterAspectPassive( IHealthComponent damageable )
    {
       
        // Implementiere die Logik für den Blitz-Aspekt
        if ( damageable is Npc npc )
        {
            Random random = new Random();
            int chance = random.Next( 0, 100 );
            if ( chance < 10 )
            {
                var stunEffect = new StunEffect( 3 ); // Dauer in Sekunden
                npc.ApplyStatusEffect( stunEffect );

                // Erzeuge einen Tornado
                var tornadoPrefab = ResourceLibrary.Get<PrefabFile>( "particles/prefabs/firebullet_air_extra.prefab" );
                if ( tornadoPrefab != null )
                {
                    for ( int i = 0; i < 2; i++ )
                    {
                        var tornadoObject = GameObject.Clone( tornadoPrefab );
                        if ( tornadoObject != null )
                        {
                            float heightOffset = random.Next( 5, 25 ); // Zufällige Höhe zwischen 5 und 20
                            float speed = random.Next( 30, 100 ); // Zufällige Geschwindigkeit zwischen 30 und 70


                            tornadoObject.WorldPosition = npc.WorldPosition + Vector3.Up * heightOffset; // Setze die Startposition auf den NPC und versetze sie
                            var direction = Vector3.Up; // Beispielhafte Richtung, anpassen nach Bedarf
                            var endPos = npc.WorldPosition + direction * 200.0f; // Beispielhafte Endposition, anpassen nach Bedarf
                            Tornado( tornadoObject, direction, speed, endPos, null ); // Spieler ist hier nicht relevant
                        }
                        //tornadoObject.Destroy();

                    }

                }

            }
        }
    }


/// <summary>
/// Ice Aspect
/// </summary>
/// <param name="shooter"></param>
    private void FireBulletWithIceAspect( Player shooter )
    {
       
        Owner.ApplyRecoil( Recoil );
        EffectRenderer?.Set( "b_empty", AmmoInClip == 0 );
        EffectRenderer?.Set( "b_attack", true );
        EffectRenderer?.Set( "b_reload", false );
        NextAttackTime = 1f / FireRate;
        AmmoInClip--;

        var cameraPos = Owner.PlyCamera.WorldPosition;
        var cameraDir = Owner.PlyCamera.WorldRotation.Forward;

        // Zielstrahl vom Kamerazentrum durch das Fadenkreuz
        var targetRay = Scene.Trace.Ray( cameraPos, cameraPos + cameraDir * 5000f )
            .IgnoreGameObjectHierarchy( GameObject.Root )
            .WithoutTags( "player" )
            .UseHitboxes( true )
            .Run();

        Vector3 targetPoint = targetRay.Hit ? targetRay.EndPosition : cameraPos + cameraDir * 5000f;

        // 2. Mündungsposition abhängig vom Kameramodus bestimmen
        Vector3 muzzlePos;

        if ( Owner.CameraMode == 0 ) // First-Person
        {
            // In Ego-Perspektive: Mündungsposition aus dem Waffen-Attachment
            var attachment = EffectRenderer.GetAttachment( "muzzle" );
            muzzlePos = attachment?.Position ?? cameraPos;
        }
        else // Third-Person
        {
            // In Third-Person: Mündung aus dem sichtbaren Waffen-Model
            var muzzlePoint = GameObject.Components.GetInDescendantsOrSelf<MuzzlePoint>();
            if ( muzzlePoint != null )
            {
                muzzlePos = muzzlePoint.WorldPosition;
            }
            else
            {
                // Fallback auf die Position der Waffe + Offset in Blickrichtung
                muzzlePos = GameObject.Transform.World.Position + GameObject.Transform.World.Rotation.Forward * 20 + Vector3.Forward * 200;
            }
        }

        // 3. Schussrichtung vom Mündungspunkt zum Zielpunkt
        Vector3 shootDirection = (targetPoint - muzzlePos).Normal;

        // Korrektur für Third-Person: Versatz nach links hinzufügen
        // Korrektur für Third-Person: Versatz nach links und oben hinzufügen
        if ( Owner.CameraMode != 0 ) // Third-Person
        {

        }

        // 4. Jetzt erst den Spread hinzufügen
        shootDirection += Vector3.Random * Spread;

        // 5. Endpunkt berechnen
        var endPos = muzzlePos + shootDirection * 5000f;

        var trace = Scene.Trace.Ray( muzzlePos, endPos )
            .IgnoreGameObjectHierarchy( GameObject.Root )
            .WithoutTags( "player" )
            .UseHitboxes( true )
            .Run();

        // Setze endPos auf die Trefferposition, wenn etwas getroffen wird
        if ( trace.Hit )
        {
            endPos = trace.EndPosition;
        }

        if ( Trail != null )
        {
            var trailInstance = ResourceLibrary.Get<PrefabFile>( "particles/prefabs/aspects/firebullet_ice.prefab" ); // Verwende das neue Prefab
            if ( trailInstance != null )
            {
                var trailobject = GameObject.Clone( trailInstance );
                if ( trailobject != null )
                {
                    trailobject.WorldPosition = muzzlePos; // Setze die Startposition auf die Mündung

                    var trailobjectRenderer = trailobject.Components.Get<ParticleEffect>();
                    if ( trailobjectRenderer != null )
                    {
                        trailobjectRenderer.Yaw = Rotation.LookAt( shootDirection ).Yaw();
                        trailobjectRenderer.Pitch = Rotation.LookAt( shootDirection ).Pitch();
                    }

                    var speed = BulletSpeed * 1000f; // Geschwindigkeit des Schusses basierend auf BulletSpeed
                    UpdateTrailObjectPosition( trailobject, shootDirection, speed, endPos, shooter );
                }
            }
        }

        if ( trace.Hit )
        {
            var damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();
            if ( damageable != null )
            {
                ApplyIceAspectPassive( damageable, shooter );
            }
        }

        SendAttackMessage( muzzlePos, endPos, trace.Distance, trace );
    }
    private void ApplyIceAspectPassive( IHealthComponent damageable, Player shooter )
    {
        Random random = new Random();
        random.Next( 0, 101 );
        if ( random.Next( 0, 101 ) <= 10 )
        {
            if ( damageable is Npc npc  ) 
            {
                if(!npc.HasStatusEffect<FreezeEffectNpc>())
                {
                    var freezeEffect = new FreezeEffectNpc( 5 ); // Dauer in Sekunden
                    npc.ApplyStatusEffect( freezeEffect );
                }
                

            }
        }
           
    }



/// <summary>
/// Earth Aspect
/// </summary>
/// <param name="shooter"></param>
    private void FireBulletWithEarthAspect( Player shooter )
    {
       
        Owner.ApplyRecoil( Recoil );
        EffectRenderer?.Set( "b_empty", AmmoInClip == 0 );
        EffectRenderer?.Set( "b_attack", true );
        EffectRenderer?.Set( "b_reload", false );
        NextAttackTime = 1f / FireRate;
        AmmoInClip--;

        var cameraPos = Owner.PlyCamera.WorldPosition;
        var cameraDir = Owner.PlyCamera.WorldRotation.Forward;

        // Zielstrahl vom Kamerazentrum durch das Fadenkreuz
        var targetRay = Scene.Trace.Ray( cameraPos, cameraPos + cameraDir * 5000f )
            .IgnoreGameObjectHierarchy( GameObject.Root )
            .WithoutTags( "player" )
            .UseHitboxes( true )
            .Run();

        Vector3 targetPoint = targetRay.Hit ? targetRay.EndPosition : cameraPos + cameraDir * 5000f;

        // 2. Mündungsposition abhängig vom Kameramodus bestimmen
        Vector3 muzzlePos;

        if ( Owner.CameraMode == 0 ) // First-Person
        {
            // In Ego-Perspektive: Mündungsposition aus dem Waffen-Attachment
            var attachment = EffectRenderer.GetAttachment( "muzzle" );
            muzzlePos = attachment?.Position ?? cameraPos;
        }
        else // Third-Person
        {
            // In Third-Person: Mündung aus dem sichtbaren Waffen-Model
            var muzzlePoint = GameObject.Components.GetInDescendantsOrSelf<MuzzlePoint>();
            if ( muzzlePoint != null )
            {
                muzzlePos = muzzlePoint.WorldPosition;
            }
            else
            {
                // Fallback auf die Position der Waffe + Offset in Blickrichtung
                muzzlePos = GameObject.Transform.World.Position + GameObject.Transform.World.Rotation.Forward * 20 + Vector3.Forward * 200;
            }
        }

        // 3. Schussrichtung vom Mündungspunkt zum Zielpunkt
        Vector3 shootDirection = (targetPoint - muzzlePos).Normal;

        // Korrektur für Third-Person: Versatz nach links hinzufügen
        // Korrektur für Third-Person: Versatz nach links und oben hinzufügen
        if ( Owner.CameraMode != 0 ) // Third-Person
        {

        }

        // 4. Jetzt erst den Spread hinzufügen
        shootDirection += Vector3.Random * Spread;

        // 5. Endpunkt berechnen
        var endPos = muzzlePos + shootDirection * 5000f;

        var trace = Scene.Trace.Ray( muzzlePos, endPos )
            .IgnoreGameObjectHierarchy( GameObject.Root )
            .WithoutTags( "player" )
            .UseHitboxes( true )
            .Run();

        // Setze endPos auf die Trefferposition, wenn etwas getroffen wird
        if ( trace.Hit )
        {
            endPos = trace.EndPosition;
        }

        if ( Trail != null )
        {
            var trailInstance = ResourceLibrary.Get<PrefabFile>( "particles/prefabs/aspects/firebullet_earth.prefab" );
            if ( trailInstance != null )
            {
                var trailobject = GameObject.Clone( trailInstance );
                if ( trailobject != null )
                {
                    trailobject.WorldPosition = muzzlePos; // Setze die Startposition auf die Mündung

                    var trailobjectRenderer = trailobject.Components.Get<ParticleEffect>();
                    if ( trailobjectRenderer != null )
                    {
                        trailobjectRenderer.Yaw = Rotation.LookAt( shootDirection ).Yaw();
                        trailobjectRenderer.Pitch = Rotation.LookAt( shootDirection ).Pitch();
                    }

                    var speed = BulletSpeed * 400f; // Geschwindigkeit des Schusses basierend auf BulletSpeed
                    UpdateTrailObjectPosition( trailobject, shootDirection, speed, endPos, shooter );
                }
            }
        }

        if ( trace.Hit )
        {
            var damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();
            if ( damageable != null )
            {
                Random random = new Random();
                random.Next( 0, 101 );
                if ( random.Next( 0, 101 ) <= 5 )
                {

                    
                    ApplyEarthAspectDamage( damageable, shooter );
                    SpawnEarthProjectilesAroundTarget( trace.EndPosition, shooter );
                }
            }
        }

        SendAttackMessage( muzzlePos, endPos, trace.Distance, trace );
    }

    private void SpawnEarthProjectilesAroundTarget( Vector3 targetPosition, Player shooter )
    {
        var prefab = ResourceLibrary.Get<PrefabFile>( "particles/prefabs/aspects/firebullet_earth.prefab" );
        if ( prefab != null )
        {
            for ( int i = 0; i < 4; i++ )
            {
                var projectile = GameObject.Clone( prefab );
                if ( projectile != null )
                {
                    var offset = GetOffsetPosition( i, targetPosition );
                    projectile.WorldPosition = offset;

                    var direction = (targetPosition - offset).Normal;
                    var speed = BulletSpeed * 400f;
                    MoveProjectileToCenter( projectile, targetPosition, direction, speed, shooter );
                }
            }
        }
    }

    private Vector3 GetOffsetPosition( int index, Vector3 center )
    {
        float angle = MathF.PI / 2 * index;
        float radius = 500f; // Abstand vom Zentrum
        return center + new Vector3( MathF.Cos( angle ) * radius, MathF.Sin( angle ) * radius, 0 );
    }

    private async void MoveProjectileToCenter( GameObject projectile, Vector3 center, Vector3 direction, float speed, Player shooter )
    {
        while ( (projectile.WorldPosition - center).Length > 1.0f )
        {
            projectile.WorldPosition += direction * speed * Time.Delta;

            // Überprüfen, ob das Projektil etwas trifft
            var trace = Scene.Trace.Ray( projectile.WorldPosition, projectile.WorldPosition + direction * 100f )
                .IgnoreGameObjectHierarchy( GameObject.Root )
                .WithoutTags( "player" )
                .UseHitboxes( true )
                .Run();

            if ( trace.Hit )
            {
                var damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();
                if ( damageable != null )
                {
                    ApplyEarthAspectDamage( damageable, shooter );
                }
                projectile.Destroy();
                return;
            }

            await Task.Delay( 10 );
        }

        projectile.Destroy();
    }

    private void ApplyEarthAspectDamage( IHealthComponent damageable, Player shooter )
    {
        if ( damageable is Npc npc )
        {
            
            
                var freezeEffect = new StunEffect( 5 ); // Dauer in Sekunden
                npc.ApplyStatusEffect( freezeEffect );
            
            // Füge dem NPC Schaden zu
            npc.TakeDamage( DamageType.Bullet, 50f, npc.WorldPosition, Vector3.Zero, shooter.Id, shooter.Id );
        }
    }




    private void FireBulletWithShadowAspect( Player shooter )
    {
       
        Owner.ApplyRecoil( Recoil );
        EffectRenderer?.Set( "b_empty", AmmoInClip == 0 );
        EffectRenderer?.Set( "b_attack", true );
        EffectRenderer?.Set( "b_reload", false );
        NextAttackTime = 1f / FireRate;
        AmmoInClip--;

        var cameraPos = Owner.PlyCamera.WorldPosition;
        var cameraDir = Owner.PlyCamera.WorldRotation.Forward;

        // Zielstrahl vom Kamerazentrum durch das Fadenkreuz
        var targetRay = Scene.Trace.Ray( cameraPos, cameraPos + cameraDir * 5000f )
            .IgnoreGameObjectHierarchy( GameObject.Root )
            .WithoutTags( "player" )
            .UseHitboxes( true )
            .Run();

        Vector3 targetPoint = targetRay.Hit ? targetRay.EndPosition : cameraPos + cameraDir * 5000f;

        // 2. Mündungsposition abhängig vom Kameramodus bestimmen
        Vector3 muzzlePos;

        if ( Owner.CameraMode == 0 ) // First-Person
        {
            // In Ego-Perspektive: Mündungsposition aus dem Waffen-Attachment
            var attachment = EffectRenderer.GetAttachment( "muzzle" );
            muzzlePos = attachment?.Position ?? cameraPos;
        }
        else // Third-Person
        {
            // In Third-Person: Mündung aus dem sichtbaren Waffen-Model
            var muzzlePoint = GameObject.Components.GetInDescendantsOrSelf<MuzzlePoint>();
            if ( muzzlePoint != null )
            {
                muzzlePos = muzzlePoint.WorldPosition;
            }
            else
            {
                // Fallback auf die Position der Waffe + Offset in Blickrichtung
                muzzlePos = GameObject.Transform.World.Position + GameObject.Transform.World.Rotation.Forward * 20 + Vector3.Forward * 200;
            }
        }

        // 3. Schussrichtung vom Mündungspunkt zum Zielpunkt
        Vector3 shootDirection = (targetPoint - muzzlePos).Normal;

        // Korrektur für Third-Person: Versatz nach links hinzufügen
        // Korrektur für Third-Person: Versatz nach links und oben hinzufügen
        if ( Owner.CameraMode != 0 ) // Third-Person
        {

        }

        // 4. Jetzt erst den Spread hinzufügen
        shootDirection += Vector3.Random * Spread;

        // 5. Endpunkt berechnen
        var endPos = muzzlePos + shootDirection * 5000f;

        var trace = Scene.Trace.Ray( muzzlePos, endPos )
            .IgnoreGameObjectHierarchy( GameObject.Root )
            .WithoutTags( "player" )
            .UseHitboxes( true )
            .Run();

        // Setze endPos auf die Trefferposition, wenn etwas getroffen wird
        if ( trace.Hit )
        {
            endPos = trace.EndPosition;
        }

        if ( Trail != null )
        {
            var trailInstance = ResourceLibrary.Get<PrefabFile>( "particles/prefabs/aspects/firebullet_shadow.prefab" );
            if ( trailInstance != null )
            {
                var trailobject = GameObject.Clone( trailInstance );
                if ( trailobject != null )
                {
                    trailobject.WorldPosition = muzzlePos; // Setze die Startposition auf die Mündung

                    var trailobjectRenderer = trailobject.Components.Get<ParticleEffect>();
                    if ( trailobjectRenderer != null )
                    {
                        trailobjectRenderer.Yaw = Rotation.LookAt( shootDirection ).Yaw();
                        trailobjectRenderer.Pitch = Rotation.LookAt( shootDirection ).Pitch();
                    }

                    var speed = BulletSpeed * 1000f; // Geschwindigkeit des Schusses basierend auf BulletSpeed
                    UpdateTrailObjectPosition( trailobject, shootDirection, speed, endPos, shooter );
                }
            }
        }

        if ( trace.Hit )
        {
            var damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();
            if ( damageable != null )
            {
                Random random = new Random();
                if ( random.Next( 0, 100 ) < 20 ) // 20% Chance
                {
                    SpawnShadowProjectiles( trace.Component.GameObject, shooter );
                }
            }
        }

        SendAttackMessage( muzzlePos, endPos, trace.Distance, trace );
    }

    private void SpawnShadowProjectiles( GameObject target, Player shooter )
    {
        var prefab = ResourceLibrary.Get<PrefabFile>( "particles/prefabs/aspects/firebullet_shadow.prefab" );
        if ( prefab != null )
        {
            for ( int i = 0; i < 3; i++ )
            {
                var projectile = GameObject.Clone( prefab );
                if ( projectile != null )
                {
                    var offset = GetOffsetPositionShadow( i, target.WorldPosition ) + Vector3.Up * 50f; // Versetze die Position um 50 Einheiten nach oben
                    projectile.WorldPosition = offset; // Setze die Startposition um den NPC
                    MoveProjectileToTarget( projectile, target, shooter );
                }
            }
        }
    }
    private Vector3 GetOffsetPositionShadow( int index, Vector3 center )
    {
        float angle = MathF.PI / 1.5f * index;
        float radius = 500f; // Abstand vom Zentrum
        return center + new Vector3( MathF.Cos( angle ) * radius, MathF.Sin( angle ) * radius, 0 );
    }

    private async void MoveProjectileToTarget( GameObject projectile, GameObject target, Player shooter )
    {
        var speed = 500f; // Geschwindigkeit des Projektils
        var targetPosition = target.WorldPosition + Vector3.Up * 50f; // Zielposition um 50 Einheiten nach oben versetzen

        while ( projectile.IsValid() && target.IsValid() && (projectile.WorldPosition - targetPosition).Length > 1.0f )
        {
            var direction = (targetPosition - projectile.WorldPosition).Normal;
            projectile.WorldPosition += direction * speed * Time.Delta;

            // Überprüfen, ob das Projektil etwas trifft
            var trace = Scene.Trace.Ray( projectile.WorldPosition, projectile.WorldPosition + direction * 100f )
                .IgnoreGameObjectHierarchy( GameObject.Root )
                .WithoutTags( "player" )
                .UseHitboxes( true )
                .Run();

            if ( trace.Hit )
            {
                var damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();
                if ( damageable != null )
                {
                    ApplyShadowAspectDamage( damageable, shooter );
                }
                projectile.Destroy();
                return;
            }

            await Task.Delay( 10 );
        }

        projectile.Destroy();
    }

    private void ApplyShadowAspectDamage( IHealthComponent damageable, Player shooter )
    {
        if ( damageable is Npc npc )
        {
            npc.TakeDamage( DamageType.Bullet, 50f, npc.GameObject.WorldPosition, Vector3.Zero, shooter.Id, shooter.Id );
        }
    }




    private void FireBulletWithLightningAspect( Player shooter )
    {
      
        Owner.ApplyRecoil( Recoil );
        EffectRenderer?.Set( "b_empty", AmmoInClip == 0 );
        EffectRenderer?.Set( "b_attack", true );
        EffectRenderer?.Set( "b_reload", false );
        NextAttackTime = 1f / FireRate;
        AmmoInClip--;

        var cameraPos = Owner.PlyCamera.WorldPosition;
        var cameraDir = Owner.PlyCamera.WorldRotation.Forward;

        // Zielstrahl vom Kamerazentrum durch das Fadenkreuz
        var targetRay = Scene.Trace.Ray( cameraPos, cameraPos + cameraDir * 5000f )
            .IgnoreGameObjectHierarchy( GameObject.Root )
            .WithoutTags( "player" )
            .UseHitboxes( true )
            .Run();

        Vector3 targetPoint = targetRay.Hit ? targetRay.EndPosition : cameraPos + cameraDir * 5000f;

        // 2. Mündungsposition abhängig vom Kameramodus bestimmen
        Vector3 muzzlePos;

        if ( Owner.CameraMode == 0 ) // First-Person
        {
            // In Ego-Perspektive: Mündungsposition aus dem Waffen-Attachment
            var attachment = EffectRenderer.GetAttachment( "muzzle" );
            muzzlePos = attachment?.Position ?? cameraPos;
        }
        else // Third-Person
        {
            // In Third-Person: Mündung aus dem sichtbaren Waffen-Model
            var muzzlePoint = GameObject.Components.GetInDescendantsOrSelf<MuzzlePoint>();
            if ( muzzlePoint != null )
            {
                muzzlePos = muzzlePoint.WorldPosition;
            }
            else
            {
                // Fallback auf die Position der Waffe + Offset in Blickrichtung
                muzzlePos = GameObject.Transform.World.Position + GameObject.Transform.World.Rotation.Forward * 20 + Vector3.Forward * 200;
            }
        }

        // 3. Schussrichtung vom Mündungspunkt zum Zielpunkt
        Vector3 shootDirection = (targetPoint - muzzlePos).Normal;

        // Korrektur für Third-Person: Versatz nach links hinzufügen
        // Korrektur für Third-Person: Versatz nach links und oben hinzufügen
        if ( Owner.CameraMode != 0 ) // Third-Person
        {

        }

        // 4. Jetzt erst den Spread hinzufügen
        shootDirection += Vector3.Random * Spread;

        // 5. Endpunkt berechnen
        var endPos = muzzlePos + shootDirection * 5000f;

        var trace = Scene.Trace.Ray( muzzlePos, endPos )
            .IgnoreGameObjectHierarchy( GameObject.Root )
            .WithoutTags( "player" )
            .UseHitboxes( true )
            .Run();

        // Setze endPos auf die Trefferposition, wenn etwas getroffen wird
        if ( trace.Hit )
        {
            endPos = trace.EndPosition;
        }

        if ( Trail != null )
        {
            var trailInstance = ResourceLibrary.Get<PrefabFile>( "particles/prefabs/aspects/firebullet_lightning.prefab" ); // Verwende das neue Prefab
            if ( trailInstance != null )
            {
                var trailobject = GameObject.Clone( trailInstance );
                if ( trailobject != null )
                {
                    trailobject.WorldPosition = muzzlePos; // Setze die Startposition auf die Mündung

                    var trailobjectRenderer = trailobject.Components.Get<ParticleEffect>();
                    if ( trailobjectRenderer != null )
                    {
                        trailobjectRenderer.Yaw = Rotation.LookAt( shootDirection ).Yaw();
                        trailobjectRenderer.Pitch = Rotation.LookAt( shootDirection ).Pitch();
                    }

                    var speed = BulletSpeed * 400f; // Geschwindigkeit des Schusses basierend auf BulletSpeed
                    UpdateTrailObjectPosition( trailobject, shootDirection, speed, endPos, shooter );
                }
            }
        }
        if ( trace.Hit )
        {
            var damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();
            if ( damageable != null )
            {
                //ApplyAirAspectPassive( damageable );
            }
        }

        SendAttackMessage( muzzlePos, endPos, trace.Distance, trace );

        return;
    }
   

    private void FireBulletWithHolyAspect( Player shooter )
    {
      
        Owner.ApplyRecoil( Recoil );
        EffectRenderer?.Set( "b_empty", AmmoInClip == 0 );
        EffectRenderer?.Set( "b_attack", true );
        EffectRenderer?.Set( "b_reload", false );
        NextAttackTime = 1f / FireRate;
        AmmoInClip--;

        var cameraPos = Owner.PlyCamera.WorldPosition;
        var cameraDir = Owner.PlyCamera.WorldRotation.Forward;

        // Zielstrahl vom Kamerazentrum durch das Fadenkreuz
        var targetRay = Scene.Trace.Ray( cameraPos, cameraPos + cameraDir * 5000f )
            .IgnoreGameObjectHierarchy( GameObject.Root )
            .WithoutTags( "player" )
            .UseHitboxes( true )
            .Run();

        Vector3 targetPoint = targetRay.Hit ? targetRay.EndPosition : cameraPos + cameraDir * 5000f;

        // 2. Mündungsposition abhängig vom Kameramodus bestimmen
        Vector3 muzzlePos;

        if ( Owner.CameraMode == 0 ) // First-Person
        {
            // In Ego-Perspektive: Mündungsposition aus dem Waffen-Attachment
            var attachment = EffectRenderer.GetAttachment( "muzzle" );
            muzzlePos = attachment?.Position ?? cameraPos;
        }
        else // Third-Person
        {
            // In Third-Person: Mündung aus dem sichtbaren Waffen-Model
            var muzzlePoint = GameObject.Components.GetInDescendantsOrSelf<MuzzlePoint>();
            if ( muzzlePoint != null )
            {
                muzzlePos = muzzlePoint.WorldPosition;
            }
            else
            {
                // Fallback auf die Position der Waffe + Offset in Blickrichtung
                muzzlePos = GameObject.Transform.World.Position + GameObject.Transform.World.Rotation.Forward * 20 + Vector3.Forward * 200;
            }
        }

        // 3. Schussrichtung vom Mündungspunkt zum Zielpunkt
        Vector3 shootDirection = (targetPoint - muzzlePos).Normal;

        // Korrektur für Third-Person: Versatz nach links hinzufügen
        // Korrektur für Third-Person: Versatz nach links und oben hinzufügen
        if ( Owner.CameraMode != 0 ) // Third-Person
        {
            
        }

        // 4. Jetzt erst den Spread hinzufügen
        shootDirection += Vector3.Random * Spread;

        // 5. Endpunkt berechnen
        var endPos = muzzlePos + shootDirection * 5000f;

        var trace = Scene.Trace.Ray( muzzlePos, endPos )
            .IgnoreGameObjectHierarchy( GameObject.Root )
            .WithoutTags( "player" )
            .UseHitboxes( true )
            .Run();

        // Setze endPos auf die Trefferposition, wenn etwas getroffen wird
        if ( trace.Hit )
        {
            endPos = trace.EndPosition;
        }

        if ( Trail != null )
        {
            var trailInstance = ResourceLibrary.Get<PrefabFile>( "particles/prefabs/aspects/firebullet_holy.prefab" ); // Verwende das neue Prefab
            if ( trailInstance != null )
            {
                var trailobject = GameObject.Clone( trailInstance );
                if ( trailobject != null )
                {
                    trailobject.WorldPosition = muzzlePos; // Setze die Startposition auf die Mündung

                    var trailobjectRenderer = trailobject.Components.Get<ParticleEffect>();
                    if ( trailobjectRenderer != null )
                    {
                        trailobjectRenderer.Yaw = Rotation.LookAt( shootDirection ).Yaw();
                        trailobjectRenderer.Pitch = Rotation.LookAt( shootDirection ).Pitch();
                    }

                    var speed = BulletSpeed * 400f; // Geschwindigkeit des Schusses basierend auf BulletSpeed
                    UpdateTrailObjectPosition( trailobject, shootDirection, speed, endPos, shooter );
                }
            }
        }
        if ( trace.Hit )
        {
            var damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();
            if ( damageable != null )
            {
                ApplyHolyAspectPassive( damageable );
            }
        }

        SendAttackMessage( muzzlePos, endPos, trace.Distance, trace );

        return;
    }
    private void ApplyHolyAspectPassive( IHealthComponent damageable )
    {
        if ( damageable is Npc npc )
        {
            Random random = new Random();
            if ( random.Next( 0, 100 ) < 20 ) // 20% Chance
            {
                var stunEffect = new HolyEffect( 3 ); // Dauer in Sekunden
                npc.ApplyStatusEffect( stunEffect );

                // Erzeuge das Licht und bewege es von oben nach unten
                CreateHolyLightEffect( npc );
            }
        }
    }

    private  void CreateHolyLightEffect( Npc npc )
    {
        var lightPrefab = ResourceLibrary.Get<PrefabFile>( "particles/prefabs/holy.prefab" );
        if ( lightPrefab != null )
        {
            var startPositionOffsets = new Vector3[]
            {
            new Vector3(500f, 0f, 500f),
            new Vector3(-500f, 0f, 500f),
            new Vector3(0f, 500f, 500f)
            };

            foreach ( var offset in startPositionOffsets )
            {
                var lightObject = GameObject.Clone( lightPrefab );
                if ( lightObject != null )
                {
                    lightObject.WorldPosition = npc.WorldPosition + offset; // Startposition um den NPC

                    var startTime = Time.Now;
                    var duration = 2.0f; // Dauer der Bewegung in Sekunden
                    var speed = 550f; // Geschwindigkeit des Lichts

                    _ = MoveLightToNpc( lightObject, npc, startTime, duration, speed );
                }
            }
        }
    }

    private async Task MoveLightToNpc( GameObject lightObject, Npc npc, float startTime, float duration, float speed )
    {
        if ( lightObject == null || npc == null )
        {
            return;
        }
        while ( Time.Now - startTime < duration )
        {
            var direction = (npc.WorldPosition - lightObject.WorldPosition).Normal;
            lightObject.WorldPosition += direction * speed * Time.Delta;

            // Überprüfen, ob das Licht den NPC erreicht hat
            if ( (lightObject.WorldPosition - npc.WorldPosition).Length < 1.0f )
            {
                // Füge dem NPC Schaden zu, basierend auf seinem aktuellen Gesundheitszustand
                float damagePercentage = 0.1f; // 10% des aktuellen Gesundheitszustands
                float damage = npc.Health * damagePercentage;
                npc.TakeDamage( DamageType.holy, damage, npc.WorldPosition, Vector3.Zero, Guid.Empty, Guid.Empty );

                // Zerstöre das Licht
                lightObject.Destroy();
                return;
            }

            await Task.Delay( 10 ); // Aktualisiere die Position alle 10 Millisekunden
        }

        // Zerstöre das Licht nach Ablauf der Dauer
        lightObject.Destroy();
    }

    private void FireBulletWithPoisonAspect( Player shooter )
    {
        
        Owner.ApplyRecoil( Recoil );
        EffectRenderer?.Set( "b_empty", AmmoInClip == 0 );
        EffectRenderer?.Set( "b_attack", true );
        EffectRenderer?.Set( "b_reload", false );
        NextAttackTime = 1f / FireRate;
        AmmoInClip--;

        var cameraPos = Owner.PlyCamera.WorldPosition;
        var cameraDir = Owner.PlyCamera.WorldRotation.Forward;

        // Zielstrahl vom Kamerazentrum durch das Fadenkreuz
        var targetRay = Scene.Trace.Ray( cameraPos, cameraPos + cameraDir * 5000f )
            .IgnoreGameObjectHierarchy( GameObject.Root )
            .WithoutTags( "player" )
            .UseHitboxes( true )
            .Run();

        Vector3 targetPoint = targetRay.Hit ? targetRay.EndPosition : cameraPos + cameraDir * 5000f;

        // 2. Mündungsposition abhängig vom Kameramodus bestimmen
        Vector3 muzzlePos;

        if ( Owner.CameraMode == 0 ) // First-Person
        {
            // In Ego-Perspektive: Mündungsposition aus dem Waffen-Attachment
            var attachment = EffectRenderer.GetAttachment( "muzzle" );
            muzzlePos = attachment?.Position ?? cameraPos;
        }
        else // Third-Person
        {
            // In Third-Person: Mündung aus dem sichtbaren Waffen-Model
            var muzzlePoint = GameObject.Components.GetInDescendantsOrSelf<MuzzlePoint>();
            if ( muzzlePoint != null )
            {
                muzzlePos = muzzlePoint.WorldPosition;
            }
            else
            {
                // Fallback auf die Position der Waffe + Offset in Blickrichtung
                muzzlePos = GameObject.Transform.World.Position + GameObject.Transform.World.Rotation.Forward * 20 + Vector3.Forward * 200;
            }
        }

        // 3. Schussrichtung vom Mündungspunkt zum Zielpunkt
        Vector3 shootDirection = (targetPoint - muzzlePos).Normal;

        // Korrektur für Third-Person: Versatz nach links hinzufügen
        // Korrektur für Third-Person: Versatz nach links und oben hinzufügen
        if ( Owner.CameraMode != 0 ) // Third-Person
        {

        }

        // 4. Jetzt erst den Spread hinzufügen
        shootDirection += Vector3.Random * Spread;

        // 5. Endpunkt berechnen
        var endPos = muzzlePos + shootDirection * 5000f;

        var trace = Scene.Trace.Ray( muzzlePos, endPos )
            .IgnoreGameObjectHierarchy( GameObject.Root )
            .WithoutTags( "player" )
            .UseHitboxes( true )
            .Run();

        // Setze endPos auf die Trefferposition, wenn etwas getroffen wird
        if ( trace.Hit )
        {
            endPos = trace.EndPosition;
        }

        if ( Trail != null )
        {
            var trailInstance = ResourceLibrary.Get<PrefabFile>( "particles/prefabs/aspects/firebullet_poison.prefab" ); // Verwende das neue Prefab
            if ( trailInstance != null )
            {
                var trailobject = GameObject.Clone( trailInstance );
                if ( trailobject != null )
                {
                    trailobject.WorldPosition = muzzlePos; // Setze die Startposition auf die Mündung

                    var trailobjectRenderer = trailobject.Components.Get<ParticleEffect>();
                    if ( trailobjectRenderer != null )
                    {
                        trailobjectRenderer.Yaw = Rotation.LookAt( shootDirection ).Yaw();
                        trailobjectRenderer.Pitch = Rotation.LookAt( shootDirection ).Pitch();
                    }

                    var speed = BulletSpeed * 400f; // Geschwindigkeit des Schusses basierend auf BulletSpeed
                    UpdateTrailObjectPosition( trailobject, shootDirection, speed, endPos, shooter );
                }
            }
        }


        SendAttackMessage( muzzlePos, endPos, trace.Distance, trace );

        return;
    }
    
    private async void MovePrefabToPlayer( GameObject prefabObject, Player shooter )
    {
        var startTime = Time.Now;
        var duration = 2.0f; // Dauer der Bewegung in Sekunden, anpassen nach Bedarf
        var speed = 500.0f; // Geschwindigkeit des Prefabs

        while ( Time.Now - startTime < duration )
        {
            var direction = (shooter.WorldPosition - prefabObject.WorldPosition).Normal;
            prefabObject.WorldPosition += direction * speed * Time.Delta;

            // Überprüfen, ob das Prefab den Spieler erreicht hat
            if ( (prefabObject.WorldPosition - shooter.WorldPosition).Length < 1.0f )
            {
                // Heile den Spieler um 25% seines maximalen Lebens
                shooter.Health = Math.Min( shooter.MaxHealth, shooter.Health + shooter.MaxHealth * 0.25f );

                // Zerstöre das Prefab
                prefabObject.Destroy();
                return;
            }

            await Task.Delay( 10 ); // Aktualisiere die Position alle 10 Millisekunden
        }

        // Zerstöre das Prefab nach Ablauf der Dauer
        prefabObject.Destroy();
    }
    private IEnumerable<Npc> FindNpcsInRadius( Vector3 position, float radius )
    {
        return Scene.GetAllComponents<Npc>().Where( npc => (npc.WorldPosition - position).Length < radius );
    }


}