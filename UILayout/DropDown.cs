using System;
using System.Collections.Generic;
using System.Numerics;

namespace UILayout
{
    /// <summary>
    /// A drop-down in the input style (as an HTML select): an <see cref="InputFrame"/> with the chosen item's text on
    /// the left and the theme's arrow image on the right. The border takes the accent color when the mouse is over it
    /// or its list is open. Clicking it opens the list of items below it (above, if it doesn't fit), at least as wide as
    /// the drop-down. Text that doesn't fit is cut short with "...".
    /// </summary>
    public class DropDown<T> : UIElementWrapper
    {
        const float TextMarginX = 5;
        const float ArrowGap = 10;

        List<(string Text, T Value)> items = new List<(string Text, T Value)>();
        InputFrame frame;
        FittedTextBlock text;
        ImageElement arrow;
        int selected = -1;
        DropDownList list;

        public DropDown()
        {
            UITheme theme = Layout.Current.Theme;

            HorizontalAlignment = EHorizontalAlignment.Stretch;
            VerticalAlignment = EVerticalAlignment.Center;

            arrow = new ImageElement(Layout.Current.DefaultDropDownArrow)
            {
                HorizontalAlignment = EHorizontalAlignment.Right,
                VerticalAlignment = EVerticalAlignment.Center,
                Margin = new LayoutPadding(0, 0, TextMarginX, 0),
                Color = theme.MutedText
            };

            // Stretched, so it's given the room there is and cuts itself short to fit.
            text = new FittedTextBlock
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Center,
                Margin = new LayoutPadding(TextMarginX, 0, TextMarginX + ArrowGap + arrow.Image.LogicalWidth, 0)
            };

            Dock content = new Dock();
            content.Children.Add(text);
            content.Children.Add(arrow);

            frame = new InputFrame(content);
            Child = frame;

            ShowSelected();
        }

        /// <summary>Called when the user chooses an item (not when the selection is set from code).</summary>
        public Action SelectAction { get; set; }

        /// <summary>The most items the open list shows at once; it scrolls (mouse wheel) when there are more.</summary>
        public int MaxVisibleItems { get; set; } = 8;

        public int Count
        {
            get { return items.Count; }
        }

        public int SelectedIndex
        {
            get { return selected; }
        }

        /// <summary>The chosen item's value, or the default when there are no items.</summary>
        public T Value
        {
            get { return (selected >= 0) ? items[selected].Value : default; }
        }

        /// <summary>The chosen item's text, or empty when there are no items.</summary>
        public string Text
        {
            get { return (selected >= 0) ? items[selected].Text : ""; }
        }

        UIFont Font
        {
            get { return text.TextFont; }
        }

        string ShownText
        {
            get { return (selected >= 0) ? Text : "(none)"; }
        }

        /// <summary>The items, as the text shown for each and its value. Selects the item at <paramref name="select"/>.</summary>
        public void SetItems(IEnumerable<(string Text, T Value)> newItems, int select = 0)
        {
            items.Clear();
            items.AddRange(newItems);

            selected = (items.Count > 0) ? Math.Clamp(select, 0, items.Count - 1) : -1;

            ShowSelected();
        }

        /// <summary>Selects the first item whose value matches, or the first item.</summary>
        public void SelectValue(Func<T, bool> match)
        {
            int index = items.FindIndex(i => match(i.Value));

            selected = (index >= 0) ? index : ((items.Count > 0) ? 0 : -1);

            ShowSelected();
        }

        void ShowSelected()
        {
            text.Text = ShownText;
            text.TextColor = (selected >= 0) ? Layout.Current.Theme.Text : Layout.Current.Theme.MutedText;
        }

        bool IsOpen
        {
            get { return (list != null) && (Layout.Current.ActiveUIElement is UIElementWrapper wrapper) && (wrapper.Child == list); }
        }

        public override void HandleInput(InputManager inputManager)
        {
            base.HandleInput(inputManager);

            UITheme theme = Layout.Current.Theme;

            frame.BorderColor = (IsMouseOver || IsOpen) ? theme.Accent : theme.Border;
        }

        public override bool HandleTouch(in Touch touch)
        {
            if (IsTap(touch, this))
                Open();

            return true;
        }

        void Open()
        {
            if (items.Count == 0)
                return;

            list = new DropDownList(this);

            Layout.Current.ShowPopup(list, new Vector2(ContentBounds.X, ContentBounds.Bottom));
        }

        void Choose(int index)
        {
            if (index == selected)
                return;

            selected = index;

            ShowSelected();

            Layout.Current.UpdateLayout();

            if (SelectAction != null)
                SelectAction();
        }

        /// <summary>
        /// The open list: an <see cref="InputFrame"/> with the accent border, holding a row per visible item. The row
        /// under the mouse has the active background; the chosen item's text is in the accent color. Scrolls with the
        /// mouse wheel when there are more items than it shows, with the theme's scroll thumb at the right showing
        /// where.
        /// </summary>
        class DropDownList : InputFrame, IPopup
        {
            const float RowPadY = 4;
            const float ThumbWidth = 6;

