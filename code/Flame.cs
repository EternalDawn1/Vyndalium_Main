using Sandbox.Utility;
using System;

public sealed class Flame : Component
{
    [Property] public Light TargetLight { get; set; }
    [Property] public Color BaseLightColor { get; set; }
    [Property, Range( 0, 1 )] public float FlickerIntensity { get; set; } = 0.4f;
    [Property, Range( 0, 100 )] public float FlickerFrequency { get; set; } = 1f;
    [Property] public Vector3 JitterAmount { get; set; } = new Vector3( 1f );
    [Property, Range( 0, 100 )] public float JitterFrequency { get; set; } = 1f;

    [Property] public bool IsLit { get; set; }

    private float _targetBrightness;
    private float _seed;

    protected override void OnStart()
    {
        _seed = Game.Random.Float( 0, 1_000_000 );
    }

    protected override void OnUpdate()
    {
        if ( !TargetLight.IsValid() )
            return;

        TargetLight.LightColor = GetLightColor();
        TargetLight.LocalPosition = GetLightOffset();
    }

    private Color GetLightColor()
    {
        _targetBrightness = IsLit ? 1f : 0f;
        if ( IsLit && FlickerFrequency != 0f )
        {
            var noise = Noise.Perlin( Time.Now * FlickerFrequency + _seed );
            noise *= FlickerIntensity;
            _targetBrightness = 1f - noise;
        }
        var targetColor = BaseLightColor.ToHsv().WithValue( _targetBrightness );
        var currentColor = TargetLight.LightColor.ToHsv();
        var currentBrightness = currentColor.Value.ExpDecayTo( _targetBrightness, 8f );
        return targetColor.WithValue( currentBrightness );
    }

    private Vector3 GetLightOffset()
    {
        if ( JitterFrequency == 0f )
            return JitterAmount;

        var noiseX = Noise.Perlin( Time.Now * JitterFrequency + _seed * 2 );
        var noiseY = Noise.Perlin( Time.Now * JitterFrequency + _seed * 5 );
        var noiseZ = Noise.Perlin( Time.Now * JitterFrequency + _seed );
        return JitterAmount * new Vector3( noiseX, noiseY, noiseZ );
    }

}


public static class FloatExtensions
{
    /// <summary>
    /// Returns the result of a framerate-independent "lerp smoothing" to <paramref name="target"/>
    /// using a decay constant. For best results, choose a value for <paramref name="decay"/> that is
    /// between 1 and 25.
    /// An explanation of this method can be found here: https://youtu.be/LSNQuFEDOyQ
    /// </summary>
    public static float ExpDecayTo( this float from, float target, float decay )
    {
        return target + (from - target) * MathF.Exp( -decay * Time.Delta );
    }
}