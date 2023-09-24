using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
