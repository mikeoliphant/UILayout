using System;
using System.Collections.Generic;

namespace UILayout
{
    /// <summary>
    /// A popup menu: a <see cref="Frame"/> with the accent border, holding a row per item. The row under the mouse has
    /// the theme's active background, and clicking a row does its item. Placed at its top left: show it at an anchor
    /// point.
    /// </summary>
    public class Menu : Frame, IPopup
    {
        public Action CloseAction { get; set; }

        public UIColor TextColor { get; set; }
        public UIFont TextFont { get; set; }

        VerticalStack menuStack;

        public Menu()
        {
            UITheme theme = Layout.Current.Theme;

            HorizontalAlignment = EHorizontalAlignment.Left;
            VerticalAlignment = EVerticalAlignment.Top;
            BorderColor = theme.Accent;

            TextColor = theme.Text;
            TextFont = theme.Font;

            menuStack = new VerticalStack();
            Content = menuStack;
        }

        public Menu(List<MenuItem> menuItems)
            : this()
        {
            SetMenuItems(menuItems);
        }

        public void SetMenuItems(List<MenuItem> menuItems)
        {
            menuStack.Children.Clear();

            foreach (MenuItem menuItem in menuItems)
            {
                MenuRow row = new MenuRow(menuItem.Text, TextColor, TextFont);
                row.ClickAction = delegate { DoMenuItem(menuItem); };

                menuStack.Children.Add(row);
            }
        }

        void DoMenuItem(MenuItem item)
        {
            if (item.SelectAction != null)
                item.SelectAction();

            if (item.CloseOnSelect && (CloseAction != null))
            {
                CloseAction();
            }

            if (item.AfterCloseAction != null)
            {
                item.AfterCloseAction();
            }
        }

        public override void HandleInput(InputManager inputManager)
        {
            base.HandleInput(inputManager);

            UIColor active = Layout.Current.Theme.Active;

            foreach (UIElement child in menuStack.Children)
            {
                UIColor background = child.IsMouseOver ? active : UIColor.Transparent;

                if (child.BackgroundColor == background)
                    continue;

                child.BackgroundColor = background;

                Layout.Current.AddDirtyRect(child.LayoutBounds);
            }
        }

        public void Opened()
        {

        }

        /// <summary>One item's row: its text, clicking when released over it.</summary>
        class MenuRow : UIElementWrapper
        {
            const float TextMarginX = 8;
            const float TextMarginY = 4;

            public MenuRow(string text, UIColor textColor, UIFont textFont)
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch;

                Child = new TextBlock(text)
                {
                    TextColor = textColor,
                    TextFont = textFont,
                    HorizontalAlignment = EHorizontalAlignment.Left,
                    VerticalAlignment = EVerticalAlignment.Center,
                    Margin = new LayoutPadding(TextMarginX, TextMarginY)
                };
            }

            public Action ClickAction { get; set; }

            public override bool HandleTouch(in Touch touch)
            {
                if ((touch.TouchState == ETouchState.Released) && LayoutBounds.Contains(touch.Position) &&
                    (ClickAction != null))
                {
                    ClickAction();
                }

                return true;
            }
        }
    }

    public class MenuItem
    {
        public string Text { get; set; }
        public Action SelectAction { get; set; }
        public bool CloseOnSelect { get; set; }
        public Action AfterCloseAction { get; set; }
    }

    public class ContextMenuItem : MenuItem
    {
        public ContextMenuItem()
        {
            CloseOnSelect = true;
        }
    }
}
