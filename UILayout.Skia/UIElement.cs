using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;

namespace UILayout
{
    public partial class UIElement
    {
        public Color BackgroundColor
        {
            get { return new Color(backgroundPaint.Color); }
            set { backgroundPaint.Color = value.NativeColor; }
        }

        public SKSize BackgroundRoundRadius { get; set; }
        SKPaint backgroundPaint = new SKPaint
        {
            Color = SKColors.Transparent,
            IsAntialias = true,
            Style = SKPaintStyle.StrokeAndFill
        };

        public void Draw()
        {
            // Don't draw if we aren't in the diry rectangle
            if (!Layout.Current.HaveDirty && !Layout.Current.DirtyRect.Intersects(ref layoutBounds))
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
