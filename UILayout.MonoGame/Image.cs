using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UILayout
{
    public partial class UIImage
    {
        public Texture2D Texture { get; private set; }
        public virtual int ActualWidth { get { return Texture.Width; } }
        public virtual int ActualHeight { get { return Texture.Height; } }

        public UIImage(int width, int height)
            : this(new Texture2D(MonoGameLayout.Current.Host.GraphicsDevice, width, height, false, SurfaceFormat.Color))
        {
            // OpenGL leaves a new texture's contents undefined (garbage on Linux drivers, for example): start transparent.
            Texture.SetData(new Color[width * height]);
        }

        public UIImage(Texture2D texture)
        {
            this.Texture = texture;

            Width = Texture.Width;
            Height = Texture.Height;
        }

        /// <summary>
        /// Loads an image (PNG, JPEG, ...) with premultiplied alpha, as everything is drawn, and with mipmaps, so it
        /// draws smoothly when smaller than its size.
        /// </summary>
        public UIImage(Stream stream)
            : this(stream, mipmaps: true)
        {
        }

        public UIImage(Stream stream, bool mipmaps)
        {
            Texture2D texture = Texture2D.FromStream(MonoGameLayout.Current.Host.GraphicsDevice, stream,
                DefaultColorProcessors.PremultiplyAlpha);

            if (mipmaps)
                texture = WithMipmaps(texture);

            this.Texture = texture;

            Width = Texture.Width;
            Height = Texture.Height;
        }

        // A copy of the texture with mipmaps, each level a box filter of the one above (of premultiplied colors, so
        // transparent pixels don't darken their neighbors). Disposes the original.
        static Texture2D WithMipmaps(Texture2D source)
        {
            int width = source.Width;
            int height = source.Height;

            Color[] data = new Color[width * height];

            source.GetData(data);

            Texture2D texture = new Texture2D(source.GraphicsDevice, width, height, mipmap: true, SurfaceFormat.Color);

            texture.SetData(0, null, data, 0, data.Length);

            for (int level = 1; level < texture.LevelCount; level++)
            {
                int levelWidth = Math.Max(1, width / 2);
                int levelHeight = Math.Max(1, height / 2);

                Color[] levelData = new Color[levelWidth * levelHeight];

                for (int y = 0; y < levelHeight; y++)
                {
                    int y0 = Math.Min(y * 2, height - 1);
                    int y1 = Math.Min((y * 2) + 1, height - 1);

                    for (int x = 0; x < levelWidth; x++)
                    {
                        int x0 = Math.Min(x * 2, width - 1);
                        int x1 = Math.Min((x * 2) + 1, width - 1);

                        Color c00 = data[(y0 * width) + x0];
                        Color c10 = data[(y0 * width) + x1];
                        Color c01 = data[(y1 * width) + x0];
                        Color c11 = data[(y1 * width) + x1];

                        levelData[(y * levelWidth) + x] = new Color(
                            (byte)((c00.R + c10.R + c01.R + c11.R + 2) / 4),
                            (byte)((c00.G + c10.G + c01.G + c11.G + 2) / 4),
                            (byte)((c00.B + c10.B + c01.B + c11.B + 2) / 4),
                            (byte)((c00.A + c10.A + c01.A + c11.A + 2) / 4));
                    }
                }

                texture.SetData(level, null, levelData, 0, levelData.Length);

                data = levelData;
                width = levelWidth;
                height = levelHeight;
            }

            source.Dispose();

            return texture;
        }

        public UIImage(UIImage baseImage)
        {
            Texture = baseImage.Texture;
            Density = baseImage.Density;
        }

        UIColor[] premultipliedData;

        /// <summary>The image's pixels, with straight (not premultiplied) alpha.</summary>
        public UIColor[] GetData()
        {
            UIColor[] tmpData = new UIColor[Width * Height];

            Texture.GetData<UIColor>(0, new Rectangle(XOffset, YOffset, Width, Height), tmpData, 0, Width * Height);

            for (int i = 0; i < tmpData.Length; i++)
            {
                UIColor color = tmpData[i];

                if ((color.A != 0) && (color.A != 255))
                {
                    tmpData[i] = new UIColor((byte)Math.Min(255, (color.R * 255) / color.A),
                        (byte)Math.Min(255, (color.G * 255) / color.A), (byte)Math.Min(255, (color.B * 255) / color.A),
                        color.A);
                }
            }

            return tmpData;
        }

        /// <summary>Sets the image's pixels, given with straight (not premultiplied) alpha.</summary>
        public void SetData(UIColor[] setData)
        {
            if ((premultipliedData == null) || (premultipliedData.Length != setData.Length))
                premultipliedData = new UIColor[setData.Length];

            for (int i = 0; i < setData.Length; i++)
            {
                UIColor color = setData[i];

                premultipliedData[i] = new UIColor((byte)((color.R * color.A) / 255), (byte)((color.G * color.A) / 255),
                    (byte)((color.B * color.A) / 255), color.A);
            }

            Texture.SetData<UIColor>(0, new Rectangle(XOffset, YOffset, Width, Height), premultipliedData, 0,
                setData.Length);
        }
    }
}
