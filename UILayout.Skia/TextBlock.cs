using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;

namespace UILayout
{
    public partial class TextBlock
    {
        public SKPaint TextPaint { get; set; }

        SKRect textBounds = SKRect.Empty;

        protected override void GetContentSize(out float width, out float height)
        {
            if (string.IsNullOrEmpty(Text))
            {
                width = 0;
                height = 0;
            }
            else
            {
                textBounds = SKRect.Empty;

                TextPaint.MeasureText(Text, ref textBounds);

                width = textBounds.Width;
                height = -textBounds.Top;
            }
        }

        protected override void DrawContents()
        {
            SkiaLayout.Current.Canvas.DrawText(Text, ContentBounds.Left, ContentBounds.Bottom, TextPaint);
        }
    }
}
