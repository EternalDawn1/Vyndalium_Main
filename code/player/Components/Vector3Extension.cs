using System;
using System.Collections.Generic;
namespace GeneralGame;
using GeneralGame;
public static class Vector3Extensions
{
    public static float AngleBetween( Vector3 vector1, Vector3 vector2 )
    {
        float dotProduct = Vector3.Dot( vector1, vector2 );
        float magnitudeProduct = vector1.Length * vector2.Length;
        float angle = (float)Math.Acos( dotProduct / magnitudeProduct );
        return angle * (180.0f / (float)Math.PI); // Winkel in Grad umrechnen
    }
}
public static class SceneObjectExtensions
{
    public static void SetVisibility( this SceneObject obj, bool isVisible )
    {
        Log.Info( $"Setting visibility of  to {isVisible}" );
    }
}