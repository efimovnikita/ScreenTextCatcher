using System.Windows;
using ScreenTextCatcher.Core.Helpers;

namespace ScreenTextCatcher.Tests;

public class ToolbarPositioningTests {
  [Fact]
  public void CalculatePosition_NormalSelection_PlacesToolbarBelow() {
    var screen = new Rect(0, 0, 1920, 1080);
    var selection = new Rect(200, 200, 400, 300);
    var toolbar = new Size(200, 40);

    var pos = ToolbarPositioningHelper.CalculatePosition(selection, toolbar, screen, margin: 8.0);

    // Below selection: Top = 200 + 300 + 8 = 508
    Assert.Equal(508.0, pos.Y);
    // Right-aligned to selection or within bounds
    Assert.True(pos.X >= screen.Left);
    Assert.True(pos.X + toolbar.Width <= screen.Right);
  }

  [Fact]
  public void CalculatePosition_BottomEdgeSelection_PlacesToolbarAbove() {
    var screen = new Rect(0, 0, 1920, 1080);
    // Selection near bottom: bottom is 1060
    var selection = new Rect(200, 700, 400, 360);
    var toolbar = new Size(200, 40);

    var pos = ToolbarPositioningHelper.CalculatePosition(selection, toolbar, screen, margin: 8.0);

    // Cannot fit below (1060 + 8 + 40 = 1108 > 1080)
    // Must be placed above selection: Top = 700 - 40 - 8 = 652
    Assert.Equal(652.0, pos.Y);
  }

  [Fact]
  public void CalculatePosition_RightEdgeSelection_ClampsWithMargin() {
    var screen = new Rect(0, 0, 1920, 1080);
    // Selection touching right edge (Right = 1920)
    var selection = new Rect(1700, 200, 220, 300);
    var toolbar = new Size(250, 40);

    var pos = ToolbarPositioningHelper.CalculatePosition(selection, toolbar, screen, margin: 8.0);

    // Toolbar must be clamped inside screen with 8px margin: 1920 - 250 - 8 = 1662
    Assert.Equal(1662.0, pos.X);
    Assert.True(pos.X + toolbar.Width <= screen.Right - 8.0);
  }

  [Fact]
  public void CalculatePosition_LeftEdgeSelection_ClampsWithMargin() {
    var screen = new Rect(0, 0, 1920, 1080);
    // Selection touching left edge (Left = 0)
    var selection = new Rect(0, 200, 100, 300);
    var toolbar = new Size(250, 40);

    var pos = ToolbarPositioningHelper.CalculatePosition(selection, toolbar, screen, margin: 8.0);

    // Toolbar must be clamped with 8px margin from left edge: 0 + 8 = 8
    Assert.Equal(8.0, pos.X);
  }

  [Fact]
  public void CalculatePosition_FullScreenSelection_ClampsToBottomWithMargin() {
    var screen = new Rect(0, 0, 1920, 1080);
    var selection = new Rect(0, 0, 1920, 1080);
    var toolbar = new Size(300, 40);

    var pos = ToolbarPositioningHelper.CalculatePosition(selection, toolbar, screen, margin: 8.0);

    // When selection covers full height and width:
    // Y must clamp to bottom with margin: 1080 - 40 - 8 = 1032
    Assert.Equal(1032.0, pos.Y);
    // X must clamp to right with margin: 1920 - 300 - 8 = 1612
    Assert.Equal(1612.0, pos.X);
  }

  [Fact]
  public void CalculatePosition_MultiMonitorOffset_PositionsWithinSecondaryMonitor() {
    // Secondary monitor located to the right: X from 1920 to 3840
    var screen = new Rect(1920, 0, 1920, 1080);
    var toolbar = new Size(200, 40);

    // Selection near right edge of secondary monitor (Right = 3840)
    var selectionRight = new Rect(3700, 300, 140, 200);
    var posRight = ToolbarPositioningHelper.CalculatePosition(selectionRight, toolbar, screen, margin: 8.0);
    // 3840 - 200 - 8 = 3632
    Assert.Equal(3632.0, posRight.X);

    // Selection near left edge of secondary monitor (Left = 1920)
    var selectionLeft = new Rect(1920, 300, 100, 200);
    var posLeft = ToolbarPositioningHelper.CalculatePosition(selectionLeft, toolbar, screen, margin: 8.0);
    // 1920 + 8 = 1928
    Assert.Equal(1928.0, posLeft.X);
  }
}
