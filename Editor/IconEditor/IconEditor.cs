using Editor;

using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace GeneralGame;

public class IconEditor : GraphicsView
{
	public const int RENDER_RESOLUTION = 128;
	public readonly IconClass Instance = new();


	public class IconClass 
	{
		public IconSettings icon;
		public SceneObject _obj;

		public string _modelPath { get; set; } 
		public Material _materialgroup { get; set; } 
		public Material _materialoverride { get; set; }
		public float _lightRadius { get; set; }
		public Color _lightColor { get; set; }
		public float _lightBrightness { get; set; }
		public Vector3 _lightPosition { get; set; }
		public Angles _directionalLightRotation { get; set; }
		public Color _directionalLightColor { get; set; }
		public float _directionalLightBrightness { get; set; }
		public Vector3 _position { get; set; }
		public Angles _angles { get; set; }
		public Color _color { get; set; }
		public SceneCamera _camera { get; set; }
		public SceneLight _light { get; set; }
		public SceneLight _directionalLight { get; set; }

		public void UpdateModel( string modelPath )
		{
		
			var mdl = Model.Load( modelPath );
			if ( mdl?.ResourcePath != "models/dev/error.vmdl" )
			{
				_obj.Model = mdl;
				_modelPath = modelPath;
				
			}
			else
			{
			
				if ( mdl == null )
				{
					Log.Error( "Model.Load returned null." );
				}
				else
				{
					Log.Error( $"Model.ResourcePath: {mdl.ResourcePath}" );
				}
			}
		}

	}
	

	
	public IconEditor( Widget parent, string modelpath ) : base( parent )
	{
		var world = new SceneWorld();
		Instance._camera = new SceneCamera()
		{
			World = world,
			AmbientLightColor = Color.White,
			AntiAliasing = false,
			BackgroundColor = Color.Transparent,
			FieldOfView = 40,
			ZFar = 5000,
			ZNear = 2
		};


		Instance._light = new SceneLight( world, Vector3.Forward * 15f, 1000f, Color.White * 0.7f );
		Instance._directionalLight = new SceneDirectionalLight( world, global::Rotation.From( 45, -45, 45 ), Color.White * 10f );
		Instance._obj = new SceneObject( world, Model.Load( modelpath ) );
		Instance.UpdateModel( modelpath );

		var filePath = $"{Project.Current.GetRootPath().Replace( '\\', '/' )}/Assets/ui/icons/{Instance.icon.Guid}.json";
		LoadIconSettings( filePath );

		if ( string.IsNullOrEmpty( modelpath ) )
		{
			Log.Error( "Model path is null or empty." );
			return;
		}
		else
		{
			Log.Info( $"Model path: {modelpath}" );
		}




		Instance._lightColor = Color.White;
		Instance._color = Color.White;
		Instance._position = Vector3.Zero;
		Instance._angles = Angles.Zero;
		Instance._lightRadius = 1000f;
		Instance._lightBrightness = 0.7f;
		Instance._lightPosition = Vector3.Forward * 15f;
		Instance._directionalLightRotation = new Angles( 45, -45, 45 );
		Instance._directionalLightColor = Color.White;
		Instance._directionalLightBrightness = 10f;


		var so = Instance.GetSerialized();
		var cs = new ControlSheet();

		Layout = Layout.Column();
		Layout.Add( cs );
		Layout.AddStretchCell();

		cs.AddRow( so.GetProperty( nameof( IconClass._modelPath ) ) );

		cs.AddProperty( Instance, x => x._angles );
		cs.AddProperty( Instance, x => x._position );

		cs.AddProperty( Instance, x => x._color );
		cs.AddProperty( Instance, x => x._materialgroup );
		cs.AddProperty( Instance, x => x._materialoverride );
		cs.AddProperty( Instance, x => x._lightRadius );
		cs.AddProperty( Instance, x => x._lightColor );
		cs.AddProperty( Instance, x => x._lightBrightness );
		cs.AddProperty( Instance, x => x._lightPosition );
		cs.AddProperty( Instance, x => x._directionalLightRotation );
		cs.AddProperty( Instance, x => x._directionalLightColor );
		cs.AddProperty( Instance, x => x._directionalLightBrightness );

		var property = (parent as IconEditorPopup).Property;
		var icon = property.GetValue<IconSettings>();

		if ( Instance.icon.Guid == Guid.Empty ) // Generate if empty.
		{
			property.SetValue( Instance.icon = new IconSettings
			{
				Model = Instance.icon.Model,
				MaterialGroup = Instance.icon.MaterialGroup,
				MaterialOverride = Instance.icon.MaterialOverride,
				Colour = Instance.icon.Colour,
				Rotation = global::Rotation.Identity,
				Position = Vector3.Zero,
				Guid = Guid.NewGuid()
			} );
		}


	
		



		var button = Layout.Add( new Button( this )
		{
			Clicked = () => Instance.icon.Guid = Guid.NewGuid(),
			ToolTip = " GUID doesn't overwrite icons.",
			Text = "Reset Guid"
		}, 0 );

		Layout.AddSpacingCell( 4 );
		var renderer = Layout.Add( new NativeRenderingWidget( this )
		{
			Camera = Instance._camera,
			TranslucentBackground = true,
			
		}, 1 );



		Layout.AddSpacingCell( 4 );
		{
			// Save Button
			var saveButton = Layout.Add( new global::Editor.Button( this )
			{
				Text = "Save Icon",
				Clicked = () =>
				{
					property.SetValue( new IconSettings()
					{
						Model = Instance._modelPath,
						MaterialGroup = Instance._materialgroup,
						MaterialOverride = Instance._materialoverride,

						LightBrightness = Instance._lightBrightness,
						LightRadius = Instance._lightRadius,
						LightColour = Instance._lightColor,

						DirectionalLightBrightness = Instance._directionalLightBrightness,
						DirectionalLightRotation = Instance._directionalLightRotation,
						DirectionalLightColour = Instance._directionalLightColor,
					} );

					var filePath = $"{Project.Current.GetRootPath().Replace( '\\', '/' )}/Assets/ui/icons/{Instance.icon.Guid}.json";
					SaveIconSettings( filePath );

					var pixmap = new Pixmap( RENDER_RESOLUTION, RENDER_RESOLUTION );
					var path = $"{Project.Current.GetRootPath().Replace( '\\', '/' )}/Assets/ui/icons/{Instance.icon.Guid}.png";
					Instance._camera.RenderToPixmap( pixmap );
					pixmap.SavePng( path );

					parent.Close();
				}
			}, 1 );
		}
		
		

	






	}
	public void SaveIconSettings( string filePath )
	{
		var iconSettings = new IconSettings
		{
			Model = Instance._modelPath,
			MaterialGroup = Instance._materialgroup,
			MaterialOverride = Instance._materialoverride,
			LightBrightness = Instance._lightBrightness,
			LightRadius = Instance._lightRadius,
			LightColour = Instance._lightColor,
			DirectionalLightBrightness = Instance._directionalLightBrightness,
			DirectionalLightRotation = Instance._directionalLightRotation,
			DirectionalLightColour = Instance._directionalLightColor,
			Colour = Instance._color,
			Position = Instance._position,
			Rotation = Instance._angles.ToRotation(),
			Guid = Instance.icon.Guid
		};

		var json = JsonSerializer.Serialize( iconSettings );
		File.WriteAllText( filePath, json );
	}

