using SkiaSharp;

namespace UILayout
{
    public partial class UIFont
    {
        public SKTypeface Typeface { get; set; }
        public float TextSize { get; set; }

        public float TextHeight
        {
            get { return ((float)TextSize * 3.0f) / 4.0f; }
        }

        static UIFont()
        {
            DefaultFont = new UIFont { Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), TextSize = 24 };
        }
    }
}
