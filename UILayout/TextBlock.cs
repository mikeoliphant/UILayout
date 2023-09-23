namespace UILayout
{
    public partial class TextBlock : UIElement
    {
        public static Font DefaultFont { get; set; }
        public static UIColor DefaultColor { get; set; }

        public string Text { get; set; }

        public TextBlock()
        {
            TextFont = DefaultFont;
            TextColor = DefaultColor;
        }
    }
}
