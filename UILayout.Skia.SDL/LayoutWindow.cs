using System.Diagnostics;
using System.Numerics;
using System.Text;
using SkiaSharp;
using SDL2;

namespace UILayout
{
    /// <summary>
    /// An SDL window drawing a <see cref="SkiaLayout"/> with OpenGL, redrawn every frame. The mouse (buttons, movement for
    /// hover, wheel), keys and typed text go to the layout, as in the WPF host.
    /// </summary>
    public class LayoutWindow
    {
        IntPtr window = IntPtr.Zero;
        GRContext glContext;
        SKSurface fbSurface;
        SkiaLayout layout;
        bool needRepaint = true;
        int currentWidth = -1;
        int currentHeight = -1;

        public LayoutWindow(string name, int width, int height)
        {
            window = SDL.SDL_CreateWindow(name,
                SDL.SDL_WINDOWPOS_CENTERED,
                SDL.SDL_WINDOWPOS_CENTERED,
                width,
                height,
                SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE | SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL
            );

            if (window == IntPtr.Zero)
            {
                Console.WriteLine($"There was an issue creating the window. {SDL.SDL_GetError()}");
            }

            IntPtr sdlContext = SDL.SDL_GL_CreateContext(window);

            int result = SDL.SDL_GL_MakeCurrent(window, sdlContext);

            glContext = GRContext.CreateGl();

            // Turn on vsync
            SDL.SDL_GL_SetSwapInterval(1);
        }

        void CreateSurface(int width, int height)
        {
            if (fbSurface != null)
            {
                fbSurface.Dispose();
                fbSurface = null;
            }

            GRBackendRenderTarget renderTarget = new GRBackendRenderTarget(
                width: width,
                height: height,
                sampleCount: 0,
                stencilBits: 0,
                glInfo: new GRGlFramebufferInfo(fboId: 0, format: SKColorType.Rgba8888.ToGlSizedFormat()));

            fbSurface = SKSurface.Create(glContext, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);

            currentWidth = width;
            currentHeight = height;
        }

        public void SetLayout(SkiaLayout layout)
        {
            this.layout = layout;

            if (fbSurface != null)
            {
                UpdateLayout();
            }
        }

        /// <summary>Sizes the window to the layout's root element (the room it needs; layout pixels are window
        /// pixels).</summary>
        public void ResizeToLayout()
        {
            if ((layout == null) || (layout.RootUIElement == null))
                return;

            layout.RootUIElement.GetSize(out float width, out float height);

            SDL.SDL_SetWindowSize(window, (int)Math.Ceiling(width), (int)Math.Ceiling(height));

            needRepaint = true;
        }

        void UpdateLayout()
        {
            layout.GraphicsContext.Canvas = fbSurface.Canvas;

            RectF bounds = new RectF(0, 0, currentWidth, currentHeight);

            layout.SetBounds(bounds);
            layout.AddDirtyRect(bounds);
            layout.UpdateLayout();
        }

        public void Run()
        {
            bool running = true;

            // Typed text comes as text input events
            SDL.SDL_StartTextInput();

            Stopwatch clock = Stopwatch.StartNew();
            double lastSeconds = 0;

            while (running)
            {
                while (SDL.SDL_PollEvent(out SDL.SDL_Event e) == 1)
                {
                    if (e.type == SDL.SDL_EventType.SDL_QUIT)
                    {
                        running = false;

                        continue;
                    }

                    if (layout != null)
                        HandleEvent(e);
                }

                if (needRepaint)
                {
                    int width;
                    int height;

                    SDL.SDL_GetWindowSize(window, out width, out height);

                    if ((fbSurface == null) || (width != currentWidth) || (height != currentHeight))
                        CreateSurface(width, height);

                    if (layout != null)
                    {
                        UpdateLayout();
                    }

                    needRepaint = false;
                }

                if (layout != null)
                {
                    double seconds = clock.Elapsed.TotalSeconds;

                    // What the mouse is over, keys, the cursor's blink (as the MonoGame hosts update each frame)
                    layout.Update((float)(seconds - lastSeconds));

                    lastSeconds = seconds;

                    // Drawn whole every frame (the window's buffers are swapped, not kept)
                    RectF bounds = new RectF(0, 0, currentWidth, currentHeight);

                    layout.AddDirtyRect(bounds);

                    if (layout.HaveDirty)
                    {
                        layout.Draw();

                        fbSurface.Canvas.Flush();

                        SDL.SDL_GL_SwapWindow(window);
                    }
                }
            }

            SDL.SDL_StopTextInput();

            // Clean up
            SDL.SDL_DestroyWindow(window);
            SDL.SDL_Quit();
        }

