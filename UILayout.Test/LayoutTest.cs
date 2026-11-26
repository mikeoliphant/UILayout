using System;
using System.Collections.Generic;
using System.Linq;

namespace UILayout.Test
{
    /// <summary>
    /// The test apps' layout: UILayout's controls, in outlined sections, all drawn from the layout's theme.
    /// </summary>
    public class LayoutTest : Dock
    {
        const int RowHeight = 34;
        const int LabelWidth = 130;
        const int Spacing = 8;
        const int LeftColumnWidth = 280;

        Frame spaceFrame;
        FittedTextBlock choiceText;
        DropDown<string> colorDropDown;
        CheckList<string> fruitList;
        TextBox nameBox;
        Slider levelSlider;

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

            TabPanel tabPanel = new TabPanel();

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

            InputDialog dialog = new InputDialog(new TextBlock { Text = "Do you want to?" });

            dialog.AddInput(new DialogInput { Text = "Ok", CloseOnInput = true, IsPrimary = true });
            dialog.AddInput(new DialogInput { Text = "Cancel", CloseOnInput = true });

            buttons.Children.Add(new TextButton("Input Dialog", () => Layout.Current.ShowPopup(dialog)));

            List<MenuItem> menuItems = new List<MenuItem>
            {
                new ContextMenuItem { Text = "Item 1" },
                new ContextMenuItem { Text = "Item 2"},
                new ContextMenuItem { Text = "Item 3 longer"}
            };

            Menu menu = new Menu(menuItems);

            TextButton menuButton = new TextButton("Popup Menu");
            menuButton.ClickAction = () => Layout.Current.ShowPopup(menu, menuButton.ContentBounds.Center);

            buttons.Children.Add(menuButton);

            UIElement swipeStack = CreateSwipeList();

            // A list to pick from: a click outside it closes it
            buttons.Children.Add(new TextButton("Swipe List", () => Layout.Current.ShowPopup(swipeStack,
                closeOnOutsideClick: true)));

            buttons.Children.Add(new TextButton("Text Input", () =>
                Layout.Current.ShowTextInputPopup("Enter text:",
                    str => Layout.Current.ShowContinuePopup("You entered: " + str))));

            FileSelector fileSelector = new FileSelector("File Selection:", canCreateFolders: false)
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Stretch
            };

            fileSelector.SetRootPath(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            fileSelector.FileAction = delegate (string filePath)
            {
                Layout.Current.ShowContinuePopup("Path Selected:\n\n" + filePath);
            };

            buttons.Children.Add(new TextButton("File Selection", () => Layout.Current.ShowPopup(fileSelector)));

            buttons.Children.Add(new TextButton("Native File Selection", () =>
                Layout.Current.GetFile(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))));

            foreach (UIElement button in buttons.Children)
            {
                button.HorizontalAlignment = EHorizontalAlignment.Stretch;
                button.DesiredHeight = RowHeight;
            }

            return Section(Header("Popups"), buttons);
        }

        // A list with a scroll bar, in a frame; choosing an item closes it.
        UIElement CreateSwipeList()
        {
            HorizontalStack swipeStack = new HorizontalStack()
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Stretch,
                ChildSpacing = 4
            };

            SwipeList swipeList = new SwipeList()
            {
                DesiredHeight = 200,
                DesiredWidth = 120
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

            // A popup's frame (the accent border, as all popups'), centered
            PopupWrapper popup = new PopupWrapper(swipeStack)
            {
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center
            };

            swipeList.SelectAction = delegate (int item)
            {
                Layout.Current.ClosePopup(popup);
            };

            return popup;
        }

        // The input controls, with what they're set to.
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
                VerticalAlignment = EVerticalAlignment.Stretch
            };
            string[] fruits = { "Apples", "Bananas", "Cherries", "Dates", "Elderberries", "Figs" };

            fruitList.SetItems(fruits.Select(f => (f, f)), new[] { "Bananas" });
            fruitList.CheckAction = ShowChoices;

            Frame fruitFrame = new Frame(fruitList)
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                DesiredHeight = 4 * 29
            };

            HorizontalStack buttons = new HorizontalStack()
            {
                DesiredHeight = RowHeight,
                ChildSpacing = Spacing
            };

            buttons.Children.Add(new TextToggleButton("On", "Off"));
            buttons.Children.Add(new TextButton("Primary") { Primary = true });
            buttons.Children.Add(new ImageButton(theme.ScrollUpArrow)
            {
                DesiredWidth = RowHeight,
                VerticalAlignment = EVerticalAlignment.Stretch
            });

            spaceFrame = new Frame(new TextBlock("Space")
            {
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center,
                Margin = (6, 0)
            })
            {
                VerticalAlignment = EVerticalAlignment.Stretch
            };
            buttons.Children.Add(spaceFrame);

            Layout.Current.InputManager.AddMapping("SpacePressed", new KeyMapping(InputKey.Space));

            nameBox = new TextBox(64)
            {
                EnterAction = ShowChoices
            };
            nameBox.SetText("Type here");

            levelSlider = new Slider()
            {
                ValueChanged = ShowChoices
            };

            choiceText = new FittedTextBlock()
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Center,
                TextColor = theme.MutedText
            };

            ShowChoices();

            return Section(Header("Input controls"), Row("Drop-down", colorDropDown), Row("Check list", fruitFrame),
                Row("Buttons", buttons), Row("Text box", nameBox), Row("Slider", levelSlider),
                Row("Chosen", choiceText));
        }

        void ShowChoices()
        {
            string chosenFruit = (fruitList.Checked.Count > 0) ? string.Join(", ", fruitList.Checked) : "no fruit";

            // Stretched, the text block's size doesn't change: setting the text redraws it, with nothing to lay out
            choiceText.Text = colorDropDown.Value + "; " + chosenFruit + "; " + nameBox.GetText() + "; " +
                ((int)Math.Round(levelSlider.Value * 100)).ToString() + "%";
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
                DragType = typeof(Frame),
                InternalOnly = true
            };

            dragDropStack.DragDropHandler = dragDropHander;

            for (int i = 0; i < 5; i++)
            {
                dragDropStack.Children.Add(new Frame(new TextBlock("Drag " + (i + 1).ToString())
                {
                    Margin = new LayoutPadding(6, 2)
                })
                {
                    HorizontalAlignment = EHorizontalAlignment.Left,
                    VerticalAlignment = EVerticalAlignment.Top
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
                VerticalAlignment = EVerticalAlignment.Center,
                DesiredWidth = LabelWidth - Spacing,
                Margin = new LayoutPadding(6, 0, 0, 0)
            });

            if (content.HorizontalAlignment != EHorizontalAlignment.Left)
                content.HorizontalAlignment = EHorizontalAlignment.Stretch;

            // Controls get the row height; text keeps its own, centered like the label.
            if ((content.DesiredHeight == 0) && !(content is FittedTextBlock))
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
