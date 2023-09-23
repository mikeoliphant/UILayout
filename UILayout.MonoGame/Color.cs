using System.Drawing;

namespace UILayout
{
    public partial class Color
    {
        public Microsoft.Xna.Framework.Color NativeColor;

        public Color(byte r, byte g, byte b)
            : this(r, g, b, 1)
        {

        }

        public Color(byte r, byte g, byte b, float opacity)
        {
            NativeColor = new Microsoft.Xna.Framework.Color(r, g, b, (byte)(opacity * 255));
        }

        public Color(Microsoft.Xna.Framework.Color nativeColor)
        {
            this.NativeColor = nativeColor;
        }
    }
}
