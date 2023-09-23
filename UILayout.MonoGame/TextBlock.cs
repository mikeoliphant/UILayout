using System;

namespace UILayout
{
    public partial class TextBlock : UIElement
    {
        public Font TextFont
        {
            get; set;
        }

        public UIColor TextColor
        {
            get; set;
        }

        static TextBlock()
        {
            DefaultColor = UIColor.Black;
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

                TextFont.SpriteFont.MeasureString(Text, out textWidth, out textHeight);

                width = textWidth;
                height = textHeight;
            }
        }

        protected override void DrawContents()
        {
            MonoGameLayout.Current.GraphicsContext.DrawText(Text, TextFont.SpriteFont, (int)ContentBounds.X, (int)ContentBounds.Y, 0.5f, TextColor.NativeColor, 1);
        }
    }
}
