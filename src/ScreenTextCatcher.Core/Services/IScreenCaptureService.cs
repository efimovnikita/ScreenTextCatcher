using System.Drawing;

namespace ScreenTextCatcher.Core.Services;

public interface IScreenCaptureService
{
    Rectangle GetVirtualScreenBounds();
    Bitmap CaptureVirtualScreen();
    byte[] CaptureRegionToPngBytes(Rectangle region);
    byte[] CropToPngBytes(Bitmap source, Rectangle cropArea);
}
