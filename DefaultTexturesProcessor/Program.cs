using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace DefaultTexturesProcessor
{
    /// <summary>
    /// UILayout's default textures and fonts, in one sheet: white images (so they tint), named as the source files
    /// (TextureLoader puts them in the layout's theme), and the fonts "Font" (UITheme.Font) and "SmallFont"
    /// (UITheme.SmallFont).
    /// </summary>
    class DefaultTexturesProcessor : ImageSheetProcessor.ImageSheetProcessor
    {
        public string FontPath { get; set; }

        public void Render()
        {
            BeginSpriteSheetGroup("DefaultSheet");

            AddFont("Font", FontPath, FontSize);
            AddFont("SmallFont", FontPath, SmallFontSize);

            // Nine-patches (frames, tabs, the scroll thumb, the slider track): corners are (size / 2) - 1 pixels, with
            // a straight pixel between each corner's curve and the 2-pixel middle (see NinePatchWrapper). One-pixel
            // outlines are on half pixels, so they're sharp. The slider thumb's parts are the same size, layered and
            // centered.
            ForceWhite = true;

            foreach (string name in Images)
                AddSvg(name);

            ForceWhite = false;

            // As drawn: its fill is white (it tints), its shadow black (a tint multiplies it, so it stays dark)
            AddSvg("PopupFill");

            EndSpriteSheetGroup();
        }

        // Font sizes in points (at 96 DPI)
        const float FontSize = 16;
        const float SmallFontSize = 12;

        // SVGs in SrcTextures
        static readonly string[] Images =
        {
            "FrameFill",
            "FrameBorder",
            "PopupBorder",
            "TabFill",
            "TabBorder",
            "DropDownArrow",
            "CheckBoxFill",
            "CheckBoxBorder",
            "CheckMark",
            "ScrollThumb",
            "ScrollUpArrow",
            "ScrollDownArrow",
            "SliderTrack",
            "SliderTrackVertical",
            "SliderThumbShadow",
            "SliderThumbBody",
            "SliderThumbHighlight",
            "SliderThumbRing",
        };
    }

    class Program
    {
        static void Main()
        {
            string root = Path.GetFullPath(Path.Combine(SourceDirectory(), ".."));
            string texturesPath = Path.Combine(root, "UILayout.DefaultTextures");

            DefaultTexturesProcessor processor = new DefaultTexturesProcessor
            {
                SrcPath = Path.Combine(texturesPath, "SrcTextures"),
                FontPath = Path.Combine(root, "Inter", "Inter_18pt-Bold.ttf"),
                ForceRegen = true
            };

            processor.BeginRenderImages(Path.Combine(texturesPath, "Textures"));

            processor.Render();

            processor.EndRenderImages();

            Console.WriteLine("Wrote " + processor.DestPath);
        }

        // This file's directory, from the compiler, so the tool finds the sources wherever it's built or run from.
        static string SourceDirectory([CallerFilePath] string path = "")
        {
            return Path.GetDirectoryName(path);
        }
    }
}
