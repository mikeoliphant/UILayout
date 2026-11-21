using System;

namespace UILayout
{
    public class Button : UIElementWrapper
    {
        public Action PressAction { get; set; }
        public Action ClickAction { get; set; }
        public Action DoubleClickAction { get; set; }
        public bool IsToggleButton { get; set; }
        public bool IsPressed { get; private set; }

        /// <summary>
        /// Whether a (non-toggle) button clicks when released outside itself. Off by default, as in most toolkits: a
        /// press shows as pressed only while it's over the button, and clicks only when released over it.
        /// </summary>
        public bool ClickWhenReleasedOutside { get; set; } = false;

        UIElement pressedElement;
        UIElement unpressedElement;
        bool trackingPress = false;

        public UIElement PressedElement
        {
            get => pressedElement;
            
            set
            {
                pressedElement = value;

                if ((pressedElement != null) && !ContentBounds.IsEmpty)
                    pressedElement.SetBounds(ContentBounds, this);

                if (IsPressed)
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

                if (!IsPressed)
                    Child = unpressedElement;
            }
        }

        public void SetPressed(bool pressed)
        {
            if (this.IsPressed != pressed)
                Toggle();
        }

        public void Toggle()
        {
            IsPressed = !IsPressed;

            Child = IsPressed ? pressedElement : unpressedElement;

            UpdateContentLayout();

            StateChanged();
        }

        /// <summary>
        /// Called when <see cref="IsPressed"/> changes. A button whose pressed and unpressed elements are the same one
        /// can restyle it here instead.
        /// </summary>
        protected virtual void StateChanged()
        {
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

                        if (PressAction != null)
                            PressAction();
                    }
                    else
                    {
                        if (!IsPressed)
                        {
                            CaptureTouch(touch);

                            trackingPress = true;

                            Toggle();

                            if (PressAction != null)
                                PressAction();
                        }
                    }
                    break;
                case ETouchState.Moved:
                case ETouchState.Held:
                    // Pressed only while over the button.
                    if (trackingPress && !ClickWhenReleasedOutside)
                        SetPressed(LayoutBounds.Contains(touch.Position));
                    break;
                case ETouchState.Released:
                    if (!IsToggleButton)
                    {
                        ReleaseTouch();

                        bool click = trackingPress && (ClickWhenReleasedOutside || LayoutBounds.Contains(touch.Position));

                        trackingPress = false;

                        if (IsPressed)
                            Toggle();

                        if (click && (ClickAction != null))
                            ClickAction();
                    }
                    break;
                case ETouchState.Invalid:
                    // The press was lost: no click.
                    if (trackingPress)
                    {
                        ReleaseTouch();

                        trackingPress = false;

                        SetPressed(false);
                    }
                    break;
            }

            if ((DoubleClickAction != null) && (IsDoubleTap(touch, this)))
            {
                DoubleClickAction();
            }

