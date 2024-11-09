using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using Editor;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace GeneralGame;

[Dock( "Editor", "SoundGenerator", "local_fire_department" )]
public class TextureGenerator : Widget
{
    private StringProperty _prompt;
    private StringProperty _objectPrompt;
    private StringProperty _stylePrompt;
    private StringProperty _artStyle;
    private StringProperty _negativePrompt;
    private StringProperty _resolution;
    private StringProperty _textureUrls;

    public TextureGenerator( Widget parent ) : base( parent, false )
    {
        // Create a Column Layout
        Layout = Layout.Column();
        // Give it some Margins/Spacing
        Layout.Margin = 2;
        Layout.Spacing = 2;
        // Apply some CSS styling
        SetStyles( "background-color: #303445; color: white; font-weight: 600;" );

        // Add some child Widgets to the Layout
        Layout.Add( new Editor.Label( "Enter your prompt:", this ) );
        _prompt = Layout.Add( new StringProperty( this ) );
        Layout.AddSpacingCell( 4 );

        Layout.Add( new Editor.Label( "Object Prompt:", this ) );
        _objectPrompt = Layout.Add( new StringProperty( this ) );
        Layout.AddSpacingCell( 4 );

        Layout.Add( new Editor.Label( "Style Prompt:", this ) );
        _stylePrompt = Layout.Add( new StringProperty( this ) );
        Layout.AddSpacingCell( 4 );

        Layout.Add( new Editor.Label( "Art Style:", this ) );
        _artStyle = Layout.Add( new StringProperty( this ) );
        Layout.AddSpacingCell( 4 );

        Layout.Add( new Editor.Label( "Negative Prompt:", this ) );
        _negativePrompt = Layout.Add( new StringProperty( this ) );
        Layout.AddSpacingCell( 4 );

        Layout.Add( new Editor.Label( "Resolution:", this ) );
        _resolution = Layout.Add( new StringProperty( this ) );
        Layout.AddSpacingCell( 4 );

        Layout.Add( new Editor.Label( "Texture URLs:", this ) );
        _textureUrls = Layout.Add( new StringProperty( this ) );
        Layout.AddSpacingCell( 4 );

        var btnTextToSpeech = Layout.Add( new Editor.Button( "Text to Speech", this ) );
        Layout.AddSpacingCell( 4 );
        btnTextToSpeech.Clicked += OnTextToSpeechClicked;

        var btnGenerateSound = Layout.Add( new Editor.Button( "Generate Sound", this ) );
        Layout.AddSpacingCell( 4 );
        btnGenerateSound.Clicked += OnGenerateSoundClicked;

        var btnGenerateTexture = Layout.Add( new Editor.Button( "Generate Texture", this ) );
        Layout.AddSpacingCell( 4 );
        btnGenerateTexture.Clicked += OnGenerateTextureClicked;
    }

    private async void OnTextToSpeechClicked()
    {
        var prompt = _prompt.Value;
        Log.Info( $"Prompt entered: {prompt}" );

        try
        {
            string voiceId = await GetVoiceId();
            string filePath = await GetGeneratedSpeechFile( prompt, voiceId );
            Log.Info( $"Sound file created at: {filePath}" );
            // Example: Open the file using the default media player
            Process.Start( new ProcessStartInfo( filePath ) { UseShellExecute = true } );
        }
        catch ( Exception ex )
        {
            Log.Error( $"Error generating sound files: {ex.Message}" );
        }
    }

    private async void OnGenerateSoundClicked()
    {
        var prompt = _prompt.Value;
        Log.Info( $"Prompt entered: {prompt}" );

        try
        {
            string filePath = await GetGeneratedSoundFile( prompt );
            Log.Info( $"Sound file created at: {filePath}" );
            // Example: Open the file using the default media player
            Process.Start( new ProcessStartInfo( filePath ) { UseShellExecute = true } );
        }
        catch ( Exception ex )
        {
            Log.Error( $"Error generating sound files: {ex.Message}" );
        }
    }

