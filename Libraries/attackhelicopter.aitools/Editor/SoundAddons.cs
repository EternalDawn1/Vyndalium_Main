using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Reflection;
using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Editor;

public static class SoundAddons
{
	private static string previousSoundGenerationPromp;
	private static string previousSoundGenerationDuration;

	// [ContextMenuFor( typeof( SoundFile ), "Generate sound", "create" )]
	public static void Generate( SoundEvent soundEvent, Action<SoundFile> finished )
	{
		string defaultInput = !string.IsNullOrEmpty( previousSoundGenerationPromp )
			? previousSoundGenerationPromp
			: "A dog barking";

		Dialog.AskString( ( description ) =>
		{
			previousSoundGenerationPromp = description;

			Dialog.AskString( async ( durationStr ) =>
			{
				previousSoundGenerationDuration = durationStr;
				if ( !float.TryParse( durationStr, out float duration ) || duration < 0.5f || duration > 22f )
				{
					Log.Error( "Duration must be between 0.5 and 22 seconds" );
					return;
				}

				try
				{
					string apiKey = SoundSettings.Settings.ElevenLabsApiKey;
					var client = new HttpClient();
					client.DefaultRequestHeaders.Add( "xi-api-key", apiKey );

					var content = new StringContent( JsonSerializer.Serialize( new
					{
						text = description,
						duration_seconds = duration,
						prompt_influence = 0.3
					} ), Encoding.UTF8, "application/json" );

					var response = await client.PostAsync(
						"https://api.elevenlabs.io/v1/sound-generation",
						content
					);

					if ( response.IsSuccessStatusCode )
					{
						var bytes = await response.Content.ReadAsByteArrayAsync();

						string safeFileName = description.Replace( " ", "_" ).Replace( "/", "_" ).Replace( "\\", "_" );
						var mp3Path = Path.Combine(
							SoundSettings.Settings.GenerationPath,
							$"{safeFileName}_{DateTime.Now:yyyyMMddHHmmss}.mp3"
						);

						var fullMp3Path = Path.Combine( Project.Current.GetAssetsPath(), mp3Path );
						Directory.CreateDirectory( Path.GetDirectoryName( fullMp3Path ) );

						await File.WriteAllBytesAsync( fullMp3Path, bytes );
						Log.Info( "File saved to: " + fullMp3Path );

						var mp3Asset = AssetSystem.RegisterFile( fullMp3Path );
						if ( mp3Asset != null )
						{
							var soundFile = SoundFile.Load( mp3Asset.RelativePath );
							if ( soundFile != null )
							{
								finished(soundFile);
								Log.Info( $"Successfully generated and linked sound: {mp3Asset.RelativePath}" );
							}
							else
							{
								Log.Error( "Failed to load generated sound file" );
							}
						}
						else
						{
							Log.Error( "Failed to register generated mp3 file as asset" );
						}
					}
					else
					{
						var errorContent = await response.Content.ReadAsStringAsync();
						Log.Error( $"Failed to generate sound: {response.StatusCode}. Error: {errorContent}" );
					}
				}
				catch ( Exception ex )
				{
					Log.Error( $"Error generating sound: {ex.Message}" );
				}
			},
			"Enter duration in seconds (0.5 to 22):",
			"Generate",
			"Cancel",
			previousSoundGenerationDuration,
			"Set Sound Duration" );
		},
		"Describe the sound effect to generate:",
		"Next",
		"Cancel",
		defaultInput,
		"Generate Sound Effect" );
	}

