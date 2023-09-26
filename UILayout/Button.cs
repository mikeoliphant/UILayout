using System;

namespace UILayout
{
    public class Button : UIElementWrapper
    {
        public Action PressAction { get; set; }
        public Action ClickAction { get; set; }
        public bool IsToggleButton { get; set; }

        UIElement pressedElement;
        UIElement unpressedElement;

        bool pressed = false;

        public UIElement PressedElement
        {
            get => pressedElement;
            
            set
            {
                pressedElement = value;

                if ((pressedElement != null) && !ContentBounds.IsEmpty)
                    pressedElement.SetBounds(ContentBounds, this);

                if (pressed)
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

                if (!pressed)
                    Child = unpressedElement;
            }
        }

        public void SetPressed(bool pressed)
        {
            if (this.pressed != pressed)
                Toggle();
        }

        public void Toggle()
        {
            pressed = !pressed;

            Child = pressed ? pressedElement : unpressedElement;

            UpdateContentLayout();
        }

        protected override void GetContentSize(out float width, out float height)
        {
            float pressedWidth;
            float pressedHight;

            pressedElement.GetSize(out pressedWidth, out pressedHight);

            float unpressedWidth;
            float unpressedHight;

            unpressedElement.GetSize(out unpressedWidth, out unpressedHight);

            width = Math.Max(pressedWidth, unpressedWidth);
            height = Math.Max(pressedHight, unpressedHight);
        }

        public override bool HandleTouch(in Touch touch)
        {
            switch (touch.TouchState)
            {
                case ETouchState.Pressed:
                    if (IsToggleButton)
                    {
                        Toggle();

                        if (ClickAction != null)
                            ClickAction();
                    }
                    else
                    {
                        if (!pressed)
                        {
                            Toggle();

                            if (PressAction != null)
                                PressAction();
                        }
                    }
                    break;
                case ETouchState.Released:
                    if (!IsToggleButton)
                    {
                        if (pressed)
                        {
                            Toggle();

                            if (ClickAction != null)
                                ClickAction();
                        }
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

    public class TextToggleButton : Button
    {
        public string PressedText
        {
            get => pressedTextBlock.Text;
            set => pressedTextBlock.Text = value;
        }

        TextBlock pressedTextBlock;

        public string UnpressedText
        {
            get => unpressedTextBlock.Text;
            set => unpressedTextBlock.Text = value;
        }

        TextBlock unpressedTextBlock;

        public TextToggleButton()
        {
            IsToggleButton = true;

            pressedTextBlock = new TextBlock
            {
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center,
            };

            unpressedTextBlock = new TextBlock
            {
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center,
            };

            if (Layout.DefaultPressedNinePatch != null)
            {
                PressedElement = new NinePatchWrapper(Layout.DefaultUnpressedNinePatch)
                {
                    Child = pressedTextBlock,
                    HorizontalAlignment = EHorizontalAlignment.Stretch,
                    VerticalAlignment = EVerticalAlignment.Stretch
                };

                UnpressedElement = new NinePatchWrapper(Layout.DefaultUnpressedNinePatch)
                {
                    Child = unpressedTextBlock,
                    HorizontalAlignment = EHorizontalAlignment.Stretch,
                    VerticalAlignment = EVerticalAlignment.Stretch
                };
            }
            else
            {
                PressedElement = new UIElementWrapper
                {
                    Child = pressedTextBlock,
                    HorizontalAlignment = EHorizontalAlignment.Stretch,
                    VerticalAlignment = EVerticalAlignment.Stretch,
                    Padding = new LayoutPadding(2, 5),
                    BackgroundColor = UIColor.Green
                };

                UnpressedElement = new UIElementWrapper()
                {
                    Child = unpressedTextBlock,
                    HorizontalAlignment = EHorizontalAlignment.Stretch,
                    VerticalAlignment = EVerticalAlignment.Stretch,
                    Padding = new LayoutPadding(2, 5),
                    BackgroundColor = UIColor.Red
                };
            }
        }

        public TextToggleButton(string pressedText, string unpressedText)
            : this()
        {
            PressedText = pressedText;
            UnpressedText = unpressedText;
        }
    }
}
