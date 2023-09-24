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
    }
}
