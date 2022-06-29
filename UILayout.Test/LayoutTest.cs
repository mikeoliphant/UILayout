using System;
using System.Collections.Generic;
using System.Text;

namespace UILayout.Test
{
    public class LayoutTest : Dock
    {
        public LayoutTest()
        {
            BackgroundColor = Color.Yellow;
            Margin = new LayoutPadding(5);
            Padding = new LayoutPadding(10);

            HorizontalStack stack = new HorizontalStack
            {
                BackgroundColor = Color.Blue,
                HorizontalAlignment = EHorizontalAlignment.Right,
                VerticalAlignment = EVerticalAlignment.Top,
                Padding = new LayoutPadding(10),
                DesiredHeight = 100,
                DesiredWidth = 100,
                ChildSpacing = 10
            };
            Children.Add(stack);

            for (int i = 0; i < 5; i++)
            {
                stack.Children.Add(new UIElement
                {
                    BackgroundColor = Color.Red,
                    HorizontalAlignment = EHorizontalAlignment.Stretch,
                    VerticalAlignment = EVerticalAlignment.Stretch,
                });
            }

            Children.Add(new TextBlock
            {
                Text = "Hello World",
                BackgroundColor = Color.Green,
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Bottom,
            });

            Children.Add(new Button()
            {
                BackgroundColor = Color.Red,
                DesiredHeight = 50,
                DesiredWidth = 50,
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center
            });
        }
    }
}
