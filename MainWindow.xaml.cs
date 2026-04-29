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

    private static readonly SolidColorBrush CyanBrush   = new(MediaColor.FromRgb(0, 212, 255));
    private static readonly SolidColorBrush YellowBrush = new(MediaColor.FromRgb(255, 200, 0));

    public MainWindow()
    {
        InitializeComponent();
        _timer.Tick += (_, _) => Refresh();
        PositionNearTaskbar();
    }

    private void PositionNearTaskbar()
    {
        var area = SystemParameters.WorkArea;
        Left = area.Right  - Width  - 14;
        Top  = area.Bottom - Height - 14;
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
            PlayPauseIcon.Text = "▶";
            PlayPauseIcon.Foreground = CyanBrush;
        }
        else
        {
            _sw.Start();
            _timer.Start();
            PlayPauseIcon.Text = "⏸";
            PlayPauseIcon.Foreground = YellowBrush;
            ResetBtn.IsEnabled = true;
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

        PlayPauseIcon.Text = "▶";
        PlayPauseIcon.Foreground = CyanBrush;
        ResetBtn.IsEnabled = false;
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
