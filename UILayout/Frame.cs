namespace UILayout
{
    /// <summary>
    /// The frame of an input-style control: the theme's rounded fill nine-patch with its border nine-patch over it,
    /// both in the same bounds, around the content. Controls built on it (buttons, drop-downs, lists) change
    /// <see cref="FillColor"/> and <see cref="BorderColor"/> for hover, pressed and open.
    /// </summary>
    public class InputFrame : Dock
    {
        NinePatchWrapper fill;
        NinePatchWrapper border;

        public InputFrame()
            : this(null)
        {
        }

        public InputFrame(UIElement content)
        {
            UITheme theme = Layout.Current.Theme;

            fill = new NinePatchWrapper(Layout.Current.DefaultInputFillNinePatch)
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Stretch,
                Color = theme.Surface
            };

            border = new NinePatchWrapper(Layout.Current.DefaultInputBorderNinePatch)
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Stretch,
                DrawCenter = false,
                Color = theme.Border,
                Child = content
            };

            Children.Add(fill);
            Children.Add(border);
        }

        /// <summary>The room the border takes around the content.</summary>
        public LayoutPadding BorderPadding
        {
            get { return border.Padding; }
        }

        /// <summary>What's in the frame, inside the border.</summary>
        public UIElement Content
        {
            get { return border.Child; }
            set { border.Child = value; }
        }

        public UIColor FillColor
        {
            get { return fill.Color; }

            set
            {
                if (fill.Color.Equals(value))
                    return;

                fill.Color = value;

                Layout.Current.AddDirtyRect(LayoutBounds);
            }
        }

        public UIColor BorderColor
        {
            get { return border.Color; }

            set
            {
                if (border.Color.Equals(value))
                    return;

                border.Color = value;

                Layout.Current.AddDirtyRect(LayoutBounds);
            }
        }
    }
}
