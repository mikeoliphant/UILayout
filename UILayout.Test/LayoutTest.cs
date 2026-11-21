using System;
using System.Collections.Generic;
using System.Linq;

namespace UILayout.Test
{
    /// <summary>
    /// The test apps' layout: UILayout's controls, in outlined sections in the theme's colors (the input-style
    /// controls' look). Older controls (dialogs, menus, tabs) keep their own look until they are themed.
    /// </summary>
    public class LayoutTest : Dock
    {
        const int RowHeight = 34;
        const int LabelWidth = 130;
        const int Spacing = 8;
        const int LeftColumnWidth = 260;

        InputFrame spaceFrame;
        FittedTextBlock choiceText;
        DropDown<string> colorDropDown;
        CheckList<string> fruitList;

        public LayoutTest()
        {
            UITheme theme = Layout.Current.Theme;

            BackgroundColor = theme.Background;
            Padding = 10;

            // Two columns: popups and tabs on the left, the input controls and drag and drop on the right.
            HorizontalStack columns = new HorizontalStack()
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Stretch,
                ChildSpacing = Spacing
            };
            Children.Add(columns);

            VerticalStack left = new VerticalStack()
            {
                DesiredWidth = LeftColumnWidth,
                VerticalAlignment = EVerticalAlignment.Stretch,
                ChildSpacing = Spacing
            };
            columns.Children.Add(left);

            VerticalStack right = new VerticalStack()
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Stretch,
                ChildSpacing = Spacing
            };
            columns.Children.Add(right);

            left.Children.Add(CreatePopupSection());

            right.Children.Add(CreateInputSection());
            right.Children.Add(CreateDragDropSection());

            TabPanel tabPanel = new TabPanel(new UIColor(100, 100, 100), UIColor.White,
                Layout.Current.GetImage("TabPanelBackground"), Layout.Current.GetImage("TabForeground"),
                Layout.Current.GetImage("TabBackground"), 5, 5);

            tabPanel.AddTab("Tab 1", new TextBlock("This is tab 1"));
            tabPanel.AddTab("Tab 2", new TextBlock("This is tab 2"));
            tabPanel.AddTab("Tab 3", new TextBlock("This is tab 3"));

