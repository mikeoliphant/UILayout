using SkiaSharp;

namespace UILayout
{
    public partial class TextBlock
    {       
        SKPaint textPaint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            Style = SKPaintStyle.StrokeAndFill
        };

        SKRect textBounds = SKRect.Empty;

        static TextBlock()
        {
            DefaultFont = new UIFont { Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), TextSize = 24 };
            DefaultColor = UIColor.Black;
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

                textPaint.Typeface = TextFont.Typeface; // Shouldn't be doing this here
                textPaint.TextSize = TextFont.TextSize;
                textPaint.MeasureText(Text, ref textBounds);

                width = textBounds.Width;
                height = -textBounds.Top;
            }
        }
    }
}
