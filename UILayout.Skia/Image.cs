using System.Drawing;
using System.IO;
using System.Reflection;
using SkiaSharp;

namespace UILayout
{
    public partial class Image
    {
        SKBitmap bitmap;

        public SKBitmap Bitmap
        {
            get => bitmap;

            set
            {
                bitmap = value;

                this.Width = bitmap.Width;
                this.Height = bitmap.Height;
            }
        }

        public Image(string resourceName)
        {
            Assembly assembly = Assembly.GetCallingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(assembly.GetName().Name + ".Resources." + resourceName + ".png"))
            {
                Bitmap = SKBitmap.Decode(stream);
            }
        }

        public Image(SKBitmap bitmap)
        {
            this.Bitmap = bitmap;
        }

        public void Draw(float x, float y)
        {
            SkiaLayout.Current.Canvas.DrawBitmap(Bitmap, x, y);
        }

        public void Draw(in System.Drawing.Rectangle srcRectangle, in RectF destRectangle)
        {
            SkiaLayout.Current.Canvas.DrawBitmap(Bitmap, SKRectI.Create(srcRectangle.X, srcRectangle.Y, srcRectangle.Width, srcRectangle.Height), SKRect.Create(destRectangle.X, destRectangle.Y, destRectangle.Width, destRectangle.Height));
        }
    }
}
