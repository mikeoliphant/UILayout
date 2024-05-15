﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#if !ANDROID
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
#endif

namespace UILayout
{
    public class MonoGameLayout : Layout
    {        
        public static new MonoGameLayout Current { get { return Layout.Current as MonoGameLayout; } }

        public MonoGameHost Host { get; private set; }

        public float Scale { get; set; } = 1.0f;
        public override bool InputIsActive
        {
            get
            {
#if ANDROID
return true;
#else
                return System.Windows.Forms.Form.ActiveForm == (System.Windows.Forms.Control.FromHandle(Host.Window.Handle) as System.Windows.Forms.Form);
#endif
            }
        }

        public MonoGameLayout()
        {
        }

        public virtual void SetHost(Game host)
        {
            this.Host = host as MonoGameHost;

            GraphicsContext = new GraphicsContext2D(new SpriteBatch(Host.GraphicsDevice)) { Scale = Scale };
        }

        public void LoadImageManifest(string manifestName)
        {
            using (Stream manifestStream = Host.OpenContentStream(Path.Combine("Textures", manifestName)))
            {
                ImageManifest.Load(manifestStream, this);
            }
        }

        public override Task<string> GetKeyboardInputAsync(string title, string defaultText)
        {
            return KeyboardInput.Show(title, null);
        }

        public override void SetBounds(in RectF bounds)
        {
            base.SetBounds(new RectF(bounds.X, bounds.Y, bounds.Width / Scale, bounds.Height / Scale));
        }

        public override void Draw(UIElement startElement)
        {
            GraphicsContext.BeginDraw();

            base.Draw(startElement);

            GraphicsContext.EndDraw();
        }
    }
}
