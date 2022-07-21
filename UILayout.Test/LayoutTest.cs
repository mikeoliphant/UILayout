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

            Image ninePatch = new Image("NinePatch");
            NinePatchWrapper ninePatchWrapper = new NinePatchWrapper
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Stretch,
                Image = ninePatch
            };
            Children.Add(ninePatchWrapper);

            HorizontalStack stack = new HorizontalStack
            {
                BackgroundColor = new Color(0, 0, 255),
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
                    BackgroundColor = new Color(0, 0, 0, 0.2f + (i * .2f)),
                    HorizontalAlignment = EHorizontalAlignment.Stretch,
                    VerticalAlignment = EVerticalAlignment.Stretch,
                });
            }

            Children.Add(new TextBlock
            {
                Text = "Hello World",
                TextColor = Color.Black,
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
