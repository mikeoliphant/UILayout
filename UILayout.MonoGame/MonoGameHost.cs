﻿using System;
#if WINDOWS
using System.Windows.Forms;
#endif
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace UILayout
{
    public class MonoGameHost : Game
    {
        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }

        public MonoGameLayout Layout { get; private set; }

#if WINDOWS
        public Form Form
        {
            get
            {
                { return Window.GetType().GetField("Form", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Window) as Form; }
            }
        }
#endif

        GraphicsDeviceManager graphics;

        public MonoGameHost(int screenWidth, int screenHeight, bool fullscreen)
        {
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;

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

            Window.AllowUserResizing = true;
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
#if WINDOWS
            return AssemblyRelativeContentManager.OpenAseemblyRelativeStream(Path.Combine(Content.RootDirectory, contentPath));
#else
            return TitleContainer.OpenStream(Path.Combine(Content.RootDirectory, contentPath));
#endif
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

        protected override void OnExiting(object sender, EventArgs args)
        {
            Layout.Exiting();

            base.OnExiting(sender, args);
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
