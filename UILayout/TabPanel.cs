using System;
using System.Collections.Generic;
using System.Text;
using UILayout;

namespace UILayout
{
    public class TabPanelTab
    {
        public TabPanelTab(string name, UIElement contents)
        {
            this.Name = name;
            this.Contents = contents;
        }

        public string Name { get; set; }
        public UIElement Contents { get; set; }
        public TabPanelButton Button { get; internal set; }
    }

    /// <summary>
    /// A tab's button: a toggle <see cref="TextButton"/> drawn with the theme's tab images (top corners rounded, no
    /// bottom edge). On (its tab shown), it's filled as the panel below and reaches over the panel's top border, so it
    /// joins the panel; off, it's in the background color and stops at the border. The border takes the accent color
    /// when the mouse is over a tab that's off.
    /// </summary>
    public class TabPanelButton : TextButton
    {
        public TabPanelButton(string text)
            : base(text)
        {
            IsToggleButton = true;

            UITheme theme = Layout.Current.Theme;

            Frame.SetImages(theme.TabFill, theme.TabBorder);

            // No bottom edge: no room for it below the text.
            LayoutPadding padding = Frame.ContentPadding;

            Frame.ContentPadding = new LayoutPadding(padding.Left, padding.Top, padding.Right, 1);

            VerticalAlignment = EVerticalAlignment.Stretch;

            UpdateColors();
        }

        protected override void StateChanged()
        {
            base.StateChanged();

            // Off, it stops above the panel's border (which then closes it below).
            Margin = new LayoutPadding(0, IsPressed ? 0 : 1, 0, IsPressed ? 0 : 1);
        }

        protected override void UpdateColors()
        {
            // Called from FrameButton's constructor too, before the text exists.
            if (Content == null)
                return;

            UITheme theme = Layout.Current.Theme;

            Frame.FillColor = IsPressed ? theme.Surface : theme.Background;
            Frame.BorderColor = (IsMouseOver && !IsPressed) ? theme.Accent : theme.Border;
            TextColor = IsPressed ? theme.Text : theme.MutedText;
        }
    }

    /// <summary>
    /// Tabs: a row of tab buttons, one per tab, on a <see cref="Frame"/> showing the chosen tab's contents. The chosen
    /// tab's button joins the frame. It's as big as its biggest tab.
    /// </summary>
    public class TabPanel : VerticalStack
    {
        const float TabIndent = 6;
        const float TabSpacing = 2;

        public TabPanelTab ActiveTab { get; private set; }

        Dictionary<string, TabPanelTab> tabs = new Dictionary<string, TabPanelTab>();

        HorizontalStack headerStack;
        UIElementWrapper contentsWrapper;

        public TabPanel()
        {
            HorizontalAlignment = EHorizontalAlignment.Stretch;
            VerticalAlignment = EVerticalAlignment.Stretch;

            // The tabs reach a pixel down over the frame's top border, so they are drawn after it.
            DrawInReverse = true;

            headerStack = new HorizontalStack()
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                ChildSpacing = TabSpacing,
                Margin = new LayoutPadding(TabIndent, 0, TabIndent, -1)
            };
            Children.Add(headerStack);

            contentsWrapper = new UIElementWrapper
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Stretch
            };

            Children.Add(new Frame(contentsWrapper)
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Stretch,
                ContentPadding = new LayoutPadding(8)
            });
        }

        protected override void GetContentSize(out float width, out float height)
        {
            width = 0;
            height = 0;

            UIElement currentContents = contentsWrapper.Child;

            foreach (var tab in tabs.Values)
            {
                float tabWidth;
                float tabHeight;

                contentsWrapper.Child = tab.Contents;

                base.GetContentSize(out tabWidth, out tabHeight);

                width = Math.Max(width, tabWidth);
                height = Math.Max(height, tabHeight);
            }

            contentsWrapper.Child = currentContents;
        }

        public TabPanelTab AddTab(string text, UIElement contents)
        {
            TabPanelTab tab = new TabPanelTab(text, contents);

            tabs[tab.Name] = tab;

            headerStack.Children.Add(tab.Button = new TabPanelButton(tab.Name)
            {
                PressAction = delegate
                {
                    SetTab(tab.Name);
                }
            });

            if (tabs.Count == 1)
            {
                ActiveTab = tab;
                contentsWrapper.Child = tab.Contents;
            }

            tab.Button.SetPressed(tab == ActiveTab);

            // Off from the start doesn't change its state: set its look for off.
            if (tab != ActiveTab)
                tab.Button.Margin = new LayoutPadding(0, 1, 0, 1);

            return tab;
        }

        public void SetTab(string tabName)
        {
            if (tabs.TryGetValue(tabName, out TabPanelTab active))
            {
                ActiveTab = active;

                contentsWrapper.Child = active.Contents;
            }

            // Pressing the shown tab's button again toggles it off: keep it on.
            foreach (TabPanelTab tab in tabs.Values)
                tab.Button.SetPressed(tab == ActiveTab);

            // Lays out the tabs (their margins) and the shown contents, and makes the panel dirty
            UpdateContentLayout();
        }
    }
}
