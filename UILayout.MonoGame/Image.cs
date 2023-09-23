using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UILayout
{
    public partial class Image
    {
        public Texture2D Texture { get; private set; }

        public int XOffset { get; set; }
        public int YOffset { get; set; }

        public Image(string resourceName)
        {
            Texture = MonoGameLayout.Current.Host.Content.Load<Texture2D>(Path.Combine("Textures", resourceName));
        }

        public void Draw(float x, float y)
        {
            MonoGameLayout.Current.GraphicsContext.DrawImage(this, x, y, 0.5f);
        }

        public void Draw(in System.Drawing.Rectangle srcRectangle, in RectF destRectangle)
        {
            MonoGameLayout.Current.GraphicsContext.DrawImage(this, 0.5f, new Rectangle(srcRectangle.X, srcRectangle.Y, srcRectangle.Width, srcRectangle.Height),
                new Rectangle((int)destRectangle.X, (int)destRectangle.Y, (int)destRectangle.Width, (int)destRectangle.Height));
        }
    }
}
