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
        {
            NativeColor = new SKColor(r, g, b);
        }

        public Color(SKColor nativeColor)
        {
            this.NativeColor = nativeColor;
        }
    }
}
