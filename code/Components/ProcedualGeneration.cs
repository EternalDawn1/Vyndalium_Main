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
    public int HallwayLength { get; set; } = 1; // Anzahl der Flure



   private List<GameObject> spawnedObjects = new List<GameObject>();

    [Property, Button( "Generate Random" )]
    public void GenerateRandom()
    {
        // Lösche alle zuvor erstellten Objekte
        foreach ( var obj in spawnedObjects )
        {
            obj?.Destroy();
        }
        spawnedObjects.Clear();

        if ( StartingRoom == null || Hallway == null || EndpointRoom == null )
        {
            Log.Warning( "Please assign all prefab files (StartingRoom, Hallway, EndpointRoom) before generating." );
            return;
        }

        Vector3 currentPosition = Vector3.Zero;
        Rotation currentRotation = Rotation.Identity;

        // Spawn the starting room
        var startingRoomPrefab = ResourceLibrary.Get<PrefabFile>( StartingRoom.ResourcePath );
        if ( startingRoomPrefab != null )
        {
            var startingRoomObject = SceneUtility.GetPrefabScene( startingRoomPrefab ).Clone();
            startingRoomObject.WorldPosition = currentPosition;
            startingRoomObject.WorldRotation = currentRotation;
            startingRoomObject.NetworkSpawn();
            spawnedObjects.Add( startingRoomObject );
        }

        // Spawn hallways
        for ( int i = 0; i < HallwayLength; i++ )
        {
            var hallwayPrefab = ResourceLibrary.Get<PrefabFile>( Hallway.ResourcePath );
            if ( hallwayPrefab != null )
            {
                var hallwayObject = SceneUtility.GetPrefabScene( hallwayPrefab ).Clone();
                hallwayObject.WorldPosition = currentPosition;
                hallwayObject.WorldRotation = currentRotation;
                hallwayObject.NetworkSpawn();
                spawnedObjects.Add( hallwayObject );

                // Verschiebe die Position entlang der Y-Achse (z. B. -385.52)
                currentPosition += new Vector3( 0, -385.52f, 0 );
            }
        }

        // Spawn the endpoint room
        var endpointRoomPrefab = ResourceLibrary.Get<PrefabFile>( EndpointRoom.ResourcePath );
        if ( endpointRoomPrefab != null )
        {
            var endpointRoomObject = SceneUtility.GetPrefabScene( endpointRoomPrefab ).Clone();
            endpointRoomObject.WorldPosition = currentPosition;
            endpointRoomObject.WorldRotation = currentRotation;
            endpointRoomObject.NetworkSpawn();
            spawnedObjects.Add( endpointRoomObject );
        }
    }
}