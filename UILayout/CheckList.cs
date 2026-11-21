using System;
using System.Collections.Generic;

namespace UILayout
{
    /// <summary>
    /// A list of items, each with a check box, that scrolls (mouse wheel or drag). Clicking an item checks or unchecks
    /// it. Items are drawn as <see cref="SwipeList"/> draws them, with the theme's check box images before the text.
    /// Put it in an <see cref="InputFrame"/> for the input-style box.
    /// </summary>
    public class CheckList<T> : SwipeList
    {
        const float BoxGap = 8;

        List<(string Text, T Value)> items = new List<(string Text, T Value)>();
        HashSet<T> checkedValues;

        /// <param name="comparer">How values are compared to find the checked ones (default: the type's own equality).</param>
        public CheckList(IEqualityComparer<T> comparer = null)
        {
            checkedValues = new HashSet<T>(comparer ?? EqualityComparer<T>.Default);

            // The list's own: clicking an item toggles it (use CheckAction to hear about it).
            SelectAction = Toggle;

            TextColor = Layout.Current.Theme.Text;
            Padding = new LayoutPadding(0, 1);
        }

        /// <summary>Called when the user checks or unchecks an item.</summary>
        public Action CheckAction { get; set; }

        /// <summary>Shown when there are no items.</summary>
        public string EmptyText { get; set; } = "None";

        /// <summary>The checked items' values, in the items' order.</summary>
        public List<T> Checked
        {
            get
            {
                List<T> result = new List<T>();

                foreach (var item in items)
                {
                    if (checkedValues.Contains(item.Value))
                        result.Add(item.Value);
                }

                return result;
            }
        }

        /// <summary>The items, as the text shown for each and its value, and which values are checked.</summary>
        public void SetItems(IEnumerable<(string Text, T Value)> newItems, IEnumerable<T> checkedItems)
        {
            items.Clear();
            items.AddRange(newItems);

            checkedValues.Clear();

            foreach (T value in checkedItems)
                checkedValues.Add(value);

            List<string> texts = new List<string>();

            foreach (var item in items)
                texts.Add(item.Text);

            if (texts.Count == 0)
                texts.Add(EmptyText);

            Items = texts;
        }

        public bool IsChecked(T value)
        {
            return checkedValues.Contains(value);
        }

        void Toggle(int index)
        {
            // Checked, not highlighted as selected.
            LastSelectedItem = -1;

            if (index >= items.Count)
                return;

            T value = items[index].Value;

            if (!checkedValues.Remove(value))
                checkedValues.Add(value);

            if (CheckAction != null)
                CheckAction();
        }

        // The item under the mouse has the theme's active background.
        protected override void DrawItem(int item, float y)
        {
            if ((item < items.Count) && IsMouseOver)
            {
                float mouseY = Layout.Current.InputManager.MousePosition.Y;

                if ((mouseY >= y) && (mouseY < (y + ItemHeight)))
                    Layout.Current.GraphicsContext.DrawRectangle(new RectF(ContentBounds.X, y, ContentBounds.Width, ItemHeight), Layout.Current.Theme.Active);
            }

            base.DrawItem(item, y);
        }

        // A check box, then the item's text.
        public override void DrawItemContents(int item, float x, float y)
        {
            if (item >= items.Count)
            {
                // The empty text.
                base.DrawItemContents(item, x, y);

                return;
            }

            UITheme theme = Layout.Current.Theme;
            GraphicsContext2D g = Layout.Current.GraphicsContext;
            bool isChecked = checkedValues.Contains(items[item].Value);

            UIImage fill = Layout.Current.DefaultCheckBoxFill;
            UIImage border = Layout.Current.DefaultCheckBoxBorder;
            UIImage check = Layout.Current.DefaultCheckMark;

            int boxX = (int)x;
            int boxY = (int)(y + ((Font.TextHeight - fill.Height) / 2));

            g.DrawImage(fill, boxX, boxY, isChecked ? theme.Accent : theme.Background);
            g.DrawImage(border, boxX, boxY, isChecked ? theme.Accent : theme.Border);

            if (isChecked)
                g.DrawImage(check, boxX + ((fill.Width - check.Width) / 2), boxY + ((fill.Height - check.Height) / 2), theme.Background);

            g.DrawText(items[item].Text, Font, x + fill.Width + BoxGap, y, TextColor, FontScale);
        }
    }
}
