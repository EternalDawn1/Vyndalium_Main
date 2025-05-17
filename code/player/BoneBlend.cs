using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using Editor;

namespace GeneralGame;

/// <summary>
/// Mischt prozedurale Knochensteuerung mit normalen Animationen
/// </summary>
[Title( "Knochen-Blend-Controller" ), Category( "Animation" ), Icon( "swap_horiz" )]
public class BoneBlendController : Component
{
    [Property, Category( "Komponenten" )]
    public SkinnedModelRenderer ModelRenderer { get; set; }

    /// <summary>
    /// Konfigurationen für die Knochen, die gemischt werden sollen
    /// </summary>
    [Property, Category( "Knochen" ), InlineEditor, WideMode]
    public List<BoneBlendConfig> BoneConfigs { get; set; } = new();

    // Speichert die ursprünglichen Rotationen der Knochen
    private Dictionary<string, Rotation> originalRotations = new();

    // Speichert die aktuellen Offset-Rotationen
    private Dictionary<string, Rotation> currentOffsets = new();

    // Cache der Bone-GameObjects
    private Dictionary<string, GameObject> boneObjects = new();

    // Verbindung zum BoneAnimationController für Koordination
    private BoneAnimationController animController;

    protected override void OnStart()
    {
        if ( ModelRenderer == null )
        {
            ModelRenderer = GameObject.Components.Get<SkinnedModelRenderer>();
        }

        // Verbindung zum AnimationController herstellen für Koordination
        animController = GameObject.Components.Get<BoneAnimationController>();

        // Bone-Cache aktualisieren
        FindAllBoneObjects();

        // Ursprüngliche Rotationen speichern
        foreach ( var config in BoneConfigs )
        {
            if ( !string.IsNullOrEmpty( config.BoneName ) )
            {
                var boneObject = GetBoneObject( config.BoneName );
                if ( boneObject != null )
                {
                    originalRotations[config.BoneName] = boneObject.LocalRotation;

                    // Knochen als prozedural markieren
                    if ( config.EnableBlending )
                    {
                        MakeBoneProcedural( config.BoneName );
                    }
                }
            }
        }
    }

    protected override void OnUpdate()
    {
        // Für jeden konfigurierten Knochen das Blending anwenden
        foreach ( var config in BoneConfigs )
        {
            if ( !config.EnableBlending ) continue;

            var boneObject = GetBoneObject( config.BoneName );
            if ( boneObject == null ) continue;

            // Prüfen, ob der Knochen aktiv vom AnimationController bearbeitet wird
            bool isAnimControlled = false;
            if ( animController != null && animController.ActiveAnimationNames != null )
            {
                // Prüfe, ob eine Animation diesen Knochen verwendet
                foreach ( var animName in animController.ActiveAnimationNames )
                {
                    var anim = animController.Animations.Find( a => a.Name == animName );
                    if ( anim != null && anim.BoneName.Equals( config.BoneName, StringComparison.OrdinalIgnoreCase ) )
                    {
                        isAnimControlled = true;
                        break;
                    }
                }
            }

            // Wenn der Knochen von einer Animation gesteuert wird und wir keine Überschreibung wollen
            if ( isAnimControlled && !config.OverrideActiveAnimations )
            {
                continue;
            }

            // Rotationsoffset anwenden
            Angles offset = config.RotationOffset;
            var offsetRotation = Rotation.From( offset );

            // Wenn wir eine Animation haben, mischen wir dazwischen
            if ( isAnimControlled && config.BlendWithAnimations )
            {
                // Hier könnten wir die aktuelle Animation-Pose mit dem Offset mischen
                // Vereinfacht: Interpolation zwischen aktueller Rotation und gewünschtem Offset
                var currentRot = boneObject.LocalRotation;
                var targetRot = originalRotations[config.BoneName] * offsetRotation;
                boneObject.LocalRotation = Rotation.Slerp( currentRot, targetRot, config.BlendAmount * Time.Delta * 10 );
            }
            else
            {
                // Direkt anwenden
                boneObject.LocalRotation = originalRotations[config.BoneName] * offsetRotation;
            }

            // Offset für diesen Frame speichern
            currentOffsets[config.BoneName] = offsetRotation;
        }
    }

    /// <summary>
    /// Setzt einen bestimmten Knochen auf einen neuen Rotationsoffset
    /// </summary>
    public void SetBoneRotation( string boneName, Angles rotationOffset, float blendTime = 0.2f )
    {
        var config = BoneConfigs.FirstOrDefault( c => c.BoneName.Equals( boneName, StringComparison.OrdinalIgnoreCase ) );
        if ( config != null )
        {
            config.RotationOffset = rotationOffset;
            config.EnableBlending = true;
        }
        else
        {
            // Neue Konfiguration erstellen und hinzufügen
            BoneConfigs.Add( new BoneBlendConfig
            {
                BoneName = boneName,
                RotationOffset = rotationOffset,
                EnableBlending = true,
                BlendAmount = 1.0f
            } );

            // Ursprüngliche Rotation speichern falls noch nicht vorhanden
            var boneObject = GetBoneObject( boneName );
            if ( boneObject != null && !originalRotations.ContainsKey( boneName ) )
            {
                originalRotations[boneName] = boneObject.LocalRotation;
                MakeBoneProcedural( boneName );
            }
        }
    }

