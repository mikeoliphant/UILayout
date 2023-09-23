using SkiaSharp;

namespace UILayout
{
    public class SkiaLayout : Layout
    {
        public static new SkiaLayout Current { get { return Layout.Current as SkiaLayout; } }

        public SkiaLayout()
        {
            GraphicsContext = new GraphicsContext2D();
        }
    }
}
