using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ScreenTextCatcher.Core.Helpers;

namespace ScreenTextCatcher.Tests;

public class AnnotationGeometryTests {
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
  public void CalculateRectangleBounds_CorrectBounds_ForStandardAndInverseDrag() {
    var p1 = new Point(20, 30);
    var p2 = new Point(100, 80);

    var (rect1, left1, top1) = AnnotationGeometryHelper.CalculateRectangleBounds(p1, p2);
    Assert.Equal(20, left1);
    Assert.Equal(30, top1);
    Assert.Equal(80, rect1.Width);
    Assert.Equal(50, rect1.Height);

    // Inverse drag: bottom-right to top-left
    var (rect2, left2, top2) = AnnotationGeometryHelper.CalculateRectangleBounds(p2, p1);
    Assert.Equal(20, left2);
    Assert.Equal(30, top2);
    Assert.Equal(80, rect2.Width);
    Assert.Equal(50, rect2.Height);
  }

  [Fact]
  public void CalculateArrowHeadPoints_HorizontalArrow_ReturnsSymmetricPoints() {
    var start = new Point(0, 0);
    var end = new Point(100, 0);

    var (leftBarb, rightBarb) = AnnotationGeometryHelper.CalculateArrowHeadPoints(start, end, 15.0, 25.0);

    // Tip is (100, 0), so barbs must be behind the tip (X < 100)
    Assert.True(leftBarb.X < 100);
    Assert.True(rightBarb.X < 100);
    Assert.Equal(leftBarb.X, rightBarb.X, precision: 3);

    // Symmetric around Y = 0
    Assert.Equal(leftBarb.Y, -rightBarb.Y, precision: 3);
    Assert.True(leftBarb.Y > 0);
    Assert.True(rightBarb.Y < 0);
  }

  [Fact]
  public void CalculateArrowHeadPoints_VerticalArrow_ReturnsSymmetricPoints() {
    var start = new Point(0, 0);
    var end = new Point(0, 100);

    var (leftBarb, rightBarb) = AnnotationGeometryHelper.CalculateArrowHeadPoints(start, end, 15.0, 25.0);

    // Tip is (0, 100), barbs must be behind the tip (Y < 100)
    Assert.True(leftBarb.Y < 100);
    Assert.True(rightBarb.Y < 100);
    Assert.Equal(leftBarb.Y, rightBarb.Y, precision: 3);

    // Symmetric around X = 0
    Assert.Equal(-leftBarb.X, rightBarb.X, precision: 3);
  }

  [Fact]
  public void CreateRectangle_SetsCorrectStyleAndCanvasPositions() {
    RunInSta(() => {
      var p1 = new Point(10, 15);
      var p2 = new Point(60, 75);

      var rect = AnnotationGeometryHelper.CreateRectangle(p1, p2);

      Assert.NotNull(rect);
      Assert.Equal(3.0, rect.StrokeThickness);
      Assert.True(rect.Stroke is SolidColorBrush brush && brush.Color == AnnotationGeometryHelper.AnnotationColor);
      Assert.Equal(Brushes.Transparent, rect.Fill);
      Assert.Equal(10.0, Canvas.GetLeft(rect));
      Assert.Equal(15.0, Canvas.GetTop(rect));
      Assert.Equal(50.0, rect.Width);
      Assert.Equal(60.0, rect.Height);
    });
  }

  [Fact]
  public void CreateArrow_SetsCorrectStyleAndGeometry() {
    RunInSta(() => {
      var p1 = new Point(10, 10);
      var p2 = new Point(90, 90);

      var arrow = AnnotationGeometryHelper.CreateArrow(p1, p2);

      Assert.NotNull(arrow);
      Assert.Equal(3.0, arrow.StrokeThickness);
      Assert.True(arrow.Stroke is SolidColorBrush sBrush && sBrush.Color == AnnotationGeometryHelper.AnnotationColor);
      Assert.True(arrow.Fill is SolidColorBrush fBrush && fBrush.Color == AnnotationGeometryHelper.AnnotationColor);
      Assert.NotNull(arrow.Data);
      Assert.IsType<PathGeometry>(arrow.Data);

      var geom = (PathGeometry)arrow.Data;
      // Figure 1: line body, Figure 2: filled arrowhead triangle
      Assert.Equal(2, geom.Figures.Count);
      Assert.False(geom.Figures[0].IsFilled);
      Assert.True(geom.Figures[1].IsFilled);
      Assert.True(geom.Figures[1].IsClosed);
    });
  }

  [Fact]
  public void UpdateRectangle_And_UpdateArrow_UpdateExistingShapes() {
    RunInSta(() => {
      var p1 = new Point(10, 10);
      var p2 = new Point(50, 50);

      var rect = AnnotationGeometryHelper.CreateRectangle(p1, p2);
      AnnotationGeometryHelper.UpdateRectangle(rect, p1, new Point(100, 100));
      Assert.Equal(90.0, rect.Width);
      Assert.Equal(90.0, rect.Height);

      var arrow = AnnotationGeometryHelper.CreateArrow(p1, p2);
      AnnotationGeometryHelper.UpdateArrow(arrow, p1, new Point(200, 200));
      var geom = (PathGeometry)arrow.Data;
      Assert.Equal(2, geom.Figures.Count);
    });
  }
}
