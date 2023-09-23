using Microsoft.Xna.Framework;

namespace UILayout
{
    public partial class UIColor
    {
        public Color NativeColor;

        public UIColor(byte r, byte g, byte b)
            : this(r, g, b, 1)
        {

        }

        public UIColor(byte r, byte g, byte b, float opacity)
        {
            NativeColor = new Color(r, g, b, (byte)(opacity * 255));
        }

        public UIColor(Color nativeColor)
        {
            this.NativeColor = nativeColor;
        }
    }
}
