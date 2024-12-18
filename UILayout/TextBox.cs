using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace UILayout
{
    public class TextBox : UIElement
    {
        public UIColor TextColor { get; set; }
        public UIFont TextFont { get; set; }

        public int InsertPosition { get; private set; } = 0;
        public Action EnterAction { get; set; } = null;

        List<char> text = new();
        int startDrawChar = 0;
        int endDrawChar = 0;
        RectF cursorRect;
        float blinkSecs = 0;
        int maxSize;
        bool haveFocus = false;

        public TextBox(int maxSize)
        {
            this.maxSize = maxSize;

            TextFont = Layout.Current.DefaultFont;
            TextColor = Layout.Current.DefaultForegroundColor;
        }

        public string GetText()
        {
            return new string(CollectionsMarshal.AsSpan<char>(text));
        }

        public ReadOnlySpan<char> GetTextSpan()
        {
            return CollectionsMarshal.AsSpan<char>(text);
        }

        public void Focus()
        {
            haveFocus = true;
            blinkSecs = 0;
        }

        protected override void GetContentSize(out float width, out float height)
        {
            TextFont.MeasureString(CollectionsMarshal.AsSpan<char>(text).Slice(startDrawChar, endDrawChar - startDrawChar), out width, out height);

            width = 0;
            height = TextFont.TextHeight;
        }

        public override void UpdateContentLayout()
        {
            base.UpdateContentLayout();

            UpdateCursor();
        }

        float GetVisibleTextWidth()
        {
            float width;
            float height;

            TextFont.MeasureString(CollectionsMarshal.AsSpan<char>(text).Slice(startDrawChar, endDrawChar - startDrawChar), out width, out height);

            return width;
        }

        void AddChar(char c)
        {
            text.Insert(InsertPosition, c);
            InsertPosition++;
            endDrawChar++;

            UpdateCursor();
        }

        void RemoveChar()
        {
            if (InsertPosition > 0)
            {
                text.RemoveAt(InsertPosition - 1);
                InsertPosition--;
                endDrawChar--;
            }

            UpdateCursor();
        }

        void UpdateCursor()
        {
            if (InsertPosition < startDrawChar)
            {
                startDrawChar = Math.Max(InsertPosition - 5, 0);
            }

            if (InsertPosition > endDrawChar)
            {
                endDrawChar = Math.Min(endDrawChar + 5, text.Count);
            }

            while (GetVisibleTextWidth() >= ContentBounds.Width)
            {
                if (endDrawChar > InsertPosition)
                {
                    endDrawChar--;
                }
                else
                {
                    startDrawChar++;
                }
            }

            while (GetVisibleTextWidth() < ContentBounds.Width)
            {
                if (endDrawChar < (text.Count - 1))
                {
                    endDrawChar++;

                    if (GetVisibleTextWidth() >= ContentBounds.Width)
                    {
                        endDrawChar--;

                        break;
                    }
                }
                else if (startDrawChar > 0)
                {
                    startDrawChar--;

                    if (GetVisibleTextWidth() >= ContentBounds.Width)
                    {
                        startDrawChar++;

                        break;
                    }
                }
                else
                {
                    break;
                }
            }

                //while (startDrawChar > 0)
                //{
                //    if 
                //    {
                //        startDrawChar++;
                //        drawChars--;

                //        break;
                //    }

                //    startDrawChar--;
                //    drawChars++;
                //}

            float width;
            float height;

            TextFont.MeasureString(CollectionsMarshal.AsSpan<char>(text).Slice(startDrawChar, InsertPosition - startDrawChar), out width, out height);

            cursorRect.X = ContentBounds.X + width + 2;
            cursorRect.Y = ContentBounds.Y + 2;
            cursorRect.Width = 1;
            cursorRect.Height = ContentBounds.Height - 4;
        }

        public override bool HandleTextInput(char c)
        {
            if (!haveFocus)
                return false;

            blinkSecs = 0;

            if (text.Count < maxSize)
            {
                if (TextFont.HasGlyph(c))
                {
                    AddChar(c);
                }
            }

            return true;
        }

        public override void HandleInput(InputManager inputManager)
        {
            if (!haveFocus)
            {
                base.HandleInput(inputManager);

                return;
            }

            if (inputManager.WasPressed("LeftArrow"))
            {
                if (InsertPosition > 0)
                {
                    InsertPosition--;

                    UpdateCursor();
                }
            }
            else if (inputManager.WasPressed("RightArrow"))
            {
                if (InsertPosition < text.Count)
                {
                    InsertPosition++;

                    UpdateCursor();
                }
            }            
            else if (inputManager.WasPressed("Backspace"))
            {
                if (InsertPosition > 0)
                {
                    RemoveChar();
                }
            }
            else if (inputManager.WasPressed("Enter"))
            {
                if (EnterAction != null)
                    EnterAction();

                haveFocus = false;
            }
        }

        public override bool HandleTouch(in Touch touch)
        {
            if (touch.TouchState == ETouchState.Pressed)
            {
                Focus();

                return true;
            }

            return base.HandleTouch(touch);
        }

        protected override void DrawContents()
        {
            base.DrawContents();

            Layout.Current.GraphicsContext.DrawText(CollectionsMarshal.AsSpan<char>(text).Slice(startDrawChar, endDrawChar - startDrawChar), TextFont, ContentBounds.X, ContentBounds.Y, TextColor);

            if (haveFocus)
            {
                blinkSecs += Layout.Current.SecondsElapsed;

                int blink = (int)(blinkSecs * 2);

                if ((blink % 2) == 0)
                    Layout.Current.GraphicsContext.DrawRectangle(cursorRect, TextColor);
            }
        }
    }
}
