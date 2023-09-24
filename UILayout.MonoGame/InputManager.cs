using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace UILayout
{
    public partial class InputManager
    {
        KeyboardState lastState;
        KeyboardState currentState;

        MouseState lastMouseState;

        internal bool IsKeyDown(InputKey key)
        {
            return currentState.IsKeyDown((Keys)key);
        }

        internal bool WasPressed(InputKey key)
        {
            return currentState.IsKeyDown((Keys)key) && !lastState.IsKeyDown((Keys)key);
        }

        internal bool WasReleased(InputKey key)
        {
            return !currentState.IsKeyDown((Keys)key) && lastState.IsKeyDown((Keys)key);
        }

        protected void PlatformUpdate(float secondsElapsed)
        {
            lastState = currentState;
            currentState = Keyboard.GetState();
        }

        public IEnumerable<Touch> GetTouches()
        {
            MouseState mouseState = Mouse.GetState();

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (lastMouseState.LeftButton == ButtonState.Released)
                {
                    yield return new Touch()
                    {
                        Position = new PointF(mouseState.X, mouseState.Y),
                        TouchState = ETouchState.Pressed
                    };
                }
                else
                {
                    yield return new Touch()
                    {
                        Position = new PointF(mouseState.X, mouseState.Y),
                        TouchState = ETouchState.Held
                    };
                }
            }
            else
            {
                if (lastMouseState.LeftButton == ButtonState.Pressed)
                {
                    yield return new Touch()
                    {
                        Position = new PointF(mouseState.X, mouseState.Y),
                        TouchState = ETouchState.Released
                    };
                }
            }

            lastMouseState = mouseState;
        }
    }
}
