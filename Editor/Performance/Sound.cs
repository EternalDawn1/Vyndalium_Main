using System;
using System.Diagnostics;
using System.Linq;
using Editor;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;

namespace GeneralGame;

[Dock( "Editor", "TextureGenerator", "local_fire_department" )]
public class TextureGenerator : Widget
{
    private StringProperty _prompt;

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

        var btn = Layout.Add( new Editor.Button( "Submit", this ) );
        Layout.AddSpacingCell( 4 );
        btn.Clicked += OnSubmitClicked;
    }

    private async void OnSubmitClicked()
    {
        var prompt = _prompt.Value;
        Log.Info( $"Prompt entered: {prompt}" );

        string textureFilePath = await GetGeneratedTextureFile( prompt );
        DisplayTextureFromFile( textureFilePath );
    }

    async Task<string> GetGeneratedTextureFile( string prompt )
    {
        using ( HttpClient client = new HttpClient() )
        {
            client.DefaultRequestHeaders.Add( "Tiger", "sk_77e95acf8b10d9a28df7918d889a6ebc8e24905655bc89a7" );

            var requestContent = new StringContent( $"{{\"text\": \"{prompt}\"}}", System.Text.Encoding.UTF8, "application/json" );
            HttpResponseMessage response = await client.PostAsync( "https://elevenlabs.io/app/settings/api-keys", requestContent );
            response.EnsureSuccessStatusCode();
            byte[] textureBytes = await response.Content.ReadAsByteArrayAsync();

            string filePath = Path.Combine( Path.GetTempPath(), "generated_texture.png" );
            File.WriteAllBytes( filePath, textureBytes );
            return filePath;
        }
    }

    void DisplayTextureFromFile( string filePath )
    {
        // Implement the logic to display the texture from the file
        // This could involve loading the texture into a UI element or applying it to a 3D model
    }
}