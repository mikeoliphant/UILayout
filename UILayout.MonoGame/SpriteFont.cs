using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace UILayout
{
    public struct SpriteFontGlyph
    {
        public UInt16 Character;
        public int X;
        public int Y;
        public int Width;
        public int Height;
    }

    public class SpriteFontDefinition
    {
        public string Name { get; set; }
        public bool IsFixedWidth { get; set; }
        public int GlyphWidth { get; set; }
        public int GlyphHeight { get; set; }
        public int GlyphsPerRow { get; set; }
        public string GlyphString { get; set; }
        public SpriteFontGlyph[] Glyphs { get; set; }
    }

    public class SpriteFont
    {
        public float Spacing { get; set; }
        public float LineSpacing { get; set; }
        public float TextHeight { get; protected set; }

        int numGlyphs = (127 - 32);

        protected UIImage fontImage;
        protected Dictionary<int, SpriteFontGlyph> glyphs = new Dictionary<int, SpriteFontGlyph>();
        protected int rowHeight;
        protected int width;
        protected int height;
        protected float emptyRowHeight;

        [XmlIgnore]
        public UIImage FontImage
        {
            get { return fontImage; }
        }

        public int RowHeight
        {
            get { return rowHeight; }
        }

        public float EmptyLinePercent
        {
            set
            {
                emptyRowHeight = rowHeight * value;
            }
        }

        public Dictionary<int, SpriteFontGlyph> Glyphs
        {
            get { return glyphs; }
            set { glyphs = value; }
        }

        public bool IsFixedWidth { get; protected set; }

        public static SpriteFont CreateFromDefinition(SpriteFontDefinition fontDefinition)
        {
            string name = fontDefinition.Name;

            if (fontDefinition.IsFixedWidth)
            {
                fontDefinition.Glyphs = new SpriteFontGlyph[fontDefinition.GlyphString.Length];

                int row = 0;
                int col = 0;

                for (int pos = 0; pos < fontDefinition.GlyphString.Length; pos++)
                {
                    if (col == fontDefinition.GlyphsPerRow)
                    {
                        row++;
                        col = 0;
                    }

                    fontDefinition.Glyphs[pos] = new SpriteFontGlyph
                    {
                        Character = fontDefinition.GlyphString[pos],
                        X = col * fontDefinition.GlyphWidth,
                        Y = row * fontDefinition.GlyphHeight,
                        Width = fontDefinition.GlyphWidth,
                        Height = fontDefinition.GlyphHeight
                    };

                    col++;
                }
            }

            SpriteFont font = new SpriteFont(Layout.Current.GetImage(name), fontDefinition.Glyphs);

            font.Spacing = -1;
            font.LineSpacing = -1;
            font.EmptyLinePercent = 0.5f;

            return font;
        }

        public SpriteFont(UIImage fontImage, SpriteFontGlyph[] glyphs)
        {
            this.fontImage = fontImage;

            foreach (SpriteFontGlyph glyph in glyphs)
            {
                this.glyphs[glyph.Character] = glyph;
            }

            emptyRowHeight = TextHeight = rowHeight = glyphs[0].Height;
        }

        public SpriteFontGlyph GetGlyph(char c)
        {
            if (!glyphs.ContainsKey(c))
            {
                // This really should be any non-unicode...
                if (IsFixedWidth)
                {
                    return glyphs[' '];
                }

                // Map ascii space to unicode space if necessary
                if (c == 0x20)
                {
                    return glyphs[0x3000];
                }
                else if (c == 0x2D)
                {
                    return glyphs[0x2212];
                }
                else if (c == 0x27)
                {
                    return glyphs[0x2019];
                }

                c = (char)(c + 0xFEE0);

                if (!glyphs.ContainsKey(c))
                {
                    // If not found, use underscore

                    if (glyphs.ContainsKey('_'))
                        return glyphs['_'];

                    return glyphs[0xFF3F];
                }
            }

            return glyphs[c];
        }

        // *** Note don't forget that this should be the same as the StringBuilder version ***
        public void DrawString(String str, GraphicsContext2D graphicsContext, float x, float y, UIColor color, float scale)
        {
            if (String.IsNullOrEmpty(str))
                return;

            float xOffset = x;
            float yOffset = y;

            System.Drawing.Rectangle drawRect = System.Drawing.Rectangle.Empty;

            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];

                if (c == '\n')
                {
                    yOffset += (((x == xOffset) ? emptyRowHeight : rowHeight) + LineSpacing) * scale;
                    xOffset = x;
                }
                else
                {
                    SpriteFontGlyph glyph = GetGlyph(c);

                    drawRect.X = glyph.X;
                    drawRect.Y = glyph.Y;
                    drawRect.Width = glyph.Width;
                    drawRect.Height = glyph.Height;

                    graphicsContext.DrawImage(fontImage, (int)xOffset, (int)yOffset, drawRect, color, scale);

                    xOffset += (glyph.Width + Spacing) * scale;
                }
            }
        }

        // *** Note don't forget that this should be the same as the StringBuilder version ***
        public void MeasureString(String str, out float width, out float height)
        {
            MeasureString(str, 1, out width, out height);
        }

        // *** Note don't forget that this should be the same as the StringBuilder version ***
        public void MeasureString(String str, float scale, out float width, out float height)
        {
            width = 0;
            height = 0;

            float rowWidth = 0;

            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];

                if (c == '\n')
                {
                    width = Math.Max(width, rowWidth);

                    height += (((rowWidth == 0) ? emptyRowHeight : rowHeight) + LineSpacing) * scale;

                    rowWidth = 0;
                }
                else
                {

                    SpriteFontGlyph glyph = GetGlyph(c);

                    rowWidth += (glyph.Width + Spacing) * scale;
                }
            }

            width = Math.Max(width, rowWidth);
            height += rowHeight * scale;
        }

        // *** Note don't forget that this should be the same as the String version ***
        public void DrawString(StringBuilder stringBuilder, GraphicsContext2D graphicsContext, float x, float y, UIColor color, float scale)
        {
            float xOffset = x;
            float yOffset = y;

            System.Drawing.Rectangle drawRect = System.Drawing.Rectangle.Empty;

            for (int i = 0; i < stringBuilder.Length; i++)
            {
                char c = stringBuilder[i];

                if (c == '\n')
                {
                    yOffset += (((x == xOffset) ? emptyRowHeight : rowHeight) + LineSpacing) * scale;
                    xOffset = x;
                }
                else
                {
                    SpriteFontGlyph glyph = GetGlyph(c);

                    drawRect.X = glyph.X;
                    drawRect.Y = glyph.Y;
                    drawRect.Width = glyph.Width;
                    drawRect.Height = glyph.Height;

                    graphicsContext.DrawImage(fontImage, (int)xOffset, (int)yOffset, drawRect, color, scale);

                    xOffset += (glyph.Width + Spacing) * scale;
                }
            }
        }

        // *** Note don't forget that this should be the same as the String version ***
        public void MeasureString(StringBuilder stringBuilder, out float width, out float height)
        {
            MeasureString(stringBuilder, 1, out width, out height);
        }

        // *** Note don't forget that this should be the same as the String version ***
        public void MeasureString(StringBuilder stringBuilder, float scale, out float width, out float height)
        {
            width = 0;
            height = 0;

            float rowWidth = 0;

            for (int i = 0; i < stringBuilder.Length; i++)
            {
                char c = stringBuilder[i];

                if (c == '\n')
                {
                    width = Math.Max(width, rowWidth);

                    height += (((rowWidth == 0) ? emptyRowHeight : rowHeight) + LineSpacing) * scale;

                    rowWidth = 0;
                }
                else
                {

                    SpriteFontGlyph glyph = GetGlyph(c);

                    rowWidth += (glyph.Width + Spacing) * scale;
                }
            }

            width = Math.Max(width, rowWidth);
            height += rowHeight * scale;
        }
    }

    public class SpriteFontFixedWidth : SpriteFont
    {
        public SpriteFontFixedWidth(UIImage fontImage)
            : base(fontImage, new SpriteFontGlyph[0])   // fix this
        {
            this.fontImage = fontImage;

            width = fontImage.Width;
            height = fontImage.Height;

            InitializeFixedWidth();

            TextHeight = emptyRowHeight = rowHeight;
        }

        void InitializeFixedWidth()
        {
            IsFixedWidth = true;

            rowHeight = 8;

            for (int i = 0; i < 26; i++)
            {
                glyphs['A' + i] = new SpriteFontGlyph { Character = (UInt16)('A' + i), X = i * 8, Y = 0, Width = 8, Height = 8 };
                glyphs['a' + i] = new SpriteFontGlyph { Character = (UInt16)('a' + i), X = i * 8, Y = 0, Width = 8, Height = 8 };
            }

            for (int i = 0; i < 10; i++)
            {
                glyphs['0' + i] = new SpriteFontGlyph { Character = (UInt16)('0' + i), X = i * 8, Y = 8, Width = 8, Height = 8 };
            }

            glyphs[' '] = new SpriteFontGlyph { Character = (UInt16)' ', X = width - 8, Y = height - 8, Width = 8, Height = 8 };

            glyphs['.'] = new SpriteFontGlyph { Character = (UInt16)('.'), X = 10 * 8, Y = 8, Width = 8, Height = 8 };
            glyphs[','] = new SpriteFontGlyph { Character = (UInt16)(','), X = 11 * 8, Y = 8, Width = 8, Height = 8 };
            glyphs[';'] = new SpriteFontGlyph { Character = (UInt16)(';'), X = 12 * 8, Y = 8, Width = 8, Height = 8 };
            glyphs[':'] = new SpriteFontGlyph { Character = (UInt16)(':'), X = 13 * 8, Y = 8, Width = 8, Height = 8 };
            glyphs['!'] = new SpriteFontGlyph { Character = (UInt16)('!'), X = 14 * 8, Y = 8, Width = 8, Height = 8 };
            glyphs['?'] = new SpriteFontGlyph { Character = (UInt16)('?'), X = 15 * 8, Y = 8, Width = 8, Height = 8 };
            glyphs['-'] = new SpriteFontGlyph { Character = (UInt16)('-'), X = 17 * 8, Y = 8, Width = 8, Height = 8 };
            glyphs['%'] = new SpriteFontGlyph { Character = (UInt16)('%'), X = 19 * 8, Y = 8, Width = 8, Height = 8 };
        }
    }
}