        void HandleEvent(in SDL.SDL_Event e)
        {
            InputManager input = layout.InputManager;

            switch (e.type)
            {
                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                    {
                        Vector2 position = new Vector2(e.button.x, e.button.y);

                        input.SetMousePosition(position);

                        layout.HandleTouch(new Touch()
                        {
                            Position = position,
                            TouchState = (e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN) ? ETouchState.Pressed :
                                ETouchState.Released
                        });
                    }
                    break;

                // Tracked for hover, and a touch moving while the button is down (a motion event's position is its own:
                // the event's fields overlap)
                case SDL.SDL_EventType.SDL_MOUSEMOTION:
                    {
                        Vector2 position = new Vector2(e.motion.x, e.motion.y);

                        input.SetMousePosition(position);

                        if ((e.motion.state & SDL.SDL_BUTTON_LMASK) != 0)
                        {
                            layout.HandleTouch(new Touch()
                            {
                                Position = position,
                                TouchState = ETouchState.Moved
                            });
                        }
                    }
                    break;

                // Notches (120 per notch, positive away from the user, as the other hosts)
                case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                    {
                        int notches = e.wheel.y;

                        if (e.wheel.direction == (uint)SDL.SDL_MouseWheelDirection.SDL_MOUSEWHEEL_FLIPPED)
                            notches = -notches;

                        input.AddMouseWheel(notches * 120);
                    }
                    break;

                // Repeats while a key is held are left out (key mappings repeat on their own)
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    if (e.key.repeat == 0)
                        input.KeyDown(ToInputKey(e.key.keysym.sym));
                    break;

                case SDL.SDL_EventType.SDL_KEYUP:
                    input.KeyUp(ToInputKey(e.key.keysym.sym));
                    break;

                case SDL.SDL_EventType.SDL_TEXTINPUT:
                    HandleText(e);
                    break;

                case SDL.SDL_EventType.SDL_WINDOWEVENT:
                    switch (e.window.windowEvent)
                    {
                        case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:
                            needRepaint = true;
                            break;

                        // Off the window: the mouse is over nothing
                        case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_LEAVE:
                            input.SetMousePosition(new Vector2(float.MinValue, float.MinValue));
                            break;

                        // Key releases go elsewhere now: nothing stays held
                        case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST:
                            input.ReleaseAllKeys();
                            break;
                    }
                    break;
            }
        }

        // Typed text, UTF-8 in the event, to the element with focus. Control characters are left out: Backspace and
        // Enter are keys to the layout. Decoded on the stack (typing doesn't allocate).
        unsafe void HandleText(in SDL.SDL_Event e)
        {
            SDL.SDL_TextInputEvent textEvent = e.text;

            byte* bytes = textEvent.text;

            int length = 0;

            while ((length < SDL.SDL_TEXTINPUTEVENT_TEXT_SIZE) && (bytes[length] != 0))
                length++;

            char* chars = stackalloc char[SDL.SDL_TEXTINPUTEVENT_TEXT_SIZE];

            int count = Encoding.UTF8.GetChars(bytes, length, chars, SDL.SDL_TEXTINPUTEVENT_TEXT_SIZE);

            for (int i = 0; i < count; i++)
            {
                if (!char.IsControl(chars[i]))
                    layout.HandleTextInput(chars[i]);
            }
        }

