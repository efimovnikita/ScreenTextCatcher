using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Drawing;
using Point = System.Windows.Point;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using MouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;

namespace ScreenTextCatcher.Views;

public partial class OverlayWindow : Window
{
    private static System.Windows.Input.Cursor? _cachedRedCrosshair;
    private Point _startPoint;
    private bool _isSelecting = false;

    public event Action<Rectangle>? AreaSelected;
    public event Action? Cancelled;

    public OverlayWindow()
    {
        InitializeComponent();

        Cursor = GetOrCreateRedCrosshair();
        OverlayCanvas.Cursor = Cursor;

        Left = SystemParameters.VirtualScreenLeft;
        Top = SystemParameters.VirtualScreenTop;
        Width = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;

        Loaded += (s, e) =>
        {
            Canvas.SetLeft(HintBorder, (Width - HintBorder.ActualWidth) / 2);
            Focus();
            Activate();
        };

        KeyDown += OnWindowKeyDown;
    }

    private void OnWindowKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            CancelSelection();
        }
    }

    private void OnCanvasMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.RightButton == MouseButtonState.Pressed)
        {
            CancelSelection();
            return;
        }

        if (e.LeftButton == MouseButtonState.Pressed)
        {
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
        }
    }

    private void OnCanvasMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isSelecting)
        {
            return;
        }

        var currentPoint = e.GetPosition(OverlayCanvas);

        var x = Math.Min(_startPoint.X, currentPoint.X);
        var y = Math.Min(_startPoint.Y, currentPoint.Y);
        var w = Math.Abs(currentPoint.X - _startPoint.X);
        var h = Math.Abs(currentPoint.Y - _startPoint.Y);

        Canvas.SetLeft(SelectionRect, x);
        Canvas.SetTop(SelectionRect, y);
        SelectionRect.Width = w;
        SelectionRect.Height = h;

        // Position corner brackets
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
    }

    private void OnCanvasMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isSelecting)
        {
            return;
        }

        _isSelecting = false;
        OverlayCanvas.ReleaseMouseCapture();

        var endPoint = e.GetPosition(OverlayCanvas);
        var w = Math.Abs(endPoint.X - _startPoint.X);
        var h = Math.Abs(endPoint.Y - _startPoint.Y);

        // Minimum threshold to prevent accidental clicks
        if (w < 8 || h < 8)
        {
            CancelSelection();
            return;
        }

        var left = Math.Min(_startPoint.X, endPoint.X);
        var top = Math.Min(_startPoint.Y, endPoint.Y);

        var source = PresentationSource.FromVisual(this);
        var scaleX = source?.CompositionTarget?.TransformToDevice.M11 ?? 1.0;
        var scaleY = source?.CompositionTarget?.TransformToDevice.M22 ?? 1.0;

        var screenX = (int)((Left + left) * scaleX);
        var screenY = (int)((Top + top) * scaleY);
        var screenW = (int)(w * scaleX);
        var screenH = (int)(h * scaleY);

        var selectedRect = new Rectangle(screenX, screenY, screenW, screenH);

        Close();
        AreaSelected?.Invoke(selectedRect);
    }

    private void CancelSelection()
    {
        _isSelecting = false;
        OverlayCanvas.ReleaseMouseCapture();
        Close();
        Cancelled?.Invoke();
    }

    private static System.Windows.Input.Cursor GetOrCreateRedCrosshair()
    {
        if (_cachedRedCrosshair != null) return _cachedRedCrosshair;

        int size = 33;
        int hotspotX = 16;
        int hotspotY = 16;

        using var bmp = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.Clear(System.Drawing.Color.Transparent);

            // Bright vivid neon red: #FF1744
            var redColor = System.Drawing.Color.FromArgb(255, 255, 23, 68);
            var blackColor = System.Drawing.Color.FromArgb(220, 0, 0, 0);

            using var redPen = new System.Drawing.Pen(redColor, 2f);
            using var blackPen = new System.Drawing.Pen(blackColor, 1f);

            // Black outline for high contrast against white and black screens
            g.DrawLine(blackPen, 4, hotspotY - 1, size - 5, hotspotY - 1);
            g.DrawLine(blackPen, 4, hotspotY + 2, size - 5, hotspotY + 2);
            g.DrawLine(blackPen, 3, hotspotY, 3, hotspotY + 1);
            g.DrawLine(blackPen, size - 4, hotspotY, size - 4, hotspotY + 1);

            g.DrawLine(blackPen, hotspotX - 1, 4, hotspotX - 1, size - 5);
            g.DrawLine(blackPen, hotspotX + 2, 4, hotspotX + 2, size - 5);
            g.DrawLine(blackPen, hotspotX, 3, hotspotX + 1, 3);
            g.DrawLine(blackPen, hotspotX, size - 4, hotspotX + 1, size - 4);

            // Main vivid red crosshair lines (2px thick)
            g.DrawLine(redPen, 4, hotspotY, size - 5, hotspotY);
            g.DrawLine(redPen, hotspotX, 4, hotspotX, size - 5);
        }

        using var pngMs = new MemoryStream();
        bmp.Save(pngMs, System.Drawing.Imaging.ImageFormat.Png);
        var pngBytes = pngMs.ToArray();

        using var curMs = new MemoryStream();
        using var bw = new BinaryWriter(curMs);

        // CUR Header
        bw.Write((short)0); // reserved
        bw.Write((short)2); // 2 = CURSOR
        bw.Write((short)1); // 1 image

        // CUR Directory Entry
        bw.Write((byte)size);       // width
        bw.Write((byte)size);       // height
        bw.Write((byte)0);          // colors
        bw.Write((byte)0);          // reserved
        bw.Write((short)hotspotX);  // X hotspot
        bw.Write((short)hotspotY);  // Y hotspot
        bw.Write((int)pngBytes.Length); // size of image
        bw.Write((int)22);          // offset of image (6 + 16 = 22)

        // Write image data
        bw.Write(pngBytes);
        bw.Flush();

        curMs.Position = 0;
        _cachedRedCrosshair = new System.Windows.Input.Cursor(curMs);
        return _cachedRedCrosshair;
    }
}
