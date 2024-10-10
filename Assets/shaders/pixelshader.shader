
HEADER
{
	Description = "";
}

FEATURES
{
	#include "common/features.hlsl"
}

MODES
{
	VrForward();
	Depth(); 
	ToolsVis( S_MODE_TOOLS_VIS );
	ToolsWireframe( "vr_tools_wireframe.shader" );
	ToolsShadingComplexity( "tools_shading_complexity.shader" );
}

COMMON
{
	#ifndef S_ALPHA_TEST
	#define S_ALPHA_TEST 1
	#endif
	#ifndef S_TRANSLUCENT
	#define S_TRANSLUCENT 0
	#endif
	
	#include "common/shared.hlsl"
	#include "procedural.hlsl"

	#define S_UV2 1
	#define CUSTOM_MATERIAL_INPUTS
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
	float4 vColor : COLOR0 < Semantic( Color ); >;
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
	float3 vPositionOs : TEXCOORD14;
	float3 vNormalOs : TEXCOORD15;
	float4 vTangentUOs_flTangentVSign : TANGENT	< Semantic( TangentU_SignV ); >;
	float4 vColor : COLOR0;
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput v )
	{
		PixelInput i = ProcessVertex( v );
		i.vPositionOs = v.vPositionOs.xyz;
		i.vColor = v.vColor;

		VS_DecodeObjectSpaceNormalAndTangent( v, i.vNormalOs, i.vTangentUOs_flTangentVSign );

		return FinalizeVertex( i );
	}
}