    /// <summary>
    /// Markiert einen Knochen als prozedural
    /// </summary>
    private void MakeBoneProcedural( string boneName )
    {
        var boneObject = GetBoneObject( boneName );
        if ( boneObject == null ) return;

        boneObject.Flags |= GameObjectFlags.ProceduralBone;

        // Original-Rotation speichern falls noch nicht vorhanden
        if ( !originalRotations.ContainsKey( boneName ) )
        {
            originalRotations[boneName] = boneObject.LocalRotation;
        }
    }

    /// <summary>
    /// Entfernt die prozedurale Steuerung eines Knochens
    /// </summary>
    public void RemoveProceduralControl( string boneName )
    {
        var config = BoneConfigs.FirstOrDefault( c => c.BoneName.Equals( boneName, StringComparison.OrdinalIgnoreCase ) );
        if ( config != null )
        {
            config.EnableBlending = false;
        }

        var boneObject = GetBoneObject( boneName );
        if ( boneObject == null ) return;

        // Prozedural-Flag entfernen
        boneObject.Flags &= ~GameObjectFlags.ProceduralBone;

        // Knochen zur Original-Rotation zurücksetzen
        if ( originalRotations.TryGetValue( boneName, out var originalRotation ) )
        {
            boneObject.LocalRotation = originalRotation;
        }
    }

    #region Bone Finding Utilities

    /// <summary>
    /// Aktualisiert den Cache aller Knochen-Objekte
    /// </summary>
    public void RefreshBoneCache()
    {
        FindAllBoneObjects();
    }

    /// <summary>
    /// Findet alle Knochen-GameObjects und speichert sie im Cache
    /// </summary>
    private void FindAllBoneObjects()
    {
        boneObjects.Clear();

        if ( ModelRenderer == null )
        {
            ModelRenderer = GameObject.Components.Get<SkinnedModelRenderer>();
            if ( ModelRenderer == null ) return;
        }

        if ( ModelRenderer.Model != null )
        {
            // Rekursiv alle GameObjects durchsuchen
            FindBonesRecursive( GameObject );

            // Zusätzlich den Transform-Baum durchsuchen
            FindBonesInTransformHierarchy( ModelRenderer.GameObject );
        }
    }

    /// <summary>
    /// Sucht rekursiv nach allen Bone-GameObjects
    /// </summary>
    private void FindBonesRecursive( GameObject obj )
    {
        if ( obj == null ) return;

        if ( !boneObjects.ContainsKey( obj.Name ) )
        {
            boneObjects[obj.Name] = obj;
        }

        if ( obj.Children != null )
        {
            foreach ( var child in obj.Children )
            {
                FindBonesRecursive( child );
            }
        }
    }

    /// <summary>
    /// Durchsucht die Transform-Hierarchie nach Bones
    /// </summary>
    private void FindBonesInTransformHierarchy( GameObject obj )
    {
        if ( obj == null ) return;

        string name = obj.Name;
        if ( IsPotentialBoneName( name ) && !boneObjects.ContainsKey( name ) )
        {
            boneObjects[name] = obj;
        }

        foreach ( var child in obj.Children )
        {
            FindBonesInTransformHierarchy( child );
        }
    }

    /// <summary>
    /// Prüft, ob ein Name ein potenzieller Knochenname ist
    /// </summary>
    private bool IsPotentialBoneName( string name )
    {
        // Typische Knochennamen
        string[] boneKeywords = new[] {
            "bone", "clavicle", "spine", "neck", "head", "arm", "hand", "finger", "leg", "foot", "toe",
            "pelvis", "hip", "thigh", "shin", "shoulder", "elbow", "wrist", "knee", "ankle",
            "thumb", "index", "middle", "ring", "pinky", "jaw", "eyeball", "brow"
        };

        name = name.ToLower();

        foreach ( var keyword in boneKeywords )
        {
            if ( name.Contains( keyword ) )
                return true;
        }

        if ( name.StartsWith( "bip" ) || name.StartsWith( "bone" ) || name.StartsWith( "b_" ) )
            return true;
    
        return false;
    }

    /// <summary>
    /// Gibt das GameObject für einen Knochen zurück
    /// </summary>
    public GameObject GetBoneObject( string boneName )
    {
        if ( string.IsNullOrEmpty( boneName ) ) return null;

        string normalizedName = boneName.ToLower();

        // Wenn nicht im Cache, Cache aktualisieren
        if ( boneObjects.Count == 0 )
        {
            FindAllBoneObjects();
        }

        // Case-insensitive Suche
        foreach ( var kvp in boneObjects )
        {
            if ( string.Equals( kvp.Key, normalizedName, StringComparison.OrdinalIgnoreCase ) )
            {
                return kvp.Value;
            }
        }

        return null;
    }

    #endregion
}

/// <summary>
/// Konfiguration für einen Knochen im Blend-Controller
/// </summary>
[Serializable]
public class BoneBlendConfig
{
    [Property] public string BoneName { get; set; } = "clavicle_R";
    [Property] public Angles RotationOffset { get; set; } = new Angles( 0, 0, 0 );
    [Property] public bool EnableBlending { get; set; } = true;
    [Property] public bool BlendWithAnimations { get; set; } = true;
    [Property] public bool OverrideActiveAnimations { get; set; } = false;
    [Property, Range( 0f, 1f )] public float BlendAmount { get; set; } = 1.0f;
}