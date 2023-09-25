﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace UILayout
{
    public class MonoGameHost : Game
    {
        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }

        public MonoGameLayout Layout { get; private set; }

        GraphicsDeviceManager graphics;

        public MonoGameHost(int screenWidth, int screenHeight, bool fullscreen)
        {
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;

            graphics = new GraphicsDeviceManager(this);

            graphics.IsFullScreen = fullscreen;
            graphics.SynchronizeWithVerticalRetrace = true;

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

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Layout.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);

            if ((GraphicsDevice.Viewport.Bounds.Width != Layout.Bounds.Width) || (GraphicsDevice.Viewport.Bounds.Height != Layout.Bounds.Height))
            {
                Layout.SetBounds(new RectF(0, 0, GraphicsDevice.Viewport.Bounds.Width, GraphicsDevice.Viewport.Bounds.Height));

                Layout.UpdateLayout();
            }

            Layout.AddDirtyRect(Layout.Bounds);
            Layout.Draw();
        }
    }
}
