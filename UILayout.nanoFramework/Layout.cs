using nanoFramework.UI;
using nanoFramework.Presentation.Media;

namespace UILayout
{
    public class BitmapLayout : Layout
    {
        public new static BitmapLayout Current { get { return Layout.Current as BitmapLayout;  } }

        public Bitmap FullScreenBitmap { get; set; }

        public override void Draw(UIElement startElement)
        {
            if (!haveDirty)
                return;

            int dirtyX = (int)dirtyRect.X;
            int dirtyY = (int)dirtyRect.Y;
            int dirtyWidth = (int)Math.Cieling(dirtyRect.Width);
            int dirtyHeight = (int)Math.Cieling(dirtyRect.Height);

            FullScreenBitmap.SetClippingRectangle(dirtyX, dirtyY, dirtyWidth, dirtyHeight);

            base.Draw(startElement);       
            
            FullScreenBitmap.Flush(dirtyX, dirtyY, dirtyWidth, dirtyHeight);
        }
    }
}
