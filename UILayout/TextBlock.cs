using System;
using System.Text;

namespace UILayout
{
    public static class StringBuilderExtensions
    {
        static char[] numberBuffer = new char[16];  // Not thread safe - should find a better way

        public static StringBuilder AppendNumber(this StringBuilder stringBuilder, int number)
        {
            return stringBuilder.AppendNumber(number, 0);
        }

        public static StringBuilder AppendNumber(this StringBuilder stringBuilder, int number, int minDigits)
        {
            if (number < 0)
            {
                stringBuilder.Append('-');
                number = -number;
            }

            int index = 0;

            do
            {
                int digit = number % 10;
                numberBuffer[index] = (char)('0' + digit);
                number /= 10;
                ++index;
            }
            while (number > 0 || index < minDigits);

            for (--index; index >= 0; --index)
            {
                stringBuilder.Append(numberBuffer[index]);
            }

            return stringBuilder;
        }
    }

    /// <summary>A line (or lines) of text, in the theme's font and text color.</summary>
    public class TextBlock : UIElement
    {
        public string Text { get; set; }

        public UIColor TextColor { get; set; }
        public UIFont TextFont { get; set; }

        public TextBlock()
        {
            TextFont = Layout.Current.Theme.Font;
            TextColor = Layout.Current.Theme.Text;
        }

        public TextBlock(string text)
            : this()
        {
            Text = text;
        }

        protected override void GetContentSize(out float width, out float height)
        {
            if (string.IsNullOrEmpty(Text))
            {
                width = 0;
                height = 0;
            }
            else
            {
                float textWidth = 0;
                float textHeight = 0;

                TextFont.MeasureString(Text, out textWidth, out textHeight);

                width = textWidth;
                height = textHeight;
            }
        }

        protected override void DrawContents()
        {
            base.DrawContents();

            Layout.Current.GraphicsContext.DrawText(Text, TextFont, ContentBounds.X, ContentBounds.Y, TextColor);
        }
    }

    public class StringBuilderTextBlock : UIElement
    {
        public StringBuilder StringBuilder { get; set; }

        public UIColor TextColor { get; set; }
        public UIFont TextFont { get; set; }

        public StringBuilderTextBlock()
        {
            TextFont = Layout.Current.Theme.Font;
            TextColor = Layout.Current.Theme.Text;

            StringBuilder = new StringBuilder();
        }

        public StringBuilderTextBlock(string text)
            : this()
        {
            StringBuilder = new StringBuilder(text);
        }

        protected override void GetContentSize(out float width, out float height)
        {
            if (StringBuilder.Length == 0)
            {
                width = 0;
                height = 0;
            }
            else
            {
                float textWidth = 0;
                float textHeight = 0;

                TextFont.MeasureString(StringBuilder, out textWidth, out textHeight);

                width = textWidth;
                height = textHeight;
            }
        }

        protected override void DrawContents()
        {
            base.DrawContents();

            Layout.Current.GraphicsContext.DrawText(StringBuilder, TextFont, ContentBounds.X, ContentBounds.Y, TextColor);
        }
    }

    /// <summary>
    /// Text that's cut short with "..." when it doesn't fit its width. Its size is the whole text's, so layout gives it
    /// the room it wants when there is some; it draws the shortened text when there isn't. Give it a stretched alignment
    /// to be cut to the room available.
    /// </summary>
    public class FittedTextBlock : UIElement
    {
        string text = "";
        string fitted = "";
        float fittedWidth = -1;
        float fittedTextWidth;

        public FittedTextBlock()
        {
            TextColor = Layout.Current.Theme.Text;
            TextFont = Layout.Current.Theme.Font;
        }

        public string Text
        {
            get { return text; }

            set
            {
                if (value == text)
                    return;

                text = value ?? "";
                fittedWidth = -1;
            }
        }

        public UIColor TextColor { get; set; }

        public UIFont TextFont { get; set; }

        /// <summary>
        /// Where the (fitted) text sits across the element's width: Left (the default), Center or Right. Stretch and
        /// Absolute have no meaning for text and act as Left.
        /// </summary>
        public EHorizontalAlignment TextAlignment { get; set; } = EHorizontalAlignment.Left;

        protected override void GetContentSize(out float width, out float height)
        {
            TextFont.MeasureString(text, out width, out _);

            // As high as a TextBlock with the text (so they line up side by side); when empty, as a line of text.
            TextFont.MeasureString((text.Length > 0) ? text : "Ag", out _, out height);
        }

        protected override void DrawContents()
        {
            base.DrawContents();

            if (ContentBounds.Width != fittedWidth)
            {
                fittedWidth = ContentBounds.Width;
                fitted = Fit(text, fittedWidth);

                TextFont.MeasureString(fitted, out fittedTextWidth, out _);
            }

            float x = ContentBounds.X;

            if (TextAlignment == EHorizontalAlignment.Center)
                x += (ContentBounds.Width - fittedTextWidth) / 2;
            else if (TextAlignment == EHorizontalAlignment.Right)
                x += ContentBounds.Width - fittedTextWidth;

            Layout.Current.GraphicsContext.DrawText(fitted, TextFont, x, ContentBounds.Y, TextColor);
        }

        // The text, cut with "..." to fit the width.
        string Fit(string full, float width)
        {
            TextFont.MeasureString(full, out float fullWidth, out _);

            if (fullWidth <= width)
                return full;

            for (int length = full.Length - 1; length > 0; length--)
            {
                string cut = full.Substring(0, length) + "...";

                TextFont.MeasureString(cut, out float cutWidth, out _);

                if (cutWidth <= width)
                    return cut;
            }

            return "";
        }
    }
}
