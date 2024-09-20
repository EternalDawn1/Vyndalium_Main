using System.Collections.Generic;
using System.Linq;
using Sandbox;

public class Voxel
{
    public Vector3 Position { get; set; }
    public Color Color { get; set; }

    public Voxel( Vector3 position, Color color )
    {
        Position = position;
        Color = color;
    }
}

public class Vox : GameObject
{
    public List<Voxel> Voxels { get; private set; } = new List<Voxel>();

    public void VoxelizeObject( GameObject obj, float voxelSize )
    {
        // Beispielhafte Voxelization-Logik
        BBox bounds = obj.GetBounds();
        for ( float x = bounds.Mins.x; x < bounds.Maxs.x; x += voxelSize )
        {
            for ( float y = bounds.Mins.y; y < bounds.Maxs.y; y += voxelSize )
            {
                for ( float z = bounds.Mins.z; z < bounds.Maxs.z; z += voxelSize )
                {
                    Vector3 position = new Vector3( x, y, z );
                    Color color = GetColorAtPosition( obj, position );
                    Voxel voxel = new Voxel( position, color );
                    Voxels.Add( voxel );
                }
            }
        }
    }

    private Color GetColorAtPosition( GameObject obj, Vector3 position )
    {
        // Beispielhafte Methode zur Bestimmung der Farbe an einer bestimmten Position
        // Dies könnte durch Raycasting oder andere Techniken implementiert werden
        return Color.White; // Platzhalter
    }

   
}
public class Bounds
{
    public Vector3 Min { get; set; }
    public Vector3 Max { get; set; }

    public Bounds( Vector3 min, Vector3 max )
    {
        Min = min;
        Max = max;
    }
}