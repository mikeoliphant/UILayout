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

        // How far the images reach outside the frame (the popup style's shadow ring; none otherwise)
        float shadowWidth;

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
        /// Draws the frame as a popup's: the theme's popup images (the frame inside a ring of soft shadow, setting it
        /// apart from what's behind), the border in the accent color. The shadow is outside the frame's place in the
        /// layout (a negative margin): the frame is placed and sized as it would be without it, while its bounds (what's
        /// drawn and made dirty) take the shadow in. The content padding stays as it was, inside the frame's edge. The
        /// frame's margin is the shadow's: give any margin to what holds it.
        /// </summary>
        public void SetPopupStyle()
        {
            UITheme theme = Layout.Current.Theme;

            LayoutPadding contentPadding = ContentPadding;

            shadowWidth = theme.PopupShadowWidth;

            fill.Image = theme.PopupFill;
            border.Image = theme.PopupBorder;

            // Setting the border image set its padding to its corners: back to the content's room inside the frame
            ContentPadding = contentPadding;

            Margin = new LayoutPadding(-shadowWidth);

            BorderColor = theme.Accent;
        }

        /// <summary>
        /// The room around the content, inside the frame's edge (by default the border image's corners). Keep it at
        /// least that, or the content overlaps the corners. (A popup's shadow, outside the edge, isn't part of it.)
        /// </summary>
        public LayoutPadding ContentPadding
        {
            get
            {
                LayoutPadding padding = border.Padding;

                return new LayoutPadding(padding.Left - shadowWidth, padding.Top - shadowWidth,
                    padding.Right - shadowWidth, padding.Bottom - shadowWidth);
            }

            set
            {
                border.Padding = new LayoutPadding(value.Left + shadowWidth, value.Top + shadowWidth,
                    value.Right + shadowWidth, value.Bottom + shadowWidth);
            }
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

        /// <summary>The fill's color. A changed color is redrawn (the nine-patch's own setter makes it dirty).</summary>
        public UIColor FillColor
        {
            get { return fill.Color; }
            set { fill.Color = value; }
        }

        /// <summary>The border's color. A changed color is redrawn (the nine-patch's own setter makes it dirty).</summary>
        public UIColor BorderColor
        {
            get { return border.Color; }
            set { border.Color = value; }
        }
    }
}
