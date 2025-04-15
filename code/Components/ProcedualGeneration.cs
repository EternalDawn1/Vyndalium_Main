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
    public PrefabFile LeftRoomOpen { get; set; }

    [Property]
    public PrefabFile RightRoomOpen { get; set; }

    [Property]
    public PrefabFile SidewaysRoom { get; set; }

    [Property]
    public bool GenerateSidewaysRooms { get; set; } = true;

    [Property]
    public int DungeonRooms { get; set; } = 5; // Anzahl der zufälligen Dungeon-Räume

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

        // Spawn the starting room
        SpawnRoom( StartingRoom, ref currentPosition, Vector3.Zero );

        // Generiere Dungeon-Räume
        for ( int i = 0; i < DungeonRooms; i++ )
        {
            // Zufällige Richtung (links, rechts, geradeaus)
            int direction = Game.Random.Int( 0, 2 );
            Vector3 offset = direction switch
            {
                0 => new Vector3( -305.217f, 0, 0 ), // Links
                1 => new Vector3( 305.217f, 0, 0 ),  // Rechts
                _ => new Vector3( 0, -385.52f, 0 )   // Geradeaus
            };

            // Zufälliger Raumtyp
            PrefabFile roomPrefab = Game.Random.FromList( new List<PrefabFile> { MiddleRoom, SidewaysRoom } );
            if ( roomPrefab != null )
            {
                var roomObject = SpawnRoom( roomPrefab, ref currentPosition, offset );

                // Optional: Spiegeln
                if ( Game.Random.Int( 0, 1 ) == 1 )
                {
                    roomObject.WorldRotation *= Rotation.FromYaw( 180 );
                }

                // Spawn Verbindungen (Hallways)
                if ( Hallway != null )
                {
                    SpawnRoom( Hallway, ref currentPosition, new Vector3( 0, -385.52f, 0 ) );
                }

                // Spawn LeftRoomOpen oder RightRoomOpen
                if ( direction == 0 && LeftRoomOpen != null )
                {
                    SpawnRoom( LeftRoomOpen, ref currentPosition, new Vector3( -305.217f, 0, 0 ) );
                }
                else if ( direction == 1 && RightRoomOpen != null )
                {
                    SpawnRoom( RightRoomOpen, ref currentPosition, new Vector3( 305.217f, 0, 0 ) );
                }
            }
        }

        // Spawn the endpoint room
        SpawnRoom( EndpointRoom, ref currentPosition, Vector3.Zero );
    }
    private GameObject SpawnRoom( PrefabFile prefab, ref Vector3 position, Vector3 offset )
    {
        var prefabObject = ResourceLibrary.Get<PrefabFile>( prefab.ResourcePath );
        if ( prefabObject != null )
        {
            var roomObject = SceneUtility.GetPrefabScene( prefabObject ).Clone();
            roomObject.WorldPosition = position + offset;
            roomObject.WorldRotation = Rotation.Identity;
            roomObject.NetworkSpawn();
            spawnedObjects.Add( roomObject );
            position += offset; // Aktualisiere die Position
            return roomObject;
        }
        return null;
    }
}