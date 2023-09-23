using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using UILayout;

namespace MonoGameTest
{
    public class TestGame : Game
    {
        private GraphicsDeviceManager _graphics;

        MonoGameLayout ui;


        public TestGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            Window.AllowUserResizing = true;
        }

        protected override void LoadContent()
        {
            ui = new MonoGameLayout(this);

            Dock dock = new Dock {  HorizontalAlignment = EHorizontalAlignment.Stretch, VerticalAlignment = EVerticalAlignment.Stretch, BackgroundColor = new UIColor(255, 0, 0) };

            ui.RootUIElement = dock; // new UILayout.Test.LayoutTest();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);

            if ((GraphicsDevice.Viewport.Bounds.Width != ui.Bounds.Width) || (GraphicsDevice.Viewport.Bounds.Height != ui.Bounds.Height))
            {
                ui.SetBounds(new RectF(0, 0, GraphicsDevice.Viewport.Bounds.Width, GraphicsDevice.Viewport.Bounds.Height));

                ui.UpdateLayout();
            }
            
            ui.AddDirtyRect(ui.Bounds);
            ui.Draw();
        }
    }
}