using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace DefaultTexturesProcessor
{
    /// <summary>
    /// UILayout's default textures and fonts, in one sheet. Image names are the source files' names (as
    /// TextureLoader and the controls look them up); the fonts are "Font" (Layout.DefaultFont) and "SmallFont"
    /// (Layout.DefaultSmallFont).
    /// </summary>
    class DefaultTexturesProcessor : ImageSheetProcessor.ImageSheetProcessor
    {
        public string FontPath { get; set; }

        public void Render()
        {
            BeginSpriteSheetGroup("DefaultSheet");

            AddFont("Font", FontPath, FontSize);
            AddFont("SmallFont", FontPath, SmallFontSize);

            foreach (string name in Images)
                Add(name);

            EndSpriteSheetGroup();
        }

        // Font sizes in points (at 96 DPI)
        const float FontSize = 16;
        const float SmallFontSize = 12;

        static readonly string[] Images =
        {
            "ScrollBar",
            "ScrollBarGutter",
            "ScrollUpArrow",
            "ScrollDownArrow",
            "HorizontalSlider",
            "VerticalSlider",
            "VerticalPointerRight",
            "VerticalPointerLeft",
            "TabPanelBackground",
            "TabBackground",
            "TabForeground",
            "OutlineNinePatch",
            "ButtonPressed",
            "ButtonUnpressed",
            "InputFill",
            "InputBorder",
            "DropDownArrow",
            "CheckBoxFill",
            "CheckBoxBorder",
            "CheckMark",
            "ScrollThumb",
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
