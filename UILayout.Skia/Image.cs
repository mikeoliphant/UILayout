using System;
using System.IO;
using System.Reflection;
using SkiaSharp;

namespace UILayout
{
    public partial class UIImage
    {
        public virtual int ActualWidth { get { return Bitmap.Width; } }
        public virtual int ActualHeight { get { return Bitmap.Height; } }

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

        public UIImage(string resourceName)
        {
            Assembly assembly = Assembly.GetEntryAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(assembly.GetName().Name + ".Resources." + resourceName + ".png"))
            {
                Bitmap = SKBitmap.Decode(stream);
            }
        }

        public UIImage(int width, int height)
            : this(new SKBitmap(width, height))
        {
        }

        public UIImage(SKBitmap bitmap)
        {
            this.Bitmap = bitmap;
        }

        public UIImage(UIImage baseImage)
        {
            Bitmap = baseImage.Bitmap;
        }

        public UIColor[] GetData()
        {
            throw new NotImplementedException();
        }

        public void SetData(UIColor[] setData)
        {
            throw new NotImplementedException();
        }
    }
}