using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace UILayout
{
    public class MonoGameLayout : Layout
    {
        public static new MonoGameLayout Current { get { return Layout.Current as MonoGameLayout; } }

        public Game Host { get; private set; }
        public GraphicsContext2D GraphicsContext { get; private set; }

        MouseState lastMouseState;

        public MonoGameLayout(Game host)
        {
            this.Host = host;

            GraphicsContext = new MonoGameGraphicsContext2D(new SpriteBatch(Host.GraphicsDevice));
        }

        public override void Draw(UIElement startElement)
        {
            MouseState mouseState = Mouse.GetState();

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (lastMouseState.LeftButton == ButtonState.Released)
                {
                    HandleTouch(new Touch()
                    {
                        Position = new PointF(mouseState.X, mouseState.Y),
                        TouchState = ETouchState.Pressed
                    });
                }
            }
            else
            {
                if (lastMouseState.LeftButton == ButtonState.Pressed)
                {
                    HandleTouch(new Touch()
                    {
                        Position = new PointF(mouseState.X, mouseState.Y),
                        TouchState = ETouchState.Released
                    });
                }
            }

            lastMouseState = mouseState;

            GraphicsContext.BeginDraw();

            base.Draw(startElement);

            GraphicsContext.EndDraw();
        }
    }
}
