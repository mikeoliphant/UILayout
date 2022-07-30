using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;

namespace UILayout
{
    public partial class TextBlock
    {       
        public Color TextColor
        {
            get { return new Color(textPaint.Color); }
            set { textPaint.Color = value.NativeColor; }
        }

        public Font TextFont
        {
            get { return new Font { Typeface = textPaint.Typeface, TextSize = textPaint.TextSize }; }
            set
            {
                textPaint.Typeface = value.Typeface;
                textPaint.TextSize = value.TextSize;
            }
        }

        SKPaint textPaint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            Style = SKPaintStyle.StrokeAndFill,
        };

        SKRect textBounds = SKRect.Empty;

        static TextBlock()
        {
            DefaultFont = new Font { Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), TextSize = 24 };
            DefaultColor = Color.Black;
        }

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

                textPaint.MeasureText(Text, ref textBounds);

                width = textBounds.Width;
                height = -textBounds.Top;
            }
        }

        protected override void DrawContents()
        {
            SkiaLayout.Current.Canvas.DrawText(Text, ContentBounds.Left, ContentBounds.Bottom, textPaint);
        }
    }
}
