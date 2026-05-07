using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using TinyStopwatch.Services;
using WpfApplication = System.Windows.Application;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace TinyStopwatch;

public partial class MainWindow : Window
{
    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    private const uint WM_SYSCOMMAND  = 0x0112;
    private const uint SC_SIZE_RIGHT  = 0xF002; // SC_SIZE + WMSZ_RIGHT

    private readonly Stopwatch _sw = new();
    private readonly System.Windows.Threading.DispatcherTimer _timer = new()
    {
        Interval = TimeSpan.FromSeconds(1)
    };
    private bool _running;
    private readonly HistoryService _history = new();


    public MainWindow()
    {
        InitializeComponent();
        _timer.Tick += (_, _) => Refresh();
        PositionBottomRight();
    }

    private void PositionBottomRight()
    {
        var area = SystemParameters.WorkArea;
        Left = area.Right  - Width  - 14;
        Top  = area.Bottom - Height - 14;
    }

    private void Refresh()
    {
        var t = _sw.Elapsed;
        TimerDisplay.Text = t.TotalHours >= 1
            ? $"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}"
            : $"{t.Minutes:00}:{t.Seconds:00}";

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
        }
        else
        {
            _sw.Start();
            _timer.Start();
            PlayPauseIcon.Text = "⏸";
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
        ResetBtn.IsEnabled = false;
        Refresh();
    }

    private void History_Click(object sender, RoutedEventArgs e)
        => ((App)WpfApplication.Current).ShowHistory();

    private void DragBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed) DragMove();
    }

    // Grab the right edge to resize width
    private void ResizeRight_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;
        var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
        SendMessage(hwnd, WM_SYSCOMMAND, (IntPtr)SC_SIZE_RIGHT, IntPtr.Zero);
    }

    private void HideWindow_Click(object sender, RoutedEventArgs e) => Hide();

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }
}
