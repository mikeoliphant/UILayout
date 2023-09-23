using System;

namespace UILayout
{
    public class Button : UIElementWrapper
    {
        public Action PressAction { get; set; }
        public Action ClickAction { get; set; }

        UIElement pressedElement;
        UIElement unpressedElement;

        bool down = false;

        public UIElement PressedElement
        {
            get => pressedElement;
            
            set
            {
                pressedElement = value;

                if ((pressedElement != null) && !ContentBounds.IsEmpty)
                    pressedElement.SetBounds(ContentBounds, this);

                if (down)
                    Child = pressedElement;
            }
        }

        public UIElement UnpressedElement
        {
            get => unpressedElement;

            set
            {
                unpressedElement = value;

                if ((unpressedElement != null) && !ContentBounds.IsEmpty)
                    unpressedElement.SetBounds(ContentBounds, this);

                if (!down)
                    Child = unpressedElement;
            }
        }

        public void Toggle()
        {
            down = !down;

            Child = down ? pressedElement : unpressedElement;

            UpdateContentLayout();
        }

        public override bool HandleTouch(in Touch touch)
        {
            switch (touch.TouchState)
            {
                case ETouchState.Pressed:
                    if (!down)
                    {
                        Toggle();

                        if (PressAction != null)
                            PressAction();
                    }
                    break;
                case ETouchState.Released:
                    if (down)
                    {
                        Toggle();

                        if (ClickAction != null)
                            ClickAction();
                    }
                    break;
            }

            return true;
        }
    }

    public class TextButton : Button
    {
        public string Text
        {
            get => textBlock.Text;
            set => textBlock.Text = value;
        }

        TextBlock textBlock;

        public TextButton()
        {
            textBlock = new TextBlock
            {
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center,
            };

            if (Layout.DefaultPressedNinePatch != null)
            {
                PressedElement = new NinePatchWrapper(Layout.DefaultPressedNinePatch)
                {
                    Child = textBlock,
                    HorizontalAlignment = EHorizontalAlignment.Stretch,
                    VerticalAlignment = EVerticalAlignment.Stretch
                };

                UnpressedElement = new NinePatchWrapper(Layout.DefaultUnpressedNinePatch)
                {
                    Child = textBlock,
                    HorizontalAlignment = EHorizontalAlignment.Stretch,
                    VerticalAlignment = EVerticalAlignment.Stretch
                };
            }
            else
            {
                PressedElement = new UIElementWrapper
                {
                    Child = textBlock,
                    HorizontalAlignment = EHorizontalAlignment.Stretch,
                    VerticalAlignment = EVerticalAlignment.Stretch,
                    Padding = new LayoutPadding(2, 5),
                    BackgroundColor = UIColor.Green
                };

                UnpressedElement = new UIElementWrapper()
                {
                    Child = textBlock,
                    HorizontalAlignment = EHorizontalAlignment.Stretch,
                    VerticalAlignment = EVerticalAlignment.Stretch,
                    Padding = new LayoutPadding(2, 5),
                    BackgroundColor = UIColor.Red
                };
            }
        }

        public TextButton(string text)
            : this()
        {
            Text = text;
        }
    }
}
