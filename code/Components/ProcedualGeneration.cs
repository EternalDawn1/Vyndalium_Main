using Sandbox;
using System.Collections.Generic;

namespace GeneralGame;
[CustomEditor( typeof( ProceduralGeneration ) )]
public class ProceduralGeneration : Component
{
    [Property] public MeshComponent prefabs;
    [Property] public int numberOfObjects = 10;
    [Property] public float radius = 5f;

    [Property, Button( "Generate Row" )]
    public void GenerateRowButton()
    {
        GenerateRow();
    }

    private List<MeshComponent> spawnedObjects = new();

    public void GenerateRow()
    {
        ClearObjects();

        for ( int i = 0; i < numberOfObjects; i++ )
        {
            var position = new Vector3( i * radius, 0, 0 ); // Alle 5 in einer Reihe entlang der X-Achse
            var newGameObject = new GameObject(); // Neues GameObject erstellen
            var newMeshComponent = newGameObject.AddComponent<MeshComponent>(); // MeshComponent hinzufügen
            newMeshComponent.LocalPosition = position;
            newMeshComponent.Mesh = prefabs.Mesh; // Übernimmt das Mesh des Prefabs
            spawnedObjects.Add( newMeshComponent );
        }
    }

    private void ClearObjects()
    {
        foreach ( var obj in spawnedObjects )
        {
            obj.GameObject.Destroy();
        }
        spawnedObjects.Clear();
    }
}