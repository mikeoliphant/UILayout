using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Xna.Framework.Input;

namespace UILayout
{
    public partial class InputManager
    {
        KeyboardState lastState;
        KeyboardState currentState;

        MouseState lastMouseState;
        Vector2 lastMousePosition;

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

            Vector2 position = new Vector2(mouseState.X, mouseState.Y);

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (lastMouseState.LeftButton == ButtonState.Released)
                {
                    yield return new Touch()
                    {
                        Position = position,
                        TouchState = ETouchState.Pressed
                    };
                }
                else
                {
                    if (Vector2.Distance(position, lastMousePosition) == 0)
                    {
                        yield return new Touch()
                        {
                            Position = new Vector2(mouseState.X, mouseState.Y),
                            TouchState = ETouchState.Held
                        };
                    }
                    else
                    {
                        yield return new Touch()
                        {
                            Position = new Vector2(mouseState.X, mouseState.Y),
                            TouchState = ETouchState.Moved
                        };
                    }
                }
            }
            else
            {
                if (lastMouseState.LeftButton == ButtonState.Pressed)
                {
                    yield return new Touch()
                    {
                        Position = new Vector2(mouseState.X, mouseState.Y),
                        TouchState = ETouchState.Released
                    };
                }
            }

            MouseWheelDelta = mouseState.ScrollWheelValue - lastMouseState.ScrollWheelValue;

            lastMouseState = mouseState;
            lastMousePosition = position;
        }
    }
}
