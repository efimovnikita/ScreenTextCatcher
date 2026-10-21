using System;
using System.Collections.Generic;
using System.Linq;
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

  [Fact]
  public void CreateLine_SetsCorrectStyleAndCoordinates() {
    RunInSta(() => {
      var p1 = new Point(10, 20);
      var p2 = new Point(80, 90);

      var line = AnnotationGeometryHelper.CreateLine(p1, p2);

      Assert.NotNull(line);
      Assert.Equal(AnnotationGeometryHelper.StrokeThickness, line.StrokeThickness);
      Assert.True(line.Stroke is SolidColorBrush brush && brush.Color == AnnotationGeometryHelper.AnnotationColor);
      Assert.Equal(PenLineCap.Round, line.StrokeStartLineCap);
      Assert.Equal(PenLineCap.Round, line.StrokeEndLineCap);
      Assert.Equal(10.0, line.X1);
      Assert.Equal(20.0, line.Y1);
      Assert.Equal(80.0, line.X2);
      Assert.Equal(90.0, line.Y2);
    });
  }

  [Fact]
  public void UpdateLine_UpdatesCoordinates() {
    RunInSta(() => {
      var p1 = new Point(10, 20);
      var p2 = new Point(50, 60);

      var line = AnnotationGeometryHelper.CreateLine(p1, p2);
      var newEnd = new Point(100, 150);
      AnnotationGeometryHelper.UpdateLine(line, p1, newEnd);

      Assert.Equal(10.0, line.X1);
      Assert.Equal(20.0, line.Y1);
      Assert.Equal(100.0, line.X2);
      Assert.Equal(150.0, line.Y2);
    });
  }

  [Fact]
  public void SnapToAngle_SnapsToExpected45DegreeMultiples() {
    var start = new Point(100, 100);

    // Identical start and current returns start
    var same = AnnotationGeometryHelper.SnapToAngle(start, start);
    Assert.Equal(start.X, same.X);
    Assert.Equal(start.Y, same.Y);

    // ~5 degrees (near 0 deg / horizontal right): dx = 100, dy = 10
    var near0 = AnnotationGeometryHelper.SnapToAngle(start, new Point(200, 110));
    Assert.Equal(start.Y, near0.Y, precision: 3);
    Assert.True(near0.X > start.X);

    // ~40 degrees (near 45 deg diagonal down-right): dx = 100, dy = 90
    var near45 = AnnotationGeometryHelper.SnapToAngle(start, new Point(200, 190));
    var dist45 = Math.Sqrt(Math.Pow(near45.X - start.X, 2) + Math.Pow(near45.Y - start.Y, 2));
    var origDist45 = Math.Sqrt(100 * 100 + 90 * 90);
    Assert.Equal(origDist45, dist45, precision: 3);
    Assert.Equal(near45.X - start.X, near45.Y - start.Y, precision: 3);

    // ~85 degrees (near 90 deg vertical down): dx = 10, dy = 100
    var near90 = AnnotationGeometryHelper.SnapToAngle(start, new Point(110, 200));
    Assert.Equal(start.X, near90.X, precision: 3);
    Assert.True(near90.Y > start.Y);

    // ~175 degrees (near 180 deg horizontal left): dx = -100, dy = 10
    var near180 = AnnotationGeometryHelper.SnapToAngle(start, new Point(0, 110));
    Assert.Equal(start.Y, near180.Y, precision: 3);
    Assert.True(near180.X < start.X);

    // ~275 degrees (near 270 deg vertical up): dx = 10, dy = -100
    var near270 = AnnotationGeometryHelper.SnapToAngle(start, new Point(110, 0));
    Assert.Equal(start.X, near270.X, precision: 3);
    Assert.True(near270.Y < start.Y);
  }

  [Fact]
  public void SimplifyPoints_CollinearPoints_RemovesRedundantIntermediatePoints() {
    var points = new List<Point> {
      new(0, 0),
      new(10, 0),
      new(20, 0),
      new(30, 0),
      new(40, 0)
    };

    var simplified = AnnotationGeometryHelper.SimplifyPoints(points, epsilon: 2.5);

    Assert.Equal(2, simplified.Count);
    Assert.Equal(new Point(0, 0), simplified[0]);
    Assert.Equal(new Point(40, 0), simplified[1]);
  }

  [Fact]
  public void SimplifyPoints_NoisyPoints_FiltersMinorJitterAndPreservesCorners() {
    // Points along horizontal line with minor jitter (< 2.5 px)
    var jitterPoints = new List<Point> {
      new(0, 0),
      new(10, 1.0),
      new(20, -1.2),
      new(30, 0.8),
      new(40, 0)
    };

    var filtered = AnnotationGeometryHelper.SimplifyPoints(jitterPoints, epsilon: 2.5);
    Assert.Equal(2, filtered.Count);
    Assert.Equal(new Point(0, 0), filtered[0]);
    Assert.Equal(new Point(40, 0), filtered[1]);

    // Points with a prominent corner (> 2.5 px) and minor jitter along the edges
    var cornerPoints = new List<Point> {
      new(0, 0),
      new(10, 7.8), // slight deviation from chord Y=7.5
      new(20, 15.0), // sharp peak
      new(30, 7.2), // slight deviation from chord Y=7.5
      new(40, 0)
    };

    var cornerFiltered = AnnotationGeometryHelper.SimplifyPoints(cornerPoints, epsilon: 2.5);
    Assert.Equal(3, cornerFiltered.Count);
    Assert.Equal(new Point(0, 0), cornerFiltered[0]);
    Assert.Equal(new Point(20, 15.0), cornerFiltered[1]);
    Assert.Equal(new Point(40, 0), cornerFiltered[2]);
  }

  [Fact]
  public void SimplifyPoints_EdgeCases_HandlesFewPoints() {
    var empty = AnnotationGeometryHelper.SimplifyPoints(new List<Point>());
    Assert.Empty(empty);

    var single = AnnotationGeometryHelper.SimplifyPoints(new List<Point> { new(5, 5) });
    Assert.Single(single);
    Assert.Equal(new Point(5, 5), single[0]);

    var two = AnnotationGeometryHelper.SimplifyPoints(new List<Point> { new(0, 0), new(10, 10) });
    Assert.Equal(2, two.Count);
    Assert.Equal(new Point(0, 0), two[0]);
    Assert.Equal(new Point(10, 10), two[1]);
  }

  [Fact]
  public void CreateSmoothPathGeometry_EdgeCases_AndTwoPoints() {
    RunInSta(() => {
      var geomEmpty = AnnotationGeometryHelper.CreateSmoothPathGeometry(new List<Point>());
      Assert.Empty(geomEmpty.Figures);
      Assert.True(geomEmpty.IsFrozen);

      var geomSingle = AnnotationGeometryHelper.CreateSmoothPathGeometry(new List<Point> { new(10, 10) });
      Assert.Single(geomSingle.Figures);
      Assert.Empty(geomSingle.Figures[0].Segments);

      var geomTwo = AnnotationGeometryHelper.CreateSmoothPathGeometry(new List<Point> { new(10, 10), new(50, 50) });
      Assert.Single(geomTwo.Figures);
      Assert.Single(geomTwo.Figures[0].Segments);
      Assert.IsType<LineSegment>(geomTwo.Figures[0].Segments[0]);
      var lineSeg = (LineSegment)geomTwo.Figures[0].Segments[0];
      Assert.Equal(new Point(50, 50), lineSeg.Point);
      Assert.True(geomTwo.IsFrozen);
    });
  }

  [Fact]
  public void CreateSmoothPathGeometry_MultiplePoints_GeneratesCubicBezierSegments() {
    RunInSta(() => {
      var points = new List<Point> {
        new(0, 0),
        new(20, 40),
        new(50, 10),
        new(80, 60)
      };

      var geom = AnnotationGeometryHelper.CreateSmoothPathGeometry(points);

      Assert.Single(geom.Figures);
      var figure = geom.Figures[0];
      Assert.Equal(new Point(0, 0), figure.StartPoint);
      Assert.False(figure.IsClosed);
      Assert.False(figure.IsFilled);

      // 4 points produce 3 cubic Bezier segments
      Assert.Equal(3, figure.Segments.Count);
      foreach (var segment in figure.Segments) {
        Assert.IsType<BezierSegment>(segment);
      }

      var lastBezier = (BezierSegment)figure.Segments[2];
      Assert.Equal(new Point(80, 60), lastBezier.Point3);
      Assert.True(geom.IsFrozen);
    });
  }

  [Fact]
  public void CreatePenPath_VisualProperties_MatchesAnnotationDesignSystem() {
    RunInSta(() => {
      var path = AnnotationGeometryHelper.CreatePenPath();

      Assert.NotNull(path);
      Assert.Equal(AnnotationGeometryHelper.StrokeThickness, path.StrokeThickness);
      Assert.True(path.Stroke is SolidColorBrush brush && brush.Color == AnnotationGeometryHelper.AnnotationColor);
      Assert.Equal(Brushes.Transparent, path.Fill);
      Assert.Equal(PenLineCap.Round, path.StrokeStartLineCap);
      Assert.Equal(PenLineCap.Round, path.StrokeEndLineCap);
      Assert.Equal(PenLineJoin.Round, path.StrokeLineJoin);
    });
  }

  [Fact]
  public void UpdatePenPreview_And_FinalizePenStroke_UpdatesPathDataCorrectly() {
    RunInSta(() => {
      var path = AnnotationGeometryHelper.CreatePenPath();
      var points = new List<Point> {
        new(0, 0),
        new(20, 100),
        new(50, 120),
        new(80, 100),
        new(100, 0)
      };

      // 1. Interactive Preview
      AnnotationGeometryHelper.UpdatePenPreview(path, points);
      Assert.NotNull(path.Data);
      Assert.IsType<PathGeometry>(path.Data);
      var previewGeom = (PathGeometry)path.Data;
      Assert.Single(previewGeom.Figures);
      Assert.Single(previewGeom.Figures[0].Segments);
      Assert.IsType<PolyLineSegment>(previewGeom.Figures[0].Segments[0]);

      // 2. Finalization with smoothing
      AnnotationGeometryHelper.FinalizePenStroke(path, points, epsilon: 2.5);
      Assert.NotNull(path.Data);
      Assert.IsType<PathGeometry>(path.Data);
      var finalGeom = (PathGeometry)path.Data;
      Assert.True(finalGeom.IsFrozen);
      Assert.Single(finalGeom.Figures);
      Assert.True(finalGeom.Figures[0].Segments.Count > 0);
      Assert.IsType<BezierSegment>(finalGeom.Figures[0].Segments[0]);
    });
  }

  [Fact]
  public void SmoothPoints_ReducesJitterAndPreservesEndpoints() {
    var raw = new List<Point> {
      new(0, 0),
      new(10, 10), // jitter
      new(20, -5), // jitter
      new(30, 8),  // jitter
      new(40, 0)
    };

    var smoothed = AnnotationGeometryHelper.SmoothPoints(raw, iterations: 2);

    Assert.Equal(raw.Count, smoothed.Count);
    // Endpoints strictly preserved
    Assert.Equal(raw[0], smoothed[0]);
    Assert.Equal(raw[^1], smoothed[^1]);

    // Intermediate points have drastically lower variance / jitter
    Assert.True(Math.Abs(smoothed[1].Y) < Math.Abs(raw[1].Y));
    Assert.True(Math.Abs(smoothed[2].Y) < Math.Abs(raw[2].Y));
    Assert.True(Math.Abs(smoothed[3].Y) < Math.Abs(raw[3].Y));
  }

  [Fact]
  public void SmoothPoints_EdgeCases_HandlesFewPointsOrZeroIterations() {
    var empty = AnnotationGeometryHelper.SmoothPoints(new List<Point>());
    Assert.Empty(empty);

    var two = new List<Point> { new(0, 0), new(10, 10) };
    var smoothedTwo = AnnotationGeometryHelper.SmoothPoints(two, iterations: 3);
    Assert.Equal(2, smoothedTwo.Count);

    var three = new List<Point> { new(0, 0), new(5, 5), new(10, 10) };
    var zeroIter = AnnotationGeometryHelper.SmoothPoints(three, iterations: 0);
    Assert.Equal(three.Count, zeroIter.Count);
  }
}
