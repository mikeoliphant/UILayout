using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkiaSharp;

namespace UILayout.Skia.WPF
{
    [DefaultEvent("PaintSurface")]
    [DefaultProperty("Name")]
    public class LayoutControl : FrameworkElement
    {
        private const double BitmapDpi = 96.0;

        private readonly bool designMode;

        private WriteableBitmap bitmap;
        private SKSurface surface;
        private bool ignorePixelScaling;

        float scaleX = 1.0f;
        float scaleY = 1.0f;
        bool needRePaint = false;

        // When the last frame was rendered, for the layout's update
        TimeSpan lastRenderingTime = TimeSpan.Zero;

        public SkiaLayout Layout { get; private set; }

        public SKSize CanvasSize { get; private set; }

        public bool IgnorePixelScaling
        {
            get => ignorePixelScaling;
            set
            {
                ignorePixelScaling = value;

                UpdatePaint();
            }
        }

        public LayoutControl()
        {
            designMode = DesignerProperties.GetIsInDesignMode(this);

            // Takes keyboard focus (on a click), for keys and typed text; no focus rectangle around the layout
            Focusable = true;
            FocusVisualStyle = null;
        }

        public void SetLayout(SkiaLayout layout)
        {
            this.Layout = layout;

            // Repainted only when something is dirty, and only the dirty area copied to the screen
            layout.RedrawsDirtyArea = true;

            // Its size is the layout's now
            InvalidateMeasure();
        }

        /// <summary>
        /// The room the layout's root element needs (in WPF's device-independent units), so the control can be sized to
        /// its layout, as a window with SizeToContent is. Given more, the layout fills it.
        /// </summary>
        protected override Size MeasureOverride(Size availableSize)
        {
            if ((Layout == null) || (Layout.RootUIElement == null))
                return base.MeasureOverride(availableSize);

            Layout.RootUIElement.GetSize(out float width, out float height);

            // Layout pixels are device pixels (as mouse positions are converted to them), unless pixel scaling is off
            DpiScale dpi = VisualTreeHelper.GetDpi(this);

            double scaleX = IgnorePixelScaling ? 1 : dpi.DpiScaleX;
            double scaleY = IgnorePixelScaling ? 1 : dpi.DpiScaleY;

            return new Size(Math.Ceiling(width) / scaleX, Math.Ceiling(height) / scaleY);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            CompositionTarget.Rendering += CompositionTarget_Rendering;

            MouseDown += LayoutControl_MouseDown;
            MouseUp += LayoutControl_MouseUp;
            MouseMove += LayoutControl_MouseMove;
            MouseLeave += LayoutControl_MouseLeave;
            MouseWheel += LayoutControl_MouseWheel;

            KeyDown += LayoutControl_KeyDown;
            KeyUp += LayoutControl_KeyUp;
            TextInput += LayoutControl_TextInput;
            LostKeyboardFocus += LayoutControl_LostKeyboardFocus;
        }

        // WPF's own units: 120 per notch, positive away from the user
        private void LayoutControl_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            Layout.InputManager.AddMouseWheel(e.Delta);

            e.Handled = true;
        }

        // The key's virtual-key code (InputKey's values). With Alt held, WPF reports the key as System.
        static InputKey ToInputKey(System.Windows.Input.KeyEventArgs e)
        {
            System.Windows.Input.Key key = e.Key;

            if (key == System.Windows.Input.Key.System)
                key = e.SystemKey;
            else if (key == System.Windows.Input.Key.ImeProcessed)
                key = e.ImeProcessedKey;

            return (InputKey)System.Windows.Input.KeyInterop.VirtualKeyFromKey(key);
        }

        // Keys WPF would otherwise use to move keyboard focus away (the layout uses them itself)
        static bool IsNavigationKey(System.Windows.Input.Key key)
        {
            switch (key)
            {
                case System.Windows.Input.Key.Tab:
                case System.Windows.Input.Key.Left:
                case System.Windows.Input.Key.Right:
                case System.Windows.Input.Key.Up:
                case System.Windows.Input.Key.Down:
                case System.Windows.Input.Key.PageUp:
                case System.Windows.Input.Key.PageDown:
                case System.Windows.Input.Key.Home:
                case System.Windows.Input.Key.End:
                    return true;
            }

            return false;
        }

        // Only navigation keys are marked handled: a handled key types no text (TextInput isn't raised for it), and Alt
        // shortcuts (as Alt+F4) still reach the window
        private void LayoutControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Layout.InputManager.KeyDown(ToInputKey(e));

