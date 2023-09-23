using SkiaSharp;

namespace UILayout
{
    public partial class UIElement
    {
        public UIColor BackgroundColor
        {
            get { return new UIColor(backgroundPaint.Color); }
            set { backgroundPaint.Color = value.NativeColor; }
        }

        public SKSize BackgroundRoundRadius { get; set; }

        SKPaint backgroundPaint = new SKPaint
        {
            Color = SKColors.Transparent,
            IsAntialias = false,
            Style = SKPaintStyle.StrokeAndFill
        };

        public void Draw()
        {
            // Don't draw if we aren't in the diry rectangle
            if (!Layout.Current.HaveDirty && !Layout.Current.DirtyRect.Intersects(layoutBounds))
                return;

            if (backgroundPaint.Color.Alpha > 0)
            {
                if ((BackgroundRoundRadius.Width > 0) || (BackgroundRoundRadius.Height > 0))
                    SkiaLayout.Current.Canvas.DrawRoundRect(LayoutBounds.ToSKRect(), BackgroundRoundRadius, backgroundPaint);
                else
                    SkiaLayout.Current.Canvas.DrawRect(LayoutBounds.ToSKRect(), backgroundPaint);
            }

            DrawContents();
        }

        protected virtual void DrawContents()
        {

        }
    }
}
