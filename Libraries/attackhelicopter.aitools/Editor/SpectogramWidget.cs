using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Editor;
using Sandbox;
using System.Text;

class SpectogramWidget : Widget 
{
    private short[] samples;
    private int sampleRate;
    private List<int> splitPoints = new List<int>();
    private int? dragPoint = null;
    private Label loadingLabel;
    private Label dropLabel;
    private bool isLoading = true;
    public SoundFile CurrentSound { get; private set; }

    public SpectogramWidget(SoundFile soundFile) : base(null)
    {
        MinimumSize = 100;
        MouseTracking = true;
        AcceptDrops = true;

        loadingLabel = new Label(this);
        loadingLabel.Text = "Loading audio data...";
        loadingLabel.Visible = false;
        
        dropLabel = new Label(this);
        dropLabel.Text = "Drop a sound file here";
        dropLabel.SetStyles("font-size: 18px; color: #aaa; text-align: center;");
        
        if (soundFile != null)
        {
            LoadSound(soundFile);
        }
    }

    public async void LoadSound(SoundFile soundFile)
    {
        CurrentSound = soundFile;
        isLoading = true;
        samples = null;
        splitPoints.Clear();
        
        loadingLabel.Visible = true;
        dropLabel.Visible = false;

        await LoadAudioDataAsync(soundFile);
    }

    private async Task LoadAudioDataAsync(SoundFile soundFile)
    {
        try 
        {
            await soundFile.LoadAsync();
            samples = await soundFile.GetSamplesAsync();
            
            if (samples == null)
            {
                loadingLabel.Text = "Failed to load audio data";
                return;
            }

            sampleRate = soundFile.Rate;
            splitPoints.Add(0);
            splitPoints.Add(samples.Length - 1);

            loadingLabel.Visible = false;
            isLoading = false;

            Update();
        }
        catch (Exception ex)
        {
            loadingLabel.Text = $"Error loading audio: {ex.Message}";
        }
    }

    protected override void DoLayout()
    {
        base.DoLayout();

        if (loadingLabel != null)
        {
            loadingLabel.Position = new Vector2(10, Height / 2 - 10);
            loadingLabel.Size = new Vector2(Width - 20, 20);
        }

        if (dropLabel != null)
        {
            dropLabel.Position = new Vector2(10, Height / 2 - 10);
            dropLabel.Size = new Vector2(Width - 20, 20);
        }
    }

    public override void OnDragDrop(DragEvent e)
    {
        base.OnDragDrop(e);

        if (!e.Data.HasFileOrFolder) return;

        var asset = AssetSystem.FindByPath(e.Data.FileOrFolder);
        if (asset?.AssetType != AssetType.SoundFile) return;

        var soundFile = SoundFile.Load(asset.Path);
        if (soundFile != null)
        {
            LoadSound(soundFile);
        }
    }

    public override void OnDragHover(DragEvent e)
    {
        base.OnDragHover(e);

        if (!e.Data.HasFileOrFolder) return;

        var asset = AssetSystem.FindByPath(e.Data.FileOrFolder);
        if (asset?.AssetType != AssetType.SoundFile) return;

        e.Action = DropAction.Link;
    }

    protected override void OnMouseClick(MouseEvent e)
    {
        if (isLoading) return;
        
        base.OnMouseClick(e);

        if (e.Button == MouseButtons.Left)
        {
            var samplePos = (int)(e.LocalPosition.x / Width * samples.Length);
            var nearPoint = splitPoints.FirstOrDefault(p => Math.Abs(p - samplePos) < (samples.Length / Width * 5));
            
            if (nearPoint != default)
            {
                dragPoint = splitPoints.IndexOf(nearPoint);
            }
            else
            {
                splitPoints.Add(samplePos);
                splitPoints.Sort();
                Update();
            }
        }
    }

