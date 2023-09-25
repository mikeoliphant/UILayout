using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Xml.Serialization;
using UILayout;

namespace MonoGameTest
{
    public class TestGameHost : MonoGameHost
    {
        public TestGameHost(int screenWidth, int screenHeight, bool isFullscreen)
            : base(screenWidth, screenHeight, isFullscreen)
        {
        }

        protected override void LoadContent()
        {
            using (Stream fontStream = File.OpenRead(Path.Combine(Content.RootDirectory, "Textures\\Font.xml")))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SpriteFontDefinition));

                SpriteFontDefinition fontDef = serializer.Deserialize(fontStream) as SpriteFontDefinition;

                Layout.AddImage(fontDef.Name);

                UIFont.DefaultFont = new UIFont { SpriteFont = UILayout.SpriteFont.CreateFromDefinition(fontDef) };
            }

            Layout.GraphicsContext.SingleWhitePixelImage = new UIImage("SingleWhitePixel");
            TextBlock.DefaultColor = UIColor.Black;

            Layout.RootUIElement = new UILayout.Test.LayoutTest();
        }
    }
}