using System.IO;
using FluentAssertions;
using ScreenTextCatcher.Core.Services;

namespace ScreenTextCatcher.Tests;

public class HistoryServiceTests : IDisposable
{
    private readonly string _tempDbPath;

    public HistoryServiceTests()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "ScreenTextCatcher_DbTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        _tempDbPath = Path.Combine(tempDir, "history.db");
    }

    public void Dispose()
    {
        var dir = Path.GetDirectoryName(_tempDbPath);
        if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
        {
            try
            {
                Directory.Delete(dir, true);
            }
            catch
            {
                // Ignore cleanup lock
            }
        }
    }

    [Fact]
    public void Add_And_GetRecent_ReturnsItemsOrderedByNewestFirst()
    {
        var service = new HistoryService(_tempDbPath);

        service.Add("First snippet");
        service.Add("Second snippet");

        var items = service.GetRecent();
        items.Should().HaveCount(2);
        items[0].Text.Should().Be("Second snippet");
        items[1].Text.Should().Be("First snippet");
        service.Count().Should().Be(2);
    }

    [Fact]
    public void Add_WhenExceeding100Items_KeepsOnlyLatest100()
    {
        var service = new HistoryService(_tempDbPath);

        for (int i = 1; i <= 105; i++)
        {
            service.Add($"Snippet {i}");
        }

        service.Count().Should().Be(100);
        var items = service.GetRecent(100);
        items.Should().HaveCount(100);
        items.First().Text.Should().Be("Snippet 105");
        items.Last().Text.Should().Be("Snippet 6");
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        var service = new HistoryService(_tempDbPath);
        service.Add("Some text");
        service.Count().Should().Be(1);

        service.Clear();
        service.Count().Should().Be(0);
        service.GetRecent().Should().BeEmpty();
    }
}
