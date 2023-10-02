﻿using System;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UILayout
{
    public class MonoGameLayout : Layout
    {
        public static new MonoGameLayout Current { get { return Layout.Current as MonoGameLayout; } }

        public MonoGameHost Host { get; private set; }

        public MonoGameLayout()
        {
        }

        public virtual void SetHost(Game host)
        {
            this.Host = host as MonoGameHost;

            GraphicsContext = new GraphicsContext2D(new SpriteBatch(Host.GraphicsDevice));
        }

        public void LoadImageManifest(string manifestName)
        {
            using (Stream manifestStream = Host.OpenContentStream(Path.Combine("Textures", manifestName)))
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
