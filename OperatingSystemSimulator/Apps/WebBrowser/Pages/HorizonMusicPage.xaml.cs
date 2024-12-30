using NAudio.Wave;
using OperatingSystemSimulator.ProcessHelper;
using OperatingSystemSimulator.ViewModels.AppViewModels;
using System.ComponentModel;
using System.Timers;

namespace OperatingSystemSimulator.Apps.WebBrowser.Pages;
public sealed partial class HorizonMusicPage : UserControl
{

    private WaveOutEvent? _waveOut;
    private AudioFileReader? _audioFile;
    private System.Timers.Timer _timer;

    private float _volume = 1;
    private float Volume
    {
        get => _volume;
        set
        {
            _volume = value;
            if (_audioFile != null)
            {
                _audioFile.Volume = value;
            }
        }
    }

    private readonly BrowserViewModel _browserViewModel;

    public HorizonMusicPage(BrowserViewModel browserViewModel)
    {
        InitializeComponent();
        _timer = new System.Timers.Timer(500);
        _timer.Elapsed += UpdateSliderAndTime;
        _browserViewModel = browserViewModel;

        _browserViewModel.PropertyChanged += BrowserViewModel_PropertyChanged;
    }

    public async void StopButton_Click(object? sender, RoutedEventArgs? e)
    {
        ProcessManager.Instance.BringToFront(_browserViewModel.PID);
        await ProcessManager.Instance.EnqueueRunningProcessAsync(_browserViewModel.PID);

        if (_waveOut != null)
        {
            PlayingTextBlock.Visibility = Visibility.Collapsed;
            _waveOut.Stop();
            _waveOut.Dispose();
            _audioFile?.Dispose();
            _waveOut = null;
            _audioFile = null;
            _timer.Stop();

            HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.AudioOutput, HardwareStatuses.Idle);
            StopButton.IsEnabled = false;
            PlayButton.IsEnabled = true;
            PauseButton.IsEnabled = false;
            SeekSlider.Value = 0;
            SeekSlider.IsEnabled = false;
            CurrentTimeText.Text = "00:00 / 00:00";
        }
    }

    private async void PauseButton_Click(object sender, RoutedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(_browserViewModel.PID);
        await ProcessManager.Instance.EnqueueRunningProcessAsync(_browserViewModel.PID);

        _waveOut?.Pause();
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.AudioOutput, HardwareStatuses.Idle);
        PlayButton.IsEnabled = true;
        PauseButton.IsEnabled = false;
        _timer.Stop();
    }

    private async void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(_browserViewModel.PID);
        await ProcessManager.Instance.EnqueueRunningProcessAsync(_browserViewModel.PID);

        try
        {
            if (_waveOut == null)
            {
                var musicPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/Musics", "music.mp3");
                _audioFile = new AudioFileReader(musicPath);
                _audioFile.Volume = Volume;
                _waveOut = new WaveOutEvent
                {
                    DesiredLatency = 200
                };

                _waveOut.Init(_audioFile);

                SeekSlider.IsEnabled = true;
                SeekSlider.Maximum = _audioFile.TotalTime.TotalSeconds;
            }

            HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.AudioOutput, HardwareStatuses.Running);

            _waveOut.Play();
            PlayButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            PauseButton.IsEnabled = true;
            PlayingTextBlock.Visibility = Visibility.Visible;
            _timer.Start();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error playing music: {ex.Message}");
        }
    }

    private void SeekSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (_audioFile != null && _waveOut != null && Math.Abs(e.NewValue - e.OldValue) > 0.5)
        {
            _audioFile.CurrentTime = TimeSpan.FromSeconds(SeekSlider.Value);
        }
    }


    private void UpdateSliderAndTime(object? sender, ElapsedEventArgs? e)
    {
        if (_audioFile != null && _waveOut != null && _waveOut.PlaybackState == PlaybackState.Playing)
        {
            ProcessManager.Instance.EnqueueRunningProcessAsync(_browserViewModel.PID);
            SeekSlider.Value = _audioFile.CurrentTime.TotalSeconds;
            CurrentTimeText.Text = $"{_audioFile.CurrentTime:mm\\:ss} / {_audioFile.TotalTime:mm\\:ss}";
        }
    }

    private void VolumeSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (_audioFile != null)
        {
            Volume = (float)e.NewValue / 100;
        }
    }

    private void BrowserViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BrowserViewModel.CurrentPage))
        {
            StopButton_Click(this, null);
        }
    }

    public void TryDispose()
    {
        if (_browserViewModel != null)
        {
            _browserViewModel.PropertyChanged -= BrowserViewModel_PropertyChanged;
        }
        _timer.Dispose();
        StopButton_Click(this, null);
    }

    private async void Slider_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(_browserViewModel.PID);
        await ProcessManager.Instance.EnqueueRunningProcessAsync(_browserViewModel.PID);
    }

}
