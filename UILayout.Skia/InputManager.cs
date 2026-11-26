using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace UILayout
{
    public partial class InputManager
    {
        /// <summary>
        /// Where the mouse is, in layout pixels, for hover (the host calls this as the mouse moves: input comes through
        /// the host's control, not polled). A position outside the layout's bounds, as when the mouse leaves, is over
        /// nothing.
        /// </summary>
        public void SetMousePosition(in Vector2 position)
        {
            MousePosition = position;
        }

        // Keys by virtual-key code (InputKey's values)
        const int KeyCount = 256;

        // The keys as the host's events leave them, between updates; and keys pressed and released again before an
        // update saw them, kept down for that update so the press isn't lost
        bool[] keysDown = new bool[KeyCount];
        bool[] releasePending = new bool[KeyCount];

        // The keys as of the last update, and the one before (for WasPressed and WasReleased)
        bool[] currentKeys = new bool[KeyCount];
        bool[] lastKeys = new bool[KeyCount];

        // The wheel turned since the last update (120 per notch, positive away from the user)
        int wheel;

        /// <summary>A key went down (the host calls this from its key events; repeats while held are ignored).</summary>
        public void KeyDown(InputKey key)
        {
            int k = (int)key;

            if ((k <= 0) || (k >= KeyCount))
                return;

            keysDown[k] = true;
            releasePending[k] = false;
        }

        /// <summary>A key went up (the host calls this from its key events).</summary>
        public void KeyUp(InputKey key)
        {
            int k = (int)key;

            if ((k <= 0) || (k >= KeyCount) || !keysDown[k])
                return;

            // Down since the last update, and up again already: it stays down for the next update
            if (!currentKeys[k])
                releasePending[k] = true;
            else
                keysDown[k] = false;
        }

        /// <summary>
        /// All keys go up, for example when the host loses keyboard focus (the key releases then go elsewhere).
        /// </summary>
        public void ReleaseAllKeys()
        {
            Array.Clear(keysDown);
            Array.Clear(releasePending);
        }

        /// <summary>The mouse wheel turned (the host calls this from its wheel events): 120 per notch, positive away from
        /// the user. Added up until the next update.</summary>
        public void AddMouseWheel(int delta)
        {
            wheel += delta;
        }

        /// <summary>Whether the key is down (for example a modifier held during a drag).</summary>
        public bool IsKeyDown(InputKey key)
        {
            int k = (int)key;

            return (k > 0) && (k < KeyCount) && currentKeys[k];
        }

        /// <summary>Whether the key went down since the last update.</summary>
        public bool WasPressed(InputKey key)
        {
            int k = (int)key;

            return (k > 0) && (k < KeyCount) && currentKeys[k] && !lastKeys[k];
        }

        /// <summary>Whether the key went up since the last update.</summary>
        public bool WasReleased(InputKey key)
        {
            int k = (int)key;

            return (k > 0) && (k < KeyCount) && !currentKeys[k] && lastKeys[k];
        }

        // The keys and the wheel as the events since the last update left them. Doesn't allocate.
        protected void PlatformUpdate(float secondsElapsed)
        {
            (lastKeys, currentKeys) = (currentKeys, lastKeys);

            Array.Copy(keysDown, currentKeys, KeyCount);

            // Released before this update saw them: seen down now, up from the next
            for (int k = 0; k < KeyCount; k++)
            {
                if (releasePending[k])
                {
                    keysDown[k] = false;
                    releasePending[k] = false;
                }
            }

            MouseWheelDelta = wheel;
            wheel = 0;
        }

        // No touches here (input comes through the WPF control): an empty list, as the MonoGame backend returns a list.
        List<Touch> touches = new List<Touch>();

        public List<Touch> GetTouches()
        {
            return touches;
        }
    }
}
