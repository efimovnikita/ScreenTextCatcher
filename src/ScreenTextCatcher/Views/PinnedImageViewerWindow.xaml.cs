using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ScreenTextCatcher.Core.Helpers;
using ScreenTextCatcher.Core.Services;

namespace ScreenTextCatcher.Views;

public partial class PinnedImageViewerWindow : Window {
  public PinnedImageViewerWindow(byte[] pngBytes, Rect selectionRect) {
    InitializeComponent();

    using (var stream = new MemoryStream(pngBytes)) {
      var bitmap = new BitmapImage();
      bitmap.BeginInit();
      bitmap.CacheOption = BitmapCacheOption.OnLoad;
      bitmap.StreamSource = stream;
      bitmap.EndInit();
      bitmap.Freeze();
      ViewerImage.Source = bitmap;

      int pixelWidth = bitmap.PixelWidth;
      int pixelHeight = bitmap.PixelHeight;

      string titleTemplate = LocalizationManager.GetString(
          "Loc_PinnedViewerTitle",
          "ScreenTextCatcher — Просмотр ({0} × {1})");
      Title = string.Format(titleTemplate, pixelWidth, pixelHeight);

      PositionWindow(selectionRect, bitmap.Width, bitmap.Height);
    }
  }

  private void PositionWindow(Rect selectionRect, double imageWidth, double imageHeight) {
    WindowStartupLocation = WindowStartupLocation.Manual;

    var centerPoint = new System.Drawing.Point(
        (int)(selectionRect.Left + selectionRect.Width / 2.0),
        (int)(selectionRect.Top + selectionRect.Height / 2.0));
    var screen = System.Windows.Forms.Screen.FromPoint(centerPoint)
        ?? System.Windows.Forms.Screen.PrimaryScreen
        ?? System.Windows.Forms.Screen.AllScreens.FirstOrDefault();

    var dpi = VisualTreeHelper.GetDpi(this);
    double dpiScaleX = dpi.DpiScaleX > 0 ? dpi.DpiScaleX : 1.0;
    double dpiScaleY = dpi.DpiScaleY > 0 ? dpi.DpiScaleY : 1.0;

    Rect workingAreaDip;
    if (screen != null) {
      workingAreaDip = new Rect(
          screen.WorkingArea.Left / dpiScaleX,
          screen.WorkingArea.Top / dpiScaleY,
          screen.WorkingArea.Width / dpiScaleX,
          screen.WorkingArea.Height / dpiScaleY);
    } else {
      workingAreaDip = new Rect(0, 0, SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height);
    }

    int cascadeIndex = 0;
    if (System.Windows.Application.Current != null) {
      cascadeIndex = System.Windows.Application.Current.Windows
          .OfType<PinnedImageViewerWindow>()
          .Count(w => w != this);
    }

    var bounds = PinnedViewerPositioningHelper.CalculateWindowBounds(
        imageWidth,
        imageHeight,
        workingAreaDip,
        cascadeIndex);

    Left = bounds.Left;
    Top = bounds.Top;
    Width = bounds.Width;
    Height = bounds.Height;
  }
}