        // SDL's key codes to InputKey's (Windows virtual-key codes, as the other hosts use)
        static InputKey ToInputKey(SDL.SDL_Keycode key)
        {
            // Letters: SDL's are lowercase characters, virtual keys the uppercase ones
            if ((key >= SDL.SDL_Keycode.SDLK_a) && (key <= SDL.SDL_Keycode.SDLK_z))
                return (InputKey)((int)key - 'a' + 'A');

            if ((key >= SDL.SDL_Keycode.SDLK_0) && (key <= SDL.SDL_Keycode.SDLK_9))
                return (InputKey)((int)key - '0' + (int)InputKey.D0);

            if ((key >= SDL.SDL_Keycode.SDLK_F1) && (key <= SDL.SDL_Keycode.SDLK_F12))
                return (InputKey)((int)key - (int)SDL.SDL_Keycode.SDLK_F1 + (int)InputKey.F1);

            if ((key >= SDL.SDL_Keycode.SDLK_KP_1) && (key <= SDL.SDL_Keycode.SDLK_KP_9))
                return (InputKey)((int)key - (int)SDL.SDL_Keycode.SDLK_KP_1 + (int)InputKey.NumPad1);

            switch (key)
            {
                case SDL.SDL_Keycode.SDLK_BACKSPACE: return InputKey.Back;
                case SDL.SDL_Keycode.SDLK_TAB: return InputKey.Tab;
                case SDL.SDL_Keycode.SDLK_RETURN: return InputKey.Enter;
                case SDL.SDL_Keycode.SDLK_KP_ENTER: return InputKey.Enter;
                case SDL.SDL_Keycode.SDLK_ESCAPE: return InputKey.Escape;
                case SDL.SDL_Keycode.SDLK_SPACE: return InputKey.Space;
                case SDL.SDL_Keycode.SDLK_PAUSE: return InputKey.Pause;
                case SDL.SDL_Keycode.SDLK_CAPSLOCK: return InputKey.CapsLock;

                case SDL.SDL_Keycode.SDLK_PAGEUP: return InputKey.PageUp;
                case SDL.SDL_Keycode.SDLK_PAGEDOWN: return InputKey.PageDown;
                case SDL.SDL_Keycode.SDLK_END: return InputKey.End;
                case SDL.SDL_Keycode.SDLK_HOME: return InputKey.Home;
                case SDL.SDL_Keycode.SDLK_LEFT: return InputKey.Left;
                case SDL.SDL_Keycode.SDLK_UP: return InputKey.Up;
                case SDL.SDL_Keycode.SDLK_RIGHT: return InputKey.Right;
                case SDL.SDL_Keycode.SDLK_DOWN: return InputKey.Down;
                case SDL.SDL_Keycode.SDLK_INSERT: return InputKey.Insert;
                case SDL.SDL_Keycode.SDLK_DELETE: return InputKey.Delete;

                case SDL.SDL_Keycode.SDLK_KP_0: return InputKey.NumPad0;
                case SDL.SDL_Keycode.SDLK_KP_MULTIPLY: return InputKey.Multiply;
                case SDL.SDL_Keycode.SDLK_KP_PLUS: return InputKey.Add;
                case SDL.SDL_Keycode.SDLK_KP_MINUS: return InputKey.Subtract;
                case SDL.SDL_Keycode.SDLK_KP_PERIOD: return InputKey.Decimal;
                case SDL.SDL_Keycode.SDLK_KP_DIVIDE: return InputKey.Divide;
                case SDL.SDL_Keycode.SDLK_NUMLOCKCLEAR: return InputKey.NumLock;
                case SDL.SDL_Keycode.SDLK_SCROLLLOCK: return InputKey.Scroll;

                case SDL.SDL_Keycode.SDLK_LSHIFT: return InputKey.LeftShift;
                case SDL.SDL_Keycode.SDLK_RSHIFT: return InputKey.RightShift;
                case SDL.SDL_Keycode.SDLK_LCTRL: return InputKey.LeftControl;
                case SDL.SDL_Keycode.SDLK_RCTRL: return InputKey.RightControl;
                case SDL.SDL_Keycode.SDLK_LALT: return InputKey.LeftAlt;
                case SDL.SDL_Keycode.SDLK_RALT: return InputKey.RightAlt;

                case SDL.SDL_Keycode.SDLK_SEMICOLON: return InputKey.OemSemicolon;
                case SDL.SDL_Keycode.SDLK_EQUALS: return InputKey.OemPlus;
                case SDL.SDL_Keycode.SDLK_COMMA: return InputKey.OemComma;
                case SDL.SDL_Keycode.SDLK_MINUS: return InputKey.OemMinus;
                case SDL.SDL_Keycode.SDLK_PERIOD: return InputKey.OemPeriod;
                case SDL.SDL_Keycode.SDLK_SLASH: return InputKey.OemQuestion;
                case SDL.SDL_Keycode.SDLK_BACKQUOTE: return InputKey.OemTilde;
                case SDL.SDL_Keycode.SDLK_LEFTBRACKET: return InputKey.OemOpenBrackets;
                case SDL.SDL_Keycode.SDLK_BACKSLASH: return InputKey.OemPipe;
                case SDL.SDL_Keycode.SDLK_RIGHTBRACKET: return InputKey.OemCloseBrackets;
                case SDL.SDL_Keycode.SDLK_QUOTE: return InputKey.OemQuotes;
            }

            return InputKey.None;
        }
    }
}
