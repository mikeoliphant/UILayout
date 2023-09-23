using SkiaSharp;

namespace UILayout
{
    public partial class UIColor
    {
        public SKColor NativeColor { get { return new SKColor(R, G, B, A); } }
    }
}
