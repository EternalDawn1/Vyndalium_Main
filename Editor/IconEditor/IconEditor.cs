using Editor;
using GeneralGame;
using Sandbox;
using System;

namespace GeneralGame;

public class IconEditor : GraphicsView
{
	public const int RENDER_RESOLUTION = 256;

	private SceneObject _obj;
	private StringProperty _model;
	private StringProperty _materialgroup;
	private StringProperty _materialoverride;
	private ColorProperty _color;
	private AnglesProperty _angles;
	private Vector3Property _position;
	private SceneCamera _camera;
	private SceneLight _light;
	private SceneDirectionalLight _directionalLight;
	private FloatProperty _lightBrightness;
	private FloatProperty _directionalLightBrightness;
	private Vector3Property _lightPosition;
	private FloatProperty _lightRadius;
	private ColorProperty _lightColor;
	private AnglesProperty _directionalLightRotation;
	private ColorProperty _directionalLightColor;


	public IconEditor( Widget parent ) : base( parent )
	{
		// Scene
		var world = new SceneWorld();
		_camera = new SceneCamera()
		{
			World = world,
			AmbientLightColor = Color.White,
			AntiAliasing = false,
			BackgroundColor = Color.Transparent,
			FieldOfView = 40,
			ZFar = 5000,
			ZNear = 2
		};

		_lightPosition = new Vector3Property( this ) { Value = Vector3.Forward * 15f };
		_lightRadius = new FloatProperty( this ) { Value = 1000f };
		_lightColor = new ColorProperty( this ) { Value = Color.White * 1f };
		_lightBrightness = new FloatProperty( this ) { Value = 1f };

		_directionalLightRotation = new AnglesProperty( this ) { Value = global::Rotation.From( 45, -45, 45 ).Angles() };
		_directionalLightColor = new ColorProperty( this ) { Value = Color.White * 10f };
		_directionalLightBrightness = new FloatProperty( this ) { Value = 1f };

		_light = new SceneLight( world, _lightPosition.Value, _lightRadius.Value, _lightColor.Value );
		_directionalLight = new SceneDirectionalLight( world, global::Rotation.From( _directionalLightRotation.Value ), _directionalLightColor.Value );

		var property = (parent as IconEditorPopup).Property;
		var icon = property.GetValue<IconSettings>();
		if ( icon.Guid == Guid.Empty ) // Generate if empty.
		{
			property.SetValue( icon = new IconSettings
			{
				Model = icon.Model,
				MaterialGroup = icon.MaterialGroup,
				MaterialOverride = icon.MaterialOverride,
				
				Colour = icon.Colour,
				Rotation = global::Rotation.Identity,
				Position = Vector3.Zero,
				Guid = Guid.NewGuid()
			} );
		}

		// Layout
		Layout = Layout.Column();
		Layout.Margin = 25;
		{

			Layout.Add( new Label( this ) { Text = "Light Position" } );
			Layout.Add( _lightPosition );
			Layout.AddSpacingCell( 4 );
			Layout.Add( new Label( this ) { Text = "Light Radius" } );
			Layout.Add( _lightRadius );
			Layout.AddSpacingCell( 4 );
			Layout.Add( new Label( this ) { Text = "Light Color" } );
			Layout.Add( _lightColor );
			Layout.AddSpacingCell( 4 );
			Layout.Add( new Label( this ) { Text = "Light Brightness" } );
			Layout.Add( _lightBrightness );

			
			Layout.AddSpacingCell( 4 );
			Layout.Add( new Label( this ) { Text = "Directional Light Rotation" } );
			Layout.Add( _directionalLightRotation );
			Layout.AddSpacingCell( 4 );
			Layout.Add( new Label( this ) { Text = "Directional Light Color" } );
			Layout.Add( _directionalLightColor );
			Layout.AddSpacingCell( 4 );
			Layout.Add( new Label( this ) { Text = "Directional Light Brightness" } );
			Layout.Add( _directionalLightBrightness );
			Layout.AddSpacingCell( 4 );
			// Properties
			Layout.AddSpacingCell( 4 );
			Layout.Add( new Label( this ) { Text = "Angles" } );
			_angles = Layout.Add( new AnglesProperty( this )
			{
				Value = icon.Rotation.Angles()
			}, 0 );

			Layout.AddSpacingCell( 4 );
			Layout.Add( new Label( this ) { Text = "Position" } );
			_position = Layout.Add( new Vector3Property( this )
			{
				Value = icon.Position
			}, 0 );

			Layout.AddSpacingCell( 4 );
			Layout.Add( new Label( this ) { Text = "Model Icon" } );
			_model = Layout.Add( new StringProperty( this )
			{
				Value = icon.Model
			}, 0 );
		
			_model.TextEdited += ( text ) =>
			{
				var mdl = Model.Load( text );
				_obj.Model = mdl?.ResourcePath == "models/dev/error.vmdl"
					? Model.Load( "models/dev/box.vmdl" )
					: mdl;
			};
			Layout.Add( new Label( this ) { Text = "Model Color" } );
			_color = Layout.Add( new ColorProperty( this )
			{
				Value = icon.Colour.WithAlpha( 1 )
			}, 0 );
			Layout.AddSpacingCell( 4 );

			Layout.Add( new Label( this ) { Text = "Material Group" } );

			_materialgroup = Layout.Add( new StringProperty( this )
			{
				Value = icon.MaterialGroup
			}, 0 );

			Layout.AddSpacingCell( 4 );
		
			_materialgroup.TextEdited += ( text ) =>
			{
				_obj.SetMaterialGroup( text );
			};

			Layout.AddSpacingCell( 4 );

			Label label = Layout.Add( new Label( this )
			{
				Text = "Material Override"
			}, 0 );

			_materialoverride = Layout.Add( new StringProperty( this )
			{
				Value = icon.MaterialOverride
			}, 0 );

			Layout.AddSpacingCell( 4 );

			_materialoverride.TextEdited += ( text ) =>
			{
				_obj.SetMaterialOverride( material: Material.Load( text ) );
			};

			Layout.AddSpacingCell( 4 );

			var button = Layout.Add( new Button( this )
			{
				Clicked = () => icon.Guid = Guid.NewGuid(),
				ToolTip = "Only use this when duplicating prefabs so the GUID doesn't overwrite icons.",
				Text = "WARNING!!! RESET GUID"
			}, 0 );
		}
		

		Layout.AddSpacingCell( 4 );
		{
			// Scene
			var renderer = Layout.Add( new NativeRenderingWidget( this )
			{
				Camera = _camera,
				TranslucentBackground = true,
			}, 1 );
		}

		Layout.AddSpacingCell( 4 );
		{
			// Save Button
			var button = Layout.Add( new global::Editor.Button( this )
			{
				Text = "Save Icon Settings",
				Clicked = () =>
				{
					property.SetValue( new IconSettings()
					{
						Model = _model.Value,
						MaterialGroup = _materialgroup.Value,
						MaterialOverride = _materialoverride.Value,
						
						LightBrightness = _lightBrightness.Value,
						LightRadius = _lightRadius.Value,
						LightColour = _lightColor.Value,

						DirectionalLightBrightness = _directionalLightBrightness.Value,
						DirectionalLightRotation = _directionalLightRotation.Value,
						DirectionalLightColour = _directionalLightColor.Value,

						Colour = _color.Value,
						Position = _position.Value,
						Rotation = _angles.Value,
						Guid = icon.Guid,
					} );

					var pixmap = new Pixmap( RENDER_RESOLUTION, RENDER_RESOLUTION );
					var path = $"{Project.Current.GetRootPath().Replace( '\\', '/' )}/Assets/ui/icons/{icon.Guid}.png";
					_camera.RenderToPixmap( pixmap );
					pixmap.SavePng( path );

					parent.Close();
				}
			}, 1 );
		}

		// Object
		var mdl = Model.Load( _model.Value );
		_obj = new SceneObject(
			world,
			(mdl?.IsError ?? true)
				? Model.Load( "models/dev/box.vmdl" )
				: mdl
		);
		_obj.SetMaterialGroup( _materialgroup.Value );
		
		var materialPath = _materialoverride.Value;
		var material = Material.Load( materialPath );
		if ( material != null )
		{
			_obj.SetMaterialOverride( material, "attributeName", 1 );
		}
	}
	

