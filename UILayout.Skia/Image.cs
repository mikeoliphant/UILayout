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

        // Loaded images are never changed: immutable, Skia draws them without copying, and a GPU canvas (as the SDL
        // window's) keeps their texture instead of uploading it again for every draw
        public UIImage(Stream stream)
        {
            SKBitmap bitmap = SKBitmap.Decode(stream);

            bitmap.SetImmutable();

            Bitmap = bitmap;
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