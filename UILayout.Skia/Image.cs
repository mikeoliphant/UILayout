using System;
using System.IO;
using System.Reflection;
using SkiaSharp;

namespace UILayout
{
    public partial class UIImage
    {
        public static Assembly ResourceAssembly { get; set; } = Assembly.GetEntryAssembly();

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
            using (Stream stream = ResourceAssembly.GetManifestResourceStream(ResourceAssembly.GetName().Name + ".Textures." + resourceName + ".png"))
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

        public void SetPixel(int x, int y, SKColor color)
        {
            Bitmap.SetPixel(x, y, color);
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