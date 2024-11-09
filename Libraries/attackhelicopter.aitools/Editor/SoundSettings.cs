using System;
using System.IO;
using System.Text.Json;
using Editor;
using Sandbox;

public class SoundSettings : Widget
{
    public class Config : ConfigData
    {
        public string ElevenLabsApiKey { get; set; } = "";
        public string GenerationPath { get; set; } = "generated";
    }

    private LineEdit apiKeyInput;
    private LineEdit pathInput;
    private static Config _settings;
    private Button saveButton;

    public static Config Settings 
    {
        get
        {
            if (_settings == null)
                _settings = EditorUtility.LoadProjectSettings<Config>("settings.json");
            return _settings;
        }
    }

    public SoundSettings() : base(null)
    {
        Layout = Layout.Column();
        var mainLayout = Layout.Add(Layout.Column());
        mainLayout.Margin = 16;

        var apiKeyGroup = mainLayout.AddRow();
        apiKeyGroup.Add(new Label("ElevenLabs API Key:"));
        
        apiKeyInput = new LineEdit(this);
        apiKeyInput.MinimumWidth = 300;
        apiKeyInput.PlaceholderText = "Enter your ElevenLabs API key";
        apiKeyInput.Text = Settings.ElevenLabsApiKey;
        apiKeyGroup.Add(apiKeyInput);

        mainLayout.AddSpacingCell(8);

        var pathGroup = mainLayout.AddRow();
        pathGroup.Add(new Label("Generation Path:"));
        
        pathInput = new LineEdit(this);
        pathInput.MinimumWidth = 300;
        pathInput.PlaceholderText = "Enter generation path (relative to assets)";
        pathInput.Text = Settings.GenerationPath;
        pathGroup.Add(pathInput);

        mainLayout.AddStretchCell();

        var buttonLayout = mainLayout.AddRow();
        buttonLayout.AddStretchCell();

        saveButton = new Button.Primary("Save", "save");
        saveButton.Clicked = SaveSettings;
        buttonLayout.Add(saveButton);
    }

    private void SaveSettings()
    {
        Settings.ElevenLabsApiKey = apiKeyInput.Text;
        Settings.GenerationPath = pathInput.Text;
        EditorUtility.SaveProjectSettings(Settings, "settings.json");
        Dialog.AskConfirm(() => {}, "Settings saved successfully.", "Success", "Okay");
    }

    [Menu("Editor", "AI Tools/Settings")]
    public static void OpenSettings()
    {
        var dialog = new Dialog();
        dialog.Window.Title = "Sound Generation Settings";
        dialog.Window.Size = new Vector2(500, 200);
        dialog.Layout = Layout.Column();
        dialog.Layout.Add(new SoundSettings());
        dialog.Show();
    }

    public static string GetGenerationPath()
    {
        return Path.Combine(Project.Current.GetAssetsPath(), Settings.GenerationPath);
    }
}