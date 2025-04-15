using Sandbox;
using System.Collections.Generic;

namespace GeneralGame;

[CustomEditor( typeof( ProceduralRoomGeneration ) )]
public class ProceduralRoomGeneration : Component
{
    [Property]
    public PrefabFile StartingRoom { get; set; }

    [Property]
    public PrefabFile Hallway { get; set; }

    [Property]
    public PrefabFile EndpointRoom { get; set; }

    [Property]
    public PrefabFile MiddleRoom { get; set; }

    [Property]
    public List<PrefabFile> RoomPrefabs { get; set; } = new List<PrefabFile>();

    [Property, Button( "Generate Random" )]
    public void GenerateRandom()
    {
        if ( StartingRoom == null || Hallway == null || EndpointRoom == null )
        {
            Log.Warning( "Please assign all prefab files (StartingRoom, Hallway, EndpointRoom) before generating." );
            return;
        }

        Vector3 currentPosition = Vector3.Zero;

        // Spawn the starting room
        var startingRoomPrefab = ResourceLibrary.Get<PrefabFile>(StartingRoom.ResourcePath);
        if (startingRoomPrefab != null)
        {
            // Klonen des GameObjects aus dem Prefab
            var startingRoomObject = SceneUtility.GetPrefabScene(startingRoomPrefab).Clone();

            // Setze die Position des Raums
            startingRoomObject.WorldPosition = currentPosition;

            // Optional: Setze die Rotation des Raums
            startingRoomObject.WorldRotation = Rotation.Identity;

            // Optional: Füge das GameObject zur Szene hinzu
            startingRoomObject.NetworkSpawn();

            // Aktualisiere die Position für den nächsten Raum
            currentPosition += new Vector3(500, 0, 0); // Beispiel: Verschiebe den nächsten Raum um 500 Einheiten
        }

        var hallwayPrefab = ResourceLibrary.Get<PrefabFile>( Hallway.ResourcePath );
        if ( hallwayPrefab != null )
        {
            // Klonen des GameObjects aus dem Prefab
            var hallwayObject = SceneUtility.GetPrefabScene( hallwayPrefab ).Clone();

            // Setze die Position des Flurs direkt vor den StartingRoom
            hallwayObject.WorldPosition = currentPosition;

            // Optional: Setze die Rotation des Flurs
            hallwayObject.WorldRotation = Rotation.Identity;

            // Optional: Füge das GameObject zur Szene hinzu
            hallwayObject.NetworkSpawn();

            // Aktualisiere die Position für den nächsten Raum
            currentPosition += new Vector3( 500, 0, 0 ); // Beispiel: Verschiebe den nächsten Raum um 500 Einheiten
        }

        var endpointRoomPrefab = ResourceLibrary.Get<PrefabFile>( EndpointRoom.ResourcePath );
        if ( endpointRoomPrefab != null )
        {
            // Klonen des GameObjects aus dem Prefab
            var endpointRoomObject = SceneUtility.GetPrefabScene( endpointRoomPrefab ).Clone();

            // Setze die Position des Endraums
            endpointRoomObject.WorldPosition = currentPosition;

            // Optional: Setze die Rotation des Endraums
            endpointRoomObject.WorldRotation = Rotation.Identity;

            // Optional: Füge das GameObject zur Szene hinzu
            endpointRoomObject.NetworkSpawn();

            currentPosition += new Vector3( 500, 0, 0 );
        }
        var middleRoomPrefab = ResourceLibrary.Get<PrefabFile>( MiddleRoom.ResourcePath );
        if ( middleRoomPrefab != null )
        {
            
            // Klonen des GameObjects aus dem Prefab
            var middleRoomObject = SceneUtility.GetPrefabScene( middleRoomPrefab ).Clone();

            // Setze die Position des Mittelraums
            middleRoomObject.WorldPosition = currentPosition;

            // Optional: Setze die Rotation des Mittelraums
            middleRoomObject.WorldRotation = Rotation.Identity;

            // Optional: Füge das GameObject zur Szene hinzu
            middleRoomObject.NetworkSpawn();

            currentPosition += new Vector3( 500, 0, 0 );
        }

        
    }
}