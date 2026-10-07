using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace ScreenTextCatcher.Core.Helpers;

public static class ImageConversionHelper {
  public static BitmapSource ToBitmapSource(Bitmap bitmap) {
    ArgumentNullException.ThrowIfNull(bitmap);

    using var ms = new MemoryStream();
    bitmap.Save(ms, ImageFormat.Png);
    ms.Position = 0;

    var bitmapImage = new BitmapImage();
    bitmapImage.BeginInit();
    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
    bitmapImage.StreamSource = ms;
    bitmapImage.EndInit();
    bitmapImage.Freeze();

    return bitmapImage;
  }
}
