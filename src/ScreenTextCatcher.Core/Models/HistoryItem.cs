namespace ScreenTextCatcher.Core.Models;

public record HistoryItem(
    long Id,
    DateTime Timestamp,
    string Text
)
{
    public string Preview => Text.Length > 60
        ? Text.Substring(0, 57).Replace("\r", " ").Replace("\n", " ") + "..."
        : Text.Replace("\r", " ").Replace("\n", " ");
}