            DropDown<T> owner;
            List<Row> rows = new List<Row>();
            NinePatchWrapper thumb;
            Dock content;
            float rowHeight;
            int top;

            public DropDownList(DropDown<T> owner)
            {
                this.owner = owner;

                UITheme theme = Layout.Current.Theme;

                // Placed by the popup at its top left. Not Absolute: an Absolute element reports no size to its parent, so
                // the popup couldn't move it above the drop-down when it doesn't fit below, or find what's under the mouse.
                HorizontalAlignment = EHorizontalAlignment.Left;
                VerticalAlignment = EVerticalAlignment.Top;
                BorderColor = theme.Accent;

                rowHeight = owner.Font.TextHeight + (2 * RowPadY);

                VerticalStack stack = new VerticalStack { HorizontalAlignment = EHorizontalAlignment.Stretch };

                for (int i = 0; i < Shown; i++)
                {
                    Row row = new Row(this, i, owner.Font) { DesiredHeight = rowHeight };

                    rows.Add(row);
                    stack.Children.Add(row);
                }

                thumb = new NinePatchWrapper(Layout.Current.DefaultScrollThumbNinePatch)
                {
                    HorizontalAlignment = EHorizontalAlignment.Right,
                    VerticalAlignment = EVerticalAlignment.Top,
                    DesiredWidth = ThumbWidth,
                    Color = theme.Border,
                    Visible = owner.items.Count > Shown
                };

                content = new Dock();
                content.Children.Add(stack);
                content.Children.Add(thumb);

                Content = content;

                // At least as wide as the drop-down, and as the widest item.
                float width = owner.ContentBounds.Width;

                foreach (var item in owner.items)
                {
                    owner.Font.MeasureString(item.Text, out float textWidth, out _);

                    width = Math.Max(width, textWidth + (2 * Row.TextMarginX) + BorderPadding.Left + BorderPadding.Right + ThumbWidth);
                }

                DesiredWidth = width;

                // Opens with the chosen item in view.
                top = Math.Clamp(owner.selected - (Shown / 2), 0, owner.items.Count - Shown);

                ShowRows();
            }

            public Action CloseAction { get; set; }

            public void Opened()
            {
            }

            int Shown
            {
                get { return Math.Max(1, Math.Min(owner.MaxVisibleItems, owner.items.Count)); }
            }

            void ShowRows()
            {
                UITheme theme = Layout.Current.Theme;

                for (int i = 0; i < rows.Count; i++)
                {
                    int index = top + i;

                    rows[i].Text.Text = owner.items[index].Text;
                    rows[i].Text.TextColor = (index == owner.selected) ? theme.Accent : theme.Text;
                }

                if (thumb.Visible)
                {
                    float track = Shown * rowHeight;

                    // No shorter than the thumb image's corners.
                    thumb.DesiredHeight = Math.Max(thumb.Image.LogicalHeight, track * Shown / owner.items.Count);
                    thumb.Margin = new LayoutPadding(0, track * top / owner.items.Count, 1, 0);
                }

                content.UpdateContentLayout();
            }

            public override void HandleInput(InputManager inputManager)
            {
                base.HandleInput(inputManager);

                UITheme theme = Layout.Current.Theme;

                foreach (Row row in rows)
                    row.BackgroundColor = row.IsMouseOver ? theme.Active : UIColor.Transparent;

                int wheel = inputManager.MouseWheelDelta;

                if (wheel != 0)
                {
                    int newTop = Math.Clamp(top + ((wheel > 0) ? -1 : 1), 0, owner.items.Count - Shown);

                    if (newTop != top)
                    {
                        top = newTop;

                        ShowRows();
                    }
                }
            }

            void Choose(int row)
            {
                if (CloseAction != null)
                    CloseAction();

                owner.Choose(top + row);
            }

            /// <summary>One item's row: its text, with the active background when the mouse is over it.</summary>
            class Row : UIElementWrapper
            {
                public const float TextMarginX = 5;

                DropDownList list;
                int index;

                public Row(DropDownList list, int index, UIFont font)
                {
                    this.list = list;
                    this.index = index;

                    HorizontalAlignment = EHorizontalAlignment.Stretch;

                    Text = new TextBlock
                    {
                        TextFont = font,
                        HorizontalAlignment = EHorizontalAlignment.Left,
                        VerticalAlignment = EVerticalAlignment.Center,
                        Margin = new LayoutPadding(TextMarginX, 0)
                    };

                    Child = Text;
                }

                public TextBlock Text { get; private set; }

                public override bool HandleTouch(in Touch touch)
                {
                    if (touch.TouchState == ETouchState.Released)
                        list.Choose(index);

                    return true;
                }
            }
        }
    }
}
