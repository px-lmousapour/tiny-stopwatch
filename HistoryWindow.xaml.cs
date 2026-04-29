using System.Windows;
using System.Windows.Input;
using TinyStopwatch.Services;

namespace TinyStopwatch;

public partial class HistoryWindow : Window
{
    public HistoryWindow()
    {
        InitializeComponent();
        LoadHistory();
    }

    private void LoadHistory()
    {
        var entries = new HistoryService().Load();
        if (entries.Count == 0)
        {
            EmptyPanel.Visibility = Visibility.Visible;
            ListScroller.Visibility = Visibility.Collapsed;
        }
        else
        {
            EmptyPanel.Visibility = Visibility.Collapsed;
            ListScroller.Visibility = Visibility.Visible;
            HistoryList.ItemsSource = entries;
        }
    }

    private void Header_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed) DragMove();
    }

    private void CloseWindow_Click(object sender, RoutedEventArgs e) => Close();
}