PS
{
	#include "common/pixel.hlsl"
	
	SamplerState g_sSampler0 < Filter( POINT ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D( Color, Srgb, 8, "None", "_color", "Color,1/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( DetailTexture, Srgb, 8, "None", "_color", "Detail,2/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( TinMask, Srgb, 8, "None", "_mask", "Color,0/,0/1", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( SelfIllum, Srgb, 8, "None", "_selfillum", "Self illum,2/,0/3", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( AlphaMask, Srgb, 8, "None", "_trans", "Translucent,1/,0/2", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tColor < Channel( RGBA, Box( Color ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tDetailTexture < Channel( RGBA, Box( DetailTexture ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tTinMask < Channel( RGBA, Box( TinMask ), Linear ); OutputFormat( DXT1 ); SrgbRead( False ); >;
	Texture2D g_tSelfIllum < Channel( RGBA, Box( SelfIllum ), Srgb ); OutputFormat( DXT1 ); SrgbRead( True ); >;
	Texture2D g_tAlphaMask < Channel( RGBA, Box( AlphaMask ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	float g_flScrollX < UiStep( 0.01 ); UiGroup( "Color,0/Scrolling,1/0" ); Default1( 0 ); Range1( 0, 10 ); >;
	float g_flScrollY < UiStep( 0.01 ); UiGroup( "Color,0/Scrolling,1/0" ); Default1( 0 ); Range1( 0, 10 ); >;
	float g_flTextureScale < UiType( Slider ); UiGroup( "Detail,1/,0/1" ); Default1( 1 ); Range1( 0, 20 ); >;
	float2 g_vDetailTilling < UiType( Slider ); UiStep( 1 ); UiGroup( "Detail,4/,0/4" ); Default2( 1,1 ); >;
	float2 g_vDetailOffset < UiStep( 0.01 ); UiGroup( "Detail,3/,0/3" ); Default2( 0,0 ); >;
	float g_flBlendinfluence < UiType( Slider ); UiGroup( "Detail,2/,0/2" ); Default1( 0 ); Range1( 0, 5 ); >;
	bool g_bAdditive < UiGroup( "Detail,0/,0/0" ); Default( 0 ); >;
	float4 g_vTint < UiType( Color ); UiGroup( "Color,0/Tint,0/0" ); Default4( 1.00, 1.00, 1.00, 1.00 ); >;
	float g_flSelfillumStrength < UiGroup( "Self Illum,1/,0/0" ); Default1( 0 ); Range1( 0, 10 ); >;
	float g_flRoughnessAmount < UiGroup( "PBR,8/,0/0" ); Default1( 1 ); Range1( 0, 1 ); >;
	float g_flMetalnessAmount < UiGroup( "PBR,8/,0/0" ); Default1( 0 ); Range1( 0, 1 ); >;
	float g_flAmbientAmount < UiGroup( "PBR,8/,0/0" ); Default1( 1 ); Range1( 0, 1 ); >;
		
	float Overlay_blend( float a, float b )
	{
	    if ( a <= 0.5f )
	        return 2.0f * a * b;
	    else
	        return 1.0f - 2.0f * ( 1.0f - a ) * ( 1.0f - b );
	}
	
	float3 Overlay_blend( float3 a, float3 b )
	{
	    return float3(
	        Overlay_blend( a.r, b.r ),
	        Overlay_blend( a.g, b.g ),
	        Overlay_blend( a.b, b.b )
		);
	}
	
	float4 Overlay_blend( float4 a, float4 b, bool blendAlpha = false )
	{
	    return float4(
	        Overlay_blend( a.rgb, b.rgb ).rgb,
	        blendAlpha ? Overlay_blend( a.a, b.a ) : max( a.a, b.a )
	    );
	}
	
	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = Material::Init();
		m.Albedo = float3( 1, 1, 1 );
		m.Normal = float3( 0, 0, 1 );
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		m.TintMask = 1;
		m.Opacity = 1;
		m.Emission = float3( 0, 0, 0 );
		m.Transmission = 0;
		
		float l_0 = g_flScrollX;
		float l_1 = g_flScrollY;
		float4 l_2 = float4( l_0, l_1, 0, 0 );
		float4 l_3 = l_2 * float4( g_flTime, g_flTime, g_flTime, g_flTime );
		float2 l_4 = frac( TileAndOffsetUv( i.vTextureCoords.xy, float2( 1, 1 ), l_3.xy ) );
		float4 l_5 = Tex2DS( g_tColor, g_sSampler0, l_4 );
		float2 l_6 = i.vTextureCoords.xy * float2( 1, 1 );
		float l_7 = g_flTextureScale;
		float2 l_8 = l_6 * float2( l_7, l_7 );
		float2 l_9 = g_vDetailTilling;
		float2 l_10 = g_vDetailOffset;
		float2 l_11 = TileAndOffsetUv( l_8, l_9, l_10 );
		float2 l_12 = l_4 * l_11;
		float4 l_13 = Tex2DS( g_tDetailTexture, g_sSampler0, l_12 );
		float l_14 = g_flBlendinfluence;
		float4 l_15 = saturate( lerp( l_5, min( 1.0f, (l_5) + (l_13) ), l_14 ) );
		float4 l_16 = saturate( lerp( l_5, Overlay_blend( l_5, l_13 ), l_14 ) );
		float4 l_17 = g_bAdditive ? l_15 : l_16;
		float4 l_18 = g_vTint;
		float4 l_19 = Tex2DS( g_tTinMask, g_sSampler0, l_4 );
		float4 l_20 = saturate( lerp( l_17, l_17*l_18, l_19 ) );
		float2 l_21 = l_4 * float2( 1, 1 );
		float4 l_22 = Tex2DS( g_tSelfIllum, g_sSampler0, l_21 );
		float l_23 = g_flSelfillumStrength;
		float4 l_24 = lerp( float4( 0, 0, 0, 0 ), l_22, l_23 );
		float4 l_25 = Tex2DS( g_tAlphaMask, g_sSampler0, l_4 );
		float l_26 = g_flRoughnessAmount;
		float l_27 = g_flMetalnessAmount;
		float l_28 = g_flAmbientAmount;
		
		m.Albedo = l_20.xyz;
		m.Emission = l_24.xyz;
		m.Opacity = l_25.x;
		m.Roughness = l_26;
		m.Metalness = l_27;
		m.AmbientOcclusion = l_28;
		
		m.AmbientOcclusion = saturate( m.AmbientOcclusion );
		m.Roughness = saturate( m.Roughness );
		m.Metalness = saturate( m.Metalness );
		m.Opacity = saturate( m.Opacity );

		// Result node takes normal as tangent space, convert it to world space now
		m.Normal = TransformNormal( m.Normal, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );

		// for some toolvis shit
		m.WorldTangentU = i.vTangentUWs;
		m.WorldTangentV = i.vTangentVWs;
        m.TextureCoords = i.vTextureCoords.xy;
		
		return ShadingModelStandard::Shade( i, m );
	}
}
