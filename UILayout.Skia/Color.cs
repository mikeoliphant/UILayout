using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;

namespace UILayout
{
    public partial class Color
    {
        public SKColor NativeColor;

        public Color(byte r, byte g, byte b)
            : this(r, g, b, 1)
        {

        }

        public Color(byte r, byte g, byte b, float opacity)
        {
            NativeColor = new SKColor(r, g, b, (byte)(opacity * 255));
        }

        public Color(SKColor nativeColor)
        {
            this.NativeColor = nativeColor;
        }
    }
}
