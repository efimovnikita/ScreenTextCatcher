using System.Drawing;
using System.Drawing.Imaging;
using ScreenTextCatcher.Core.Helpers;

namespace ScreenTextCatcher.Tests;

public class ImageConversionTests {
  [Fact]
  public void ToBitmapSource_ConvertsValidBitmap_ReturnsFrozenBitmapSource() {
    using var bmp = new Bitmap(50, 40, PixelFormat.Format32bppArgb);
    using (var g = Graphics.FromImage(bmp)) {
      g.Clear(Color.Blue);
    }

    var source = ImageConversionHelper.ToBitmapSource(bmp);

    Assert.NotNull(source);
    Assert.Equal(50, source.PixelWidth);
    Assert.Equal(40, source.PixelHeight);
    Assert.True(source.IsFrozen);
  }
}
