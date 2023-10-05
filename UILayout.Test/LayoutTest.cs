using System;
#if !GENERICS_UNSUPPORTED
using System.Collections.Generic;
using MenuItemCollection = System.Collections.Generic.List<UILayout.MenuItem>;
#else
using MenuItemCollection = ArrayList;
#endif

namespace UILayout.Test
{
    public class LayoutTest : Dock
    {
        TextBlock spaceText;

        public LayoutTest()
        {
            BackgroundColor = UIColor.Yellow;
            Padding = new LayoutPadding(10);

            Layout.DefaultOutlineNinePatch = Layout.Current.AddImage("OutlineNinePatch");

            Layout.DefaultPressedNinePatch = Layout.Current.AddImage("ButtonPressed");
            Layout.DefaultUnpressedNinePatch = Layout.Current.AddImage("ButtonUnpressed");

            Layout.DefaultDragImage = Layout.Current.GetImage("ButtonPressed");

            TextBlock.DefaultColor = UIColor.Black;

            VerticalSlider verticalSlider = new VerticalSlider("ButtonPressed")
            {
                DesiredHeight = 100
            };
            Children.Add(verticalSlider);

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

            HorizontalStack buttonStack = new HorizontalStack()
            {
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center
            };
            Children.Add(buttonStack);

            InputDialog dialog = new InputDialog(Layout.DefaultOutlineNinePatch, new TextBlock { Text = "Do you want to?", TextColor = UIColor.Black });

            dialog.AddInput(new DialogInput { Text = "Ok", CloseOnInput = true } );
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

            MenuItemCollection menuItems = new MenuItemCollection()
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

            SwipeList swipeList = new SwipeList()
            {
                Font = TextBlock.DefaultFont,
                TextColor = UIColor.Black,
                VerticalAlignment = EVerticalAlignment.Stretch,
                HorizontalAlignment = EHorizontalAlignment.Center,
                DesiredWidth = 100,
                BackgroundColor = UIColor.White
            };

            List<string> items = new List<string>();

            for (int i = 0; i < 30; i++)
            {
                items.Add("Item " + i);
            }

            swipeList.Items = items;
            swipeList.SelectAction = delegate (int item)
            {
                Layout.Current.ClosePopup(swipeList);
            };

            buttonStack.Children.Add(new TextButton()
            {
                Text = "Swipe List",
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center,
                ClickAction = delegate
                {
                    Layout.Current.ShowPopup(swipeList);
                }
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
