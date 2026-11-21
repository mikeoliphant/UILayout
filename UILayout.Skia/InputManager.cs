using System;
using System.Collections.Generic;
using System.Linq;

namespace UILayout
{
    public partial class InputManager
    {
        internal bool IsKeyDown(InputKey key)
        {
            return false;
        }

        internal bool WasPressed(InputKey key)
        {
            return false;
        }

        internal bool WasReleased(InputKey key)
        {
            return false;
        }

        protected void PlatformUpdate(float secondsElapsed)
        {
        }

        // No touches here (input comes through the WPF control): an empty list, as the MonoGame backend returns a list.
        List<Touch> touches = new List<Touch>();

        public List<Touch> GetTouches()
        {
            return touches;
        }
    }
}