            return true;
        }
    }

    public class NinePatchButton : Button
    {
        UIImage pressedNinePatchImage;
        UIImage unpressedNinePatchImage;

        public NinePatchWrapper PressedNinePatch
        {
            get { return PressedElement as NinePatchWrapper; }
        }

        public NinePatchWrapper UnpressedNinePatch
        {
            get { return UnpressedElement as NinePatchWrapper; }
        }

        public NinePatchButton()
            : this(Layout.Current.DefaultPressedNinePatch, Layout.Current.DefaultUnpressedNinePatch)
        {
        }

        public NinePatchButton(UIImage ninePatchImage)
        {
            this.pressedNinePatchImage = ninePatchImage;
            this.unpressedNinePatchImage = ninePatchImage;
        }

        public NinePatchButton(UIImage pressedNinePatchImage, UIImage unpressedNinePatchImage)
        {
            this.pressedNinePatchImage = pressedNinePatchImage;
            this.unpressedNinePatchImage = unpressedNinePatchImage;
        }

        public void SetElements(UIElement pressedElement, UIElement unpressedElement)
        {
            if (pressedNinePatchImage != null)
            {
                PressedElement = new NinePatchWrapper(pressedNinePatchImage)
                {
                    Child = pressedElement,
                    HorizontalAlignment = EHorizontalAlignment.Stretch,
                    VerticalAlignment = EVerticalAlignment.Stretch
                };

                UnpressedElement = new NinePatchWrapper(unpressedNinePatchImage)
                {
                    Child = unpressedElement,
                    HorizontalAlignment = EHorizontalAlignment.Stretch,
                    VerticalAlignment = EVerticalAlignment.Stretch
                };
            }
            else
            {
                PressedElement = new UIElementWrapper
                {
                    Child = pressedElement,
                    HorizontalAlignment = EHorizontalAlignment.Stretch,
                    VerticalAlignment = EVerticalAlignment.Stretch,
                    Padding = new LayoutPadding(2, 5),
                    BackgroundColor = UIColor.Green
                };

                UnpressedElement = new UIElementWrapper()
                {
                    Child = unpressedElement,
                    HorizontalAlignment = EHorizontalAlignment.Stretch,
                    VerticalAlignment = EVerticalAlignment.Stretch,
                    Padding = new LayoutPadding(2, 5),
                    BackgroundColor = UIColor.Red
                };
            }
        }
    }

    public class EmptyButton : NinePatchButton
    {
        public EmptyButton()
        {
            SetElements(null, null);
        }
    }

    public class TextButton : NinePatchButton
    {
        public string Text
        {
            get => textBlock.Text;
            set => textBlock.Text = value;
        }

        public UIColor TextColor
        {
            get { return textBlock.TextColor; }
            set { textBlock.TextColor = value; }
        }

        public UIFont TextFont
        {
            get { return textBlock.TextFont; }
            set { textBlock.TextFont = value; }
        }

        TextBlock textBlock;

        public TextButton(string text)
            : this()
        {
            Text = text;
        }

        public TextButton()
        {
            textBlock = new TextBlock
            {
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center,
            };

            SetElements(textBlock, textBlock);
        }
    }

    public class TextToggleButton : NinePatchButton
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

        public TextToggleButton(string pressedText, string unpressedText)
            : this()
        {
            PressedText = pressedText;
            UnpressedText = unpressedText;
        }

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

            SetElements(pressedTextBlock, unpressedTextBlock);
        }
    }

    public class ImageButton : NinePatchButton
    {
        public UIColor ImageColor
        {
            get { return imageElement.Color;  }
            set { imageElement.Color = value; }
        }


        ImageElement imageElement;

        public ImageButton(string imageName)
            : this(Layout.Current.GetImage(imageName))
        {
        }

        public ImageButton(UIImage image)
        {
            imageElement = new ImageElement(image)
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Stretch
            };

            SetElements(imageElement, imageElement);
        }
    }

    public class ImageToggleButton : NinePatchButton
    {
        public UIColor ImageColor
        {
            get { return pressedImageElement.Color; }
            set { pressedImageElement.Color = unpressedImageElement.Color = value; }
        }

        ImageElement pressedImageElement;
        ImageElement unpressedImageElement;

        public ImageToggleButton(string pressedImageName, string unpressedImageName)
            : this(Layout.Current.GetImage(pressedImageName), Layout.Current.GetImage(unpressedImageName))
        {
        }

        public ImageToggleButton(UIImage pressedImage, UIImage unpressedImage)
        {
            pressedImageElement = new ImageElement(pressedImage);
            unpressedImageElement = new ImageElement(unpressedImage);

            SetElements(pressedImageElement, unpressedImageElement);

            IsToggleButton = true;
        }
    }

    /// <summary>
    /// A button drawn in the input style: one <see cref="InputFrame"/> with the text, used as both its pressed and
    /// unpressed element. Its states only change the frame's colors: the fill takes the theme's active color while
    /// pressed (or toggled on), the border the accent color when the mouse is over it. A primary button (a dialog's
    /// default, such as OK) always has the accent border.
    /// </summary>
    public class FrameButton : Button
    {
        const float TextMarginX = 9;

        InputFrame frame;
        TextBlock textBlock;
        bool primary;

        public FrameButton()
            : this("", null)
        {
        }

        public FrameButton(string text, Action clickAction)
        {
            textBlock = new TextBlock
            {
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center,
                Margin = new LayoutPadding(TextMarginX, 0),
                TextColor = Layout.Current.Theme.Text
            };

            frame = new InputFrame(textBlock);

            PressedElement = frame;
            UnpressedElement = frame;

            Text = text;
            ClickAction = clickAction;

            HorizontalAlignment = EHorizontalAlignment.Left;
            VerticalAlignment = EVerticalAlignment.Center;

            UpdateColors();
        }

        public string Text
        {
            get { return textBlock.Text; }

            set
            {
                textBlock.Text = value;

                UpdateContentLayout();
            }
        }

        /// <summary>A dialog's default button: always the accent border.</summary>
        public bool Primary
        {
            get { return primary; }

            set
            {
                primary = value;

                UpdateColors();
            }
        }

        protected override void StateChanged()
        {
            base.StateChanged();

            UpdateColors();
        }

        public override void HandleInput(InputManager inputManager)
        {
            base.HandleInput(inputManager);

            // The mouse moving over it or off it.
            UpdateColors();
        }

        void UpdateColors()
        {
            UITheme theme = Layout.Current.Theme;

            frame.FillColor = IsPressed ? theme.Active : theme.Surface;
            frame.BorderColor = (IsMouseOver || primary) ? theme.Accent : theme.Border;
        }
    }
}
