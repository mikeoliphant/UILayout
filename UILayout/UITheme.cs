namespace UILayout
{
    /// <summary>
    /// Colors for controls drawn in an input style (drop-downs, input buttons, check lists, outlined sections), as
    /// <see cref="Layout.Theme"/>. The defaults are a dark theme. Images drawn in these colors are white, so they take
    /// any color.
    /// </summary>
    public class UITheme
    {
        /// <summary>Behind a panel of controls.</summary>
        public UIColor Background { get; set; } = new UIColor(28, 30, 35);

        /// <summary>Inside an input box or button.</summary>
        public UIColor Surface { get; set; } = new UIColor(42, 46, 54);

        /// <summary>Input borders and section outlines.</summary>
        public UIColor Border { get; set; } = new UIColor(74, 80, 90);

        /// <summary>The border of what the mouse is over or what's open, a chosen item, a default button.</summary>
        public UIColor Accent { get; set; } = new UIColor(78, 161, 255);

        /// <summary>Behind a pressed button, a toggle that's on, the item under the mouse in a list.</summary>
        public UIColor Active { get; set; } = new UIColor(43, 69, 102);

        public UIColor Text { get; set; } = new UIColor(230, 232, 235);

        /// <summary>Labels and secondary text.</summary>
        public UIColor MutedText { get; set; } = new UIColor(154, 161, 171);

        public UIColor Warning { get; set; } = new UIColor(255, 180, 60);

        /// <summary>A level meter: its background, and its bar when low, hot (from half scale) and clipping.</summary>
        public UIColor MeterBackground { get; set; } = new UIColor(20, 22, 26);
        public UIColor MeterLow { get; set; } = new UIColor(80, 190, 90);
        public UIColor MeterHot { get; set; } = new UIColor(255, 180, 60);
        public UIColor MeterClip { get; set; } = new UIColor(230, 60, 50);
    }
}
