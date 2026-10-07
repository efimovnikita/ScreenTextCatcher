using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ScreenTextCatcher.Core.Helpers;
using ScreenTextCatcher.Core.Services;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Cursors = System.Windows.Input.Cursors;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;
using Rectangle = System.Drawing.Rectangle;

namespace ScreenTextCatcher.Views;

public partial class OverlayWindow : Window {
  private enum OverlayState {
    SelectingArea,
    Annotating
  }

  private enum AnnotationTool {
    None,
    Rectangle,
    Arrow
  }

  private static System.Windows.Input.Cursor? _cachedRedCrosshair;

  private readonly bool _isScreenshotMode;
  private OverlayState _state = OverlayState.SelectingArea;
  private AnnotationTool _activeTool = AnnotationTool.None;

  private readonly List<UIElement> _annotationHistory = new();
  private UIElement? _currentDrawingShape;

  private Point _startPoint;
  private Point _drawingStartPoint;
  private bool _isSelecting = false;
  private bool _isDrawing = false;

  private Bitmap? _frozenScreenBitmap;
  private Rect _selectedAreaRect;
  private double _dpiScaleX = 1.0;
  private double _dpiScaleY = 1.0;

  public event Action<Rectangle>? AreaSelected;
  public event Action<byte[], bool>? ScreenshotReady;
  public event Action? Cancelled;

  public OverlayWindow(bool isScreenshotMode = false) {
    _isScreenshotMode = isScreenshotMode;
    InitializeComponent();

    Cursor = GetOrCreateRedCrosshair();
    OverlayCanvas.Cursor = Cursor;

    Left = SystemParameters.VirtualScreenLeft;
    Top = SystemParameters.VirtualScreenTop;
    Width = SystemParameters.VirtualScreenWidth;
    Height = SystemParameters.VirtualScreenHeight;

    Loaded += (s, e) => {
      Canvas.SetLeft(HintBorder, (Width - HintBorder.ActualWidth) / 2);

      var source = PresentationSource.FromVisual(this);
      _dpiScaleX = source?.CompositionTarget?.TransformToDevice.M11 ?? 1.0;
      _dpiScaleY = source?.CompositionTarget?.TransformToDevice.M22 ?? 1.0;

      if (_isScreenshotMode) {
        var captureService = new ScreenCaptureService();
        _frozenScreenBitmap = captureService.CaptureVirtualScreen();
        var frozenSource = ImageConversionHelper.ToBitmapSource(_frozenScreenBitmap);

        FrozenScreenImage.Source = frozenSource;
        FrozenScreenImage.Width = Width;
        FrozenScreenImage.Height = Height;
        FrozenScreenImage.Visibility = Visibility.Visible;

        DimMask.Width = Width;
        DimMask.Height = Height;
        DimMask.Visibility = Visibility.Visible;
        Background = Brushes.Transparent;
      } else {
        Background = new SolidColorBrush(Color.FromArgb(0x80, 0, 0, 0));
      }

      Focus();
      Activate();
    };

    KeyDown += OnWindowKeyDown;
    Closed += (s, e) => {
      _frozenScreenBitmap?.Dispose();
      _frozenScreenBitmap = null;
    };
  }

  private void OnWindowKeyDown(object sender, KeyEventArgs e) {
    if (e.Key == Key.Escape) {
      CancelSelection();
      return;
    }

    if (_state == OverlayState.Annotating) {
      if (e.Key == Key.Enter) {
        FinishAndExportScreenshot(copyImageToClipboard: false);
        return;
      }

      if (e.Key == Key.Z && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
        UndoLastAnnotation();
        return;
      }
    }
  }

