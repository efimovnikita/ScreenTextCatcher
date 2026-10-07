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
    if (y + toolbar.Height > screenBounds.Bottom - margin) {
      double yAbove = selection.Top - toolbar.Height - margin;
      if (yAbove >= screenBounds.Top + margin) {
        y = yAbove;
      } else {
        double minY = screenBounds.Top + margin;
        double maxY = Math.Max(minY, screenBounds.Bottom - toolbar.Height - margin);
        y = Math.Clamp(y, minY, maxY);
      }
    }

    double x = selection.Right - toolbar.Width;
    if (x < selection.Left) {
      x = selection.Left;
    }

    double minX = screenBounds.Left + margin;
    double maxX = Math.Max(minX, screenBounds.Right - toolbar.Width - margin);
    x = Math.Clamp(x, minX, maxX);

    return new Point(x, y);
  }
}
