using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TinyStopwatch.Models;
using TinyStopwatch.Services;

namespace TinyStopwatch;

public partial class HistoryWindow : Window
{
    private readonly HistoryService _service = new();
    private ObservableCollection<StopwatchEntry> _entries = [];

    public HistoryWindow()
    {
        InitializeComponent();
        LoadHistory();
    }

    private void LoadHistory()
    {
        _entries = new ObservableCollection<StopwatchEntry>(_service.Load());
        HistoryList.ItemsSource = _entries;
        UpdateEmptyState();
    }

    private void DeleteEntry_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button { DataContext: StopwatchEntry entry })
        {
            _entries.Remove(entry);
            HistoryService.Save([.. _entries]);
            UpdateEmptyState();
        }
    }

    private void UpdateEmptyState()
    {
        EmptyPanel.Visibility  = _entries.Count == 0 ? Visibility.Visible  : Visibility.Collapsed;
        ListScroller.Visibility = _entries.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    private void DurationTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.TextBox tb) return;
        if (tb.DataContext is not StopwatchEntry entry) return;

        if (TryParseDuration(tb.Text, out long ticks))
        {
            entry.DurationTicks = ticks;
            HistoryService.Save([.. _entries]);
        }
        else
        {
            // Revert to the stored value
            tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty)?.UpdateTarget();
        }
    }

    private void DurationTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter && sender is System.Windows.Controls.TextBox tb)
        {
            tb.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && sender is System.Windows.Controls.TextBox tb2)
        {
            tb2.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty)?.UpdateTarget();
            tb2.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            e.Handled = true;
        }
    }

    // Accepts: MM:SS  MM:SS.d  HH:MM:SS  HH:MM:SS.d
    private static bool TryParseDuration(string text, out long ticks)
    {
        ticks = 0;
        var parts = text.Trim().Split(':');
        try
        {
            int hours = 0, minutes, seconds, deci = 0;
            if (parts.Length == 2)
            {
                minutes = int.Parse(parts[0]);
                var sp = parts[1].Split('.');
                seconds = int.Parse(sp[0]);
                if (sp.Length > 1) deci = int.Parse(sp[1][..1]);
            }
            else if (parts.Length == 3)
            {
                hours   = int.Parse(parts[0]);
                minutes = int.Parse(parts[1]);
                var sp = parts[2].Split('.');
                seconds = int.Parse(sp[0]);
                if (sp.Length > 1) deci = int.Parse(sp[1][..1]);
            }
            else return false;

            if (minutes >= 60 || seconds >= 60) return false;
            ticks = (new TimeSpan(hours, minutes, seconds) + TimeSpan.FromMilliseconds(deci * 100)).Ticks;
            return true;
        }
        catch { return false; }
    }

    private void NoteTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        HistoryService.Save([.. _entries]);
    }

    private void NoteTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter && sender is System.Windows.Controls.TextBox tb)
        {
            tb.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            e.Handled = true;
        }
    }

    private void Header_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed) DragMove();
    }

    private void CloseWindow_Click(object sender, RoutedEventArgs e) => Close();
}
