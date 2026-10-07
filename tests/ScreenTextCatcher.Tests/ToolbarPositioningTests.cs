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
  public void CalculatePosition_RightEdgeSelection_ClampsToolbarInsideScreen() {
    var screen = new Rect(0, 0, 1920, 1080);
    // Selection touching right edge
    var selection = new Rect(1700, 200, 220, 300);
    var toolbar = new Size(250, 40);

    var pos = ToolbarPositioningHelper.CalculatePosition(selection, toolbar, screen, margin: 8.0);

    // Toolbar must not exceed screen right edge
    Assert.True(pos.X + toolbar.Width <= screen.Right);
    Assert.True(pos.X >= screen.Left);
  }

  [Fact]
  public void CalculatePosition_LeftEdgeSelection_ClampsToolbarInsideScreen() {
    var screen = new Rect(0, 0, 1920, 1080);
    var selection = new Rect(0, 200, 100, 300);
    var toolbar = new Size(250, 40);

    var pos = ToolbarPositioningHelper.CalculatePosition(selection, toolbar, screen, margin: 8.0);

    Assert.True(pos.X >= screen.Left);
  }
}
