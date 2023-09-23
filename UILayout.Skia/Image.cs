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
    }
}
