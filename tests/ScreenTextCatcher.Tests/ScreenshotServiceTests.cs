using System.IO;
using FluentAssertions;
using ScreenTextCatcher.Core.Models;
using ScreenTextCatcher.Core.Services;

namespace ScreenTextCatcher.Tests;

public class ScreenshotServiceTests : IDisposable
{
    private readonly string _tempFolder;

    public ScreenshotServiceTests()
    {
        _tempFolder = Path.Combine(Path.GetTempPath(), "ScreenTextCatcher_ScreenshotTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempFolder);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempFolder))
        {
            Directory.Delete(_tempFolder, true);
        }
    }

    [Fact]
    public void SaveScreenshot_CreatesDirectoryAndFileWithPngBytes()
    {
        var targetSubfolder = Path.Combine(_tempFolder, "Nested", "Folder");
        var fixedTime = new DateTime(2026, 10, 7, 14, 30, 15);
        var service = new ScreenshotService(timeProvider: () => fixedTime);
        var fakePng = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        var savedPath = service.SaveScreenshot(fakePng, targetSubfolder);

        File.Exists(savedPath).Should().BeTrue();
        Path.GetFileName(savedPath).Should().Be("Screenshot_20261007_143015.png");
        File.ReadAllBytes(savedPath).Should().Equal(fakePng);
    }

    [Fact]
    public void SaveScreenshot_HandlesFileNameCollisionGracefully()
    {
        var fixedTime = new DateTime(2026, 10, 7, 14, 30, 15);
        var service = new ScreenshotService(timeProvider: () => fixedTime);
        var fakePng = new byte[] { 1, 2, 3 };

        // Save first time
        var path1 = service.SaveScreenshot(fakePng, _tempFolder);
        Path.GetFileName(path1).Should().Be("Screenshot_20261007_143015.png");

        // Save second time with same timestamp
        var path2 = service.SaveScreenshot(fakePng, _tempFolder);
        Path.GetFileName(path2).Should().Be("Screenshot_20261007_143015_1.png");

        // Save third time with same timestamp
        var path3 = service.SaveScreenshot(fakePng, _tempFolder);
        Path.GetFileName(path3).Should().Be("Screenshot_20261007_143015_2.png");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\r\n\t  ")]
    public void CalculateUpdatedClipboardText_EmptyOrWhitespace_ReturnsSinglePath(string? clipboardText)
    {
        var service = new ScreenshotService();
        var newFile = Path.Combine(_tempFolder, "Screenshot_20261007_120000.png");

        var result = service.CalculateUpdatedClipboardText(clipboardText, newFile, _tempFolder);

        result.Should().Be(newFile);
    }

    [Fact]
    public void CalculateUpdatedClipboardText_InvalidTextInClipboard_ResetsToSinglePath()
    {
        var service = new ScreenshotService();
        var newFile = Path.Combine(_tempFolder, "Screenshot_20261007_120000.png");

        var result = service.CalculateUpdatedClipboardText("Just some random text copied by user", newFile, _tempFolder);

        result.Should().Be(newFile);
    }

    [Fact]
    public void CalculateUpdatedClipboardText_ExternalFileInClipboard_ResetsToSinglePath()
    {
        var service = new ScreenshotService();
        var externalFile = Path.Combine(Path.GetTempPath(), "external.png");
        File.WriteAllText(externalFile, "dummy");

        try
        {
            var newFile = Path.Combine(_tempFolder, "Screenshot_20261007_120000.png");
            var result = service.CalculateUpdatedClipboardText(externalFile, newFile, _tempFolder);

            result.Should().Be(newFile);
        }
        finally
        {
            if (File.Exists(externalFile))
            {
                File.Delete(externalFile);
            }
        }
    }

    [Fact]
    public void CalculateUpdatedClipboardText_NonExistentFileInFolder_ResetsToSinglePath()
    {
        var service = new ScreenshotService();
        var deletedFile = Path.Combine(_tempFolder, "deleted_screenshot.png");
        var newFile = Path.Combine(_tempFolder, "Screenshot_20261007_120000.png");

        var result = service.CalculateUpdatedClipboardText(deletedFile, newFile, _tempFolder);

        result.Should().Be(newFile);
    }

    [Fact]
    public void CalculateUpdatedClipboardText_AllExistingFilesInFolder_AppendsToEnd()
    {
        var service = new ScreenshotService();
        var file1 = Path.Combine(_tempFolder, "Screenshot_1.png");
        var file2 = Path.Combine(_tempFolder, "Screenshot_2.png");
        var file3 = Path.Combine(_tempFolder, "Screenshot_3.png");

        File.WriteAllText(file1, "dummy1");
        File.WriteAllText(file2, "dummy2");

        var clipboard = $"{file1}\r\n{file2}";
        var result = service.CalculateUpdatedClipboardText(clipboard, file3, _tempFolder);

        var lines = result.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        lines.Should().HaveCount(3);
        lines[0].Should().Be(file1);
        lines[1].Should().Be(file2);
        lines[2].Should().Be(file3);
    }

    [Fact]
    public void SaveAndAccumulateClipboard_SavesFileAndUpdatesClipboard()
    {
        string? capturedClipboard = null;
        var fixedTime = new DateTime(2026, 10, 7, 10, 0, 0);

        var service = new ScreenshotService(
            timeProvider: () => fixedTime,
            clipboardSetter: text => capturedClipboard = text,
            clipboardGetter: () => capturedClipboard);

        var fakePng = new byte[] { 10, 20, 30 };
        var savedPath = service.SaveAndAccumulateClipboard(fakePng, _tempFolder);

        File.Exists(savedPath).Should().BeTrue();
        capturedClipboard.Should().Be(savedPath);

        // Next screenshot
        var fixedTime2 = new DateTime(2026, 10, 7, 10, 0, 5);
        var service2 = new ScreenshotService(
            timeProvider: () => fixedTime2,
            clipboardSetter: text => capturedClipboard = text,
            clipboardGetter: () => capturedClipboard);

        var savedPath2 = service2.SaveAndAccumulateClipboard(fakePng, _tempFolder);
        File.Exists(savedPath2).Should().BeTrue();
        capturedClipboard.Should().Be($"{savedPath}{Environment.NewLine}{savedPath2}");
    }

    [Fact]
    public void CalculateUpdatedClipboardText_SpaceDelimiter_AllExistingFiles_AppendsSeparatedBySpace()
    {
        var service = new ScreenshotService();
        var file1 = Path.Combine(_tempFolder, "Screenshot_1.png");
        var file2 = Path.Combine(_tempFolder, "Screenshot_2.png");
        var file3 = Path.Combine(_tempFolder, "Screenshot_3.png");

        File.WriteAllText(file1, "dummy1");
        File.WriteAllText(file2, "dummy2");

        var clipboard = $"{file1} {file2}";
        var result = service.CalculateUpdatedClipboardText(clipboard, file3, _tempFolder, ClipboardDelimiter.Space);

        result.Should().Be($"{file1} {file2} {file3}");
    }

    [Fact]
    public void SaveAndAccumulateClipboard_SpaceDelimiter_AccumulatesWithSpaces()
    {
        string? capturedClipboard = null;
        var fixedTime1 = new DateTime(2026, 10, 7, 10, 0, 0);

        var service1 = new ScreenshotService(
            timeProvider: () => fixedTime1,
            clipboardSetter: text => capturedClipboard = text,
            clipboardGetter: () => capturedClipboard);

        var fakePng = new byte[] { 1, 2, 3 };
        var path1 = service1.SaveAndAccumulateClipboard(fakePng, _tempFolder, ClipboardDelimiter.Space);
        capturedClipboard.Should().Be(path1);

        var fixedTime2 = new DateTime(2026, 10, 7, 10, 0, 10);
        var service2 = new ScreenshotService(
            timeProvider: () => fixedTime2,
            clipboardSetter: text => capturedClipboard = text,
            clipboardGetter: () => capturedClipboard);

        var path2 = service2.SaveAndAccumulateClipboard(fakePng, _tempFolder, ClipboardDelimiter.Space);
        capturedClipboard.Should().Be($"{path1} {path2}");
    }

    [Fact]
    public void SaveAndCopyImageToClipboard_SavesFileAndInvokesImageClipboardSetter()
    {
        byte[]? capturedImageBytes = null;
        var fixedTime = new DateTime(2026, 10, 7, 15, 0, 0);

        var service = new ScreenshotService(
            timeProvider: () => fixedTime,
            imageClipboardSetter: bytes => capturedImageBytes = bytes);

        var fakePng = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x01, 0x02, 0x03, 0x04 };
        var savedPath = service.SaveAndCopyImageToClipboard(fakePng, _tempFolder);

        File.Exists(savedPath).Should().BeTrue();
        Path.GetFileName(savedPath).Should().Be("Screenshot_20261007_150000.png");
        File.ReadAllBytes(savedPath).Should().Equal(fakePng);
        capturedImageBytes.Should().NotBeNull();
        capturedImageBytes.Should().Equal(fakePng);
    }

    [Fact]
    public void SaveAndCopyImageToClipboard_HandlesCollisionAndReturnsUniquePath()
    {
        byte[]? capturedImageBytes = null;
        var fixedTime = new DateTime(2026, 10, 7, 15, 0, 0);

        var service = new ScreenshotService(
            timeProvider: () => fixedTime,
            imageClipboardSetter: bytes => capturedImageBytes = bytes);

        var fakePng = new byte[] { 10, 20, 30 };
        var path1 = service.SaveAndCopyImageToClipboard(fakePng, _tempFolder);
        var path2 = service.SaveAndCopyImageToClipboard(fakePng, _tempFolder);

        Path.GetFileName(path1).Should().Be("Screenshot_20261007_150000.png");
        Path.GetFileName(path2).Should().Be("Screenshot_20261007_150000_1.png");
        File.Exists(path1).Should().BeTrue();
        File.Exists(path2).Should().BeTrue();
        capturedImageBytes.Should().Equal(fakePng);
    }
}


