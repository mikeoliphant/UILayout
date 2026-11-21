using System;

namespace UILayout
{
    public partial class UIImage
    {
        /// <summary>The image's size in pixels (as source rectangles are).</summary>
        public int Width { get; set; }
        public int Height { get; set; }
        public int XOffset { get; set; }
        public int YOffset { get; set; }

        /// <summary>Pixels per layout unit: 2 for an image made for twice the size it is laid out at.</summary>
        public float Density { get; set; } = 1;

        /// <summary>The image's size in layout units: its pixel size divided by its density.</summary>
        public float LogicalWidth => Width / Density;
        public float LogicalHeight => Height / Density;

        public UIImage()
        {
        }
    }
}
