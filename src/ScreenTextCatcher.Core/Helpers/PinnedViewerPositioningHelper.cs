using System;
using System.Windows;

namespace ScreenTextCatcher.Core.Helpers;

public static class PinnedViewerPositioningHelper {
  public const double DefaultCascadeStep = 24.0;
  public const double DefaultMaxCoverage = 0.9;
  public const double DefaultFrameExtraWidth = 16.0;
  public const double DefaultFrameExtraHeight = 39.0;
  public const double MinWindowWidth = 120.0;
  public const double MinWindowHeight = 80.0;

  public static Rect CalculateWindowBounds(
      double imageWidth,
      double imageHeight,
      Rect workingArea,
      int cascadeIndex = 0,
      double cascadeStep = DefaultCascadeStep,
      double maxCoverage = DefaultMaxCoverage,
      double frameExtraWidth = DefaultFrameExtraWidth,
      double frameExtraHeight = DefaultFrameExtraHeight) {
    if (workingArea.Width <= 0 || workingArea.Height <= 0) {
      return new Rect(0, 0, Math.Max(MinWindowWidth, imageWidth + frameExtraWidth), Math.Max(MinWindowHeight, imageHeight + frameExtraHeight));
    }

    double desiredWidth = imageWidth + frameExtraWidth;
    double desiredHeight = imageHeight + frameExtraHeight;

    double maxWidth = workingArea.Width * maxCoverage;
    double maxHeight = workingArea.Height * maxCoverage;

    double targetWidth = Math.Clamp(desiredWidth, MinWindowWidth, maxWidth);
    double targetHeight = Math.Clamp(desiredHeight, MinWindowHeight, maxHeight);

    if (targetWidth > workingArea.Width) {
      targetWidth = workingArea.Width;
    }
    if (targetHeight > workingArea.Height) {
      targetHeight = workingArea.Height;
    }

    double baseLeft = workingArea.Left + (workingArea.Width - targetWidth) / 2.0;
    double baseTop = workingArea.Top + (workingArea.Height - targetHeight) / 2.0;

    double offset = Math.Max(0, cascadeIndex) * cascadeStep;
    double left = baseLeft + offset;
    double top = baseTop + offset;

    if (left + targetWidth > workingArea.Right) {
      left = Math.Max(workingArea.Left, workingArea.Right - targetWidth);
    }
    if (top + targetHeight > workingArea.Bottom) {
      top = Math.Max(workingArea.Top, workingArea.Bottom - targetHeight);
    }

    return new Rect(left, top, targetWidth, targetHeight);
  }
}
