using SkiaSharp;
using System;
using System.Threading.Tasks;

namespace UILayout
{
    public class SkiaLayout : Layout
    {
        public static new SkiaLayout Current { get { return Layout.Current as SkiaLayout; } }
        public static string DefaultTextureNamespace { get; set; } = "UILayout";

        public SkiaLayout()
        {
            GraphicsContext = new GraphicsContext2D();

            var loader = new AssemblyResourceContentLoader(typeof(UILayout.DefaultTextures.TextureLoader).Assembly, DefaultTextureNamespace);

            UILayout.DefaultTextures.TextureLoader.LoadDefaultTextures(loader);

            DefaultFont = new UIFont { Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), TextSize = 24 };
        }
    }
}
