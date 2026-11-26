using System;
#if WINDOWS
using System.Windows.Forms;
#endif
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;

namespace UILayout
{
    public class MonoGameHost : Game
    {
        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }

        public MonoGameLayout Layout { get; private set; }
        public GraphicsDeviceManager GraphicsDeviceManager { get; private set; }

#if WINDOWS
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref System.Drawing.Rectangle rectangle);

        public Form Form
        {
            get
            {
                { return Window.GetType().GetField("Form", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Window) as Form; }
            }
        }
#endif

        IntPtr parentWindow = IntPtr.Zero;

        // ---- DPI (Windows) ----
        //
        // The process is made per-monitor DPI aware before MonoGame creates its window (a static constructor runs
        // before the Game constructor), so Windows does not stretch (blur) the window; the layout's DisplayScale then
        // follows the window's DPI. An app manifest that declares a DPI awareness takes precedence (the call then
        // fails, harmlessly); a DPI-unaware app gets DisplayScale 1 and Windows stretches it as before. Other
        // platforms: DisplayScale 1 for now.

        [DllImport("user32.dll", EntryPoint = "SetProcessDpiAwarenessContext")]
        static extern bool SetProcessDpiAwarenessContextNative(IntPtr value);

        [DllImport("user32.dll", EntryPoint = "GetDpiForWindow")]
        static extern uint GetDpiForWindowNative(IntPtr hwnd);

        static readonly IntPtr DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new IntPtr(-4);

        static MonoGameHost()
        {
            // Only in a standalone app: inside another process (a plugin in a DAW, which has no managed entry
            // assembly) the DPI awareness is the host's to decide
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 15063) && (Assembly.GetEntryAssembly() != null))
            {
                try
                {
                    SetProcessDpiAwarenessContextNative(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
                }
                catch (EntryPointNotFoundException)
                {
                }
            }
        }

        /// <summary>
        /// The window's display scale (its DPI; 1 = 96 DPI), or 1 where it is not known.
        /// </summary>
        public float WindowDisplayScale
        {
            get
            {
                if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 14393) || (Window == null) || (Window.Handle == IntPtr.Zero))
                    return 1.0f;

                uint dpi = GetDpiForWindowNative(Window.Handle);

                return (dpi > 0) ? (dpi / 96.0f) : 1.0f;
            }
        }

        public MonoGameHost(int screenWidth, int screenHeight, bool fullscreen)
            : this(IntPtr.Zero, screenWidth, screenHeight, fullscreen)
        {
        }

        public MonoGameHost(IntPtr parentWindow, int screenWidth, int screenHeight, bool fullscreen)
        {
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;

            this.parentWindow = parentWindow;

            GraphicsDeviceManager = new GraphicsDeviceManager(this);

            GraphicsDeviceManager.IsFullScreen = fullscreen;
            GraphicsDeviceManager.SynchronizeWithVerticalRetrace = true;
            IsFixedTimeStep = false;

            if (screenWidth == 0)
            {
                ScreenWidth = GraphicsDeviceManager.PreferredBackBufferWidth;
                ScreenHeight = GraphicsDeviceManager.PreferredBackBufferHeight;
            }
            else
            {
                GraphicsDeviceManager.PreferredBackBufferWidth = ScreenWidth;
                GraphicsDeviceManager.PreferredBackBufferHeight = ScreenHeight;
            }

#if WINDOWS
            Content = new AssemblyRelativeContentManager(Services);
#endif

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            Layout.DisplayScale = WindowDisplayScale;

            Layout.SetHost(this);

            base.Initialize();

            // The requested size is in layout units at DisplayScale 1: keep the window's apparent size on a scaled
            // display
            if ((Layout.DisplayScale != 1) && !GraphicsDeviceManager.IsFullScreen && (parentWindow == IntPtr.Zero))
            {
                RequestResize((int)Math.Round(ScreenWidth * Layout.DisplayScale), (int)Math.Round(ScreenHeight * Layout.DisplayScale));
            }

#if WINDOWS
            if (parentWindow != IntPtr.Zero)
            {
                Window.Position = new Microsoft.Xna.Framework.Point(0, 0);
                Window.IsBorderless = true;

                SetParent(Window.Handle, parentWindow);
            }
#endif

            Window.AllowUserResizing = true;

#if !ANDROID
            Window.TextInput += Window_TextInput;
#endif
        }

        private void Window_TextInput(object sender, TextInputEventArgs e)
        {
            if (this.Layout != null)
            {
                this.Layout.HandleTextInput(e.Character);
            }
        }

        protected override void LoadContent()
        {
        }

        public void StartGame(MonoGameLayout ui)
        {
            this.Layout = ui;

            Run();
        }

        bool requestResize = false;
        int requestResizeWidth;
        int requestResizeHeight;

        public void RequestResize(int newWidth, int newHeight)
        {
            requestResize = true;

            requestResizeWidth = newWidth;
            requestResizeHeight = newHeight;
        }

        /// <summary>
        /// Sizes the window to the layout's root element (the room it needs, in window pixels at the layout's scale).
        /// Call once the root element is set, after <see cref="Initialize"/> (which sizes the window for the display's
        /// scale itself).
        /// </summary>
        public void ResizeToLayout()
        {
            if (Layout.RootUIElement == null)
                return;

            Layout.RootUIElement.GetSize(out float width, out float height);

            RequestResize((int)Math.Ceiling(width * Layout.TotalScale), (int)Math.Ceiling(height * Layout.TotalScale));
        }

        protected override void Update(GameTime gameTime)
        {
            if (requestResize)
            {
                GraphicsDeviceManager.PreferredBackBufferWidth = requestResizeWidth;
                GraphicsDeviceManager.PreferredBackBufferHeight = requestResizeHeight;
                GraphicsDeviceManager.ApplyChanges();

                requestResize = false;
            }

            // The window moved to a monitor with another DPI: the layout follows (the window keeps its pixel size)
            float displayScale = WindowDisplayScale;

            if (displayScale != Layout.DisplayScale)
            {
                Layout.DisplayScale = displayScale;
                Layout.UpdateLayout();
            }

            base.Update(gameTime);

            Layout.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);

            // UnscaledBounds: window pixels (Bounds is in layout units, which differ once a scale is set)
            if ((GraphicsDevice.Viewport.Bounds.Width != Layout.UnscaledBounds.Width) || (GraphicsDevice.Viewport.Bounds.Height != Layout.UnscaledBounds.Height))
            {
                ScreenWidth = GraphicsDevice.Viewport.Bounds.Width;
                ScreenHeight = GraphicsDevice.Viewport.Bounds.Height;

                Layout.SetBounds(new RectF(0, 0, GraphicsDevice.Viewport.Bounds.Width, GraphicsDevice.Viewport.Bounds.Height));

                Layout.UpdateLayout();
            }

            Layout.AddDirtyRect(Layout.Bounds);
            Layout.Draw();
        }

        protected override void OnExiting(object sender, ExitingEventArgs args)
        {
            Layout.Exiting();

            base.OnExiting(sender, args);
        }
    }
}
