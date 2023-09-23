using SkiaSharp;

namespace UILayout
{
    public partial class ImageElement
    {
        protected override void DrawContents()
        {
            SkiaLayout.Current.Canvas.DrawBitmap(Image.Bitmap, ContentBounds.X, ContentBounds.Y);
        }
    }
}
