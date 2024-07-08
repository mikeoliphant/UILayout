﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace UILayout
{
    public class MonoGameLayout : Layout
    {        
        public static new MonoGameLayout Current { get { return Layout.Current as MonoGameLayout; } }

        public MonoGameHost Host { get; private set; }
        public float Scale
        {
            get => scale;
            set
            {
                if (GraphicsContext != null)
                    GraphicsContext.Scale = value;

                if (!unscaledBounds.IsEmpty)
                {
                    base.SetBounds(new RectF(unscaledBounds.X, unscaledBounds.Y, unscaledBounds.Width / Scale, unscaledBounds.Height / Scale));
                }

                scale = value;
            }
        }

        float scale = 1.0f;
        RectF unscaledBounds = RectF.Empty;

        public override bool InputIsActive
        {
            get
            {
#if WINDOWS
                return System.Windows.Forms.Form.ActiveForm == (System.Windows.Forms.Control.FromHandle(Host.Window.Handle) as System.Windows.Forms.Form);
#else
                return true;
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
#if WINDOWS || ANDROID
            return KeyboardInput.Show(title, null, defaultText);
#else
            Process process = new Process();
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.FileName = "zenity";
            process.StartInfo.Arguments = "--entry --title=\"" + title + "\" --entry-text=\"" + defaultText + "\"";

            process.Start();
            return process.StandardOutput.ReadToEndAsync();
#endif
        }

        public override void SetBounds(in RectF bounds)
        {
            this.unscaledBounds = bounds;

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
