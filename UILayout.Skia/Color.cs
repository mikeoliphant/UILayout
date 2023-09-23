using SkiaSharp;

namespace UILayout
{
    public partial class UIColor
    {
        public SKColor NativeColor;

        public UIColor(byte r, byte g, byte b)
            : this(r, g, b, 1)
        {

        }

        public UIColor(byte r, byte g, byte b, float opacity)
        {
            NativeColor = new SKColor(r, g, b, (byte)(opacity * 255));
        }

        public UIColor(SKColor nativeColor)
        {
            this.NativeColor = nativeColor;
        }
    }
}
