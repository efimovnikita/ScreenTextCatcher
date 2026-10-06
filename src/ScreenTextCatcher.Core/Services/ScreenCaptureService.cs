using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace ScreenTextCatcher.Core.Services;

public class ScreenCaptureService : IScreenCaptureService
{
    private const int SM_XVIRTUALSCREEN = 76;
    private const int SM_YVIRTUALSCREEN = 77;
    private const int SM_CXVIRTUALSCREEN = 78;
    private const int SM_CYVIRTUALSCREEN = 79;

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    public Rectangle GetVirtualScreenBounds()
    {
        var x = GetSystemMetrics(SM_XVIRTUALSCREEN);
        var y = GetSystemMetrics(SM_YVIRTUALSCREEN);
        var width = GetSystemMetrics(SM_CXVIRTUALSCREEN);
        var height = GetSystemMetrics(SM_CYVIRTUALSCREEN);

        // Fallback for non-standard environments
        if (width <= 0 || height <= 0)
        {
            width = 1920;
            height = 1080;
        }

        return new Rectangle(x, y, width, height);
    }

    public Bitmap CaptureVirtualScreen()
    {
        var bounds = GetVirtualScreenBounds();
        var bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);

        using (var g = Graphics.FromImage(bitmap))
        {
            g.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
        }

        return bitmap;
    }

    public byte[] CaptureRegionToPngBytes(Rectangle region)
    {
        if (region.Width <= 0 || region.Height <= 0)
        {
            return Array.Empty<byte>();
        }

        using var bitmap = new Bitmap(region.Width, region.Height, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.CopyFromScreen(region.X, region.Y, 0, 0, region.Size, CopyPixelOperation.SourceCopy);
        }

        using var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }

    public byte[] CropToPngBytes(Bitmap source, Rectangle cropArea)
    {
        ArgumentNullException.ThrowIfNull(source);

        var clampedX = Math.Max(0, cropArea.X);
        var clampedY = Math.Max(0, cropArea.Y);
        var clampedWidth = Math.Min(source.Width - clampedX, cropArea.Width - (clampedX - cropArea.X));
        var clampedHeight = Math.Min(source.Height - clampedY, cropArea.Height - (clampedY - cropArea.Y));

        if (clampedWidth <= 0 || clampedHeight <= 0)
        {
            return Array.Empty<byte>();
        }

        var clampedRect = new Rectangle(clampedX, clampedY, clampedWidth, clampedHeight);

        using var cropped = source.Clone(clampedRect, source.PixelFormat);
        using var ms = new MemoryStream();
        cropped.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }
}
