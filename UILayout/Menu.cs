using System;
using System.Collections;
#if !GENERICS_UNSUPPORTED
using System.Collections.Generic;
using MenuItemCollection = System.Collections.Generic.List<UILayout.MenuItem>;
#else
using MenuItemCollection = ArrayList;
#endif

namespace UILayout
{
    public class Menu : VerticalStack, IPopup
    {
        public Action CloseAction { get; set; }

        public static Color DefaultTextColor = Color.White;
        public static Color DefaultTextHighlightColor = new Color(255, 255, 100, 255);

        public Color TextColor { get; set; }
        public Color TextHighlightColor { get; set; }

        public Menu()
        {
            TextColor = DefaultTextColor;
            TextHighlightColor = DefaultTextHighlightColor;

            HorizontalAlignment = EHorizontalAlignment.Center;
            VerticalAlignment = EVerticalAlignment.Center;
        }

        public Menu(MenuItemCollection menuItems)
            : this()
        {
            SetMenuItems(menuItems);
        }

        void SetMenuItems(MenuItemCollection menuItems)
        {
            Children.Clear();

            foreach (MenuItem menuItem in menuItems)
            {
                TextButton button = new TextButton(menuItem.Text);
                button.ClickAction = delegate { DoMenuItem(menuItem); };

                Children.Add(button);
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

        public void Opened()
        {

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
