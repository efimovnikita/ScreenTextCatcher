using System;
using System.Collections.Generic;
using System.Linq;
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
  public const double DefaultPenSmoothingEpsilon = 5.0;
  public const double MinPenPointDistance = 3.5;

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

  public static List<Point> SimplifyPoints(
      IReadOnlyList<Point> points,
      double epsilon = DefaultPenSmoothingEpsilon) {
    if (points == null || points.Count <= 2) {
      return points?.ToList() ?? new List<Point>();
    }

    var keep = new bool[points.Count];
    keep[0] = true;
    keep[points.Count - 1] = true;

    RdpMark(points, 0, points.Count - 1, epsilon, keep);

    var result = new List<Point>(points.Count);
    for (int i = 0; i < points.Count; i++) {
      if (keep[i]) {
        result.Add(points[i]);
      }
    }
    return result;
  }

  private static void RdpMark(
      IReadOnlyList<Point> points,
      int start,
      int end,
      double epsilon,
      bool[] keep) {
    if (end <= start + 1) {
      return;
    }

    double maxDistance = 0.0;
    int indexMax = -1;

    Point pStart = points[start];
    Point pEnd = points[end];

    double dx = pEnd.X - pStart.X;
    double dy = pEnd.Y - pStart.Y;
    double lineLengthSq = dx * dx + dy * dy;
    double lineLength = Math.Sqrt(lineLengthSq);

    for (int i = start + 1; i < end; i++) {
      Point p = points[i];
      double distance;
      if (lineLengthSq < 1e-10) {
        double dpx = p.X - pStart.X;
        double dpy = p.Y - pStart.Y;
        distance = Math.Sqrt(dpx * dpx + dpy * dpy);
      } else {
        double num = Math.Abs(dy * p.X - dx * p.Y + pEnd.X * pStart.Y - pEnd.Y * pStart.X);
        distance = num / lineLength;
      }

      if (distance > maxDistance) {
        maxDistance = distance;
        indexMax = i;
      }
    }

    if (maxDistance > epsilon && indexMax != -1) {
      keep[indexMax] = true;
      RdpMark(points, start, indexMax, epsilon, keep);
      RdpMark(points, indexMax, end, epsilon, keep);
    }
  }

  public static PathGeometry CreateSmoothPathGeometry(IReadOnlyList<Point> points) {
    var geom = new PathGeometry();
    if (points == null || points.Count == 0) {
      if (geom.CanFreeze) {
        geom.Freeze();
      }
      return geom;
    }

    var figure = new PathFigure {
      StartPoint = points[0],
      IsFilled = false,
      IsClosed = false
    };

    if (points.Count == 1) {
      geom.Figures.Add(figure);
      if (geom.CanFreeze) {
        geom.Freeze();
      }
      return geom;
    }

    if (points.Count == 2) {
      figure.Segments.Add(new LineSegment(points[1], true));
      geom.Figures.Add(figure);
      if (geom.CanFreeze) {
        geom.Freeze();
      }
      return geom;
    }

    int n = points.Count - 1;
    for (int i = 0; i < n; i++) {
      Point p0 = (i == 0)
          ? new Point(2 * points[0].X - points[1].X, 2 * points[0].Y - points[1].Y)
          : points[i - 1];
      Point p1 = points[i];
      Point p2 = points[i + 1];
      Point p3 = (i + 2 <= n)
          ? points[i + 2]
          : new Point(2 * points[n].X - points[n - 1].X, 2 * points[n].Y - points[n - 1].Y);

      Point c1 = new Point(p1.X + (p2.X - p0.X) / 6.0, p1.Y + (p2.Y - p0.Y) / 6.0);
      Point c2 = new Point(p2.X - (p3.X - p1.X) / 6.0, p2.Y - (p3.Y - p1.Y) / 6.0);

      figure.Segments.Add(new BezierSegment(c1, c2, p2, true));
    }

    geom.Figures.Add(figure);
    if (geom.CanFreeze) {
      geom.Freeze();
    }
    return geom;
  }

  public static Path CreatePenPath(IReadOnlyList<Point>? initialPoints = null) {
    var brush = new SolidColorBrush(AnnotationColor);
    if (brush.CanFreeze) {
      brush.Freeze();
    }

    var path = new Path {
      Stroke = brush,
      Fill = Brushes.Transparent,
      StrokeThickness = StrokeThickness,
      StrokeStartLineCap = PenLineCap.Round,
      StrokeEndLineCap = PenLineCap.Round,
      StrokeLineJoin = PenLineJoin.Round
    };

    if (initialPoints != null && initialPoints.Count > 0) {
      UpdatePenPreview(path, initialPoints);
    }

    return path;
  }

  public static void UpdatePenPreview(Path path, IReadOnlyList<Point> points) {
    if (points == null || points.Count == 0) {
      path.Data = null;
      return;
    }

    var figure = new PathFigure {
      StartPoint = points[0],
      IsFilled = false,
      IsClosed = false
    };

    if (points.Count > 1) {
      var polyLine = new PolyLineSegment(points.Skip(1), true);
      figure.Segments.Add(polyLine);
    }

    var geom = new PathGeometry();
    geom.Figures.Add(figure);
    path.Data = geom;
  }

  public static List<Point> SmoothPoints(IReadOnlyList<Point> points, int iterations = 2) {
    if (points == null || points.Count <= 2 || iterations <= 0) {
      return points?.ToList() ?? new List<Point>();
    }

    var current = points.ToList();
    for (int iter = 0; iter < iterations; iter++) {
      var next = new List<Point>(current.Count);
      next.Add(current[0]);
      for (int i = 1; i < current.Count - 1; i++) {
        double x = 0.25 * current[i - 1].X + 0.5 * current[i].X + 0.25 * current[i + 1].X;
        double y = 0.25 * current[i - 1].Y + 0.5 * current[i].Y + 0.25 * current[i + 1].Y;
        next.Add(new Point(x, y));
      }
      next.Add(current[^1]);
      current = next;
    }

    return current;
  }

  public static void FinalizePenStroke(
      Path path,
      IReadOnlyList<Point> points,
      double epsilon = DefaultPenSmoothingEpsilon,
      int smoothingIterations = 2) {
    var smoothed = SmoothPoints(points, smoothingIterations);
    var simplified = SimplifyPoints(smoothed, epsilon);
    var smoothGeom = CreateSmoothPathGeometry(simplified);
    path.Data = smoothGeom;
  }
}