  private void OnCanvasMouseDown(object sender, MouseButtonEventArgs e) {
    if (e.RightButton == MouseButtonState.Pressed) {
      if (_state == OverlayState.SelectingArea) {
        CancelSelection();
      }
      // In Annotating mode, right click is ignored per requirements
      return;
    }

    if (e.LeftButton == MouseButtonState.Pressed) {
      if (_state == OverlayState.SelectingArea) {
        _startPoint = e.GetPosition(OverlayCanvas);
        _isSelecting = true;
        OverlayCanvas.CaptureMouse();

        Canvas.SetLeft(SelectionRect, _startPoint.X);
        Canvas.SetTop(SelectionRect, _startPoint.Y);
        SelectionRect.Width = 0;
        SelectionRect.Height = 0;
        SelectionRect.Visibility = Visibility.Visible;
        CornerTopLeft.Visibility = Visibility.Visible;
        CornerTopRight.Visibility = Visibility.Visible;
        CornerBottomLeft.Visibility = Visibility.Visible;
        CornerBottomRight.Visibility = Visibility.Visible;
        DimensionBadge.Visibility = Visibility.Visible;
        HintBorder.Visibility = Visibility.Collapsed;
      } else if (_state == OverlayState.Annotating) {
        if (_activeTool == AnnotationTool.None) {
          // Drawing is disabled until a tool is explicitly selected
          return;
        }

        _drawingStartPoint = e.GetPosition(AnnotationCanvas);
        _isDrawing = true;
        OverlayCanvas.CaptureMouse();

        if (_activeTool == AnnotationTool.Rectangle) {
          _currentDrawingShape = AnnotationGeometryHelper.CreateRectangle(_drawingStartPoint, _drawingStartPoint);
        } else if (_activeTool == AnnotationTool.Arrow) {
          _currentDrawingShape = AnnotationGeometryHelper.CreateArrow(_drawingStartPoint, _drawingStartPoint);
        }

        if (_currentDrawingShape != null) {
          AnnotationCanvas.Children.Add(_currentDrawingShape);
        }
      }
    }
  }

  private void OnCanvasMouseMove(object sender, MouseEventArgs e) {
    if (_state == OverlayState.SelectingArea && _isSelecting) {
      var currentPoint = e.GetPosition(OverlayCanvas);

      var x = Math.Min(_startPoint.X, currentPoint.X);
      var y = Math.Min(_startPoint.Y, currentPoint.Y);
      var w = Math.Abs(currentPoint.X - _startPoint.X);
      var h = Math.Abs(currentPoint.Y - _startPoint.Y);

      Canvas.SetLeft(SelectionRect, x);
      Canvas.SetTop(SelectionRect, y);
      SelectionRect.Width = w;
      SelectionRect.Height = h;

      Canvas.SetLeft(CornerTopLeft, x);
      Canvas.SetTop(CornerTopLeft, y);

      Canvas.SetLeft(CornerTopRight, Math.Max(x, x + w - CornerTopRight.Width));
      Canvas.SetTop(CornerTopRight, y);

      Canvas.SetLeft(CornerBottomLeft, x);
      Canvas.SetTop(CornerBottomLeft, Math.Max(y, y + h - CornerBottomLeft.Height));

      Canvas.SetLeft(CornerBottomRight, Math.Max(x, x + w - CornerBottomRight.Width));
      Canvas.SetTop(CornerBottomRight, Math.Max(y, y + h - CornerBottomRight.Height));

      TxtDimensions.Text = $"{(int)w} × {(int)h}";
      Canvas.SetLeft(DimensionBadge, x);
      Canvas.SetTop(DimensionBadge, Math.Max(10, y - 26));
    } else if (_state == OverlayState.Annotating && _isDrawing && _currentDrawingShape != null) {
      var currentPoint = e.GetPosition(AnnotationCanvas);

      if (_currentDrawingShape is System.Windows.Shapes.Rectangle rect) {
        AnnotationGeometryHelper.UpdateRectangle(rect, _drawingStartPoint, currentPoint);
      } else if (_currentDrawingShape is System.Windows.Shapes.Path arrow) {
        AnnotationGeometryHelper.UpdateArrow(arrow, _drawingStartPoint, currentPoint);
      }
    }
  }

