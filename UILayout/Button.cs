using System;

namespace UILayout
{
    public class Button : UIElementWrapper
    {
        public Action PressAction { get; set; }
        public Action ReleaseAction { get; set; }

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

        public override bool HandleTouch(ref Touch touch)
        {
            if (touch.TouchState == ETouchState.Released)
            {
                if (down)
                {
                    Toggle();

                    if (ReleaseAction != null)
                        ReleaseAction();
                }
            }
            else
            {
                if (!down)
                {
                    Toggle();

                    if (PressAction != null)
                        PressAction();
                }
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

            PressedElement = new UIElementWrapper
            {
                Child = textBlock,
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Stretch,
                BackgroundColor = Color.Green
            };

            UnpressedElement = new UIElementWrapper()
            {
                Child = textBlock,
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Stretch,
                BackgroundColor = Color.Red
            };
        }

        public TextButton(string text)
            : this()
        {
            Text = text;
        }
    }
}
