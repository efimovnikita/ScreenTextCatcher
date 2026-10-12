using System.Windows;
using ScreenTextCatcher.Core.Helpers;

namespace ScreenTextCatcher.Tests;

public class PinnedViewerPositioningTests {
  [Fact]
  public void CalculateWindowBounds_NormalImage_CentersWithinWorkingArea() {
    var workingArea = new Rect(0, 0, 1920, 1040);
    double imageW = 800;
    double imageH = 600;

    var bounds = PinnedViewerPositioningHelper.CalculateWindowBounds(
        imageW,
        imageH,
        workingArea,
        cascadeIndex: 0);

    // Frame adds 16px width and 39px height
    Assert.Equal(816.0, bounds.Width);
    Assert.Equal(639.0, bounds.Height);

    // Centered: (1920 - 816) / 2 = 552
    Assert.Equal(552.0, bounds.Left);
    // Centered: (1040 - 639) / 2 = 200.5
    Assert.Equal(200.5, bounds.Top);
  }

  [Fact]
  public void CalculateWindowBounds_CascadeIndex_OffsetsPositionByStep() {
    var workingArea = new Rect(0, 0, 1920, 1040);
    double imageW = 800;
    double imageH = 600;

    var firstBounds = PinnedViewerPositioningHelper.CalculateWindowBounds(
        imageW,
        imageH,
        workingArea,
        cascadeIndex: 0);

    var secondBounds = PinnedViewerPositioningHelper.CalculateWindowBounds(
        imageW,
        imageH,
        workingArea,
        cascadeIndex: 1,
        cascadeStep: 24.0);

    Assert.Equal(firstBounds.Width, secondBounds.Width);
    Assert.Equal(firstBounds.Height, secondBounds.Height);
    Assert.Equal(firstBounds.Left + 24.0, secondBounds.Left);
    Assert.Equal(firstBounds.Top + 24.0, secondBounds.Top);
  }

  [Fact]
  public void CalculateWindowBounds_LargeImage_ClampsToMaxCoverage() {
    var workingArea = new Rect(0, 0, 1920, 1000);
    double imageW = 3000;
    double imageH = 2000;

    var bounds = PinnedViewerPositioningHelper.CalculateWindowBounds(
        imageW,
        imageH,
        workingArea,
        maxCoverage: 0.9);

    // 90% of 1920 = 1728
    Assert.Equal(1728.0, bounds.Width);
    // 90% of 1000 = 900
    Assert.Equal(900.0, bounds.Height);

    // Centered: (1920 - 1728) / 2 = 96
    Assert.Equal(96.0, bounds.Left);
    // Centered: (1000 - 900) / 2 = 50
    Assert.Equal(50.0, bounds.Top);
  }

  [Fact]
  public void CalculateWindowBounds_SecondaryMonitor_PositionsOnCorrectMonitor() {
    // Secondary monitor located at X = 1920 with 1920x1080 resolution
    var workingArea = new Rect(1920, 0, 1920, 1080);
    double imageW = 400;
    double imageH = 300;

    var bounds = PinnedViewerPositioningHelper.CalculateWindowBounds(
        imageW,
        imageH,
        workingArea,
        cascadeIndex: 0);

    double expectedWidth = 400 + 16;
    double expectedHeight = 300 + 39;

    Assert.Equal(expectedWidth, bounds.Width);
    Assert.Equal(expectedHeight, bounds.Height);

    // Centered on secondary monitor: 1920 + (1920 - 416) / 2 = 1920 + 752 = 2672
    Assert.Equal(2672.0, bounds.Left);
    // Centered: (1080 - 339) / 2 = 370.5
    Assert.Equal(370.5, bounds.Top);
  }

  [Fact]
  public void CalculateWindowBounds_CascadeNearEdge_ClampsInsideWorkingArea() {
    var workingArea = new Rect(0, 0, 1000, 800);
    // Large image taking almost full width
    double imageW = 900;
    double imageH = 700;

    var bounds = PinnedViewerPositioningHelper.CalculateWindowBounds(
        imageW,
        imageH,
        workingArea,
        cascadeIndex: 10,
        cascadeStep: 50.0);

    Assert.True(bounds.Right <= workingArea.Right);
    Assert.True(bounds.Bottom <= workingArea.Bottom);
    Assert.True(bounds.Left >= workingArea.Left);
    Assert.True(bounds.Top >= workingArea.Top);
  }
}
