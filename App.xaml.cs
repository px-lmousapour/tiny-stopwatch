using System.Threading;
using System.Windows.Shell;
using WinForms = System.Windows.Forms;
using Application = System.Windows.Application;
using StartupEventArgs = System.Windows.StartupEventArgs;
using ExitEventArgs = System.Windows.ExitEventArgs;
using WindowState = System.Windows.WindowState;

namespace TinyStopwatch;

public partial class App : Application
{
    private WinForms.NotifyIcon? _trayIcon;
    private MainWindow? _mainWindow;
    private System.Drawing.Icon? _trayIconObj;
    private Mutex? _instanceMutex;
    private EventWaitHandle? _activateEvent;

    protected override void OnStartup(StartupEventArgs e)
    {
        _instanceMutex = new Mutex(true, "TinyStopwatch_SingleInstance", out bool isFirst);
        if (!isFirst)
        {
            // Signal the already-running instance to show itself, then quit.
            try { EventWaitHandle.OpenExisting("TinyStopwatch_Activate").Set(); } catch { }
            Shutdown();
            return;
        }

        // Primary instance: listen for activation signals from future launches.
        _activateEvent = new EventWaitHandle(false, EventResetMode.AutoReset, "TinyStopwatch_Activate");
        new Thread(() =>
        {
            while (_activateEvent?.WaitOne() == true)
                Dispatcher.Invoke(BringToFront);
        }) { IsBackground = true }.Start();

        base.OnStartup(e);

        SetupJumpList();
        _mainWindow = new MainWindow();

        if (e.Args.Contains("--history"))
        {
            new HistoryWindow().Show();
        }
        else
        {
            _mainWindow.Show();
        }

        InitializeTray();
    }

    private void BringToFront()
    {
        if (_mainWindow == null) return;
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    private void SetupJumpList()
    {
        var jumpList = new JumpList();
        jumpList.ShowFrequentCategory = false;
        jumpList.ShowRecentCategory = false;
        jumpList.JumpItems.Add(new JumpTask
        {
            Title = "View History",
            Arguments = "--history",
            Description = "View all recorded stopwatch sessions"
        });
        JumpList.SetJumpList(this, jumpList);
        jumpList.Apply();
    }

    private void InitializeTray()
    {
        _trayIconObj = CreateTrayIcon();
        _trayIcon = new WinForms.NotifyIcon
        {
            Icon = _trayIconObj,
            Text = "Tiny Stopwatch",
            Visible = true
        };

        var menu = new WinForms.ContextMenuStrip();
        menu.Items.Add("Show / Hide", null, (_, _) => ToggleMainWindow());
        menu.Items.Add(new WinForms.ToolStripSeparator());
        menu.Items.Add("History", null, (_, _) => ShowHistory());
        menu.Items.Add(new WinForms.ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => ExitApp());

        _trayIcon.ContextMenuStrip = menu;
        _trayIcon.DoubleClick += (_, _) => ToggleMainWindow();
    }

    internal void UpdateTrayTooltip(string time)
    {
        if (_trayIcon != null)
            _trayIcon.Text = $"Tiny Stopwatch  {time}";
    }

    internal void ShowHistory()
    {
        Dispatcher.Invoke(() => new HistoryWindow().Show());
    }

    private void ToggleMainWindow()
    {
        Dispatcher.Invoke(() =>
        {
            if (_mainWindow == null) return;
            if (_mainWindow.IsVisible)
            {
                _mainWindow.Hide();
            }
            else
            {
                _mainWindow.Show();
                _mainWindow.WindowState = WindowState.Normal;
                _mainWindow.Activate();
            }
        });
    }

    private void ExitApp()
    {
        if (_trayIcon != null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }
        Shutdown();
    }

    private static System.Drawing.Icon CreateTrayIcon()
    {
        // Load the real stopwatch icon from the embedded resource
        var uri = new Uri("pack://application:,,,/stopwatch.ico");
        using var stream = System.Windows.Application.GetResourceStream(uri)!.Stream;
        return new System.Drawing.Icon(stream, new System.Drawing.Size(32, 32));
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        _trayIconObj?.Dispose();
        _activateEvent?.Close();
        _instanceMutex?.ReleaseMutex();
        _instanceMutex?.Dispose();
        base.OnExit(e);
    }
}
