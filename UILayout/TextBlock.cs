using System.Text;

namespace UILayout
{
    public class TextBlock : UIElement
    {
        public static UIFont DefaultFont { get; set; } = UIFont.DefaultFont;
        public static UIColor DefaultColor { get; set; } = UIColor.Black;

        public string Text { get; set; }

        public UIColor TextColor { get; set; }
        public UIFont TextFont { get; set; }

        public TextBlock()
        {
            TextFont = DefaultFont;
            TextColor = DefaultColor;
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
        public static UIFont DefaultFont { get; set; } = UIFont.DefaultFont;
        public static UIColor DefaultColor { get; set; } = UIColor.Black;

        public StringBuilder StringBuilder { get; set; }

        public UIColor TextColor { get; set; }
        public UIFont TextFont { get; set; }

        public StringBuilderTextBlock()
        {
            TextFont = DefaultFont;
            TextColor = DefaultColor;

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
}
