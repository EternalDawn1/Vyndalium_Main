using Editor;
using GeneralGame;
using Sandbox;
using GeneralGame.HUD;

namespace GeneralGame;

public class IconEditorPopup : PopupWidget
{
	public SerializedProperty Property { get; private set; } 
	private readonly IconEditor _editor;
	private IconEditor.IconClass iconClassInstance { get; set; } = new IconEditor.IconClass();

	public IconEditorPopup( Widget parent, SerializedProperty property ) : base( parent )
	{
		Property = property;
		MinimumSize = new Vector2( 375, 800 );

		// Erstellen Sie den "X"-Knopf
		var closeButton = new Button( this )
		{
			Text = "X",
			ToolTip = "Schließen",
			Size = new Vector2( 20, 20 )
		};

		closeButton.Clicked += () =>
		{
			// Logik zum Schließen des Popups
			this.Close();
		};


		iconClassInstance = Property.GetValue<IconEditor.IconClass>(); // Verwenden Sie die Methode GetValue, um die Instanz zu erhalten
		if ( iconClassInstance == null )
		{
			Log.Error( "IconClass instance is null." );
			return;
		}

		var modelPath = iconClassInstance._modelPath; // Verwenden Sie die Eigenschaft _modelPath von IconClass
		if ( string.IsNullOrEmpty( modelPath ) )
		{
			Log.Error( "Model path is null or empty." );
			return;
		}


		// Erstellen Sie die IconEditor-Instanz mit dem Modellpfad
		_editor = new IconEditor( this, modelPath );
		_editor.Size = MinimumSize;
		_editor.MinimumSize = _editor.Size;

		Layout = Layout.Column();
		Layout.Margin = 18;

		// Fügen Sie den "X"-Knopf zum Layout hinzu
		Layout.Add( closeButton );

		Layout.Add( _editor );

		// Laden Sie die Icon-Einstellungen
		var filePath = $"{Project.Current.GetRootPath().Replace( '\\', '/' )}/Assets/ui/icons/{_editor.Instance.icon.Guid}.json";
		_editor.LoadIconSettings( filePath );

		// Überprüfen Sie, ob das Modell korrekt geladen wurde
		if ( string.IsNullOrEmpty( modelPath ) )
		{
			Log.Error( "Model path is null or empty." );
			return;
		}
		else
		{
			Log.Info( $"Model path: {modelPath}" );
		}

		// Aktualisieren Sie das Modell
		_editor.Instance.UpdateModel( modelPath );
	}
}