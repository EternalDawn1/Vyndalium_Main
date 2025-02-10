using Editor;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Editor.Widgets;

namespace GeneralGame;

public class IconEditor : GraphicsView
{
	public const int RENDER_RESOLUTION = 128;

	private SerializedObject Object { get; }
	private SerializedProperty Property { get; }
	private GameObject GameObject { get; }
	private IconSettings Icon { get; set; }

	private SceneObject _obj;
	private SceneCamera _camera;
	private SceneLight _light;



	private Model _previousModel;
	private string _previousMaterialGroup;

	private DropdownWidget<string> _materialGroupWidget;

	private string _materialoverride;

	private Guid GetUniqueId( string name )
	{
		if ( !GameObject.IsValid() ) return default;

		MD5 md5 = MD5.Create();
		byte[] hash = md5.ComputeHash( Encoding.UTF8.GetBytes( name ) );

		return new Guid( hash );
	}

	private void UpdateMaterialGroupWidget( Model model )
	{
		if ( _materialGroupWidget == null || model == null )
			return;

		var capacity = model.MaterialGroupCount;
		var materialGroups = new List<string>( capacity );
		for ( int i = 0; i < capacity; i++ )
			materialGroups.Insert( i, model.GetMaterialGroupName( i ) );

		_materialGroupWidget.Options = materialGroups;
	}

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

		Vector3 lightDirection = Vector3.Forward * 15f;
		float lightRange = 1000f;
		Color lightColor = Color.White * 0.7f;

		_light = new SceneLight( world, lightDirection, lightRange, lightColor );
		_ = new SceneDirectionalLight( world, global::Rotation.From( 45, -45, 45 ), Color.White * 10f );

		Property = (parent as IconEditorPopup).Property;
		GameObject = Property.Parent.GetProperty( "GameObject" ).GetValue<GameObject>();
		Object = Property.GetValue<IconSettings>().GetSerialized();
		Icon = Property.GetValue<IconSettings>();

		if ( Icon.Guid == Guid.Empty ) // Generate if empty.
		{
			var modelRenderer = GameObject?.Components.Get<ModelRenderer>( FindMode.EverythingInSelfAndChildren );
			var item = GameObject?.Components.Get<ItemComponent>( FindMode.EverythingInSelfAndChildren );

			Property.SetValue( Icon = new IconSettings
			{
				Model = modelRenderer?.Model ?? Icon.Model,
				MaterialGroup = modelRenderer?.MaterialGroup ?? Icon.MaterialGroup,
				MaterialOverride = modelRenderer?.MaterialOverride ?? Icon.MaterialOverride,
				Colour = modelRenderer?.Tint ?? Icon.Colour,
				Rotation = global::Rotation.Identity,
				Position = Vector3.Zero,
				Guid = GetUniqueId( item.Name )
			} );
		}

		// Layout
		Layout = Layout.Column();
		Layout.Margin = 10;
		{
			// Properties
			if ( Object.TryGetProperty( "Rotation", out var rotation ) )
				Layout.Add( new RotationControlWidget( rotation ), 0 );

			Layout.AddSpacingCell( 4 );

			if ( Object.TryGetProperty( "Position", out var position ) )
				Layout.Add( new VectorControlWidget( position ), 0 );

			Layout.AddSpacingCell( 4 );

			if ( Object.TryGetProperty( "Model", out var model ) )
				Layout.Add( new ResourceControlWidget( model ), 0 );

			Layout.AddSpacingCell( 4 );

			if ( Object.TryGetProperty( "MaterialGroup", out var materialGroup ) )
				_materialGroupWidget = Layout.Add( new DropdownWidget<string>( materialGroup ), 0 );

			if ( model != null ) UpdateMaterialGroupWidget( model.GetValue<Model>() );

			Layout.AddSpacingCell( 4 );

			if ( Object.TryGetProperty( "MaterialOverride", out var materialOverride ) )
				Layout.Add( new ResourceControlWidget( materialOverride ), 0 );

			Layout.AddSpacingCell( 4 );

			if ( Object.TryGetProperty( "Colour", out var color ) )
				Layout.Add( new ColorControlWidget( color ), 0 );

			Layout.AddSpacingCell( 4 );
		}

		Layout.AddSpacingCell( 4 );

		var row = Layout.Add( Layout.Row() );
		row.Alignment = TextFlag.CenterHorizontally;
		{
			// Scene
			var renderer = row.Add( new NativeRenderingWidget( this )
			{
				Camera = _camera,
				FixedSize = _camera.Size / 2f,
				TranslucentBackground = true
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
					var item = GameObject?.Components.Get<ItemComponent>( FindMode.EverythingInSelfAndChildren );
					Property.SetValue( Icon = new IconSettings()
					{
						Model = Object.GetProperty( "Model" ).GetValue<Model>(),
						MaterialGroup = Object.GetProperty( "MaterialGroup" ).GetValue<string>(),
						MaterialOverride = Object.GetProperty( "MaterialOverride" ).GetValue<string>(),
						Colour = Object.GetProperty( "Colour" ).GetValue<Color>(),
						Position = Object.GetProperty( "Position" ).GetValue<Vector3>(),
						Rotation = Object.GetProperty( "Rotation" ).GetValue<Rotation>(),
						Guid = GetUniqueId( item.Name )
					} );

					var pixmap = new Pixmap( RENDER_RESOLUTION, RENDER_RESOLUTION );
					var path = $"{Project.Current.GetRootPath().Replace( '\\', '/' )}/assets/ui/icons/{Icon.Guid}.png";
					_camera.RenderToPixmap( pixmap );
					pixmap.SavePng( path );

					parent.Close();
				}
			}, 1 );
		}

		// Object
		_previousModel = Object.GetProperty( "Model" ).GetValue<Model>();
		_previousMaterialGroup = Object.GetProperty( "MaterialGroup" ).GetValue<string>();
		_obj = new SceneObject(
			world,
			(_previousModel?.IsError ?? true)
				? Model.Load( "models/dev/box.vmdl" )
				: _previousModel
		);

		_obj.SetMaterialGroup( _previousMaterialGroup );
		
	}

	[EditorEvent.Frame]
	private void Frame()
	{
		if ( _obj == null )
			return;

		var model = Object.GetProperty( "Model" ).GetValue<Model>();
		var materialGroup = Object.GetProperty( "MaterialGroup" ).GetValue<string>();

		// Update model and material group.
		if ( _previousModel != model )
		{
			_obj.Model = model?.ResourcePath == "models/dev/error.vmdl"
				? Model.Load( "models/dev/box.vmdl" )
				: model;

			UpdateMaterialGroupWidget( model );
		}

		if ( _previousMaterialGroup != materialGroup )
			_obj.SetMaterialGroup( materialGroup );

		_previousModel = model;
		_previousMaterialGroup = materialGroup;

		// Update camera and light.
		_camera.FitModel( _obj );
		_light.Position = _camera.Position + _camera.Rotation.Backward * 20f;

		// Update object transform.
		_obj.Position = Object.GetProperty( "Position" ).GetValue<Vector3>();
		_obj.Rotation = Object.GetProperty( "Rotation" ).GetValue<Rotation>();
		_obj.ColorTint = Object.GetProperty( "Colour" ).GetValue<Color>();

		// Update material override.
		var materialOverride = Object.GetProperty( "MaterialOverride" ).GetValue<string>();
	}
}




