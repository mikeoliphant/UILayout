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

    /// <summary>
    /// A button drawn in a <see cref="Frame"/> around its content, which is both its pressed and unpressed element.
    /// Its states only change the frame's colors: the fill takes the theme's active color while pressed (or toggled
    /// on), the border the accent color when the mouse is over it. A primary button (a dialog's default, such as OK)
    /// always has the accent border. The base of <see cref="TextButton"/> and <see cref="ImageButton"/>; use it
    /// directly for other content.
    /// </summary>
    public class FrameButton : Button
    {
        Frame frame;
        bool primary;

        public FrameButton()
            : this(null)
        {
        }

        public FrameButton(UIElement content)
        {
            frame = new Frame(content);

            PressedElement = frame;
            UnpressedElement = frame;

            HorizontalAlignment = EHorizontalAlignment.Left;
            VerticalAlignment = EVerticalAlignment.Center;

            UpdateColors();
        }

        /// <summary>The frame the content is in, for example to change its padding.</summary>
        public Frame Frame
        {
            get { return frame; }
        }

        /// <summary>What's in the button's frame.</summary>
        public UIElement Content
        {
            get { return frame.Content; }

            set
            {
                frame.Content = value;

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

        // The mouse moving over it or off it shows when it's drawn: its colors are chosen then, not as it gets input
        // (while a popup is open only the popup does, and the button under it would keep its hover)
        protected override void DrawContents()
        {
            UpdateColors();

            base.DrawContents();
        }

        /// <summary>Sets the frame's colors for the button's state (pressed, mouse over, primary).</summary>
        protected virtual void UpdateColors()
        {
            UITheme theme = Layout.Current.Theme;

            frame.FillColor = IsPressed ? theme.Active : theme.Surface;
            frame.BorderColor = (IsMouseOver || primary) ? theme.Accent : theme.Border;
        }
    }

    /// <summary>A <see cref="FrameButton"/> with text, centered.</summary>
    public class TextButton : FrameButton
    {
        const float TextMarginX = 9;

        TextBlock textBlock;

        public TextButton()
            : this("", null)
        {
        }

        public TextButton(string text)
            : this(text, null)
        {
        }

        public TextButton(string text, Action clickAction)
        {
            textBlock = new TextBlock
            {
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center,
                Margin = new LayoutPadding(TextMarginX, 0)
            };

            Content = textBlock;

            Text = text;
            ClickAction = clickAction;
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

        public UIColor TextColor
        {
            get { return textBlock.TextColor; }
            set { textBlock.TextColor = value; }
        }

        public UIFont TextFont
        {
            get { return textBlock.TextFont; }

            set
            {
                textBlock.TextFont = value;

                UpdateContentLayout();
            }
        }
    }

    /// <summary>A toggle <see cref="TextButton"/> showing one text while it's on (pressed) and another while off.</summary>
    public class TextToggleButton : TextButton
    {
        string pressedText;
        string unpressedText;

        public TextToggleButton()
            : this("", "")
        {
        }

        public TextToggleButton(string pressedText, string unpressedText)
        {
            IsToggleButton = true;

            this.pressedText = pressedText;
            this.unpressedText = unpressedText;

            ShowText();
        }

        public string PressedText
        {
            get { return pressedText; }

            set
            {
                pressedText = value;

                ShowText();
            }
        }

        public string UnpressedText
        {
            get { return unpressedText; }

            set
            {
                unpressedText = value;

                ShowText();
            }
        }

        protected override void StateChanged()
        {
            base.StateChanged();

            ShowText();
        }

        void ShowText()
        {
            Text = IsPressed ? pressedText : unpressedText;
        }
    }

    /// <summary>A <see cref="FrameButton"/> with an image, centered, in the theme's text color.</summary>
    public class ImageButton : FrameButton
    {
        ImageElement imageElement;

        public ImageButton(string imageName)
            : this(Layout.Current.GetImage(imageName))
        {
        }

        public ImageButton(UIImage image)
        {
            imageElement = new ImageElement(image)
            {
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center,
                Color = Layout.Current.Theme.Text
            };

            Content = imageElement;
        }

        public UIImage Image
        {
            get { return imageElement.Image; }

            set
            {
                imageElement.Image = value;

                UpdateContentLayout();
            }
        }

        public UIColor ImageColor
        {
            get { return imageElement.Color; }
            set { imageElement.Color = value; }
        }
    }

    /// <summary>A toggle <see cref="ImageButton"/> showing one image while it's on (pressed) and another while off.</summary>
    public class ImageToggleButton : ImageButton
    {
        UIImage pressedImage;
        UIImage unpressedImage;

        public ImageToggleButton(string pressedImageName, string unpressedImageName)
            : this(Layout.Current.GetImage(pressedImageName), Layout.Current.GetImage(unpressedImageName))
        {
        }

        public ImageToggleButton(UIImage pressedImage, UIImage unpressedImage)
            : base(unpressedImage)
        {
            IsToggleButton = true;

            this.pressedImage = pressedImage;
            this.unpressedImage = unpressedImage;
        }

        protected override void StateChanged()
        {
            base.StateChanged();

            Image = IsPressed ? pressedImage : unpressedImage;
        }
    }
}
