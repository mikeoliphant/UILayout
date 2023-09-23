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
        public LayoutTest()
        {
            BackgroundColor = UIColor.Yellow;
            Padding = new LayoutPadding(10);

            Image ninePatch = new Image("OutlineNinePatch");

            Layout.DefaultPressedNinePatch = new Image("ButtonPressed");
            Layout.DefaultUnpressedNinePatch = new Image("ButtonUnpressed");

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
            //Children.Add(stack);

            for (int i = 0; i < 4; i++)
            {
                stack.Children.Add(new UIElement
                {
                    BackgroundColor = new UIColor(0, 0, 0, (byte)(0.2f + (i * .2f))),
                    HorizontalAlignment = EHorizontalAlignment.Stretch,
                    VerticalAlignment = EVerticalAlignment.Stretch,
                });
            }

            VerticalStack textStack = new VerticalStack()
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Bottom
            };
            Children.Add(textStack);

            textStack.Children.Add(new TextBlock
            {
                Text = "Some Text",
                TextColor = UIColor.Black,
                BackgroundColor = UIColor.Green,
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center,
                Padding = new LayoutPadding(0, 20, 0, 20)
            });

            textStack.Children.Add(new TextBlock
            {
                Text = "Descendery Text",
                TextColor = UIColor.Black,
                BackgroundColor = UIColor.Green,
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center,
                Margin = new LayoutPadding(20)
            });


            InputDialog dialog = new InputDialog(ninePatch, new TextBlock { Text = "Do you want to?", TextColor = UIColor.Black });

            dialog.AddInput(new DialogInput { Text = "Ok", CloseOnInput = true } );
            dialog.AddInput(new DialogInput { Text = "Cancel", CloseOnInput = true });

            MenuItemCollection menuItems = new MenuItemCollection()
            {
                new ContextMenuItem { Text = "Item 1" },
                new ContextMenuItem { Text = "Item 2"},
                new ContextMenuItem { Text = "Item 3"}
            };

            Menu menu = new Menu(menuItems)
            {
                BackgroundColor = UIColor.White
            };

            Children.Add(new TextButton()
            {
                Text = "Click!",
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center,
                ClickAction = delegate
                {
                    Layout.Current.ShowPopup(menu);
                }
            });
        }
    }
}
