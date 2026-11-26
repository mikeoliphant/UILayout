using System.IO;
using UILayout;

namespace UILayout.DefaultTextures
{
    /// <summary>
    /// Loads UILayout's default textures and fonts into the current layout and its theme: one sheet and its manifest
    /// (Textures/), rendered by DefaultTexturesProcessor from the images in SrcTextures/ and the Inter font. Images
    /// and fonts are also registered by name (Layout.GetImage, GetFont).
    /// </summary>
    public class TextureLoader
    {
        public static void LoadDefaultTextures(ContentLoader loader)
        {
            Layout layout = Layout.Current;

            using (Stream manifest = loader.OpenContentStream("Textures/ImageManifest.xml"))
            {
                ImageManifest.Load(loader, manifest, layout);
            }

            UITheme theme = layout.Theme;

            theme.FrameFill = layout.GetImage("FrameFill");
            theme.FrameBorder = layout.GetImage("FrameBorder");
            theme.PopupFill = layout.GetImage("PopupFill");
            theme.PopupBorder = layout.GetImage("PopupBorder");
            theme.TabFill = layout.GetImage("TabFill");
            theme.TabBorder = layout.GetImage("TabBorder");
            theme.DropDownArrow = layout.GetImage("DropDownArrow");
            theme.CheckBoxFill = layout.GetImage("CheckBoxFill");
            theme.CheckBoxBorder = layout.GetImage("CheckBoxBorder");
            theme.CheckMark = layout.GetImage("CheckMark");
            theme.ScrollThumb = layout.GetImage("ScrollThumb");
            theme.ScrollUpArrow = layout.GetImage("ScrollUpArrow");
            theme.ScrollDownArrow = layout.GetImage("ScrollDownArrow");
            theme.SliderTrack = layout.GetImage("SliderTrack");
            theme.SliderTrackVertical = layout.GetImage("SliderTrackVertical");
            theme.SliderThumbShadow = layout.GetImage("SliderThumbShadow");
            theme.SliderThumbBody = layout.GetImage("SliderThumbBody");
            theme.SliderThumbHighlight = layout.GetImage("SliderThumbHighlight");
            theme.SliderThumbRing = layout.GetImage("SliderThumbRing");

            // Bitmap fonts (backends that draw text their own way, as Skia, have none here and set their own)
            if (layout.TryGetFont("Font", out UIFont font))
                theme.Font = font;

            if (layout.TryGetFont("SmallFont", out UIFont smallFont))
                theme.SmallFont = smallFont;
        }
    }
}
