using System.ComponentModel;
using System.Text.Json.Serialization;

namespace TinyStopwatch.Models;

public class StopwatchEntry : INotifyPropertyChanged
{
    public DateTime Date { get; set; }

    private long _durationTicks;
    public long DurationTicks
    {
        get => _durationTicks;
        set
        {
            _durationTicks = value;
            OnPropertyChanged(nameof(DurationTicks));
            OnPropertyChanged(nameof(Duration));
            OnPropertyChanged(nameof(FormattedDuration));
        }
    }

    private string _note = string.Empty;
    public string Note
    {
        get => _note;
        set { _note = value ?? string.Empty; OnPropertyChanged(nameof(Note)); }
    }

    [JsonIgnore]
    public TimeSpan Duration => TimeSpan.FromTicks(DurationTicks);

    [JsonIgnore]
    public string FormattedDuration
    {
        get
        {
            var d = Duration;
            return d.TotalHours >= 1
                ? $"{(int)d.TotalHours:00}:{d.Minutes:00}:{d.Seconds:00}.{d.Milliseconds / 100}"
                : $"{d.Minutes:00}:{d.Seconds:00}.{d.Milliseconds / 100}";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
