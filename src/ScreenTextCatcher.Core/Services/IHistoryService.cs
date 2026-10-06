using ScreenTextCatcher.Core.Models;

namespace ScreenTextCatcher.Core.Services;

public interface IHistoryService
{
    void Add(string text);
    IReadOnlyList<HistoryItem> GetRecent(int limit = 100);
    void Clear();
    int Count();
}
