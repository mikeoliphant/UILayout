using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace UILayout
{
    public partial class InputManager
    {
        // A Game's input comes from MonoGame's global state for its window.
        private partial KeyboardState GetKeyboardState() => Keyboard.GetState();
        private partial MouseState GetMouseState() => Mouse.GetState();
        private partial TouchCollection GetTouchState() => TouchPanel.GetState();
    }
}
