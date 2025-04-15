using Sandbox;
using System.Collections.Generic;

namespace GeneralGame;

[CustomEditor( typeof( ProceduralRoomGeneration ) )]
public class ProceduralRoomGeneration : Component
{

    [Property]
    public int RoomCount { get; set; } = 10;
    [Property]
    public PrefabFile StartingRoom { get; set; }
    [Property]
    public PrefabFile Hallway { get; set; }

    [Property]
    public PrefabFile EndpointRoom { get; set; }

    [Property]
    public PrefabFile MiddleRoom { get; set; }

    [Property]
    public PrefabFile SideWayRoom { get; set; }






    private List<GameObject> spawnedObjects = new List<GameObject>();

    // ...existing code...
    // ...existing code...
    [Property, Button( "Generate Random Advanced" )]
    public void GenerateRandomAdvanced()
    {
        // Alles löschen
        foreach ( var obj in spawnedObjects )
            obj?.Destroy();
        spawnedObjects.Clear();

        Vector3 startPos = Vector3.Zero;
        var startObj = SpawnRoom( StartingRoom, ref startPos, Vector3.Zero );
        var startRoom = startObj.Components.Get<Room>();

        // Liste aller offenen Punkte (Raum, Richtung, Position)
        var openPoints = new List<(Room room, Room.RoomOpenings opening, Vector3 pos)>();

        foreach ( Room.RoomOpenings opening in Enum.GetValues( typeof( Room.RoomOpenings ) ) )
        {
            if ( opening == Room.RoomOpenings.None ) continue;
            if ( (startRoom.Openings & opening) != 0 )
                openPoints.Add( (startRoom, opening, startRoom.Position) );
        }

        // Beispiel: 10 Räume generieren
        for ( int i = 0; i < RoomCount && openPoints.Count > 0; i++ )
        {
            var (parentRoom, parentOpening, parentPos) = openPoints[0];
            openPoints.RemoveAt( 0 );

            // Zufälliges Prefab wählen
            PrefabFile prefab = GetRandomRoomPrefab();
            Vector3 offset = Room.OpeningOffsets[parentOpening];
            Vector3 newPos = parentPos + offset;

            var newObj = SpawnRoom( prefab, ref newPos, Vector3.Zero );
            var newRoom = newObj.Components.Get<Room>();

            // Gegenrichtung berechnen
            Room.RoomOpenings opposite = GetOpposite( parentOpening );

            // Offene Ausgänge im neuen Raum sammeln (außer dem, der gerade verbunden wurde)
            foreach ( Room.RoomOpenings opening in Enum.GetValues( typeof( Room.RoomOpenings ) ) )
            {
                if ( opening == Room.RoomOpenings.None || opening == opposite ) continue;
                if ( (newRoom.Openings & opening) != 0 )
                    openPoints.Add( (newRoom, opening, newPos) );
            }
        }
    }

    // Hilfsfunktion für Gegenrichtung
    private Room.RoomOpenings GetOpposite( Room.RoomOpenings opening )
    {
        return opening switch
        {
            Room.RoomOpenings.North => Room.RoomOpenings.South,
            Room.RoomOpenings.South => Room.RoomOpenings.North,
            Room.RoomOpenings.East => Room.RoomOpenings.West,
            Room.RoomOpenings.West => Room.RoomOpenings.East,
            _ => Room.RoomOpenings.None
        };
    }

    // Beispiel für zufällige Prefab-Auswahl
    private PrefabFile GetRandomRoomPrefab()
    {
        var prefabs = new List<PrefabFile> { MiddleRoom, SideWayRoom, Hallway };
        return prefabs[Game.Random.Int( 0, prefabs.Count - 1 )];
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

            // Hier: Room-Komponente holen und Position setzen!
            var roomComp = roomObject.Components.Get<Room>();
            if ( roomComp != null )
                roomComp.Position = roomObject.WorldPosition;

            return roomObject;
        }
        return null;
    }
}
public class Room : Component
{
    [Flags]
    public enum RoomOpenings
    {
        None = 0,
        North = 1 << 0,
        South = 1 << 1,
        East = 1 << 2,
        West = 1 << 3
    }
    public static readonly Dictionary<Room.RoomOpenings, Vector3> OpeningOffsets = new()
{
    { Room.RoomOpenings.North, new Vector3(0, 385.52f, 0) },
    { Room.RoomOpenings.South, new Vector3(0, -385.52f, 0) },
    { Room.RoomOpenings.East,  new Vector3(305.3f, 0, 0) },
    { Room.RoomOpenings.West,  new Vector3(-305.3f, 0, 0) }
};

    [Property] public GameObject RoomObject { get; set; }
    [Property] public Vector3 Position { get; set; }
    [Property] public RoomOpenings Openings { get; set; } = RoomOpenings.None;

    // Beispiel: "North", "South", "East", "West" als Schlüssel
}