	// [ContextMenuFor( typeof( SoundFile ), "Split sound", "content_cut" )]
	public static async void SplitSound( Widget parent, SerializedProperty property )
	{
		var soundResource = property.GetValue<SoundFile>();
		if ( soundResource == null || !soundResource.IsValid )
		{
			Log.Error( "No valid sound file selected" );
			return;
		}

		var dialog = new Dialog();
		dialog.Window.Title = "Split Sound";
		dialog.Window.Size = new Vector2( 800, 400 );

		dialog.Layout = Layout.Column();
		var mainLayout = dialog.Layout.AddColumn();
		mainLayout.Margin = 16f;

		var loadingLabel = new Label( "Loading audio data..." );
		mainLayout.Add( loadingLabel );

		dialog.Show();

		var samples = await soundResource.GetSamplesAsync();
		if ( samples == null )
		{
			loadingLabel.Text = "Failed to load audio samples";
			return;
		}

		loadingLabel.Destroy();

		var spectogramWidget = new SpectogramWidget( soundResource );
		mainLayout.Add( spectogramWidget, 1 );

		var controlsLayout = mainLayout.AddRow();

		var splitButton = new Button.Primary( "Split", "content_cut" );
		controlsLayout.Add( splitButton );

		splitButton.Clicked = () =>
		{
			var splitPoints = spectogramWidget.GetSplitPoints();
			if ( splitPoints.Count < 1 ) return;

			var listControlWidget = parent.GetAncestor<ListControlWidget>();
			if ( listControlWidget == null )
			{
				Log.Error( "Could not find ListControlWidget parent" );
				return;
			}

			try
			{
				var baseFileName = Path.GetFileNameWithoutExtension( soundResource.ResourcePath );
				var outputDir = Path.Combine(
					Project.Current.GetAssetsPath(),
					"generated",
					$"{baseFileName}_splits"
				);
				Directory.CreateDirectory( outputDir );

				var newSoundFiles = new List<SoundFile>();

				for ( int i = 0; i < splitPoints.Count - 1; i++ )
				{
					var start = splitPoints[i];
					var end = splitPoints[i + 1];
					var length = end - start;

					var segmentSamples = new short[length];
					Array.Copy( samples, start, segmentSamples, 0, length );

					var wavPath = Path.Combine( outputDir, $"{baseFileName}_part_{i + 1}.wav" );

					using ( var writer = new BinaryWriter( File.Create( wavPath ) ) )
					{
						writer.Write( Encoding.ASCII.GetBytes( "RIFF" ) );
						writer.Write( 36 + (segmentSamples.Length * 2) );
						writer.Write( Encoding.ASCII.GetBytes( "WAVE" ) );

						writer.Write( Encoding.ASCII.GetBytes( "fmt " ) );
						writer.Write( 16 );
						writer.Write( (short)1 );
						writer.Write( (short)soundResource.Channels );
						writer.Write( soundResource.Rate );
						writer.Write( soundResource.Rate * soundResource.Channels * 2 );
						writer.Write( (short)(soundResource.Channels * 2) );
						writer.Write( (short)16 );

						writer.Write( Encoding.ASCII.GetBytes( "data" ) );
						writer.Write( segmentSamples.Length * 2 );

						foreach ( var sample in segmentSamples )
						{
							writer.Write( sample );
						}
					}

					var asset = AssetSystem.RegisterFile( wavPath );
					if ( asset != null )
					{
						var soundFile = SoundFile.Load( asset.RelativePath );
						if ( soundFile != null )
						{
							newSoundFiles.Add( soundFile );
						}
					}
				}

				if ( newSoundFiles.Count > 0 )
				{
					try
					{
						var collectionField = listControlWidget.GetType()
							.GetFields( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public )
							.FirstOrDefault( f => typeof( SerializedCollection ).IsAssignableFrom( f.FieldType ) );

						if ( collectionField == null )
						{
							return;
						}

						var collection = collectionField.GetValue( listControlWidget ) as SerializedCollection;
						if ( collection == null )
						{
							return;
						}

						if ( property.TryGetAsObject( out var original ) )
						{
							foreach ( var soundFile in newSoundFiles )
							{
								collection.Add( soundFile );
							}

							collection.Remove( property );
						}
					}
					catch ( Exception ex )
					{
						Log.Error( $"Error manipulating collection: {ex.Message}" );
						Log.Error( $"Stack trace: {ex.StackTrace}" );
					}
				}

				dialog.Close();
			}
			catch ( Exception ex )
			{
				Log.Error( $"Error splitting sound: {ex.Message}" );
			}
		};
	}
}