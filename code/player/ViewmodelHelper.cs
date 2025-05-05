using Sandbox;
using System.Linq;
using System;

namespace GeneralGame;

/// <summary>
/// Hilfsmethoden für Viewmodel-Animationen
/// </summary>
/// 
/// [Title("Viewmodel-Knochen-Controller"), Category("Animation"), Icon("motion_photos_on")]
public sealed class ViewmodelBoneAnimationController : BoneAnimationController
{
    [Property, Category( "Debug" )]
    public bool DebugMode { get; set; } = false;

    protected override void OnStart()
    {
        // Suche den ModelRenderer im viewmodel, falls nicht explizit gesetzt
        if ( ModelRenderer == null )
        {
            // Zuerst direkt im eigenen GameObject suchen
            ModelRenderer = GameObject.Components.Get<SkinnedModelRenderer>();

            // Falls nicht gefunden, in Kindern suchen
            if ( ModelRenderer == null )
            {
                foreach ( var child in GameObject.Children )
                {
                    var renderer = child.Components.Get<SkinnedModelRenderer>();
                    if ( renderer != null )
                    {
                        if ( DebugMode ) Log.Info( $"Viewmodel ModelRenderer gefunden: {child.Name}" );
                        ModelRenderer = renderer;
                        break;
                    }
                }
            }
        }

        if ( DebugMode && ModelRenderer != null )
        {
            Log.Info( $"Viewmodel verwendet ModelRenderer: {ModelRenderer.GameObject.Name}" );
        }

        // Wichtig: Basis-Klassen-Implementation aufrufen
        base.OnStart();
    }
}
