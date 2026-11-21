using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace UILayout
{
    /// <summary>
    /// The embedded backend's host: drives a MonoGameLayout inside a native window that something else owns (for
    /// example a plugin editor window created by a DAW/plugin adapter), without MonoGame's Game class. It offers
    /// the members layouts use from the Game backend's MonoGameHost (GraphicsDevice, Content, InactiveSleepTime,
    /// ScreenWidth/ScreenHeight, Layout), so layout code compiles against either backend.
    ///
    /// - MonoGame's own SDL2 is preloaded by full path through the caller's assembly load context (MonoGame
    ///   searches the process base directory, which inside a DAW is the DAW's folder).
    /// - The native window is wrapped with SDL_CreateWindowFrom, and SdlGameWindow.Instance (where
    ///   GraphicsDevice looks for its window) is set to a minimal GameWindow. MonoGame keeps that state in
    ///   statics, so one host per copy of MonoGame (for plugins: per plugin instance load context).
    /// - The owner calls Tick() regularly on the host's thread, and forwards input on that thread. Several hosts
    ///   can share a thread: each makes its OpenGL context current before drawing.
    /// - The host's thread is the one that created it, until the owner moves the host to another thread
    ///   (ReleaseFromCurrentThread, then BindToCurrentThread on the new one), for example to render on a thread
    ///   of its own with vsync. MonoGame has one "UI thread" per copy of MonoGame, where all graphics work must
    ///   happen; binding makes the calling thread that thread.
    ///
    /// Positions are window pixels. Steady-state ticks do not allocate.
    /// </summary>
    public unsafe class MonoGameHost : IDisposable
    {
        const uint SDL_INIT_VIDEO = 0x20;
        const int SDL_GL_RED_SIZE = 0, SDL_GL_GREEN_SIZE = 1, SDL_GL_BLUE_SIZE = 2, SDL_GL_ALPHA_SIZE = 3;
        const int SDL_GL_DOUBLEBUFFER = 5, SDL_GL_DEPTH_SIZE = 6, SDL_GL_STENCIL_SIZE = 7;
        const int SDL_GL_CONTEXT_MAJOR_VERSION = 17, SDL_GL_CONTEXT_MINOR_VERSION = 18;
        const uint SDL_FIRSTEVENT = 0, SDL_LASTEVENT = 0xFFFF;

        static IntPtr sdl;
        static delegate* unmanaged<byte*, byte*, int> SDL_SetHint;
        static delegate* unmanaged<uint, int> SDL_InitSubSystem;
        static delegate* unmanaged<uint, void> SDL_QuitSubSystem;
        static delegate* unmanaged<int, int, int> SDL_GL_SetAttribute;
        static delegate* unmanaged<IntPtr, IntPtr> SDL_CreateWindowFrom;
        static delegate* unmanaged<IntPtr, void> SDL_DestroyWindow;
        static delegate* unmanaged<IntPtr> SDL_GL_GetCurrentContext;
        static delegate* unmanaged<IntPtr, IntPtr, int> SDL_GL_MakeCurrent;
        static delegate* unmanaged<uint, uint, void> SDL_FlushEvents;
        static delegate* unmanaged<byte*> SDL_GetError;
        // The platform's "current context" query (wglGetCurrentContext / glXGetCurrentContext), or null.
        static delegate* unmanaged<IntPtr> GetNativeCurrentContext;

        // MonoGame's thread bookkeeping (internal class Microsoft.Xna.Framework.Threading): the id of its "UI
        // thread", and Run, which executes graphics work other threads queued for that thread.
        static FieldInfo monoGameThreadId;
        static Action runQueuedGraphicsWork;

        IntPtr sdlWindow;
        IntPtr glContext;
        bool sizeChanged;

        // Input state synthesized from the owner's events, read by InputManager (InputManager.Embedded.cs).
        int mouseX, mouseY, wheel;
        bool leftDown, leftSeen, leftReleasePending, middleDown, rightDown;
        readonly bool[] keysDown = new bool[256];
        KeyboardState keyboardState;
        bool keyboardChanged;

        /// <param name="nativeWindow">The window to draw into (HWND, X11 Window).</param>
        /// <param name="width">Its width in window pixels.</param>
        /// <param name="height">Its height in window pixels.</param>
        /// <param name="vsync">
        /// Whether Present waits for the vertical retrace. Only for a host that has its thread to itself: hosts that
        /// share a thread would wait for each other.
        /// </param>
        public MonoGameHost(IntPtr nativeWindow, int width, int height, bool vsync = false)
        {
            ScreenWidth = Math.Max(width, 1);
            ScreenHeight = Math.Max(height, 1);

            LoadSdl();
            SetMonoGameThread();
            Check(SDL_InitSubSystem(SDL_INIT_VIDEO) == 0, "SDL_InitSubSystem(VIDEO)");
            SetHint("SDL_VIDEO_FOREIGN_WINDOW_OPENGL", "1");
            // The attributes MonoGame's GraphicsDeviceManager sets before creating its own window.
            SDL_GL_SetAttribute(SDL_GL_RED_SIZE, 8);
            SDL_GL_SetAttribute(SDL_GL_GREEN_SIZE, 8);
            SDL_GL_SetAttribute(SDL_GL_BLUE_SIZE, 8);
            SDL_GL_SetAttribute(SDL_GL_ALPHA_SIZE, 8);
            SDL_GL_SetAttribute(SDL_GL_DEPTH_SIZE, 24);
            SDL_GL_SetAttribute(SDL_GL_STENCIL_SIZE, 8);
            SDL_GL_SetAttribute(SDL_GL_DOUBLEBUFFER, 1);
            SDL_GL_SetAttribute(SDL_GL_CONTEXT_MAJOR_VERSION, 2);
            SDL_GL_SetAttribute(SDL_GL_CONTEXT_MINOR_VERSION, 1);

            sdlWindow = SDL_CreateWindowFrom(nativeWindow);
            Check(sdlWindow != IntPtr.Zero, "SDL_CreateWindowFrom");

            SetSdlGameWindowInstance(new EmbeddedGameWindow(this));

            var pp = new PresentationParameters
            {
                BackBufferWidth = ScreenWidth,
                BackBufferHeight = ScreenHeight,
                BackBufferFormat = SurfaceFormat.Color,
                DepthStencilFormat = DepthFormat.Depth24Stencil8,
                DeviceWindowHandle = sdlWindow,
                PresentationInterval = vsync ? PresentInterval.One : PresentInterval.Immediate,
                IsFullScreen = false,
            };
            GraphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, pp);
            glContext = SDL_GL_GetCurrentContext();
            Check(glContext != IntPtr.Zero, "GL context after GraphicsDevice creation");

            // Like the Game backend's Content, but always relative to this assembly (inside a DAW the working
            // folder and the process base directory are the DAW's).
            var services = new GameServiceContainer();
            services.AddService(typeof(IGraphicsDeviceService), new GraphicsDeviceService(GraphicsDevice));
            Content = new AssemblyRelativeContentManager(services) { RootDirectory = "Content" };

            keyboardState = new KeyboardState();
        }

        public GraphicsDevice GraphicsDevice { get; private set; }
        public ContentManager Content { get; private set; }
        public bool IsActive => true;
        public MonoGameLayout Layout { get; private set; }
        public Color ClearColor { get; set; } = Color.Black;
        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }

        /// <summary>Accepted for compatibility with the Game backend; the owner decides when to tick.</summary>
        public TimeSpan InactiveSleepTime { get; set; }

        /// <summary>Attaches the layout: it draws with this host's device and takes input from this host.</summary>
        public void SetLayout(MonoGameLayout layout)
        {
            MakeCurrent();
            Layout = layout;
            layout.UseExternalFileDialogs = OperatingSystem.IsLinux(); // no GTK dialogs inside the DAW's process
            layout.SetHost(this);
            layout.SetBounds(new RectF(0, 0, ScreenWidth, ScreenHeight));
            layout.UpdateLayout();
        }

        /// <summary>
        /// Makes the calling thread this host's thread: MonoGame's graphics thread, with this host's OpenGL context
        /// current. The previous thread must have called ReleaseFromCurrentThread (an OpenGL context can be
        /// current on one thread only).
        /// </summary>
        public void BindToCurrentThread()
        {
            SetMonoGameThread();
            MakeCurrent();
        }

        /// <summary>
        /// Releases this host's OpenGL context from the calling thread (the host's thread), so that another thread
        /// can bind the host.
        /// </summary>
        public void ReleaseFromCurrentThread()
        {
            if (sdlWindow != IntPtr.Zero) SDL_GL_MakeCurrent(sdlWindow, IntPtr.Zero);
        }

        /// <summary>Update, draw and present one frame. Call on the host's thread.</summary>
        public void Tick(float secondsElapsed)
        {
            if (GraphicsDevice == null) return;
            MakeCurrent();

            // Graphics work other threads handed to MonoGame's graphics thread (for example textures created
            // while loading in the background) waits for this; the Game backend runs it in its loop.
            runQueuedGraphicsWork();

            if (sizeChanged)
            {
                sizeChanged = false;
                GraphicsDevice.PresentationParameters.BackBufferWidth = ScreenWidth;
                GraphicsDevice.PresentationParameters.BackBufferHeight = ScreenHeight;
                GraphicsDevice.Viewport = new Viewport(0, 0, ScreenWidth, ScreenHeight);
                if (Layout != null)
                {
                    Layout.SetBounds(new RectF(0, 0, ScreenWidth, ScreenHeight));
                    Layout.UpdateLayout();
                }
            }

            if (Layout != null) Layout.Update(secondsElapsed);

            GraphicsDevice.Clear(ClearColor);
            if (Layout != null)
            {
                Layout.AddDirtyRect(Layout.Bounds);
                Layout.Draw();
            }
            GraphicsDevice.Present();

            // SDL's event queue is never used (input comes from the owner); don't let it fill up.
            SDL_FlushEvents(SDL_FIRSTEVENT, SDL_LASTEVENT);
        }

        /// <summary>The native window was resized (window pixels).</summary>
        public void Resize(int newWidth, int newHeight)
        {
            ScreenWidth = Math.Max(newWidth, 1);
            ScreenHeight = Math.Max(newHeight, 1);
            sizeChanged = true;
        }

        // ---- input from the owner -------------------------------------------------------

        public void MouseMove(int x, int y)
        {
            mouseX = x;
            mouseY = y;
        }

        /// <summary>button: 0 = left, 1 = right, 2 = middle.</summary>
        public void MouseButton(int button, bool down, int x, int y)
        {
            MouseMove(x, y);
            switch (button)
            {
                case 0:
                    if (down)
                    {
                        leftDown = true;
                        leftSeen = false;
                        leftReleasePending = false;
                    }
                    else if (leftSeen)
                    {
                        leftDown = false;
                    }
                    else
                    {
                        // Pressed and released between two polls: keep the press visible for one poll.
                        leftReleasePending = true;
                    }
                    break;
                case 1: rightDown = down; break;
                case 2: middleDown = down; break;
            }
        }

        /// <summary>delta in wheel units: 120 per notch, positive away from the user.</summary>
        public void MouseWheel(int delta) => wheel += delta;

        /// <summary>The mouse left the window: nothing in the layout is under it until it moves back in. Ignored
        /// while a button is down (a drag goes on outside the window).</summary>
        public void MouseLeave()
        {
            if (!leftDown && !rightDown && !middleDown)
                MouseMove(OutsideWindow, OutsideWindow);
        }

        const int OutsideWindow = -100000;

        public void Key(Keys key, bool down)
        {
            int k = (int)key;
            if (k < 0 || k >= keysDown.Length || keysDown[k] == down) return;
            keysDown[k] = down;
            keyboardChanged = true;
        }

        /// <summary>Typed text. Returns true if an element (for example a focused text box) used it.</summary>
        public bool TextInput(char c) => Layout != null && Layout.HandleTextInput(c);

        /// <summary>
        /// Release all keys, for example when the window loses keyboard focus (key releases then go elsewhere).
        /// Mouse buttons are not tied to keyboard focus: a drag continues until its button is released.
        /// </summary>
        public void ReleaseAllKeys()
        {
            for (int i = 0; i < keysDown.Length; i++)
            {
                if (keysDown[i])
                {
                    keysDown[i] = false;
                    keyboardChanged = true;
                }
            }
        }

        /// <summary>Release all keys and mouse buttons, for example when the owner loses track of the input altogether.</summary>
        public void ReleaseAllInput()
        {
            ReleaseAllKeys();
            if (leftDown) MouseButton(0, false, mouseX, mouseY);
            rightDown = false;
            middleDown = false;
        }

        internal KeyboardState GetKeyboardState()
        {
            if (keyboardChanged)
            {
                keyboardChanged = false;
                int count = 0;
                for (int i = 0; i < keysDown.Length; i++) if (keysDown[i]) count++;
                var keys = new Keys[count]; // only when the keyboard state changes
                for (int i = 0, n = 0; i < keysDown.Length; i++) if (keysDown[i]) keys[n++] = (Keys)i;
                keyboardState = new KeyboardState(keys);
            }
            return keyboardState;
        }

        internal MouseState GetMouseState()
        {
            var state = new MouseState(mouseX, mouseY, wheel,
                leftDown ? ButtonState.Pressed : ButtonState.Released,
                middleDown ? ButtonState.Pressed : ButtonState.Released,
                rightDown ? ButtonState.Pressed : ButtonState.Released,
                ButtonState.Released, ButtonState.Released);
            if (leftDown)
            {
                leftSeen = true;
                if (leftReleasePending)
                {
                    leftDown = false;
                    leftReleasePending = false;
                }
            }
            return state;
        }

        // ---- lifetime -------------------------------------------------------------------

        /// <summary>Call on the host's thread.</summary>
        public void Dispose()
        {
            if (GraphicsDevice == null) return;
            MakeCurrent();
            Layout?.Exiting();
            Content.Unload();
            Content.Dispose();
            GraphicsDevice.Dispose();
            GraphicsDevice = null;
            // For a foreign window SDL only drops its bookkeeping; the native window belongs to the owner.
            SDL_DestroyWindow(sdlWindow);
            sdlWindow = IntPtr.Zero;
            SetSdlGameWindowInstance(null);
            SDL_QuitSubSystem(SDL_INIT_VIDEO);
        }

        /// <summary>
        /// Makes this host's context current. SDL caches the current context per thread and skips the switch when
        /// it believes this one is already current, but other OpenGL users on the thread change the real current
        /// context without SDL knowing (a DAW that draws with OpenGL, or SDL itself while another editor's
        /// context is being created). So the real current context is checked, and SDL's cache reset when stale.
        /// </summary>
        void MakeCurrent()
        {
            bool sdlThinksCurrent = SDL_GL_GetCurrentContext() == glContext;
            if (sdlThinksCurrent && GetNativeCurrentContext != null && GetNativeCurrentContext() == glContext) return;
            if (sdlThinksCurrent) SDL_GL_MakeCurrent(sdlWindow, IntPtr.Zero);
            SDL_GL_MakeCurrent(sdlWindow, glContext);
        }

        // ---- SDL ------------------------------------------------------------------------

        /// <summary>Makes the calling thread MonoGame's graphics ("UI") thread.</summary>
        static void SetMonoGameThread()
        {
            if (monoGameThreadId == null)
            {
                Type threading = typeof(Game).Assembly.GetType("Microsoft.Xna.Framework.Threading", throwOnError: true);
                monoGameThreadId = threading.GetField("_mainThreadId", BindingFlags.NonPublic | BindingFlags.Static)
                    ?? throw new InvalidOperationException("MonoGame's Threading._mainThreadId not found");
                MethodInfo run = threading.GetMethod("Run", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static, Type.EmptyTypes)
                    ?? throw new InvalidOperationException("MonoGame's Threading.Run not found");
                runQueuedGraphicsWork = (Action)Delegate.CreateDelegate(typeof(Action), run);
            }

            // Otherwise MonoGame's graphics thread is whichever thread used MonoGame first (its static
            // constructor), and drawing on any other thread throws.
            monoGameThreadId.SetValue(null, Environment.CurrentManagedThreadId);
        }

        static void SetSdlGameWindowInstance(GameWindow window)
        {
            typeof(Game).Assembly.GetType("Microsoft.Xna.Framework.SdlGameWindow", throwOnError: true)
                .GetField("Instance", BindingFlags.Public | BindingFlags.Static)
                .SetValue(null, window);
        }

        static void LoadSdl()
        {
            if (sdl != IntPtr.Zero) return;

            // Preload SDL2 through our own load context (deps.json), then let MonoGame initialize: its loader
            // falls back to the bare library name, which resolves to an already loaded module. Without the preload
            // MonoGame would search the process's folder (inside a DAW, the DAW's) and not find SDL2.
            string name = OperatingSystem.IsWindows() ? "SDL2" : OperatingSystem.IsMacOS() ? "libSDL2-2.0.0.dylib" : "libSDL2-2.0.so.0";
            NativeLibrary.Load(name, typeof(MonoGameHost).Assembly, null);

            // The bare name resolves to the first SDL2 loaded in the process: with two different plugins that each
            // ship SDL2, that is the other plugin's copy, not the one preloaded above. Use whichever MonoGame bound
            // to, so that this host's SDL calls (the foreign window, the GL context) and MonoGame's go to the same
            // SDL. Sharing one SDL between plugins is like sharing it between two instances of one plugin.
            sdl = (IntPtr)typeof(Game).Assembly.GetType("Sdl", throwOnError: true).GetField("NativeLibrary").GetValue(null);
            if (sdl == IntPtr.Zero)
                throw new InvalidOperationException("MonoGame could not load SDL2");

            SDL_SetHint = (delegate* unmanaged<byte*, byte*, int>)NativeLibrary.GetExport(sdl, "SDL_SetHint");
            SDL_InitSubSystem = (delegate* unmanaged<uint, int>)NativeLibrary.GetExport(sdl, "SDL_InitSubSystem");
            SDL_QuitSubSystem = (delegate* unmanaged<uint, void>)NativeLibrary.GetExport(sdl, "SDL_QuitSubSystem");
            SDL_GL_SetAttribute = (delegate* unmanaged<int, int, int>)NativeLibrary.GetExport(sdl, "SDL_GL_SetAttribute");
            SDL_CreateWindowFrom = (delegate* unmanaged<IntPtr, IntPtr>)NativeLibrary.GetExport(sdl, "SDL_CreateWindowFrom");
            SDL_DestroyWindow = (delegate* unmanaged<IntPtr, void>)NativeLibrary.GetExport(sdl, "SDL_DestroyWindow");
            SDL_GL_GetCurrentContext = (delegate* unmanaged<IntPtr>)NativeLibrary.GetExport(sdl, "SDL_GL_GetCurrentContext");
            SDL_GL_MakeCurrent = (delegate* unmanaged<IntPtr, IntPtr, int>)NativeLibrary.GetExport(sdl, "SDL_GL_MakeCurrent");
            SDL_FlushEvents = (delegate* unmanaged<uint, uint, void>)NativeLibrary.GetExport(sdl, "SDL_FlushEvents");
            SDL_GetError = (delegate* unmanaged<byte*>)NativeLibrary.GetExport(sdl, "SDL_GetError");

            // SDL's context handle is the platform's (HGLRC / GLXContext), so it can be compared with this.
            (string library, string export) = OperatingSystem.IsWindows() ? ("opengl32.dll", "wglGetCurrentContext")
                                            : OperatingSystem.IsLinux() ? ("libGL.so.1", "glXGetCurrentContext") : (null, null);
            if (library != null && NativeLibrary.TryLoad(library, out IntPtr gl) && NativeLibrary.TryGetExport(gl, export, out IntPtr query))
                GetNativeCurrentContext = (delegate* unmanaged<IntPtr>)query;
        }

        static void SetHint(string name, string value)
        {
            byte[] n = System.Text.Encoding.UTF8.GetBytes(name + "\0");
            byte[] v = System.Text.Encoding.UTF8.GetBytes(value + "\0");
            fixed (byte* pn = n, pv = v) SDL_SetHint(pn, pv);
        }

        static void Check(bool ok, string what)
        {
            if (!ok)
                throw new InvalidOperationException(what + " failed: " + Marshal.PtrToStringUTF8((IntPtr)SDL_GetError()));
        }

        /// <summary>The minimal GameWindow that GraphicsDevice needs: its Handle is the SDL window.</summary>
        class EmbeddedGameWindow : GameWindow
        {
            readonly MonoGameHost host;

            public EmbeddedGameWindow(MonoGameHost host) { this.host = host; }

            public override bool AllowUserResizing { get { return false; } set { } }
            public override Rectangle ClientBounds => new Rectangle(0, 0, host.ScreenWidth, host.ScreenHeight);
            public override Point Position { get { return Point.Zero; } set { } }
            public override DisplayOrientation CurrentOrientation => DisplayOrientation.Default;
            public override IntPtr Handle => host.sdlWindow;
            public override string ScreenDeviceName => "";
            public override void BeginScreenDeviceChange(bool willBeFullScreen) { }
            public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight) { }
            protected override void SetSupportedOrientations(DisplayOrientation orientations) { }
            protected override void SetTitle(string title) { }
        }

        /// <summary>What ContentManager needs to create graphics resources: the device.</summary>
        class GraphicsDeviceService : IGraphicsDeviceService
        {
            public GraphicsDeviceService(GraphicsDevice graphicsDevice) { GraphicsDevice = graphicsDevice; }

            public GraphicsDevice GraphicsDevice { get; }

            // The device is created once and never reset or recreated.
            public event EventHandler<EventArgs> DeviceCreated { add { } remove { } }
            public event EventHandler<EventArgs> DeviceDisposing { add { } remove { } }
            public event EventHandler<EventArgs> DeviceReset { add { } remove { } }
            public event EventHandler<EventArgs> DeviceResetting { add { } remove { } }
        }
    }
}
