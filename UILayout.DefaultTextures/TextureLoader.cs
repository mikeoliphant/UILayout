using System.IO;
using UILayout;

namespace UILayout.DefaultTextures
{
    /// <summary>
    /// Loads UILayout's default textures and fonts into the current layout: one sheet and its manifest (Textures/),
    /// rendered by DefaultTexturesProcessor from the images in SrcTextures/ and the Inter font. Images and fonts are
    /// registered by name, so Layout.GetImage and GetFont find them as before.
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

            layout.DefaultOutlineNinePatch = layout.GetImage("OutlineNinePatch");

            layout.DefaultPressedNinePatch = layout.GetImage("ButtonPressed");
            layout.DefaultUnpressedNinePatch = layout.GetImage("ButtonUnpressed");

            layout.DefaultDragImage = layout.GetImage("ButtonUnpressed");

            layout.DefaultInputFillNinePatch = layout.GetImage("InputFill");
            layout.DefaultInputBorderNinePatch = layout.GetImage("InputBorder");
            layout.DefaultDropDownArrow = layout.GetImage("DropDownArrow");
            layout.DefaultCheckBoxFill = layout.GetImage("CheckBoxFill");
            layout.DefaultCheckBoxBorder = layout.GetImage("CheckBoxBorder");
            layout.DefaultCheckMark = layout.GetImage("CheckMark");
            layout.DefaultScrollThumbNinePatch = layout.GetImage("ScrollThumb");

            layout.DefaultForegroundColor = UIColor.White;

            // Bitmap fonts (backends that draw text their own way, as Skia, have none here and set their own)
            if (layout.TryGetFont("Font", out UIFont font))
                layout.DefaultFont = font;

            if (layout.TryGetFont("SmallFont", out UIFont smallFont))
                layout.DefaultSmallFont = smallFont;
        }
    }
}
