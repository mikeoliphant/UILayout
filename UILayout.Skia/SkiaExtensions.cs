using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using SkiaSharp;

namespace UILayout
{
    public static class SkiaExtensions
    {
        public static SKRect ToSKRect(this RectangleF rectangleF)
        {
            return new SKRect(rectangleF.Left, rectangleF.Top, rectangleF.Right, rectangleF.Bottom);
        }
    }
}