	public void UpdateLightPosition( Vector3 position )
	{
		_light.Position = position;
	}

	public void UpdateLightRadius( float radius )
	{
		_light.Radius = radius;
	}

	public void UpdateLightColor( Color color )
	{
		_light.LightColor = color;
	}

	public void UpdateDirectionalLightRotation( Angles rotation )
	{
		_directionalLight.Rotation = global::Rotation.From( rotation );
	}

	public void UpdateDirectionalLightColor( Color color )
	{
		_directionalLight.LightColor = color;
	}

	public void UpdateDirectionalLightBrightness( float brightness )
	{
		_directionalLight.LightColor = Color.White * brightness;
	}

	[EditorEvent.Frame]
	private void Frame()
	{
		if ( _obj == null )
			return;

		_camera.FitModel( _obj );
		_light.Position = _camera.Position + _camera.Rotation.Backward * 20f;
		_light.Radius = _lightRadius.Value;
		_light.LightColor = _lightColor.Value;
		_light.Radius = _lightRadius.Value;
		_light.LightColor = Color.White * _lightBrightness.Value;

		_light.Position = _lightPosition.Value;

		_directionalLight.Rotation = global::Rotation.From( _directionalLightRotation.Value );
		_directionalLightRotation.Value = _directionalLight.Rotation.Angles();
		_directionalLight.LightColor = _directionalLightColor.Value;
		_directionalLight.LightColor = Color.White * _directionalLightBrightness.Value;
		
		_obj.Position = _position.Value;
		_obj.Rotation = _angles.Value;
		_obj.ColorTint = _color.Value;
	}
}
