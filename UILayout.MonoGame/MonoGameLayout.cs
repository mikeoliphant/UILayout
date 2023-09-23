using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UILayout
{
    public class MonoGameLayout : Layout
    {
        public static new MonoGameLayout Current { get { return Layout.Current as MonoGameLayout; } }

        public Game Host { get; private set; }
        public GraphicsContext2D GraphicsContext { get; private set; }

        public MonoGameLayout(Game host)
        {
            this.Host = host;

            GraphicsContext = new MonoGameGraphicsContext2D(new SpriteBatch(Host.GraphicsDevice));
        }

        public override void Draw(UIElement startElement)
        {
            GraphicsContext.BeginDraw();

            base.Draw(startElement);

            GraphicsContext.EndDraw();
        }
    }
}
