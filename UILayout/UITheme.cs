namespace UILayout
{
    /// <summary>
    /// How controls look: colors, fonts and images, as <see cref="Layout.Theme"/>. Controls read it when they are
    /// created, so set it up before building the UI; their own properties (TextColor, TextFont, ...) override it
    /// after. The images are white (with alpha), so the controls tint them with the colors. The default colors are a
    /// dark theme; UILayout.DefaultTextures' TextureLoader fills in the fonts and images.
    /// </summary>
    public class UITheme
    {
        // Colors

        /// <summary>Behind a panel of controls.</summary>
        public UIColor Background { get; set; } = new UIColor(28, 30, 35);

        /// <summary>Inside a frame: buttons, input boxes, popups.</summary>
        public UIColor Surface { get; set; } = new UIColor(42, 46, 54);

        /// <summary>Frame borders and section outlines.</summary>
        public UIColor Border { get; set; } = new UIColor(74, 80, 90);

        /// <summary>The border of what the mouse is over or what's open, a chosen item, a default button.</summary>
        public UIColor Accent { get; set; } = new UIColor(78, 161, 255);

        /// <summary>Behind a pressed button, a toggle that's on, the item under the mouse in a list.</summary>
        public UIColor Active { get; set; } = new UIColor(43, 69, 102);

        public UIColor Text { get; set; } = new UIColor(230, 232, 235);

        /// <summary>Labels and secondary text.</summary>
        public UIColor MutedText { get; set; } = new UIColor(154, 161, 171);

        public UIColor Warning { get; set; } = new UIColor(255, 180, 60);

        // Fonts

        public UIFont Font { get; set; }

        /// <summary>A smaller font, for labels, captions and values that pop in over controls.</summary>
        public UIFont SmallFont { get; set; }

        // Images (white)

        /// <summary>A <see cref="Frame"/>: a nine-patch filled with a rounded rectangle, and its outline.</summary>
        public UIImage FrameFill { get; set; }
        public UIImage FrameBorder { get; set; }

        /// <summary>
        /// A <see cref="TabPanel"/>'s tab: as the frame's fill and border, with only the top corners rounded and no
        /// bottom edge, so the chosen tab joins the panel below it.
        /// </summary>
        public UIImage TabFill { get; set; }
        public UIImage TabBorder { get; set; }

        /// <summary>A drop-down's arrow.</summary>
        public UIImage DropDownArrow { get; set; }

        /// <summary>A check box: a rounded square, filled, its outline, and the check mark.</summary>
        public UIImage CheckBoxFill { get; set; }
        public UIImage CheckBoxBorder { get; set; }
        public UIImage CheckMark { get; set; }

        /// <summary>A scroll bar's thumb (a small rounded nine-patch), and its arrows.</summary>
        public UIImage ScrollThumb { get; set; }
        public UIImage ScrollUpArrow { get; set; }
        public UIImage ScrollDownArrow { get; set; }

        /// <summary>
        /// A <see cref="Slider"/>'s track (also its fill; nine-patches stretched only lengthwise), horizontal and
        /// vertical, and its thumb's shadow, body, highlight and ring (same-size images, layered).
        /// </summary>
        public UIImage SliderTrack { get; set; }
        public UIImage SliderTrackVertical { get; set; }
        public UIImage SliderThumbShadow { get; set; }
        public UIImage SliderThumbBody { get; set; }
        public UIImage SliderThumbHighlight { get; set; }
        public UIImage SliderThumbRing { get; set; }
    }
}
