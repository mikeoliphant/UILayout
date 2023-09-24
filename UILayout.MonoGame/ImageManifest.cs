﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace UILayout
{
    public class ImageManifestSheet
    {
        public string SheetName { get; set; }
        public List<ImageManifestSheetImage> Images { get; set; } = new List<ImageManifestSheetImage>();
        public List<SpriteFontDefinition> Fonts { get; set; } = new List<SpriteFontDefinition>();
    }

    public class ImageManifestSheetImage
    {
        public string ImageName { get; set; }
        public int XOffset { get; set; }
        public int YOffset { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class ImageManifestFont
    {
        public string Name { get; set; }
        public int Scale { get; set; }
        public bool IsFixedWidth { get; set; }
        public int GlyphWidth { get; set; }
        public int GlyphHeight { get; set; }
        public int GlyphsPerRow { get; set; }
        public string GlyphString { get; set; }
        public SpriteFontGlyph[] Glyphs { get; set; }
    }

    public class ImageManifest
    {
        public List<ImageManifestSheet> SpriteSheets { get; set; } = new List<ImageManifestSheet>();

        public static void Load(Stream manifestStream, Layout layout)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ImageManifest));

            ImageManifest manifest = serializer.Deserialize(manifestStream) as ImageManifest;

            foreach (ImageManifestSheet sheet in manifest.SpriteSheets)
            {
                UIImage sheetImage = null;

                sheetImage = layout.AddImage(sheet.SheetName);

                foreach (ImageManifestSheetImage image in sheet.Images)
                {
                    UIImage uiImage = new UIImage(sheetImage);
                    uiImage.XOffset = image.XOffset;
                    uiImage.YOffset = image.YOffset;
                    uiImage.Width = image.Width;
                    uiImage.Height = image.Height;

                    layout.AddImage(image.ImageName, uiImage);
                }
            }

            foreach (ImageManifestSheet sheet in manifest.SpriteSheets)
            {
                foreach (SpriteFontDefinition fontDefinition in sheet.Fonts)
                {
                    UIFont font = new UIFont { SpriteFont = SpriteFont.CreateFromDefinition(fontDefinition) };

                    layout.AddFont(fontDefinition.Name, font);
                }
            }
        }
    }
}
