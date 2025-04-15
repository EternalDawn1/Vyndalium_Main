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
    public PrefabFile LeftRoomOpen { get; set; } // Offener Raum links

    [Property]
    public PrefabFile RightRoomOpen { get; set; } // Offener Raum rechts

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
        for ( int i = 0; i < HallwayLength; i++ )
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

            // Spawn a middle room after the hallway
            if ( MiddleRoom != null )
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
                        var sidewaysRoomPrefab = ResourceLibrary.Get<PrefabFile>( SidewaysRoom.ResourcePath );

                        // Spawn a room to the left
                        if ( sidewaysRoomPrefab != null )
                        {
                            // Spawn a room to the left
                            var leftRoomObject = SceneUtility.GetPrefabScene( sidewaysRoomPrefab ).Clone();
                            leftRoomObject.WorldPosition = currentPosition + new Vector3( -305.217f, 0, 0 ); // Links
                            leftRoomObject.WorldRotation = currentRotation;
                            leftRoomObject.NetworkSpawn();
                            spawnedObjects.Add( leftRoomObject );

                            // Spawn a LeftRoomOpen for the left room
                            if ( LeftRoomOpen != null )
                            {
                                var leftRoomOpenPrefab = ResourceLibrary.Get<PrefabFile>( LeftRoomOpen.ResourcePath );
                                if ( leftRoomOpenPrefab != null )
                                {
                                    var leftRoomOpenObject = SceneUtility.GetPrefabScene( leftRoomOpenPrefab ).Clone();
                                    leftRoomOpenObject.WorldPosition = leftRoomObject.WorldPosition + new Vector3( -305.217f, 0, 0 ); // Korrekte Position für LeftRoomOpen
                                    leftRoomOpenObject.WorldRotation = currentRotation;
                                    leftRoomOpenObject.NetworkSpawn();
                                    spawnedObjects.Add( leftRoomOpenObject );
                                }
                            }
                        }

                        // Spawn a room to the right
                        if ( sidewaysRoomPrefab != null )
                        {
                            var rightRoomObject = SceneUtility.GetPrefabScene( sidewaysRoomPrefab ).Clone();
                            rightRoomObject.WorldPosition = currentPosition + new Vector3( 305.217f, 0, 0 ); // Rechts
                            rightRoomObject.WorldRotation = currentRotation;
                            rightRoomObject.NetworkSpawn();
                            spawnedObjects.Add( rightRoomObject );

                            // Spawn a RightRoomOpen for the right room
                            if ( RightRoomOpen != null )
                            {
                                var rightRoomOpenPrefab = ResourceLibrary.Get<PrefabFile>( RightRoomOpen.ResourcePath );
                                if ( rightRoomOpenPrefab != null )
                                {
                                    var rightRoomOpenObject = SceneUtility.GetPrefabScene( rightRoomOpenPrefab ).Clone();
                                    rightRoomOpenObject.WorldPosition = rightRoomObject.WorldPosition + new Vector3( 305.217f, 0, 0 ); // Korrekte Position für RightRoomOpen
                                    rightRoomOpenObject.WorldRotation = currentRotation;
                                    rightRoomOpenObject.NetworkSpawn();
                                    spawnedObjects.Add( rightRoomOpenObject );
                                }
                            }
                        }
                    }

                    // Verschiebe die Position entlang der Y-Achse nach dem Middle Room
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