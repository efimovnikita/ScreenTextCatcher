using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ScreenTextCatcher.Core.Helpers;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Drawing.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace ScreenTextCatcher.Tests;

public class AnnotationExportTests {
  private static void RunInSta(Action action) {
    Exception? ex = null;
    var thread = new Thread(() => {
      try {
        action();
      } catch (Exception e) {
        ex = e;
      }
    });
    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();
    thread.Join();
    if (ex != null) {
      System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
    }
  }

  [Fact]
  public void RenderVisualToPng_ValidCanvas_ReturnsValidPngBytes() {
    RunInSta(() => {
      var canvas = new Canvas {
        Width = 200,
        Height = 150,
        Background = Brushes.White
      };

      var rect = new Rectangle {
        Width = 50,
        Height = 50,
        Fill = Brushes.Red
      };
      Canvas.SetLeft(rect, 20);
      Canvas.SetTop(rect, 20);
      canvas.Children.Add(rect);

      canvas.Measure(new System.Windows.Size(200, 150));
      canvas.Arrange(new Rect(0, 0, 200, 150));

      var bytes = AnnotationExportHelper.RenderVisualToPng(canvas, 200, 150, 96.0, 96.0);

      Assert.NotNull(bytes);
      Assert.True(bytes.Length > 0);
      Assert.Equal(0x89, bytes[0]);
      Assert.Equal((byte)'P', bytes[1]);
      Assert.Equal((byte)'N', bytes[2]);
      Assert.Equal((byte)'G', bytes[3]);
    });
  }

  [Fact]
  public void CompositeAndRenderToPng_WithBackgroundAndAnnotations_ProducesNonEmptyImageWithAnnotations() {
    RunInSta(() => {
      // 1. Create 100x100 white background bitmap
      using var bmp = new Bitmap(100, 100, PixelFormat.Format32bppArgb);
      using (var g = Graphics.FromImage(bmp)) {
        g.Clear(Color.White);
      }
      var bgSource = ImageConversionHelper.ToBitmapSource(bmp);

      // 2. Create red annotation rectangle
      var rect = AnnotationGeometryHelper.CreateRectangle(new Point(10, 10), new Point(60, 60));
      var annotations = new List<UIElement> { rect };

      // 3. Composite and render
      var bytes = AnnotationExportHelper.CompositeAndRenderToPng(
          bgSource,
          annotations,
          100,
          100,
          1.0,
          1.0);

      Assert.NotNull(bytes);
      Assert.True(bytes.Length > 0);

      // 4. Verify decoded pixels: check that background is white and annotation line is red
      using var ms = new MemoryStream(bytes);
      using var decoded = new Bitmap(ms);

      Assert.Equal(100, decoded.Width);
      Assert.Equal(100, decoded.Height);

      // Check pixel at (10, 10) - border of red rectangle
      var borderPixel = decoded.GetPixel(10, 10);
      Assert.Equal(255, borderPixel.A);
      Assert.True(borderPixel.R > 200, $"Expected Red > 200, got {borderPixel.R}");

      // Check pixel at (90, 90) - white background outside rectangle
      var bgPixel = decoded.GetPixel(90, 90);
      Assert.Equal(255, bgPixel.A);
      Assert.True(bgPixel.R > 240 && bgPixel.G > 240 && bgPixel.B > 240,
          $"Expected White, got R={bgPixel.R}, G={bgPixel.G}, B={bgPixel.B}");
    });
  }
}