    private async void OnGenerateTextureClicked()
    {
        var prompt = _prompt.Value;
        var objectPrompt = _objectPrompt.Value;
        var stylePrompt = _stylePrompt.Value;
        var artStyle = _artStyle.Value;
        var negativePrompt = _negativePrompt.Value;
        var resolution = _resolution.Value;
        var textureUrls = _textureUrls.Value;

        Log.Info( $"Prompt entered: {prompt}" );
        Log.Info( $"Object Prompt: {objectPrompt}" );
        Log.Info( $"Style Prompt: {stylePrompt}" );
        Log.Info( $"Art Style: {artStyle}" );
        Log.Info( $"Negative Prompt: {negativePrompt}" );
        Log.Info( $"Resolution: {resolution}" );

        try
        {
            string taskId = await CreateTextureTask( prompt, objectPrompt, stylePrompt, artStyle, negativePrompt, resolution , textureUrls);
            Log.Info( $"Texture task created with ID: {taskId}" );
            // Example: Retrieve the texture task result
            var textureTask = await GetTextureTask( taskId );
            Log.Info( $"Texture task status: {textureTask.status}" );

            if ( textureTask.texture_urls != null && textureTask.texture_urls.Any() )
            {
                // Set the texture URLs to the StringProperty
                _textureUrls.Value = string.Join( ", ", textureTask.texture_urls.Select( url => url.base_color ) );
                // Example: Open the texture URL in the default browser
                Process.Start( new ProcessStartInfo( textureTask.texture_urls.First().base_color ) { UseShellExecute = true } );
            }
            else
            {
                Log.Error( "No texture URLs found." );
            }
        }
        catch ( Exception ex )
        {
            Log.Error( $"Error generating texture: {ex.Message}" );
        }
    }
    async Task<string> GetVoiceId()
    {
        using ( HttpClient client = new HttpClient() )
        {
            client.DefaultRequestHeaders.Add( "xi-api-key", "sk_77e95acf8b10d9a28df7918d889a6ebc8e24905655bc89a7" );

            HttpResponseMessage response = await client.GetAsync( "https://api.elevenlabs.io/v1/voices" );
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var voices = JsonSerializer.Deserialize<VoicesResponse>( responseBody );

            // Assuming you want the first voice in the list
            return voices.voices.First().voice_id;
        }
    }

    async Task<string> GetGeneratedSoundFile( string prompt )
    {
        using ( HttpClient client = new HttpClient() )
        {
            client.DefaultRequestHeaders.Add( "xi-api-key", "sk_77e95acf8b10d9a28df7918d889a6ebc8e24905655bc89a7" );

            var requestContent = new StringContent( $"{{\"text\": \"{prompt}\", \"duration_seconds\": 5, \"prompt_influence\": 0.3}}", System.Text.Encoding.UTF8, "application/json" );
            HttpResponseMessage response = await client.PostAsync( "https://api.elevenlabs.io/v1/sound-generation", requestContent );

            if ( !response.IsSuccessStatusCode )
            {
                string errorResponse = await response.Content.ReadAsStringAsync();
                Log.Error( $"Error response: {errorResponse}" );
                response.EnsureSuccessStatusCode();
            }

            string filePath = Path.Combine( Path.GetTempPath(), "generated_sound.mp3" );
            using ( var fs = new FileStream( filePath, FileMode.Create, FileAccess.Write, FileShare.None ) )
            {
                await response.Content.CopyToAsync( fs );
            }

            return filePath;
        }
    }

