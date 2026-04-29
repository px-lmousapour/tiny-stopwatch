using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TinyStopwatch.Services;
using MediaColor = System.Windows.Media.Color;
using WpfApplication = System.Windows.Application;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace TinyStopwatch;

public partial class MainWindow : Window
{
    private readonly Stopwatch _sw = new();
    private readonly System.Windows.Threading.DispatcherTimer _timer = new()
    {
        Interval = TimeSpan.FromMilliseconds(100)
    };
    private bool _running;
    private readonly HistoryService _history = new();

    // Windows 11 light-theme colours
    private static readonly SolidColorBrush BlueBrush   = new(MediaColor.FromRgb(0, 120, 212));
    private static readonly SolidColorBrush YellowBrush = new(MediaColor.FromRgb(196, 98, 0));
    private static readonly SolidColorBrush DarkBrush   = new(MediaColor.FromRgb(28, 28, 28));

    public MainWindow()
    {
        InitializeComponent();
        _timer.Tick += (_, _) => Refresh();
        PositionOnTaskbar();
    }

    private void PositionOnTaskbar()
    {
        // Span the full screen width; sit flush above the taskbar
        Width = SystemParameters.PrimaryScreenWidth;
        Left  = 0;
        Top   = SystemParameters.WorkArea.Bottom - Height;
    }

    private void Refresh()
    {
        var t = _sw.Elapsed;
        TimerDisplay.Text = t.TotalHours >= 1
            ? $"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}.{t.Milliseconds / 100}"
            : $"{t.Minutes:00}:{t.Seconds:00}.{t.Milliseconds / 100}";

        ((App)WpfApplication.Current).UpdateTrayTooltip(TimerDisplay.Text);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            PlayPause_Click(this, new RoutedEventArgs());
            e.Handled = true;
        }
        base.OnKeyDown(e);
    }

    private void PlayPause_Click(object sender, RoutedEventArgs e)
    {
        if (_running)
        {
            _sw.Stop();
            _timer.Stop();
            PlayPauseIcon.Text       = "▶";
            PlayPauseIcon.Foreground = BlueBrush;
            TimerDisplay.Foreground  = DarkBrush;
        }
        else
        {
            _sw.Start();
            _timer.Start();
            PlayPauseIcon.Text       = "⏸";
            PlayPauseIcon.Foreground = YellowBrush;
            TimerDisplay.Foreground  = YellowBrush;
            ResetBtn.IsEnabled       = true;
        }
        _running = !_running;
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        if (_sw.Elapsed.TotalMilliseconds > 100)
            _history.AddEntry(_sw.Elapsed);

        _sw.Reset();
        _timer.Stop();
        _running = false;

        PlayPauseIcon.Text       = "▶";
        PlayPauseIcon.Foreground = BlueBrush;
        TimerDisplay.Foreground  = DarkBrush;
        ResetBtn.IsEnabled       = false;
        Refresh();
    }

    private void History_Click(object sender, RoutedEventArgs e)
        => ((App)WpfApplication.Current).ShowHistory();

    private void DragBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed) DragMove();
    }

    private void HideWindow_Click(object sender, RoutedEventArgs e) => Hide();

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }
}
