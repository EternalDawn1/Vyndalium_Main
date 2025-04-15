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
    public PrefabFile HallwayLeft { get; set; }

    [Property]
    public PrefabFile HallwayRight { get; set; }

    [Property]
    public PrefabFile EndpointRoom { get; set; }

    [Property]
    public PrefabFile MiddleRoom { get; set; }

    [Property]
    public PrefabFile SidewaysRoom { get; set; }

    [Property]
    public bool GenerateSidewaysRooms { get; set; } = true;

    [Property]
    public int DungeonRooms { get; set; } = 5; // Anzahl der zufälligen Dungeon-Räume

    private List<GameObject> spawnedObjects = new List<GameObject>();

    // ...existing code...
    // ...existing code...
    [Property, Button( "Generate Random" )]
    public void GenerateRandom()
    {
        // Alles löschen
        foreach ( var obj in spawnedObjects )
            obj?.Destroy();
        spawnedObjects.Clear();

        if ( StartingRoom == null || Hallway == null || EndpointRoom == null || MiddleRoom == null )
        {
            Log.Warning( "Please assign all prefab files (StartingRoom, Hallway, EndpointRoom, MiddleRoom) before generating." );
            return;
        }

        Vector3 pos = Vector3.Zero;
        Rotation rot = Rotation.Identity;

        // Start-Raum
        SpawnRoom( StartingRoom, ref pos, Vector3.Zero );

        // Hallway nach dem Start
        SpawnRoom( Hallway, ref pos, new Vector3( 0, -385.52f, 0 ) );

        int randomParts = Game.Random.Int( 1, DungeonRooms );
        int consecutiveMiddleRooms = 0;

        for ( int i = 0; i < randomParts; i++ )
        {
         

            bool spawnMiddle = Game.Random.Int( 0, 5 ) == 0;

            if ( spawnMiddle && consecutiveMiddleRooms < 2 )
            {
                SpawnRoom( MiddleRoom, ref pos, new Vector3( 0, -385.52f, 0 ) );
                consecutiveMiddleRooms++;

                
            }
            else
            {
                SpawnRoom( Hallway, ref pos, new Vector3( 0, -385.52f, 0 ) );
                consecutiveMiddleRooms = 0;
            }
        }

        // Endraum
        SpawnRoom( EndpointRoom, ref pos, new Vector3( 0, -385.52f, 0 ) );
    }
    // ...existing code...
    // ...existing code...
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