    async Task<string> GetGeneratedSpeechFile( string prompt, string voiceId )
    {
        using ( HttpClient client = new HttpClient() )
        {
            client.DefaultRequestHeaders.Add( "xi-api-key", "sk_77e95acf8b10d9a28df7918d889a6ebc8e24905655bc89a7" );

            var requestContent = new StringContent( $"{{\"text\": \"{prompt}\", \"model_id\": \"eleven_multilingual_v2\", \"voice_settings\": {{\"stability\": 0.5, \"similarity_boost\": 0.8, \"style\": 0.0, \"use_speaker_boost\": true}}}}", System.Text.Encoding.UTF8, "application/json" );
            HttpResponseMessage response = await client.PostAsync( $"https://api.elevenlabs.io/v1/text-to-speech/{voiceId}/stream", requestContent );

            if ( !response.IsSuccessStatusCode )
            {
                string errorResponse = await response.Content.ReadAsStringAsync();
                Log.Error( $"Error response: {errorResponse}" );
                response.EnsureSuccessStatusCode();
            }

            string filePath = Path.Combine( Path.GetTempPath(), "generated_speech.mp3" );
            using ( var fs = new FileStream( filePath, FileMode.Create, FileAccess.Write, FileShare.None ) )
            {
                await response.Content.CopyToAsync( fs );
            }

            return filePath;
        }
    }

    async Task<string> CreateTextureTask( string prompt, string objectPrompt, string stylePrompt, string artStyle, string negativePrompt, string resolution , string textureUrls)
    {
        using ( HttpClient client = new HttpClient() )
        {
            client.DefaultRequestHeaders.Add( "Authorization", "Bearer msy_UJhPlZxvLlXYWAKoY4yCieZG1hvd2vWmWwDl" );

            var requestContent = new StringContent( $"{{\"model_url\": \"https://cdn.meshy.ai/model/example_model_2.glb\", \"object_prompt\": \"{objectPrompt}\", \"style_prompt\": \"{stylePrompt}\", \"art_style\": \"{artStyle}\", \"negative_prompt\": \"{negativePrompt}\", \"resolution\": \"{resolution}\", \"enable_original_uv\": true, \"enable_pbr\": true}}", System.Text.Encoding.UTF8, "application/json" );
            HttpResponseMessage response = await client.PostAsync( "https://api.meshy.ai/v1/text-to-texture", requestContent );

            if ( !response.IsSuccessStatusCode )
            {
                string errorResponse = await response.Content.ReadAsStringAsync();
                Log.Error( $"Error response: {errorResponse}" );
                response.EnsureSuccessStatusCode();
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CreateTextureTaskResponse>( responseBody );

            return result.result;
        }
    }

    async Task<TextureTask> GetTextureTask( string taskId )
    {
        using ( HttpClient client = new HttpClient() )
        {
            client.DefaultRequestHeaders.Add( "Authorization", "Bearer msy_UJhPlZxvLlXYWAKoY4yCieZG1hvd2vWmWwDl" );

            HttpResponseMessage response = await client.GetAsync( $"https://api.meshy.ai/v1/text-to-texture/{taskId}" );
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var textureTask = JsonSerializer.Deserialize<TextureTask>( responseBody );

            return textureTask;
        }
    }


    public class VoicesResponse
    {
        public List<Voice> voices { get; set; }
    }

    public class Voice
    {
        public string voice_id { get; set; }
        public string name { get; set; }
    }

    public class CreateTextureTaskResponse
    {
        public string result { get; set; }
    }

    public class TextureTask
    {
        public string id { get; set; }
        public ModelUrls model_urls { get; set; }
        public string object_prompt { get; set; }
        public string style_prompt { get; set; }
        public string art_style { get; set; }
        public string negative_prompt { get; set; }
        public string thumbnail_url { get; set; }
        public int progress { get; set; }
        public long started_at { get; set; }
        public long created_at { get; set; }
        public long expires_at { get; set; }
        public long finished_at { get; set; }
        public string status { get; set; }
        public List<TextureUrl> texture_urls { get; set; }
    }

    public class ModelUrls
    {
        public string glb { get; set; }
        public string fbx { get; set; }
        public string usdz { get; set; }
    }

    public class TextureUrl
    {
        public string base_color { get; set; }
        public string metallic { get; set; }
        public string normal { get; set; }
        public string roughness { get; set; }
    }
}