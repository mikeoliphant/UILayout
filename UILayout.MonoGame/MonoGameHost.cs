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
        public static Assembly ResourceAssembly { get; set; } = Assembly.GetEntryAssembly();

        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }

        public MonoGameLayout Layout { get; private set; }

        public bool UseEmbeddedResources { get; set; } = false;

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

        GraphicsDeviceManager graphics;
        IntPtr parentWindow = IntPtr.Zero;

        public MonoGameHost(int screenWidth, int screenHeight, bool fullscreen)
            : this(IntPtr.Zero, screenWidth, screenHeight, fullscreen)
        {
        }

        public MonoGameHost(IntPtr parentWindow, int screenWidth, int screenHeight, bool fullscreen)
        {
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;

            this.parentWindow = parentWindow;

            graphics = new GraphicsDeviceManager(this);

            graphics.IsFullScreen = fullscreen;
            graphics.SynchronizeWithVerticalRetrace = true;
            IsFixedTimeStep = false;

            if (screenWidth == 0)
            {
                ScreenWidth = graphics.PreferredBackBufferWidth;
                ScreenHeight = graphics.PreferredBackBufferHeight;
            }
            else
            {
                graphics.PreferredBackBufferWidth = ScreenWidth;
                graphics.PreferredBackBufferHeight = ScreenHeight;
            }

#if WINDOWS
            Content = new AssemblyRelativeContentManager(Services);
#endif

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            Layout.SetHost(this);

            base.Initialize();

#if WINDOWS
            if (parentWindow != IntPtr.Zero)
            {
                Window.Position = new Microsoft.Xna.Framework.Point(0, 0);
                Window.IsBorderless = true;

                SetParent(Window.Handle, parentWindow);
            }
#endif

            Window.AllowUserResizing = true;

            Window.TextInput += Window_TextInput;
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

        public Stream OpenContentStream(string contentPath)
        {
            if (UseEmbeddedResources)
            {
                return ResourceAssembly.GetManifestResourceStream(ResourceAssembly.GetName().Name + "." + contentPath.Replace('\\', '.'));
            }

#if WINDOWS
            return AssemblyRelativeContentManager.OpenAseemblyRelativeStream(Path.Combine(Content.RootDirectory, contentPath));
#else
            return TitleContainer.OpenStream(Path.Combine(Content.RootDirectory, contentPath));
#endif
        }

        public Texture2D LoadTexture(string resourceName)
        {
            if (UseEmbeddedResources)
            {
                using (Stream stream = ResourceAssembly.GetManifestResourceStream(ResourceAssembly.GetName().Name + ".Textures." + resourceName + ".png"))
                {
                    return Texture2D.FromStream(MonoGameLayout.Current.Host.GraphicsDevice, stream);
                }
            }

            return MonoGameLayout.Current.Host.Content.Load<Texture2D>(Path.Combine("Textures", resourceName));
        }

        protected override void Update(GameTime gameTime)
        {
            if (requestResize)
            {
                graphics.PreferredBackBufferWidth = requestResizeWidth;
                graphics.PreferredBackBufferHeight = requestResizeHeight;
                graphics.ApplyChanges();

                requestResize = false;
            }

            base.Update(gameTime);

            Layout.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);

            if ((GraphicsDevice.Viewport.Bounds.Width != Layout.Bounds.Width) || (GraphicsDevice.Viewport.Bounds.Height != Layout.Bounds.Height))
            {
                ScreenWidth = GraphicsDevice.Viewport.Bounds.Width;
                ScreenHeight = GraphicsDevice.Viewport.Bounds.Height;

                Layout.SetBounds(new RectF(0, 0, GraphicsDevice.Viewport.Bounds.Width, GraphicsDevice.Viewport.Bounds.Height));

                Layout.UpdateLayout();
            }

            Layout.AddDirtyRect(Layout.Bounds);
            Layout.Draw();
        }

        protected override void OnDeactivated(object sender, EventArgs args)
        {
            Layout.Exiting();

            base.OnDeactivated(sender, args);
        }
    }

    public class AssemblyRelativeContentManager : ContentManager
    {
        public static String AssemblyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        public static Stream OpenAseemblyRelativeStream(string relativePath)
        {
            return File.OpenRead(Path.Combine(AssemblyPath, relativePath));
        }

        public AssemblyRelativeContentManager(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        protected override Stream OpenStream(string assetName)
        {
            return OpenAseemblyRelativeStream(Path.Combine(RootDirectory, assetName) + ".xnb");
        }
    }
}
