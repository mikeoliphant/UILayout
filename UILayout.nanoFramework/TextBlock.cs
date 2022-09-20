﻿namespace UILayout
{
    public partial class TextBlock : UIElement
    {
        nanoFramework.UI.Font font;
        nanoFramework.Presentation.Media.Color textColor;

        public Font TextFont
        {
            get { return new Font { NativeFont = font }; }
            set { font = value.NativeFont;  }
        }

        public Color TextColor
        {
            get { return new Color(textColor, 1);  }
            set
            {
                textColor = value.NativeColor;
            }
        }

        static TextBlock()
        {
            DefaultColor = Color.Black;
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
                int textWidth;
                int textHeight;

                font.ComputeTextInRect(Text, out textWidth, out textHeight);

                width = textWidth;
                height = textHeight;
            }
        }

        protected override void DrawContents()
        {
            BitmapLayout.Current.FullScreenBitmap.DrawText(Text, font, textColor, (int)ContentBounds.X, (int)ContentBounds.Y);
        }
    }
}