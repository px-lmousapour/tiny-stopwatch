using System.IO;
using System.Text.Json;
using TinyStopwatch.Models;

namespace TinyStopwatch.Services;

public class HistoryService
{
    private static readonly string HistoryPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TinyStopwatch", "history.json");

    public List<StopwatchEntry> Load()
    {
        if (!File.Exists(HistoryPath)) return [];
        try
        {
            var json = File.ReadAllText(HistoryPath);
            return JsonSerializer.Deserialize<List<StopwatchEntry>>(json) ?? [];
        }
        catch { return []; }
    }

    public void AddEntry(TimeSpan duration)
    {
        var entries = Load();
        entries.Insert(0, new StopwatchEntry
        {
            Date = DateTime.Now,
            DurationTicks = duration.Ticks
        });
        if (entries.Count > 100) entries = entries[..100];
        Save(entries);
    }

    public static void Save(List<StopwatchEntry> entries)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(HistoryPath)!);
        File.WriteAllText(HistoryPath, JsonSerializer.Serialize(entries,
            new JsonSerializerOptions { WriteIndented = true }));
    }
}
