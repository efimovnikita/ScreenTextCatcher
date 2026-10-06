using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Input;

namespace ScreenTextCatcher.Tests;

public class CursorTests
{
    public static System.Windows.Input.Cursor CreateRedCrosshairCursor()
    {
        int size = 33;
        int hotspotX = 16;
        int hotspotY = 16;

        using var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.Transparent);

            // Red color: #FF1744
            var redColor = Color.FromArgb(255, 255, 23, 68);
            var blackColor = Color.FromArgb(200, 0, 0, 0);

            using var redPen = new Pen(redColor, 2f);
            using var blackPen = new Pen(blackColor, 1f);

            // Black outline for high contrast on any background
            // Horizontal outline
            g.DrawLine(blackPen, 4, hotspotY - 1, size - 5, hotspotY - 1);
            g.DrawLine(blackPen, 4, hotspotY + 2, size - 5, hotspotY + 2);
            g.DrawLine(blackPen, 3, hotspotY, 3, hotspotY + 1);
            g.DrawLine(blackPen, size - 4, hotspotY, size - 4, hotspotY + 1);

            // Vertical outline
            g.DrawLine(blackPen, hotspotX - 1, 4, hotspotX - 1, size - 5);
            g.DrawLine(blackPen, hotspotX + 2, 4, hotspotX + 2, size - 5);
            g.DrawLine(blackPen, hotspotX, 3, hotspotX + 1, 3);
            g.DrawLine(blackPen, hotspotX, size - 4, hotspotX + 1, size - 4);

            // Main red crosshair lines (2px thick)
            g.DrawLine(redPen, 4, hotspotY, size - 5, hotspotY);
            g.DrawLine(redPen, hotspotX, 4, hotspotX, size - 5);
        }

        // Convert to .CUR file bytes
        using var pngMs = new MemoryStream();
        bmp.Save(pngMs, ImageFormat.Png);
        var pngBytes = pngMs.ToArray();

        using var curMs = new MemoryStream();
        using var bw = new BinaryWriter(curMs);

        // CUR Header
        bw.Write((short)0); // reserved
        bw.Write((short)2); // 2 = CURSOR (1 = ICON)
        bw.Write((short)1); // 1 image

        // CUR Directory Entry
        bw.Write((byte)size);       // width
        bw.Write((byte)size);       // height
        bw.Write((byte)0);          // colors
        bw.Write((byte)0);          // reserved
        bw.Write((short)hotspotX);  // X hotspot
        bw.Write((short)hotspotY);  // Y hotspot
        bw.Write((int)pngBytes.Length); // size of image
        bw.Write((int)22);          // offset of image (6 + 16 = 22)

        // Write image data
        bw.Write(pngBytes);
        bw.Flush();

        curMs.Position = 0;
        return new System.Windows.Input.Cursor(curMs);
    }

    [Fact]
    public void CreateRedCrosshairCursor_CreatesValidCursor()
    {
        var cursor = CreateRedCrosshairCursor();
        Assert.NotNull(cursor);
    }
}
