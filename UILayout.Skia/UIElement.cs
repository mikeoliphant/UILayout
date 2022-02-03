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
            get { return backgroundPaint.Color; }
            set { backgroundPaint.Color = value; }
        }

        SKPaint backgroundPaint = new SKPaint
        {
            Color = SKColors.Transparent,
            IsAntialias = true,
            Style = SKPaintStyle.StrokeAndFill
        };

        public void Draw()
        {   
            Canvas.DrawRect(LayoutBounds.ToSKRect(), backgroundPaint);

            DrawContents();
        }

        protected virtual void DrawContents()
        {

        }
    }
}
