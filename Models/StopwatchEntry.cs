using System.Text.Json.Serialization;

namespace TinyStopwatch.Models;

public class StopwatchEntry
{
    public DateTime Date { get; set; }
    public long DurationTicks { get; set; }

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
}
