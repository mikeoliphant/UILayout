using System;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UILayout
{
    public class MonoGameLayout : Layout
    {
        public static new MonoGameLayout Current { get { return Layout.Current as MonoGameLayout; } }

        public Game Host { get; private set; }

        public MonoGameLayout()
        {
        }

        public virtual void SetHost(Game host)
        {
            this.Host = host;

            GraphicsContext = new GraphicsContext2D(new SpriteBatch(Host.GraphicsDevice));
        }

        public void LoadImageManifest(string manifestName)
        {
            using (Stream manifestStream = File.OpenRead(Path.Combine(Path.Combine(Host.Content.RootDirectory, "Textures"), manifestName)))
            {
                ImageManifest.Load(manifestStream, this);
            }
        }

        public override void Draw(UIElement startElement)
        {
            GraphicsContext.BeginDraw();

            base.Draw(startElement);

            GraphicsContext.EndDraw();
        }
    }
}
