using System.Drawing;
using System.IO;
using Xunit;

namespace ScreenTextCatcher.Tests;

public class AssetAndIconTests
{
    private static string FindAssetsDirectory()
    {
        // Search upwards from current test run directory to locate src/ScreenTextCatcher/Assets
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(current))
        {
            var candidate = Path.Combine(current, "src", "ScreenTextCatcher", "Assets");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            var candidateRoot = Path.Combine(current, "Assets");
            if (Directory.Exists(candidateRoot) && File.Exists(Path.Combine(candidateRoot, "app.ico")))
            {
                return candidateRoot;
            }

            var parent = Directory.GetParent(current);
            if (parent == null) break;
            current = parent.FullName;
        }

        throw new DirectoryNotFoundException("Could not find ScreenTextCatcher Assets directory.");
    }

    [Theory]
    [InlineData("app.ico")]
    [InlineData("tray_idle.ico")]
    [InlineData("tray_busy.ico")]
    [InlineData("tray_error.ico")]
    [InlineData("app.png")]
    public void AssetFiles_ExistInAssetsDirectory(string fileName)
    {
        var assetsDir = FindAssetsDirectory();
        var filePath = Path.Combine(assetsDir, fileName);

        Assert.True(File.Exists(filePath), $"Asset file {fileName} should exist at {filePath}");
        var fileInfo = new FileInfo(filePath);
        Assert.True(fileInfo.Length > 0, $"Asset file {fileName} should not be empty");
    }

    [Theory]
    [InlineData("app.ico")]
    [InlineData("tray_idle.ico")]
    [InlineData("tray_busy.ico")]
    [InlineData("tray_error.ico")]
    public void IcoAssets_AreLoadableAsSystemDrawingIcons(string icoFileName)
    {
        var assetsDir = FindAssetsDirectory();
        var filePath = Path.Combine(assetsDir, icoFileName);

        using var icon = new Icon(filePath);
        Assert.True(icon.Width > 0, $"{icoFileName} Width should be > 0");
        Assert.True(icon.Height > 0, $"{icoFileName} Height should be > 0");
    }

    [Fact]
    public void AppIco_ContainsMultipleResolutions()
    {
        var assetsDir = FindAssetsDirectory();
        var filePath = Path.Combine(assetsDir, "app.ico");

        using var stream = File.OpenRead(filePath);
        using var reader = new BinaryReader(stream);

        var reserved = reader.ReadUInt16();
        var type = reader.ReadUInt16();
        var count = reader.ReadUInt16();

        Assert.Equal(0, reserved);
        Assert.Equal(1, type); // 1 = ICO
        Assert.True(count >= 4, $"app.ico should contain at least 4 resolution layers, found {count}");
    }

    [Fact]
    public void CsProj_ContainsApplicationIconAndVersion()
    {
        var assetsDir = FindAssetsDirectory();
        var csprojPath = Path.Combine(Path.GetDirectoryName(assetsDir)!, "ScreenTextCatcher.csproj");

        Assert.True(File.Exists(csprojPath), $"ScreenTextCatcher.csproj should exist at {csprojPath}");
        var content = File.ReadAllText(csprojPath);

        Assert.Contains("<ApplicationIcon>Assets\\app.ico</ApplicationIcon>", content);
        Assert.Contains("<Version>1.3.0</Version>", content);
    }
}
