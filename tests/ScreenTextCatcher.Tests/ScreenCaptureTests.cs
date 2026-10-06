using System.Drawing;
using System.IO;
using FluentAssertions;
using ScreenTextCatcher.Core.Services;

namespace ScreenTextCatcher.Tests;

public class ScreenCaptureTests
{
    [Fact]
    public void CropToPngBytes_ValidRegion_ReturnsValidPngImageWithExpectedDimensions()
    {
        var service = new ScreenCaptureService();

        using var source = new Bitmap(200, 200);
        using (var g = Graphics.FromImage(source))
        {
            g.Clear(Color.Blue);
            g.FillRectangle(Brushes.Red, new Rectangle(50, 50, 60, 40));
        }

        var cropRect = new Rectangle(50, 50, 60, 40);
        var pngBytes = service.CropToPngBytes(source, cropRect);

        pngBytes.Should().NotBeNull();
        pngBytes.Length.Should().BeGreaterThan(0);

        // Verify PNG magic header: 0x89 0x50 0x4E 0x47
        pngBytes[0].Should().Be(0x89);
        pngBytes[1].Should().Be(0x50); // P
        pngBytes[2].Should().Be(0x4E); // N
        pngBytes[3].Should().Be(0x47); // G

        using var ms = new MemoryStream(pngBytes);
        using var croppedBmp = new Bitmap(ms);

        croppedBmp.Width.Should().Be(60);
        croppedBmp.Height.Should().Be(40);
    }

    [Fact]
    public void CropToPngBytes_WhenRectOutOfBounds_ClampsToBitmapBounds()
    {
        var service = new ScreenCaptureService();

        using var source = new Bitmap(100, 100);
        var outOfBoundsRect = new Rectangle(80, 80, 50, 50); // exceeds 100x100

        var pngBytes = service.CropToPngBytes(source, outOfBoundsRect);

        pngBytes.Should().NotBeEmpty();

        using var ms = new MemoryStream(pngBytes);
        using var croppedBmp = new Bitmap(ms);

        croppedBmp.Width.Should().Be(20); // 100 - 80
        croppedBmp.Height.Should().Be(20); // 100 - 80
    }

    [Fact]
    public void GetVirtualScreenBounds_ReturnsNonZeroDimensions()
    {
        var service = new ScreenCaptureService();
        var bounds = service.GetVirtualScreenBounds();

        bounds.Width.Should().BeGreaterThan(0);
        bounds.Height.Should().BeGreaterThan(0);
    }
}
