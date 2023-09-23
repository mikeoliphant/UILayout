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

            Width = Texture.Width;
            Height = Texture.Height;
        }
    }
}