            if (IsNavigationKey(e.Key))
                e.Handled = true;
        }

        private void LayoutControl_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Layout.InputManager.KeyUp(ToInputKey(e));
        }

        // Typed text, to the element with focus (a text box). Backspace and Enter come as text too ('\b', '\r'): they
        // are keys to the layout, so control characters are left out.
        private void LayoutControl_TextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (char.IsControl(c))
                    continue;

                if (Layout.HandleTextInput(c))
                    e.Handled = true;
            }
        }

        // Key releases go elsewhere now: nothing stays held
        private void LayoutControl_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            Layout.InputManager.ReleaseAllKeys();
        }

        Vector2 GetPostion(in Point p)
        {
            return new Vector2((float)p.X * (IgnorePixelScaling ? 1.0f : scaleX), (float)p.Y * (IgnorePixelScaling ? 1.0f : scaleY));
        }

        private void LayoutControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // Off the layout: the mouse is over nothing
            Layout.InputManager.SetMousePosition(new Vector2(float.MinValue, float.MinValue));

            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(this);

                Touch touch = new Touch()
                {
                    Position = GetPostion(p),
                    TouchState = ETouchState.Invalid
                };

                Layout.HandleTouch(touch);
            }
        }

        // The mouse is tracked as it moves (for hover), and is a touch moving while the button is down
        private void LayoutControl_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Vector2 position = GetPostion(e.GetPosition(this));

            Layout.InputManager.SetMousePosition(position);

            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                Touch touch = new Touch()
                {
                    Position = position,
                    TouchState = ETouchState.Moved
                };

                Layout.HandleTouch(touch);
            }
        }

        private void LayoutControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Clicked: keys and typed text come here
            Focus();

            Point p = e.GetPosition(this);

            Touch touch = new Touch()
            {
                Position = GetPostion(p),
                TouchState = ETouchState.Pressed
            };

            Layout.HandleTouch(touch);
        }

        private void LayoutControl_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this);

            Touch touch = new Touch()
            {
                Position = GetPostion(p),
                TouchState = ETouchState.Released
            };

            Layout.HandleTouch(touch);
        }

        // Each frame the layout is updated (as the MonoGame hosts do: what the mouse is over, for one), and repainted if
        // anything is dirty
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (designMode || (Layout == null))
                return;

            TimeSpan renderingTime = ((System.Windows.Media.RenderingEventArgs)e).RenderingTime;

            float secondsElapsed = (lastRenderingTime == TimeSpan.Zero) ? 0 :
                (float)(renderingTime - lastRenderingTime).TotalSeconds;

            lastRenderingTime = renderingTime;

            Layout.Update(secondsElapsed);

            if (Layout.HaveDirty)
                UpdatePaint();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (designMode)
                return;

            if (Visibility != Visibility.Visible || PresentationSource.FromVisual(this) == null)
                return;

            var size = CreateSize(out var unscaledSize, out scaleX, out scaleY);
            var userVisibleSize = IgnorePixelScaling ? unscaledSize : size;

            CanvasSize = userVisibleSize;

            if (size.Width <= 0 || size.Height <= 0)
                return;

            var info = new SKImageInfo(size.Width, size.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

            // reset the bitmap if the size has changed
            if (bitmap == null || info.Width != bitmap.PixelWidth || info.Height != bitmap.PixelHeight)
            {
                needRePaint = true;

                bitmap = new WriteableBitmap(info.Width, size.Height, BitmapDpi * scaleX, BitmapDpi * scaleY, PixelFormats.Pbgra32, null);

                surface = SKSurface.Create(info, bitmap.BackBuffer, bitmap.BackBufferStride);

                if (Layout != null)
                {
                    Layout.SetBounds(new RectF(0, 0, info.Width, info.Height));

                    Layout.UpdateLayout();
                }
            }

            if (needRePaint)
            {
                needRePaint = false;

                bitmap.Lock();

                if (IgnorePixelScaling)
                {
                    var canvas = surface.Canvas;
                    canvas.Scale(scaleX, scaleY);
                    canvas.Save();
                }

                RectF dirtyRect = Layout.DirtyRect;
                dirtyRect.IntersectWith(Layout.Bounds);

                PaintSurface(surface, info.WithSize(userVisibleSize));

                Int32Rect bitmapDirty = new Int32Rect((int)dirtyRect.X, (int)dirtyRect.Y, (int)Math.Ceiling(dirtyRect.Width), (int)Math.Ceiling(dirtyRect.Height));

                if ((bitmapDirty.X < bitmap.Width) && (bitmapDirty.Y < bitmap.Height))
                {
                    if ((bitmapDirty.X + Width) > bitmap.Width)
                    {
                        bitmapDirty.Width = (int)(bitmap.Width - bitmapDirty.X);
                    }

                    if ((bitmapDirty.Y + Height) > bitmap.Height)
                    {
                        bitmapDirty.Height = (int)(bitmap.Height - bitmapDirty.Y);
                    }

                    bitmap.AddDirtyRect(bitmapDirty);
                }

                bitmap.Unlock();
            }

            drawingContext.DrawImage(bitmap, new Rect(0, 0, ActualWidth, ActualHeight));
        }

        protected virtual void PaintSurface(SKSurface surface, SKImageInfo info)
        {
            SKCanvas canvas = surface.Canvas;

            if (Layout == null)
            {
                canvas.Clear(SKColors.White);
            }
            else
            {
                Layout.GraphicsContext.Canvas = canvas;

                if (!Layout.Bounds.IsEmpty)
                    Layout.Draw();
            }
        }

        public void UpdatePaint()
        {
            InvalidateVisual();

            needRePaint = true;
        }

        public void RePaint()
        {
            UpdatePaint();
        }

        private SKSizeI CreateSize(out SKSizeI unscaledSize, out float scaleX, out float scaleY)
        {
            unscaledSize = SKSizeI.Empty;
            scaleX = 1.0f;
            scaleY = 1.0f;

            var w = ActualWidth;
            var h = ActualHeight;

            if (!IsPositive(w) || !IsPositive(h))
                return SKSizeI.Empty;

            unscaledSize = new SKSizeI((int)w, (int)h);

            var m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
            scaleX = (float)m.M11;
            scaleY = (float)m.M22;
            return new SKSizeI((int)(w * scaleX), (int)(h * scaleY));

            bool IsPositive(double value)
            {
                return !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
            }
        }
    }
}