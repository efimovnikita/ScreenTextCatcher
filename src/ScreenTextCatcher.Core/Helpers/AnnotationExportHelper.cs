using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;
using Rect = System.Windows.Rect;
using Size = System.Windows.Size;
using UIElement = System.Windows.UIElement;

namespace ScreenTextCatcher.Core.Helpers;

public static class AnnotationExportHelper {
  public static byte[] RenderVisualToPng(
      Visual visual,
      int pixelWidth,
      int pixelHeight,
      double dpiX = 96.0,
      double dpiY = 96.0) {
    ArgumentNullException.ThrowIfNull(visual);

    if (pixelWidth <= 0 || pixelHeight <= 0) {
      return Array.Empty<byte>();
    }

    var rtb = new RenderTargetBitmap(
        pixelWidth,
        pixelHeight,
        dpiX,
        dpiY,
        PixelFormats.Pbgra32);

    rtb.Render(visual);

    var encoder = new PngBitmapEncoder();
    encoder.Frames.Add(BitmapFrame.Create(rtb));

    using var ms = new MemoryStream();
    encoder.Save(ms);
    return ms.ToArray();
  }

  public static byte[] CompositeAndRenderToPng(
      BitmapSource backgroundSource,
      IEnumerable<UIElement> annotations,
      double widthDips,
      double heightDips,
      double dpiScaleX = 1.0,
      double dpiScaleY = 1.0) {
    ArgumentNullException.ThrowIfNull(backgroundSource);
    ArgumentNullException.ThrowIfNull(annotations);

    int pixelW = (int)Math.Max(1, Math.Round(widthDips * dpiScaleX));
    int pixelH = (int)Math.Max(1, Math.Round(heightDips * dpiScaleY));

    var exportCanvas = new Canvas {
      Width = widthDips,
      Height = heightDips,
      ClipToBounds = true
    };

    var bgImage = new Image {
      Source = backgroundSource,
      Width = widthDips,
      Height = heightDips,
      Stretch = Stretch.Fill
    };
    exportCanvas.Children.Add(bgImage);

    foreach (var element in annotations) {
      exportCanvas.Children.Add(element);
    }

    exportCanvas.Measure(new Size(widthDips, heightDips));
    exportCanvas.Arrange(new Rect(0, 0, widthDips, heightDips));
    exportCanvas.UpdateLayout();

    return RenderVisualToPng(
        exportCanvas,
        pixelW,
        pixelH,
        96.0 * dpiScaleX,
        96.0 * dpiScaleY);
  }
}