	public void LoadIconSettings( string filePath )
	{
		if ( !File.Exists( filePath ) )
			return;

		var json = File.ReadAllText( filePath );
		var iconSettings = JsonSerializer.Deserialize<IconSettings>( json );

		Instance._modelPath = iconSettings.Model;
		Instance._materialgroup = iconSettings.MaterialGroup;
		Instance._materialoverride = iconSettings.MaterialOverride;
		Instance._lightBrightness = iconSettings.LightBrightness;
		Instance._lightRadius = iconSettings.LightRadius;
		Instance._lightColor = iconSettings.LightColour;
		Instance._directionalLightBrightness = iconSettings.DirectionalLightBrightness;
		Instance._directionalLightRotation = iconSettings.DirectionalLightRotation;
		Instance._directionalLightColor = iconSettings.DirectionalLightColour;
		Instance._color = iconSettings.Colour;
		Instance._position = iconSettings.Position;
		Instance._angles = iconSettings.Rotation.Angles();
		Instance.icon.Guid = iconSettings.Guid;
	}





	[EditorEvent.Frame]
	private void Frame()
	{
		if ( Instance._obj == null )
			return;

		Instance._camera.FitModel( Instance._obj );
		Instance._modelPath = Instance.icon.Model;
		
	
		Instance._light.Position = Instance._camera.Position + Instance._camera.Rotation.Backward * 20f;
		Instance._light.Radius = Instance._lightRadius;
		Instance._light.LightColor = Instance._lightColor;
		Instance._light.LightColor = Color.White * Instance._lightBrightness;

		Instance._light.Position = Instance._lightPosition;

		Instance._directionalLight.Rotation = global::Rotation.From( Instance._directionalLightRotation );
		Instance._directionalLight.LightColor = Instance._directionalLightColor;
		Instance._directionalLight.LightColor = Color.White * Instance._directionalLightBrightness;

		Instance._obj.Position = Instance._position;
		Instance._obj.Rotation = Instance._angles.ToRotation();
		Instance._obj.ColorTint = Instance._color;
	}
}
public class PropertyExtension
{
	public class StringProperty
	{
		private string _value;
		public string Value
		{
			get => _value;
			set
			{
				if ( _value != value )
				{
					_value = value;
					OnTextEdited?.Invoke( _value );
				}
			}
		}

		public event Action<string> OnTextEdited;

		public StringProperty( string value )
		{
			_value = value;
		}
	}
}
