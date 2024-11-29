using System;
using System.Collections.Generic;
using System.Numerics;
using Sandbox;

namespace GeneralGame;

public class TutorialMarker : Component
{
    [Property] public new string Id { get; set; }
    [Property] public string Message { get; set; }
    [Property] public Vector3 Position { get; set; }
    [Property] public bool IsActive { get; set; }
    private SceneObject sceneObject;

    public TutorialMarker( string id, string message, Vector3 position )
    {
        Id = id;
        Message = message;
        Position = position;
        IsActive = false;
      
    }

    public TutorialMarker()
    {
    }

   
    public void UpdatePosition( Vector3 newPosition )
    {
        Position = newPosition;
        if ( sceneObject != null )
        {
            sceneObject.Position = newPosition;
        }
    }

    public void RemoveFromScene()
    {
        sceneObject?.Delete();
        sceneObject = null;
    }
}

public class TutorialManager : Component
{
    [Property] private List<TutorialMarker> markers = new List<TutorialMarker>();
    [Property] private int currentMarkerIndex = 0;

    public void AddMarker( TutorialMarker marker )
    {
        markers.Add( marker );
    }

    public void SetMarkerOrder( List<string> markerIds )
    {
        List<TutorialMarker> orderedMarkers = new List<TutorialMarker>();
        foreach ( var id in markerIds )
        {
            var marker = markers.Find( m => m.Id == id );
            if ( marker != null )
            {
                orderedMarkers.Add( marker );
            }
        }
        markers = orderedMarkers;
    }

    public TutorialMarker GetCurrentMarker()
    {
        if ( currentMarkerIndex >= 0 && currentMarkerIndex < markers.Count )
        {
            return markers[currentMarkerIndex];
        }
        return null;
    }

    public void MoveToNextMarker()
    {
        if ( currentMarkerIndex < markers.Count - 1 )
        {
            currentMarkerIndex++;
        }
    }
}