            left.Children.Add(Section(Header("Tabs"), tabPanel));
        }

        // Buttons that open UILayout's popups, one under the other.
        UIElement CreatePopupSection()
        {
            VerticalStack buttons = new VerticalStack()
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                ChildSpacing = 4
            };

            InputDialog dialog = new InputDialog(Layout.Current.DefaultOutlineNinePatch,
                new TextBlock { Text = "Do you want to?" });

            dialog.AddInput(new DialogInput { Text = "Ok", CloseOnInput = true });
            dialog.AddInput(new DialogInput { Text = "Cancel", CloseOnInput = true });

            buttons.Children.Add(new FrameButton("Input Dialog", () => Layout.Current.ShowPopup(dialog)));

            List<MenuItem> menuItems = new List<MenuItem>
            {
                new ContextMenuItem { Text = "Item 1" },
                new ContextMenuItem { Text = "Item 2"},
                new ContextMenuItem { Text = "Item 3 longer"}
            };

            Menu menu = new Menu(menuItems);

            FrameButton menuButton = new FrameButton("Popup Menu", null);
            menuButton.ClickAction = () => Layout.Current.ShowPopup(menu, menuButton.ContentBounds.Center);

            buttons.Children.Add(menuButton);

            UIElement swipeStack = CreateSwipeList();

            buttons.Children.Add(new FrameButton("Swipe List", () => Layout.Current.ShowPopup(swipeStack)));

            buttons.Children.Add(new FrameButton("Text Input", () =>
                Layout.Current.ShowTextInputPopup("Enter text:",
                    str => Layout.Current.ShowContinuePopup("You entered: " + str))));

            FileSelector fileSelector = new FileSelector("File Selection:", canCreateFolders: false,
                Layout.Current.DefaultOutlineNinePatch)
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Stretch
            };

            fileSelector.SetRootPath(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            fileSelector.FileAction = delegate (string filePath)
            {
                Layout.Current.ShowContinuePopup("Path Selected:\n\n" + filePath);
            };

            buttons.Children.Add(new FrameButton("File Selection", () => Layout.Current.ShowPopup(fileSelector)));

            buttons.Children.Add(new FrameButton("Native File Selection", () =>
                Layout.Current.GetFile(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))));

            foreach (UIElement button in buttons.Children)
            {
                button.HorizontalAlignment = EHorizontalAlignment.Stretch;
                button.DesiredHeight = RowHeight;
            }

            return Section(Header("Popups"), buttons);
        }

        UIElement CreateSwipeList()
        {
            HorizontalStack swipeStack = new HorizontalStack()
            {
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center
            };

            SwipeList swipeList = new SwipeList()
            {
                TextColor = UIColor.Black,
                DesiredHeight = 200,
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center,
                DesiredWidth = 100,
                BackgroundColor = UIColor.White
            };
            swipeStack.Children.Add(swipeList);

            var scrollBar = new VerticalScrollBarWithArrows()
            {
                VerticalAlignment = EVerticalAlignment.Stretch,
            };

            swipeStack.Children.Add(scrollBar);

            swipeList.SetScrollBar(scrollBar.ScrollBar);

            List<string> items = new List<string>();

            for (int i = 0; i < 30; i++)
            {
                items.Add("Item " + i);
            }

            swipeList.Items = items;
            swipeList.SelectAction = delegate (int item)
            {
                Layout.Current.ClosePopup(swipeStack);
            };

            return swipeStack;
        }

        // The input-style controls, with what they're set to.
        UIElement CreateInputSection()
        {
            UITheme theme = Layout.Current.Theme;

            colorDropDown = new DropDown<string>();
            string[] colors = { "Red", "Orange", "Yellow", "Green", "Blue", "Indigo", "Violet", "Black", "White",
                "A color with a much longer name than the others" };

            colorDropDown.SetItems(colors.Select(c => (c, c)), select: 4);
            colorDropDown.SelectAction = ShowChoices;

            fruitList = new CheckList<string>()
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Stretch,
                TextColor = theme.Text
            };
            string[] fruits = { "Apples", "Bananas", "Cherries", "Dates", "Elderberries", "Figs" };

            fruitList.SetItems(fruits.Select(f => (f, f)), new[] { "Bananas" });
            fruitList.CheckAction = ShowChoices;

            InputFrame fruitFrame = new InputFrame(fruitList)
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                DesiredHeight = 4 * 29
            };

            HorizontalStack buttons = new HorizontalStack()
            {
                DesiredHeight = RowHeight,
                ChildSpacing = Spacing
            };

            buttons.Children.Add(new FrameButton("Toggle", null) { IsToggleButton = true });
            buttons.Children.Add(new FrameButton("Primary", null) { Primary = true });

            spaceFrame = new InputFrame(new TextBlock("Press Space")
            {
                TextColor = theme.Text,
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center,
                Margin = (6, 0)
            })
            {
                VerticalAlignment = EVerticalAlignment.Stretch
            };
            buttons.Children.Add(spaceFrame);

            Layout.Current.InputManager.AddMapping("SpacePressed", new KeyMapping(InputKey.Space));

            choiceText = new FittedTextBlock()
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Center,
                TextColor = theme.MutedText
            };

            ShowChoices();

            return Section(Header("Input controls"), Row("Drop-down", colorDropDown), Row("Check list", fruitFrame),
                Row("Buttons", buttons), Row("Chosen", choiceText));
        }

        void ShowChoices()
        {
            string chosenFruit = (fruitList.Checked.Count > 0) ? string.Join(", ", fruitList.Checked) : "no fruit";

            choiceText.Text = colorDropDown.Value + "; " + chosenFruit;

            Layout.Current.UpdateLayout();
        }

        UIElement CreateDragDropSection()
        {
            HorizontalStack dragDropStack = new HorizontalStack()
            {
                ChildSpacing = 2
            };

            ListUIDragDropHandler dragDropHander = new ListUIDragDropHandler()
            {
                ListElement = dragDropStack,
                DragType = typeof(NinePatchWrapper),
                InternalOnly = true
            };

            dragDropStack.DragDropHandler = dragDropHander;

            for (int i = 0; i < 5; i++)
            {
                dragDropStack.Children.Add(new NinePatchWrapper(Layout.Current.GetImage("ButtonPressed"))
                {
                    Child = new TextBlock("Drag " + (i + 1).ToString())
                });
            }

            return Section(Header("Drag and drop"), dragDropStack);
        }

        // An outlined group of rows.
        static Section Section(params UIElement[] rows)
        {
            VerticalStack stack = new VerticalStack()
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                ChildSpacing = 4
            };

            foreach (UIElement row in rows)
                stack.Children.Add(row);

            return new Section(stack)
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch
            };
        }

        static TextBlock Header(string text)
        {
            return new TextBlock(text)
            {
                TextColor = Layout.Current.Theme.MutedText,
                Margin = new LayoutPadding(6, 0, 0, 2)
            };
        }

        // A label, then the content taking the rest of the row.
        static HorizontalStack Row(string label, UIElement content)
        {
            HorizontalStack row = new HorizontalStack()
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                ChildSpacing = Spacing
            };

            row.Children.Add(new TextBlock(label)
            {
                TextColor = Layout.Current.Theme.Text,
                VerticalAlignment = EVerticalAlignment.Center,
                DesiredWidth = LabelWidth - Spacing,
                Margin = new LayoutPadding(6, 0, 0, 0)
            });

            if (content.HorizontalAlignment != EHorizontalAlignment.Left)
                content.HorizontalAlignment = EHorizontalAlignment.Stretch;

            if (content.DesiredHeight == 0)
                content.DesiredHeight = RowHeight;

            row.Children.Add(content);

            return row;
        }

        public override void HandleInput(InputManager inputManager)
        {
            base.HandleInput(inputManager);

            if (inputManager.WasPressed("SpacePressed"))
                spaceFrame.FillColor = Layout.Current.Theme.Active;

            if (inputManager.WasReleased("SpacePressed"))
                spaceFrame.FillColor = Layout.Current.Theme.Surface;
        }
    }
}
