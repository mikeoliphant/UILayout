using System;
using System.Collections.Generic;
using System.Linq;

namespace UILayout
{
    public partial class InputManager
    {
        /// <summary>Whether the key is down (for example a modifier held during a drag).</summary>
        public bool IsKeyDown(InputKey key)
        {
            return false;
        }

        /// <summary>Whether the key went down since the last update.</summary>
        public bool WasPressed(InputKey key)
        {
            return false;
        }

        /// <summary>Whether the key went up since the last update.</summary>
        public bool WasReleased(InputKey key)
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
