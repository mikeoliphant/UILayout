using System;
using SkiaSharp;
using System.Text;

namespace UILayout
{
    /// <summary>
    /// A Skia font, measured as the MonoGame backend's bitmap fonts are: text is as wide as its glyphs' advances, and a
    /// line of it as high as the font's ascent plus descent, whatever the text (so text in one font is always the same
    /// height, and lines up when placed alike). The baseline is the ascent below the top.
    /// </summary>
    public partial class UIFont
    {
        SKTypeface typeface;
        float textSize;

        // Measuring, and the metrics for the typeface and size
        SKFont measureFont = new SKFont();
        float ascent;
        float descent;
        float lineSpacing;

        public SKTypeface Typeface
        {
            get { return typeface; }

            set
            {
                typeface = value;

                UpdateMetrics();
            }
        }

        /// <summary>The font's size in pixels (an em).</summary>
        public float TextSize
        {
            get { return textSize; }

            set
            {
                textSize = value;

                UpdateMetrics();
            }
        }

        /// <summary>From one line's top to the next's (as the bitmap fonts' line height).</summary>
        public float TextHeight
        {
            get { return lineSpacing; }
        }

        /// <summary>From a line's top (where text is drawn from) to its baseline.</summary>
        public float Ascent
        {
            get { return ascent; }
        }

        static UIFont()
        {
        }

        public static UIFont FromSpriteFont(SpriteFontDefinition spriteFont)
        {
            return null;
        }

        void UpdateMetrics()
        {
            measureFont.Typeface = typeface;
            measureFont.Size = textSize;

            measureFont.GetFontMetrics(out SKFontMetrics metrics);

            // Skia's ascent is above the baseline, so negative
            ascent = -metrics.Ascent;
            descent = metrics.Descent;
            lineSpacing = measureFont.Spacing;
        }

        public bool HasGlyph(char c)
        {
            return true;
        }

        public void MeasureString(string text, out float width, out float height)
        {
            MeasureString(text.AsSpan(), out width, out height);
        }

        public void MeasureString(ReadOnlySpan<char> text, out float width, out float height)
        {
            width = 0;
            height = 0;

            if (text.IsEmpty)
                return;

            // A line's height, and the line spacing for each line after the first (the widest line is the width)
            height = ascent + descent;

            while (true)
            {
                int newLine = text.IndexOf('\n');
                ReadOnlySpan<char> line = (newLine >= 0) ? text.Slice(0, newLine) : text;

                width = Math.Max(width, measureFont.MeasureText(line));

                if (newLine < 0)
                    break;

                height += lineSpacing;
                text = text.Slice(newLine + 1);
            }
        }

        // One line: the chunks' widths added up (kerning between chunks is left out), without copying the text.
        public void MeasureString(StringBuilder sb, out float width, out float height)
        {
            width = 0;
            height = (sb.Length > 0) ? (ascent + descent) : 0;

            foreach (ReadOnlyMemory<char> chunk in sb.GetChunks())
                width += measureFont.MeasureText(chunk.Span);
        }
    }
}