  private void OnCanvasMouseUp(object sender, MouseButtonEventArgs e) {
    if (_state == OverlayState.SelectingArea && _isSelecting) {
      _isSelecting = false;
      OverlayCanvas.ReleaseMouseCapture();

      var endPoint = e.GetPosition(OverlayCanvas);
      var w = Math.Abs(endPoint.X - _startPoint.X);
      var h = Math.Abs(endPoint.Y - _startPoint.Y);

      if (w < 8 || h < 8) {
        CancelSelection();
        return;
      }

      var left = Math.Min(_startPoint.X, endPoint.X);
      var top = Math.Min(_startPoint.Y, endPoint.Y);
      _selectedAreaRect = new Rect(left, top, w, h);

      if (!_isScreenshotMode) {
        var screenX = (int)((Left + left) * _dpiScaleX);
        var screenY = (int)((Top + top) * _dpiScaleY);
        var screenW = (int)(w * _dpiScaleX);
        var screenH = (int)(h * _dpiScaleY);

        var selectedRect = new Rectangle(screenX, screenY, screenW, screenH);
        Close();
        AreaSelected?.Invoke(selectedRect);
        return;
      }

      // Enter Annotation mode
      _state = OverlayState.Annotating;
      CornerTopLeft.Visibility = Visibility.Collapsed;
      CornerTopRight.Visibility = Visibility.Collapsed;
      CornerBottomLeft.Visibility = Visibility.Collapsed;
      CornerBottomRight.Visibility = Visibility.Collapsed;
      SelectionRect.Fill = Brushes.Transparent;
      SelectionRect.Stroke = new SolidColorBrush(Color.FromRgb(0xFF, 0x17, 0x44));
      SelectionRect.StrokeThickness = 1.0;
      SelectionRect.Visibility = Visibility.Visible;
      Canvas.SetZIndex(SelectionRect, 50);

      // Position and prepare AnnotationCanvas
      Canvas.SetLeft(AnnotationCanvas, left);
      Canvas.SetTop(AnnotationCanvas, top);
      AnnotationCanvas.Width = w;
      AnnotationCanvas.Height = h;
      AnnotationCanvas.Visibility = Visibility.Visible;

      // Crop the base screenshot region into AnnotatedCroppedImage
      if (_frozenScreenBitmap != null) {
        var cropX = (int)Math.Max(0, left * _dpiScaleX);
        var cropY = (int)Math.Max(0, top * _dpiScaleY);
        var cropW = (int)Math.Max(1, w * _dpiScaleX);
        var cropH = (int)Math.Max(1, h * _dpiScaleY);

        cropW = Math.Min(cropW, _frozenScreenBitmap.Width - cropX);
        cropH = Math.Min(cropH, _frozenScreenBitmap.Height - cropY);

        if (cropW > 0 && cropH > 0) {
          using var cropped = _frozenScreenBitmap.Clone(new Rectangle(cropX, cropY, cropW, cropH), _frozenScreenBitmap.PixelFormat);
          AnnotatedCroppedImage.Source = ImageConversionHelper.ToBitmapSource(cropped);
          AnnotatedCroppedImage.Width = w;
          AnnotatedCroppedImage.Height = h;
        }
      }

      // Position Toolbar
      AnnotationToolbar.Visibility = Visibility.Visible;
      AnnotationToolbar.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
      var tbSize = AnnotationToolbar.DesiredSize;
      if (tbSize.Width <= 0 || tbSize.Height <= 0) {
        tbSize = new System.Windows.Size(320, 42);
      }

      var monitorBounds = GetActiveMonitorCanvasBounds(_selectedAreaRect);
      var tbPos = ToolbarPositioningHelper.CalculatePosition(
          _selectedAreaRect,
          tbSize,
          monitorBounds);

      Canvas.SetLeft(AnnotationToolbar, tbPos.X);
      Canvas.SetTop(AnnotationToolbar, tbPos.Y);

      // Default state: no active tool, arrow cursor
      SetActiveTool(AnnotationTool.None);
      BtnUndo.IsEnabled = false;
    } else if (_state == OverlayState.Annotating && _isDrawing && _currentDrawingShape != null) {
      _isDrawing = false;
      OverlayCanvas.ReleaseMouseCapture();

      bool isTooSmall = false;
      if (_currentDrawingShape is System.Windows.Shapes.Rectangle r) {
        isTooSmall = r.Width < 2 && r.Height < 2;
      } else if (_currentDrawingShape is System.Windows.Shapes.Path) {
        var endP = e.GetPosition(AnnotationCanvas);
        var dist = Math.Sqrt(Math.Pow(endP.X - _drawingStartPoint.X, 2) + Math.Pow(endP.Y - _drawingStartPoint.Y, 2));
        isTooSmall = dist < 3;
      }

      if (isTooSmall) {
        AnnotationCanvas.Children.Remove(_currentDrawingShape);
      } else {
        _annotationHistory.Add(_currentDrawingShape);
        BtnUndo.IsEnabled = true;
      }

      _currentDrawingShape = null;
    }
  }

