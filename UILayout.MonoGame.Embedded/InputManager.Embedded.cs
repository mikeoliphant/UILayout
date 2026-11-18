using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace UILayout
{
    public partial class InputManager
    {
        // Embedded input comes from the events the owner forwards to the host (MonoGame's global state belongs to a
        // Game's own window). No touch input.
        static readonly TouchCollection noTouches = new TouchCollection(new TouchLocation[0]);

        static MonoGameHost Host => (Layout.Current as MonoGameLayout)?.Host;

        private partial KeyboardState GetKeyboardState() => Host?.GetKeyboardState() ?? default;
        private partial MouseState GetMouseState() => Host?.GetMouseState() ?? default;
        private partial TouchCollection GetTouchState() => noTouches;
    }
}
