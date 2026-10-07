using System;
using System.Windows;

namespace ScreenTextCatcher.Core.Helpers;

public static class ToolbarPositioningHelper {
  public static Point CalculatePosition(
      Rect selection,
      Size toolbar,
      Rect screenBounds,
      double margin = 8.0) {
    double y = selection.Bottom + margin;
    if (y + toolbar.Height > screenBounds.Bottom) {
      double yAbove = selection.Top - toolbar.Height - margin;
      if (yAbove >= screenBounds.Top) {
        y = yAbove;
      } else {
        y = Math.Clamp(y, screenBounds.Top, Math.Max(screenBounds.Top, screenBounds.Bottom - toolbar.Height));
      }
    }

    double x = selection.Right - toolbar.Width;
    if (x < selection.Left) {
      x = selection.Left;
    }

    double maxX = Math.Max(screenBounds.Left, screenBounds.Right - toolbar.Width);
    x = Math.Clamp(x, screenBounds.Left, maxX);

    return new Point(x, y);
  }
}
