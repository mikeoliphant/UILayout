using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace UILayout
{
    public partial class InputManager
    {
        KeyboardState lastState;
        KeyboardState currentState;

        MouseState lastMouseState;
        Vector2 lastMousePosition;

        // Supplied by the backend (for example UILayout.MonoGame.Game polls MonoGame's global input state).
        private partial KeyboardState GetKeyboardState();
        private partial MouseState GetMouseState();
        private partial TouchCollection GetTouchState();

        public static Touch FromXNATouch(TouchLocation touchLocation)
        {
            ETouchState touchState = ETouchState.Invalid;

            switch (touchLocation.State)
            {
                case TouchLocationState.Invalid:
                    touchState = ETouchState.Invalid;
                    break;
                case TouchLocationState.Moved:
                    touchState = ETouchState.Moved;
                    break;
                case TouchLocationState.Pressed:
                    touchState = ETouchState.Pressed;
                    break;
                case TouchLocationState.Released:
                    touchState = ETouchState.Released;
                    break;
            }

            return new Touch { TouchID = touchLocation.Id, Position = new Vector2(touchLocation.Position.X, touchLocation.Position.Y) / (Layout.Current as MonoGameLayout).TotalScale, TouchState = touchState };
        }

        /// <summary>Whether the key is down (for example a modifier held during a drag).</summary>
        public bool IsKeyDown(InputKey key)
        {
            return currentState.IsKeyDown((Keys)key);
        }

        /// <summary>Whether the key went down since the last update.</summary>
        public bool WasPressed(InputKey key)
        {
            return currentState.IsKeyDown((Keys)key) && !lastState.IsKeyDown((Keys)key);
        }

        /// <summary>Whether the key went up since the last update.</summary>
        public bool WasReleased(InputKey key)
        {
            return !currentState.IsKeyDown((Keys)key) && lastState.IsKeyDown((Keys)key);
        }

        protected void PlatformUpdate(float secondsElapsed)
        {
            lastState = currentState;
            currentState = GetKeyboardState();
        }

        // This frame's touches, refilled by GetTouches: a list, so that going through them doesn't allocate (sized up
        // front, so the first touches don't grow it during a frame).
        List<Touch> touches = new List<Touch>(16);

        /// <summary>
        /// This frame's touches, with the mouse as a touch. Call once per update: it moves the mouse state on. The list
        /// is reused (refilled by the next call).
        /// </summary>
        public List<Touch> GetTouches()
        {
            touches.Clear();

            foreach (TouchLocation touch in GetTouchState())
            {
                touches.Add(FromXNATouch(touch));
            }

            MouseState mouseState = GetMouseState();

            Vector2 position = new Vector2(mouseState.X, mouseState.Y) / (Layout.Current as MonoGameLayout).TotalScale;

            MousePosition = position;

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (lastMouseState.LeftButton == ButtonState.Released)
                    touches.Add(new Touch() { Position = position, TouchState = ETouchState.Pressed });
                else if (Vector2.Distance(position, lastMousePosition) == 0)
                    touches.Add(new Touch() { Position = position, TouchState = ETouchState.Held });
                else
                    touches.Add(new Touch() { Position = position, TouchState = ETouchState.Moved });
            }
            else if (lastMouseState.LeftButton == ButtonState.Pressed)
            {
                touches.Add(new Touch() { Position = position, TouchState = ETouchState.Released });
            }

            MouseWheelDelta = mouseState.ScrollWheelValue - lastMouseState.ScrollWheelValue;

            lastMouseState = mouseState;
            lastMousePosition = position;

            return touches;
        }
    }
}
