using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Shell;
using WinForms = System.Windows.Forms;
using Application = System.Windows.Application;
using StartupEventArgs = System.Windows.StartupEventArgs;
using ExitEventArgs = System.Windows.ExitEventArgs;
using WindowState = System.Windows.WindowState;

namespace TinyStopwatch;

public partial class App : Application
{
    [DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr hIcon);

    private WinForms.NotifyIcon? _trayIcon;
    private MainWindow? _mainWindow;
    private System.Drawing.Icon? _trayIconObj;

    protected override void OnStartup(StartupEventArgs e)
    {
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
        const int sz = 32;
        using var bmp = new Bitmap(sz, sz, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            var cyan = Color.FromArgb(0, 212, 255);
            using var pen = new System.Drawing.Pen(cyan, 2f);
            using var brush = new SolidBrush(cyan);

            // Stopwatch body
            g.DrawEllipse(pen, 3, 6, 26, 24);
            // Hour and minute hands
            g.DrawLine(pen, 16, 18, 16, 12);
            g.DrawLine(pen, 16, 18, 22, 18);
            // Crown / stem
            g.FillRectangle(brush, 13, 3, 6, 3);
            // Start button (left side bump)
            g.DrawArc(pen, 1, 11, 4, 5, 90, 180);
        }

        var hIcon = bmp.GetHicon();
        var icon = (System.Drawing.Icon)System.Drawing.Icon.FromHandle(hIcon).Clone();
        DestroyIcon(hIcon);
        return icon;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        _trayIconObj?.Dispose();
        base.OnExit(e);
    }
}
