using System.Linq;
using Editor;
using Sandbox;

public static class AIToolsMenu
{
	[Menu( "Editor", "AI Tools/Generate sound" )]
    public static void Generate()
    {
		if (SoundSettings.Settings.ElevenLabsApiKey.Length < 1)
        {
            Dialog.AskConfirm(() => {}, "Set your ElevenLabs API key first in settings.", "Error", "Okay");
            return;
        }

        var inspectorObject = EditorUtility.InspectorObject as Asset;
        if (inspectorObject == null)
        {
            Dialog.AskConfirm(() => {}, "Open SoundEvent in Inspector first.", "Error", "Okay");
            return;
        }

        var resource = inspectorObject.LoadResource();
        if (resource == null)
        {
            Dialog.AskConfirm(() => {}, "Failed to load resource.", "Error", "Okay");
            return;
        }

        if (!(resource is SoundEvent))
        {
            Dialog.AskConfirm(() => {}, "Selected asset must be a SoundEvent.", "Error", "Okay");
            return;
        }

        var soundEvent = resource as SoundEvent;
        SoundAddons.Generate(soundEvent, (soundFile) => {
            soundEvent.Sounds.Add(soundFile);
			EditorUtility.InspectorObject = null;
            EditorUtility.InspectorObject = inspectorObject;
        });
    }

[Menu("Editor", "AI Tools/Split sound")]
    public static void Split()
    {
        var asset = EditorUtility.InspectorObject as Asset;
        var soundEvent = asset.LoadResource() as SoundEvent;

        var dialog = new Dialog();
        dialog.Window.Title = "Split Sound";
        dialog.Window.Size = new Vector2(800, 400);

        dialog.Layout = Layout.Column();
        var mainLayout = dialog.Layout.AddColumn();
        mainLayout.Margin = 16f;

        var spectogramWidget = new SpectogramWidget(null);
        mainLayout.Add(spectogramWidget, 1);

        var controlsLayout = mainLayout.AddRow();
        var splitButton = new Button.Primary("Split", "content_cut");
        controlsLayout.Add(splitButton);

        splitButton.Clicked = () =>
        {
            if (spectogramWidget.CurrentSound == null)
            {
                Log.Error("No sound loaded to split");
                return;
            }

            spectogramWidget.SplitCurrentSound((soundFile) => {
                soundEvent.Sounds.Add(soundFile);
				EditorUtility.InspectorObject = null;
				EditorUtility.InspectorObject = asset;
            });
        };

        dialog.Show();
    }
}
