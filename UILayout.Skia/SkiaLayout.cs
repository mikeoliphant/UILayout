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

            SKTypeface typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright);

            // The sizes of the bitmap fonts (16 and 12 points at 96 DPI)
            Theme.Font = new UIFont { Typeface = typeface, TextSize = 21 };
            Theme.SmallFont = new UIFont { Typeface = typeface, TextSize = 16 };
        }
    }
}
