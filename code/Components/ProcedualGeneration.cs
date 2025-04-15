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

    [Property]
    public PrefabFile MiddleRoom { get; set; } // Raum, der nach einem Hallway entsteht

    [Property]
    public PrefabFile SidewaysRoom { get; set; } // Seitliche Räume, die an den Middle Room angefügt werden

    [Property]
    public bool GenerateSidewaysRooms { get; set; } = true; // Option, ob Seitliche Räume generiert werden sollen

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

            // Aktualisiere die Position nach dem Startraum
            currentPosition += new Vector3( 0, -385.52f, 0 );
        }

        // Spawn hallways and middle rooms
        Random random = new Random();
        for ( int i = 0; i < HallwayLength; i++ )
        {
            // Zufällig entscheiden, ob ein Middle Room oder ein Hallway generiert wird
            bool spawnMiddleRoom = random.Next( 0, 2 ) == 0; // 50% Chance

            if ( spawnMiddleRoom && MiddleRoom != null )
            {
                var middleRoomPrefab = ResourceLibrary.Get<PrefabFile>( MiddleRoom.ResourcePath );
                if ( middleRoomPrefab != null )
                {
                    var middleRoomObject = SceneUtility.GetPrefabScene( middleRoomPrefab ).Clone();
                    middleRoomObject.WorldPosition = currentPosition;
                    middleRoomObject.WorldRotation = currentRotation;
                    middleRoomObject.NetworkSpawn();
                    spawnedObjects.Add( middleRoomObject );

                    // Optional: Generate sideways rooms
                    if ( GenerateSidewaysRooms && SidewaysRoom != null )
                    {
                        int sidewaysRoomCount = random.Next( 1, 4 ); // Zufällige Anzahl von Sideways Rooms (1 bis 3)
                        var sidewaysRoomPrefab = ResourceLibrary.Get<PrefabFile>( SidewaysRoom.ResourcePath );

                        for ( int j = 0; j < sidewaysRoomCount; j++ )
                        {
                            if ( sidewaysRoomPrefab != null )
                            {
                                var offset = 305.217f * (j + 1);
                                var leftRoomObject = SceneUtility.GetPrefabScene( sidewaysRoomPrefab ).Clone();
                                leftRoomObject.WorldPosition = currentPosition + new Vector3( -offset, 0, 0 ); // Links
                                leftRoomObject.WorldRotation = currentRotation;
                                leftRoomObject.NetworkSpawn();
                                spawnedObjects.Add( leftRoomObject );

                                var rightRoomObject = SceneUtility.GetPrefabScene( sidewaysRoomPrefab ).Clone();
                                rightRoomObject.WorldPosition = currentPosition + new Vector3( offset, 0, 0 ); // Rechts
                                rightRoomObject.WorldRotation = currentRotation;
                                rightRoomObject.NetworkSpawn();
                                spawnedObjects.Add( rightRoomObject );
                            }
                        }
                    }

                    // Verschiebe die Position entlang der Y-Achse nach dem Middle Room
                    currentPosition += new Vector3( 0, -385.52f, 0 );
                }
            }
            else
            {
                // Spawn a hallway
                var hallwayPrefab = ResourceLibrary.Get<PrefabFile>( Hallway.ResourcePath );
                if ( hallwayPrefab != null )
                {
                    var hallwayObject = SceneUtility.GetPrefabScene( hallwayPrefab ).Clone();
                    hallwayObject.WorldPosition = currentPosition;
                    hallwayObject.WorldRotation = currentRotation;
                    hallwayObject.NetworkSpawn();
                    spawnedObjects.Add( hallwayObject );

                    // Verschiebe die Position entlang der Y-Achse
                    currentPosition += new Vector3( 0, -385.52f, 0 );
                }
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