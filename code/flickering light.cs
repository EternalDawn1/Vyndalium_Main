using GeneralGame;
using System;

namespace GeneralGame;
public class FlickeringLight : Component
{
    [Property] private float flickerSpeed { get; set; }
    [Property] private float flickerIntensity { get; set; }
    private float timer;
    private Color originalColor;
    [Property] private PointLight pointLight;
    private FlickeringLight flickeringLight;

    protected override void OnStart()
    {
        pointLight = GameObject.Components.Get<PointLight>();
        flickeringLight = pointLight.GameObject.Components.Get<FlickeringLight>();

        if ( pointLight == null || flickeringLight == null )
        {

            return;
        }

        flickeringLight.flickerSpeed = 1f; // Setzen Sie die Flacker-Geschwindigkeit auf 1
        flickeringLight.flickerIntensity = 1f; // Setzen Sie die Flacker-Intensität auf 1
        originalColor = pointLight.LightColor;
    }

    protected override void OnUpdate()
    {
        if ( pointLight == null || flickeringLight == null )
        {
            return;
        }

        timer += Time.Delta;
        float brightness = MathF.Abs( MathF.Sin( timer * flickeringLight.flickerSpeed ) ) * flickeringLight.flickerIntensity;
        pointLight.LightColor = originalColor * brightness;
    }


}