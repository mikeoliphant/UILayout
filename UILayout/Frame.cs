namespace UILayout
{
    /// <summary>
    /// The frame most controls are drawn in (buttons, drop-downs, text boxes, popups, sections): the theme's rounded
    /// fill nine-patch with its border nine-patch over it, both in the same bounds, around the content. The fill is in
    /// the theme's surface color and the border in its border color; controls built on it change
    /// <see cref="FillColor"/> and <see cref="BorderColor"/> for hover, pressed and open.
    /// </summary>
    public class Frame : Dock
    {
        NinePatchWrapper fill;
        NinePatchWrapper border;

        public Frame()
            : this(null)
        {
        }

        public Frame(UIElement content)
        {
            UITheme theme = Layout.Current.Theme;

            fill = new NinePatchWrapper(theme.FrameFill)
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Stretch,
                Color = theme.Surface
            };

            border = new NinePatchWrapper(theme.FrameBorder)
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

        /// <summary>
        /// Draws the frame with other fill and border nine-patches (white), such as the theme's tab images. Sets the
        /// content padding to the border image's corners, so set it after this.
        /// </summary>
        public void SetImages(UIImage fillImage, UIImage borderImage)
        {
            fill.Image = fillImage;
            border.Image = borderImage;
        }

        /// <summary>
        /// The room around the content, inside the frame's edge (by default the border image's corners). Keep it at
        /// least that, or the content overlaps the corners.
        /// </summary>
        public LayoutPadding ContentPadding
        {
            get { return border.Padding; }
            set { border.Padding = value; }
        }

        /// <summary>What's in the frame, inside the border.</summary>
        public UIElement Content
        {
            get { return border.Child; }
            set { border.Child = value; }
        }

        /// <summary>Whether the fill is drawn (an outline only when not, as a <see cref="Section"/>).</summary>
        public bool DrawFill
        {
            get { return fill.Visible; }
            set { fill.Visible = value; }
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
