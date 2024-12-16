using System;
using System.Collections.Generic;
using System.Text;

namespace UILayout
{
    public class TextBox : UIElementWrapper
    {
        public int InsertPosition { get; private set; }

        public StringBuilderTextBlock Text { get; private set; } = new StringBuilderTextBlock();

        protected override void GetContentSize(out float width, out float height)
        {
            base.GetContentSize(out width, out height);

            height = Text.TextFont.TextHeight;
        }

        public TextBox()
        {
            Child = Text;
        }

        void AddChar(char c)
        {
            Text.StringBuilder.Insert(InsertPosition, c);
            InsertPosition++;
        }

        void RemoveChar()
        {
            if (InsertPosition > 0)
            {
                InsertPosition--;
                Text.StringBuilder.Remove(InsertPosition, 1);
            }
        }

        public override bool HandleTextInput(char c)
        {
            switch (c)
            {
                case '\b':
                    RemoveChar();
                    break;
            }

            if (Text.TextFont.HasGlyph(c))
            {
                AddChar(c);

                float width;
                float height;

                Text.GetSize(out width, out height);

                if (width > ContentBounds.Width)
                    RemoveChar();
            }

            return true;
        }
    }
}
