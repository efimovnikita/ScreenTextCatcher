using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ScreenTextCatcher.Core.Helpers;

public static class AnnotationGeometryHelper {
  public static readonly Color AnnotationColor = Color.FromRgb(0xFF, 0x17, 0x44);
  public const double StrokeThickness = 3.0;
  public const double DefaultArrowHeadLength = 15.0;
  public const double DefaultArrowHeadAngleDegrees = 25.0;

  public static (Rect Bounds, double Left, double Top) CalculateRectangleBounds(Point p1, Point p2) {
    double left = Math.Min(p1.X, p2.X);
    double top = Math.Min(p1.Y, p2.Y);
    double width = Math.Abs(p2.X - p1.X);
    double height = Math.Abs(p2.Y - p1.Y);

    return (new Rect(left, top, width, height), left, top);
  }

  public static (Point LeftBarb, Point RightBarb) CalculateArrowHeadPoints(
      Point start,
      Point end,
      double headLength = DefaultArrowHeadLength,
      double headAngleDegrees = DefaultArrowHeadAngleDegrees) {
    double dx = end.X - start.X;
    double dy = end.Y - start.Y;
    double theta = Math.Atan2(dy, dx);
    double alpha = headAngleDegrees * Math.PI / 180.0;

    double x1 = end.X - headLength * Math.Cos(theta - alpha);
    double y1 = end.Y - headLength * Math.Sin(theta - alpha);

    double x2 = end.X - headLength * Math.Cos(theta + alpha);
    double y2 = end.Y - headLength * Math.Sin(theta + alpha);

    return (new Point(x1, y1), new Point(x2, y2));
  }

  public static Rectangle CreateRectangle(Point start, Point end) {
    var brush = new SolidColorBrush(AnnotationColor);
    if (brush.CanFreeze) {
      brush.Freeze();
    }

    var rect = new Rectangle {
      Stroke = brush,
      StrokeThickness = StrokeThickness,
      Fill = Brushes.Transparent
    };

    UpdateRectangle(rect, start, end);
    return rect;
  }

  public static void UpdateRectangle(Rectangle rect, Point start, Point end) {
    var (bounds, left, top) = CalculateRectangleBounds(start, end);
    Canvas.SetLeft(rect, left);
    Canvas.SetTop(rect, top);
    rect.Width = bounds.Width;
    rect.Height = bounds.Height;
  }

  public static Path CreateArrow(Point start, Point end) {
    var brush = new SolidColorBrush(AnnotationColor);
    if (brush.CanFreeze) {
      brush.Freeze();
    }

    var path = new Path {
      Stroke = brush,
      Fill = brush,
      StrokeThickness = StrokeThickness,
      StrokeStartLineCap = PenLineCap.Round,
      StrokeEndLineCap = PenLineCap.Round
    };

    UpdateArrow(path, start, end);
    return path;
  }

  public static void UpdateArrow(
      Path arrowPath,
      Point start,
      Point end,
      double headLength = DefaultArrowHeadLength,
      double headAngleDegrees = DefaultArrowHeadAngleDegrees) {
    var (leftBarb, rightBarb) = CalculateArrowHeadPoints(start, end, headLength, headAngleDegrees);

    var bodyFigure = new PathFigure {
      StartPoint = start,
      IsFilled = false,
      IsClosed = false
    };
    bodyFigure.Segments.Add(new LineSegment(end, true));

    var headFigure = new PathFigure {
      StartPoint = end,
      IsFilled = true,
      IsClosed = true
    };
    headFigure.Segments.Add(new LineSegment(leftBarb, true));
    headFigure.Segments.Add(new LineSegment(rightBarb, true));

    var geom = new PathGeometry();
    geom.Figures.Add(bodyFigure);
    geom.Figures.Add(headFigure);

    arrowPath.Data = geom;
  }

  public static Point SnapToAngle(Point start, Point current, double stepDegrees = 45.0) {
    double dx = current.X - start.X;
    double dy = current.Y - start.Y;
    double distance = Math.Sqrt(dx * dx + dy * dy);

    if (distance < 1e-6) {
      return start;
    }

    double currentAngleRad = Math.Atan2(dy, dx);
    double currentAngleDeg = currentAngleRad * 180.0 / Math.PI;

    double snappedAngleDeg = Math.Round(currentAngleDeg / stepDegrees) * stepDegrees;
    double snappedAngleRad = snappedAngleDeg * Math.PI / 180.0;

    double newX = start.X + distance * Math.Cos(snappedAngleRad);
    double newY = start.Y + distance * Math.Sin(snappedAngleRad);

    return new Point(newX, newY);
  }

  public static Line CreateLine(Point start, Point end) {
    var brush = new SolidColorBrush(AnnotationColor);
    if (brush.CanFreeze) {
      brush.Freeze();
    }

    var line = new Line {
      Stroke = brush,
      StrokeThickness = StrokeThickness,
      StrokeStartLineCap = PenLineCap.Round,
      StrokeEndLineCap = PenLineCap.Round
    };

    UpdateLine(line, start, end);
    return line;
  }

  public static void UpdateLine(Line line, Point start, Point end) {
    line.X1 = start.X;
    line.Y1 = start.Y;
    line.X2 = end.X;
    line.Y2 = end.Y;
  }
}
