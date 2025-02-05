namespace GeneralGame;
using System.Collections.Generic;

public sealed class Rainbow : Component
{
	[Property] public MeshComponent Mesh { get; set; }
	[Property] private List<Color> colors;
	[Property] private int currentColorIndex;
	[Property] private float changeInterval = 1.0f; // Zeit in Sekunden zwischen Farbwechseln
	[Property] private float timer;
	[Property] private float blendFactor;

	protected override void OnStart()
	{
		colors = new List<Color> { Color.Red, Color.Yellow, Color.Green, Color.Cyan, Color.Blue, Color.Magenta };
		currentColorIndex = 0;
		timer = 0f;
		blendFactor = 0f;
	}

	protected override void OnUpdate()
	{
		timer += Time.Delta;
		blendFactor = timer / changeInterval;

		if ( timer >= changeInterval )
		{
			timer = 0f;
			currentColorIndex = (currentColorIndex + 1) % colors.Count;
		}

		int nextColorIndex = (currentColorIndex + 1) % colors.Count;
		Mesh.Color = Color.Lerp( colors[currentColorIndex], colors[nextColorIndex], blendFactor );
	}
}