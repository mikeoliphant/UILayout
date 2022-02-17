using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;

namespace UILayout
{
    public partial class UIElement
    {
        public static SKCanvas Canvas { get; set; }

        public SKColor BackgroundColor
        {
            get { return BackgroundPaint.Color; }
            set { BackgroundPaint.Color = value; }
        }
        public SKSize BackgroundRoundRadius { get; set; }
        public SKPaint BackgroundPaint { get => backgroundPaint; set => backgroundPaint = value; }

        SKPaint backgroundPaint = new SKPaint
        {
            Color = SKColors.Transparent,
            IsAntialias = true,
            Style = SKPaintStyle.StrokeAndFill
        };

        public void Draw()
        {
            if (BackgroundPaint.Color.Alpha > 0)
            {
                if ((BackgroundRoundRadius.Width > 0) || (BackgroundRoundRadius.Height > 0))
                    Canvas.DrawRoundRect(LayoutBounds.ToSKRect(), BackgroundRoundRadius, BackgroundPaint);
                else
                    Canvas.DrawRect(LayoutBounds.ToSKRect(), BackgroundPaint);
            }

            DrawContents();
        }

        protected virtual void DrawContents()
        {

        }
    }
}
