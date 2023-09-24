namespace UILayout
{
    public partial class TextBlock : UIElement
    {
        public static UIFont DefaultFont { get; set; }
        public static UIColor DefaultColor { get; set; }

        public string Text { get; set; }

        public UIColor TextColor { get; set; }
        public UIFont TextFont { get; set; }

        public TextBlock()
        {
            TextFont = DefaultFont;
            TextColor = DefaultColor;
        }

        protected override void DrawContents()
        {
            base.DrawContents();

            Layout.Current.GraphicsContext.DrawText(Text, TextFont, ContentBounds.X, ContentBounds.Y, TextColor);
        }
    }
}
