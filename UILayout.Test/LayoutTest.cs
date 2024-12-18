using System;
using System.Collections.Generic;

namespace UILayout.Test
{
    public class LayoutTest : Dock
    {
        TextBlock spaceText;

        public LayoutTest()
        {
            BackgroundColor = UIColor.Yellow;
            Padding = new LayoutPadding(10);

            Layout.Current.AddImage("ScrollBar");
            Layout.Current.AddImage("ScrollBarGutter");
            Layout.Current.AddImage("ScrollUpArrow");
            Layout.Current.AddImage("ScrollDownArrow");

            Layout.Current.DefaultOutlineNinePatch = Layout.Current.AddImage("OutlineNinePatch");

            Layout.Current.DefaultPressedNinePatch = Layout.Current.AddImage("ButtonPressed");
            Layout.Current.DefaultUnpressedNinePatch = Layout.Current.AddImage("ButtonUnpressed");

            Layout.Current.DefaultDragImage = Layout.Current.GetImage("ButtonUnpressed");

            Layout.Current.DefaultForegroundColor = UIColor.Black;

            VerticalSlider verticalSlider = new VerticalSlider("ButtonPressed")
            {
                DesiredHeight = 100
            };
            Children.Add(verticalSlider);

            //EditableImage editImage = new EditableImage(new UIImage(100, 100));
            //editImage.DrawCircle(50, 50, 40, UIColor.Black, fill: false);
            //editImage.Fill(50, 50, UIColor.Red);
            //editImage.UpdateImageData();

            //Children.Add(new ImageElement(editImage.Image)
            //{
            //    HorizontalAlignment = EHorizontalAlignment.Center,
            //    VerticalAlignment = EVerticalAlignment.Top
            //});

            HorizontalStack stack = new HorizontalStack
            {
                BackgroundColor = new UIColor(0, 0, 255),
                HorizontalAlignment = EHorizontalAlignment.Right,
                VerticalAlignment = EVerticalAlignment.Top,
                Padding = new LayoutPadding(10),
                DesiredHeight = 100,
                DesiredWidth = 100,
                ChildSpacing = 10
            };
            Children.Add(stack);

            for (int i = 0; i < 4; i++)
            {
                stack.Children.Add(new UIElement
                {
                    BackgroundColor = new UIColor(0, 0, 0, (32 * (i + 1))),
                    HorizontalAlignment = EHorizontalAlignment.Stretch,
                    VerticalAlignment = EVerticalAlignment.Stretch,
                });
            }

            VerticalStack bottomStack = new VerticalStack()
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Bottom,
                ChildSpacing = 20
            };
            Children.Add(bottomStack);

            TextBox textBox = new TextBox(100)
            {                
                DesiredWidth = 200,
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center,
                BackgroundColor = UIColor.Black
            };

            textBox.TextColor = UIColor.White;

            bottomStack.Children.Add(textBox);

            HorizontalStack buttonStack = new HorizontalStack()
            {
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center
            };
            bottomStack.Children.Add(buttonStack);

            InputDialog dialog = new InputDialog(Layout.Current.DefaultOutlineNinePatch, new TextBlock { Text = "Do you want to?", TextColor = UIColor.Black });

            dialog.AddInput(new DialogInput { Text = "Ok", CloseOnInput = true });
            dialog.AddInput(new DialogInput { Text = "Cancel", CloseOnInput = true });

            buttonStack.Children.Add(new TextButton()
            {
                Text = "Input Dialog",
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center,
                ClickAction = delegate
                {
                    Layout.Current.ShowPopup(dialog);
                }
            });

            List<MenuItem> menuItems = new List<MenuItem>
            {
                new ContextMenuItem { Text = "Item 1" },
                new ContextMenuItem { Text = "Item 2"},
                new ContextMenuItem { Text = "Item 3 longer"}
            };

            Menu menu = new Menu(menuItems);

            TextButton menuButton = new TextButton()
            {
                Text = "Popup Menu",
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center,
            };

            menuButton.ClickAction = delegate
            {
                Layout.Current.ShowPopup(menu, menuButton.ContentBounds.Center);
            };

            buttonStack.Children.Add(menuButton);

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

            buttonStack.Children.Add(new TextButton()
            {
                Text = "Swipe List",
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center,
                ClickAction = delegate
                {
                    Layout.Current.ShowPopup(swipeStack);
                }
            });

            buttonStack.Children.Add(new TextButton()
            {
                Text = "Text Input",
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center,
                ClickAction = delegate
                {
                    Layout.Current.GetKeyboardInputAsync("Type Something", "Default Text").ContinueWith(input => Layout.Current.ShowContinuePopup("You entered: " + input.Result));
                }
            });

            HorizontalStack dragDropStack = new HorizontalStack()
            {
                HorizontalAlignment = EHorizontalAlignment.Center,
                ChildSpacing = 2
            };
            bottomStack.Children.Add(dragDropStack);

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

            bottomStack.Children.Add(dragDropStack);

            Layout.Current.InputManager.AddMapping("SpacePressed", new KeyMapping(InputKey.Space));

            bottomStack.Children.Add(spaceText = new TextBlock
            {
                Text = "Press Space",
                TextColor = UIColor.Black,
                BackgroundColor = UIColor.Green,
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center,
                Padding = new LayoutPadding(20, 10)
            });

            bottomStack.Children.Add(new TextBlock
            {
                Text = "Descendery Text",
                TextColor = UIColor.Black,
                BackgroundColor = UIColor.Green,
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center,
                Margin = new LayoutPadding(20)
            });

        }

        public override void HandleInput(InputManager inputManager)
        {
            base.HandleInput(inputManager);

            if (inputManager.WasPressed("SpacePressed"))
            {
                spaceText.BackgroundColor = UIColor.Red;
            }

            if (inputManager.WasReleased("SpacePressed"))
            {
                spaceText.BackgroundColor = UIColor.Green;
            }
        }
    }
}