    protected override void OnMouseMove(MouseEvent e)
    {
        if (isLoading) return;
        
        base.OnMouseMove(e);

        if (dragPoint.HasValue)
        {
            var samplePos = (int)(e.LocalPosition.x / Width * samples.Length);
            splitPoints[dragPoint.Value] = samplePos;
            splitPoints.Sort();
            Update();
        }
    }

    protected override void OnMouseReleased(MouseEvent e)
    {
        if (isLoading) return;
        
        base.OnMouseReleased(e);
        dragPoint = null;
    }

    protected override void OnPaint()
    {
        base.OnPaint();

        if (isLoading || samples == null)
        {
            return;
        }

        Paint.ClearPen();
        Paint.SetBrush(Theme.Grey.WithAlpha(0.1f));
        Paint.DrawRect(LocalRect);

        Paint.SetPen(Theme.Blue);
        var samplesPerPixel = samples.Length / Width;
        for (int x = 0; x < Width; x++)
        {
            var startSample = (int)(x * samplesPerPixel);
            var endSample = Math.Min(startSample + samplesPerPixel, samples.Length);
            
            var max = short.MinValue;
            var min = short.MaxValue;
            
            for (int i = startSample; i < endSample; i++)
            {
                max = Math.Max(max, samples[i]);
                min = Math.Min(min, samples[i]);
            }

            var y1 = Height / 2 + (min / (float)short.MaxValue * Height / 2);
            var y2 = Height / 2 + (max / (float)short.MaxValue * Height / 2);
            
            Paint.DrawLine(new Vector2(x, y1), new Vector2(x, y2));
        }

        Paint.SetPen(Theme.Red);
        foreach (var point in splitPoints)
        {
            var x = point / (float)samples.Length * Width;
            Paint.DrawLine(new Vector2(x, 0), new Vector2(x, Height));
        }
    }

    public List<int> GetSplitPoints()
    {
        return new List<int>(splitPoints);
    }

    public void SplitCurrentSound(Action<SoundFile> onSoundCreated)
    {
        if (CurrentSound == null || samples == null) return;

        var splitPoints = GetSplitPoints();
        if (splitPoints.Count < 2) return;

        try
        {
            var baseFileName = Path.GetFileNameWithoutExtension(CurrentSound.ResourcePath);
            var outputDir = Path.Combine(
                Project.Current.GetAssetsPath(),
                "generated",
                $"{baseFileName}_splits"
            );
            Directory.CreateDirectory(outputDir);

            for (int i = 0; i < splitPoints.Count - 1; i++)
            {
                var start = splitPoints[i];
                var end = splitPoints[i + 1];
                var length = end - start;

                var segmentSamples = new short[length];
                Array.Copy(samples, start, segmentSamples, 0, length);

                var wavPath = Path.Combine(outputDir, $"{baseFileName}_part_{i + 1}.wav");

                using (var writer = new BinaryWriter(File.Create(wavPath)))
                {
                    writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                    writer.Write(36 + (segmentSamples.Length * 2));
                    writer.Write(Encoding.ASCII.GetBytes("WAVE"));

                    writer.Write(Encoding.ASCII.GetBytes("fmt "));
                    writer.Write(16);
                    writer.Write((short)1);
                    writer.Write((short)CurrentSound.Channels);
                    writer.Write(CurrentSound.Rate);
                    writer.Write(CurrentSound.Rate * CurrentSound.Channels * 2);
                    writer.Write((short)(CurrentSound.Channels * 2));
                    writer.Write((short)16);

                    writer.Write(Encoding.ASCII.GetBytes("data"));
                    writer.Write(segmentSamples.Length * 2);

                    foreach (var sample in segmentSamples)
                    {
                        writer.Write(sample);
                    }
                }

                var asset = AssetSystem.RegisterFile(wavPath);
                if (asset != null)
                {
                    var soundFile = SoundFile.Load(asset.RelativePath);
                    if (soundFile != null)
                    {
                        onSoundCreated?.Invoke(soundFile);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error splitting sound: {ex.Message}");
        }
    }
}