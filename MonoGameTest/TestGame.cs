using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Xml.Serialization;
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

            using (Stream fontStream = File.OpenRead(Path.Combine(Content.RootDirectory, "Textures\\Font.xml")))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SpriteFontDefinition));

                TextBlock.DefaultFont = new Font { SpriteFont = UILayout.SpriteFont.CreateFromDefinition(serializer.Deserialize(fontStream) as SpriteFontDefinition) };
            }

            TextBlock.DefaultColor = UIColor.Black;

            ui.RootUIElement = new UILayout.Test.LayoutTest();
        }

        protected override void Update(GameTime gameTime)
        {
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