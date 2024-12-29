using System;
using System.Collections.Generic;
using System.Text;
using UILayout;

namespace UILayout
{
    public class TabPanelTab
    {
        public string Name { get; set; }
        public UIElement Contents { get; set; }
        public TabPanelButton Button { get; set; }
    }

    public class TabPanelButton : TextButton
    {
        public TabPanelButton(string text)
            : base(text)
        {
            VerticalAlignment = EVerticalAlignment.Bottom;
        }
    }


    public class TabPanel : VerticalStack
    {
        public TabPanelTab ActiveTab { get; private set; }

        Dictionary<string, TabPanelTab> tabs = new Dictionary<string, TabPanelTab>();

        HorizontalStack headerStack;
        NinePatchWrapper contentsPanel;
        UIElementWrapper contentsWrapper;

        public TabPanel(UIColor backgroundColor, UIImage backroundImage, UIImage tabButtonPressed, UIImage tabButtonUnpressed, float tabStartOffsetX, float tabStartOfsetY)
        {
            HorizontalAlignment = EHorizontalAlignment.Stretch;
            VerticalAlignment = EVerticalAlignment.Stretch;

            Children.Add(new UIElement { DesiredHeight = tabStartOfsetY });

            headerStack = new HorizontalStack()
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                Padding = (tabStartOffsetX, 0),
                ChildSpacing = 5
            };
            Children.Add(headerStack);

            contentsPanel = new NinePatchWrapper(backroundImage)
            {
                DrawCenter = false,
                //DrawOnTop = true,
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Stretch
            };
            Children.Add(contentsPanel);

            contentsWrapper = new UIElementWrapper
            {
                BackgroundColor = backgroundColor,
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Stretch,
                Padding = 11
            };
            contentsPanel.Child = contentsWrapper;
        }

        public void AddTab(TabPanelTab tab)
        {
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

                //tab.Button.PressedNinePatch = tab.Button.UnpressedNinePatch = PixGame.Instance.GetImage("TabForeground");
            }
            else
            {
                //tab.Button.PressedNinePatch = tab.Button.UnpressedNinePatch = PixGame.Instance.GetImage("TabBackground");
            }
        }

        public void SetTab(string tabName)
        {
            foreach (TabPanelTab tab in tabs.Values)
            {
                if (tab.Name == tabName)
                {
                    ActiveTab = tab;

                    contentsWrapper.Child = tab.Contents;

                    //tab.Button.PressedNinePatch = tab.Button.UnpressedNinePatch = PixGame.Instance.GetImage("TabForeground");
                }
                else
                {
                    //tab.Button.PressedNinePatch = tab.Button.UnpressedNinePatch = PixGame.Instance.GetImage("TabBackground");
                }
            }

            UpdateContentLayout();
        }
    }
}
