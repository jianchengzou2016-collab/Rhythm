namespace Rhythm.App.Models;

public sealed record HistoryListItem(
    string StartedAt,
    string Result,
    string Planned,
    string Actual);
