using SkiaSharp;

namespace UILayout
{
    public partial class Font
    {
        public SKTypeface Typeface { get; set; }
        public float TextSize { get; set; }

        public float TextHeight
        {
            get { return ((float)TextSize * 3.0f) / 4.0f; }
        }
    }
}
