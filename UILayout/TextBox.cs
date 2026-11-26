using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace UILayout
{
    /// <summary>
    /// A box for typing a line of text: a <see cref="Frame"/> around the text and its cursor. The border takes the
    /// accent color while it has focus (it gets it when clicked, or from <see cref="Focus"/>) and when the mouse is over
    /// it. Enter calls <see cref="EnterAction"/> and gives up the focus.
    /// </summary>
    public class TextBox : Frame
    {
        const float TextMarginX = 4;
        const float TextMarginY = 2;

        Editor editor;

        public TextBox(int maxSize)
        {
            HorizontalAlignment = EHorizontalAlignment.Stretch;
            VerticalAlignment = EVerticalAlignment.Center;

            editor = new Editor(maxSize)
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Center,
                Margin = new LayoutPadding(TextMarginX, TextMarginY)
            };

            Content = editor;
        }

        public UIColor TextColor
        {
            get { return editor.TextColor; }
            set { editor.TextColor = value; }
        }

        public UIFont TextFont
        {
            get { return editor.TextFont; }
            set { editor.TextFont = value; }
        }

        public int InsertPosition
        {
            get { return editor.InsertPosition; }
        }

        public Action EnterAction
        {
            get { return editor.EnterAction; }
            set { editor.EnterAction = value; }
        }

        public bool HasFocus
        {
            get { return editor.HasFocus; }
        }

        public string GetText()
        {
            return editor.GetText();
        }

        public void SetText(string text)
        {
            editor.SetText(text);
        }

        public ReadOnlySpan<char> GetTextSpan()
        {
            return editor.GetTextSpan();
        }

        public void Focus()
        {
            editor.Focus();
        }

        public override void HandleInput(InputManager inputManager)
        {
            base.HandleInput(inputManager);

            // Focus coming or going (the color change makes it dirty)
            UpdateColors();
        }

        // The mouse moving over it or off it shows when it's drawn, also while a popup has the input (as a button's; see
        // FrameButton). A subclass overriding this calls the base, or loses its hover.
        protected override void DrawContents()
        {
            UpdateColors();

            base.DrawContents();
        }

        // Called in input and as it draws: only sets colors (no layout changes, which would happen in a draw)
        void UpdateColors()
        {
            UITheme theme = Layout.Current.Theme;

            BorderColor = (editor.HasFocus || IsMouseOver) ? theme.Accent : theme.Border;
        }

        public override bool HandleTouch(in Touch touch)
        {
            // Anywhere in the frame, also its edge around the text.
            return editor.HandleTouch(touch);
        }

        // The text and its cursor (drawn here, a plain leaf).
        class Editor : UIElement
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

            public Editor(int maxSize)
            {
                this.maxSize = maxSize;

                TextFont = Layout.Current.Theme.Font;
                TextColor = Layout.Current.Theme.Text;
            }

            public bool HasFocus
            {
                get { return haveFocus; }
            }

            public string GetText()
            {
                return new string(CollectionsMarshal.AsSpan<char>(text));
            }

            public void SetText(string text)
            {
                if (string.IsNullOrEmpty(text))
                {
                    this.text.Clear();

                    startDrawChar = 0;
                    endDrawChar = 0;
                    InsertPosition = 0;
                }
                else
                {
                    this.text = new List<char>(text);

                    startDrawChar = 0;
                    endDrawChar = this.text.Count - 1;
                    InsertPosition = endDrawChar + 1;
                }

                if (LayoutBounds.Width > 0)
                    UpdateCursor();
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
                // No width of its own (it takes the room it's given), and a line of text's height (as a TextBlock, so
                // they line up side by side), also when empty.
                TextFont.MeasureString("Ag", out _, out height);

                width = 0;
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

                float width;
                float height;

                TextFont.MeasureString(CollectionsMarshal.AsSpan<char>(text).Slice(startDrawChar, InsertPosition - startDrawChar), out width, out height);

                cursorRect.X = ContentBounds.X + width + 1;
                cursorRect.Y = ContentBounds.Y + 2;
                cursorRect.Width = Math.Max(1, TextFont.TextHeight / 12.0f);
                cursorRect.Height = ContentBounds.Height - 4;
            }

            int FindInsertPosition(float relativeX)
            {
                if (text.Count == 0)
                    return 0;

                float width;
                float height;

                InsertPosition = startDrawChar;

                do
                {
                    TextFont.MeasureString(CollectionsMarshal.AsSpan<char>(text).Slice(startDrawChar, InsertPosition - startDrawChar), out width, out height);

                    if (width > relativeX)
                    {
                        break;
                    }

                    InsertPosition++;
                }
                while (InsertPosition < endDrawChar);

                UpdateCursor();

                return InsertPosition;
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

                    InsertPosition = FindInsertPosition(touch.Position.X - ContentBounds.X);

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
}