  private void OnToolRectClick(object sender, RoutedEventArgs e) {
    SetActiveTool(_activeTool == AnnotationTool.Rectangle ? AnnotationTool.None : AnnotationTool.Rectangle);
  }

  private void OnToolArrowClick(object sender, RoutedEventArgs e) {
    SetActiveTool(_activeTool == AnnotationTool.Arrow ? AnnotationTool.None : AnnotationTool.Arrow);
  }

  private void SetActiveTool(AnnotationTool tool) {
    _activeTool = tool;

    var activeBg = new SolidColorBrush(Color.FromArgb(0x55, 0xFF, 0x17, 0x44));
    var activeBorder = new SolidColorBrush(Color.FromRgb(0xFF, 0x17, 0x44));

    if (_activeTool == AnnotationTool.Rectangle) {
      BtnToolRect.Background = activeBg;
      BtnToolRect.BorderBrush = activeBorder;
      BtnToolArrow.Background = Brushes.Transparent;
      BtnToolArrow.BorderBrush = Brushes.Transparent;
      Cursor = Cursors.Cross;
      OverlayCanvas.Cursor = Cursors.Cross;
    } else if (_activeTool == AnnotationTool.Arrow) {
      BtnToolArrow.Background = activeBg;
      BtnToolArrow.BorderBrush = activeBorder;
      BtnToolRect.Background = Brushes.Transparent;
      BtnToolRect.BorderBrush = Brushes.Transparent;
      Cursor = Cursors.Cross;
      OverlayCanvas.Cursor = Cursors.Cross;
    } else {
      BtnToolRect.Background = Brushes.Transparent;
      BtnToolRect.BorderBrush = Brushes.Transparent;
      BtnToolArrow.Background = Brushes.Transparent;
      BtnToolArrow.BorderBrush = Brushes.Transparent;
      Cursor = Cursors.Arrow;
      OverlayCanvas.Cursor = Cursors.Arrow;
    }
  }

  private void OnUndoClick(object sender, RoutedEventArgs e) {
    UndoLastAnnotation();
  }

  private void UndoLastAnnotation() {
    if (_annotationHistory.Count > 0) {
      var last = _annotationHistory[^1];
      _annotationHistory.RemoveAt(_annotationHistory.Count - 1);
      AnnotationCanvas.Children.Remove(last);
      BtnUndo.IsEnabled = _annotationHistory.Count > 0;
    }
  }

  private void OnCopyImageClick(object sender, RoutedEventArgs e) {
    FinishAndExportScreenshot(copyImageToClipboard: true);
  }

  private void OnDoneClick(object sender, RoutedEventArgs e) {
    FinishAndExportScreenshot(copyImageToClipboard: false);
  }

  private void OnCancelClick(object sender, RoutedEventArgs e) {
    CancelSelection();
  }

  private void FinishAndExportScreenshot(bool copyImageToClipboard = false) {
    byte[] pngBytes;

    var cropX = (int)Math.Max(0, _selectedAreaRect.Left * _dpiScaleX);
    var cropY = (int)Math.Max(0, _selectedAreaRect.Top * _dpiScaleY);
    var cropW = (int)Math.Max(1, _selectedAreaRect.Width * _dpiScaleX);
    var cropH = (int)Math.Max(1, _selectedAreaRect.Height * _dpiScaleY);

    if (_frozenScreenBitmap != null) {
      cropW = Math.Min(cropW, _frozenScreenBitmap.Width - cropX);
      cropH = Math.Min(cropH, _frozenScreenBitmap.Height - cropY);
    }

    if (_annotationHistory.Count == 0 && _frozenScreenBitmap != null) {
      var captureService = new ScreenCaptureService();
      pngBytes = captureService.CropToPngBytes(_frozenScreenBitmap, new Rectangle(cropX, cropY, cropW, cropH));
    } else if (_frozenScreenBitmap != null && cropW > 0 && cropH > 0) {
      using var croppedBmp = _frozenScreenBitmap.Clone(new Rectangle(cropX, cropY, cropW, cropH), _frozenScreenBitmap.PixelFormat);
      var bgSource = ImageConversionHelper.ToBitmapSource(croppedBmp);

      // Detach shapes from AnnotationCanvas so they can be added to the standalone exportCanvas
      AnnotationCanvas.Children.Clear();

      pngBytes = AnnotationExportHelper.CompositeAndRenderToPng(
          bgSource,
          _annotationHistory,
          _selectedAreaRect.Width,
          _selectedAreaRect.Height,
          _dpiScaleX,
          _dpiScaleY);
    } else {
      pngBytes = Array.Empty<byte>();
    }

    Close();
    ScreenshotReady?.Invoke(pngBytes, copyImageToClipboard);
  }

  private void CancelSelection() {
    _isSelecting = false;
    _isDrawing = false;
    OverlayCanvas.ReleaseMouseCapture();
    Close();
    Cancelled?.Invoke();
  }

  private static System.Windows.Input.Cursor GetOrCreateRedCrosshair() {
    if (_cachedRedCrosshair != null) return _cachedRedCrosshair;

    int size = 33;
    int hotspotX = 16;
    int hotspotY = 16;

    using var bmp = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
    using (var g = Graphics.FromImage(bmp)) {
      g.Clear(System.Drawing.Color.Transparent);

      var redColor = System.Drawing.Color.FromArgb(255, 255, 23, 68);
      var blackColor = System.Drawing.Color.FromArgb(220, 0, 0, 0);

      using var redPen = new System.Drawing.Pen(redColor, 2f);
      using var blackPen = new System.Drawing.Pen(blackColor, 1f);

      g.DrawLine(blackPen, 4, hotspotY - 1, size - 5, hotspotY - 1);
      g.DrawLine(blackPen, 4, hotspotY + 2, size - 5, hotspotY + 2);
      g.DrawLine(blackPen, 3, hotspotY, 3, hotspotY + 1);
      g.DrawLine(blackPen, size - 4, hotspotY, size - 4, hotspotY + 1);

      g.DrawLine(blackPen, hotspotX - 1, 4, hotspotX - 1, size - 5);
      g.DrawLine(blackPen, hotspotX + 2, 4, hotspotX + 2, size - 5);
      g.DrawLine(blackPen, hotspotX, 3, hotspotX + 1, 3);
      g.DrawLine(blackPen, hotspotX, size - 4, hotspotX + 1, size - 4);

      g.DrawLine(redPen, 4, hotspotY, size - 5, hotspotY);
      g.DrawLine(redPen, hotspotX, 4, hotspotX, size - 5);
    }

    using var pngMs = new MemoryStream();
    bmp.Save(pngMs, System.Drawing.Imaging.ImageFormat.Png);
    var pngBytes = pngMs.ToArray();

    using var curMs = new MemoryStream();
    using var bw = new BinaryWriter(curMs);

    bw.Write((short)0);
    bw.Write((short)2);
    bw.Write((short)1);

    bw.Write((byte)size);
    bw.Write((byte)size);
    bw.Write((byte)0);
    bw.Write((byte)0);
    bw.Write((short)hotspotX);
    bw.Write((short)hotspotY);
    bw.Write((int)pngBytes.Length);
    bw.Write((int)22);

    bw.Write(pngBytes);
    bw.Flush();

    curMs.Position = 0;
    _cachedRedCrosshair = new System.Windows.Input.Cursor(curMs);
    return _cachedRedCrosshair;
  }

  private Rect GetActiveMonitorCanvasBounds(Rect selectionRect) {
    try {
      var scaleX = _dpiScaleX > 0 ? _dpiScaleX : 1.0;
      var scaleY = _dpiScaleY > 0 ? _dpiScaleY : 1.0;

      var screenX = (int)Math.Round((Left + selectionRect.X) * scaleX);
      var screenY = (int)Math.Round((Top + selectionRect.Y) * scaleY);
      var screenW = (int)Math.Max(1, Math.Round(selectionRect.Width * scaleX));
      var screenH = (int)Math.Max(1, Math.Round(selectionRect.Height * scaleY));

      var rect = new Rectangle(screenX, screenY, screenW, screenH);
      var screen = System.Windows.Forms.Screen.FromRectangle(rect);

      var bounds = screen.Bounds;
      var canvasX = (bounds.X / scaleX) - Left;
      var canvasY = (bounds.Y / scaleY) - Top;
      var canvasW = bounds.Width / scaleX;
      var canvasH = bounds.Height / scaleY;

      return new Rect(canvasX, canvasY, canvasW, canvasH);
    } catch {
      return new Rect(0, 0, Width, Height);
    }
  }
}
