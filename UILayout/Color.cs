using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;

namespace UILayout
{
    [StructLayout(LayoutKind.Sequential, Size = 4)]
    public partial struct UIColor
    {
        public static UIColor Transparent { get => new UIColor(0, 0, 0, 0); }
        public static UIColor White { get => new UIColor(255, 255, 255); }
        public static UIColor Black { get => new UIColor(0, 0, 0); }
        public static UIColor Red { get => new UIColor(255, 0, 0); }
        public static UIColor Green { get => new UIColor(0, 255, 0); }
        public static UIColor Blue { get => new UIColor(0, 0, 255); }
        public static UIColor Yellow { get => new UIColor(255, 255, 0); }
        public static UIColor Cyan { get => new UIColor(0, 255, 255); }
        public static UIColor Magenta { get => new UIColor(255, 0, 255); }
        public static UIColor Orange { get => new UIColor(255, 165, 0); }

        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public UIColor(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public UIColor(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
            A = 255;
        }

        public UIColor(int r, int g, int b)
        {
            this.R = (byte)r;
            this.G = (byte)g;
            this.B = (byte)b;
            this.A = (byte)255;
        }

        public UIColor(int r, int g, int b, int a)
        {
            this.R = (byte)r;
            this.G = (byte)g;
            this.B = (byte)b;
            this.A = (byte)a;
        }

        public UIColor(float r, float g, float b)
        {
            this.R = (byte)(r * byte.MaxValue);
            this.G = (byte)(g * byte.MaxValue);
            this.B = (byte)(b * byte.MaxValue);
            this.A = (byte)255;
        }

        public UIColor(float r, float g, float b, float a)
        {
            this.R = (byte)(r * byte.MaxValue);
            this.G = (byte)(g * byte.MaxValue);
            this.B = (byte)(b * byte.MaxValue);
            this.A = (byte)(a * byte.MaxValue);
        }

        public UIColor(in Vector3 colorVec)
        {
            R = (byte)(colorVec.X * 255);
            G = (byte)(colorVec.Y * 255);
            B = (byte)(colorVec.Z * 255);
            A = (byte)255;
        }

        public Vector3 ToVector3()
        {
            return new Vector3((float)R / 255.0f, (float)G / 255.0f, (float)B / 255.0f);
        }

        public static UIColor FromHex(string hex)
        {
            if (!string.IsNullOrEmpty(hex))
            {
                string colorStr = hex.TrimStart('#');

                if (colorStr.Length == 6)
                {
                    return new UIColor(int.Parse(colorStr.Substring(0, 2), NumberStyles.HexNumber),
                        int.Parse(colorStr.Substring(2, 2), NumberStyles.HexNumber),
                        int.Parse(colorStr.Substring(4, 2), NumberStyles.HexNumber));
                }
            }

            return UIColor.Transparent;
        }

        public static UIColor Lerp(UIColor color1, UIColor color2, float lerp)
        {
            return new UIColor((int)MathUtil.Lerp(color1.R, color2.R, lerp),
                (int)MathUtil.Lerp(color1.G, color2.G, lerp),
                (int)MathUtil.Lerp(color1.B, color2.B, lerp),
                (int)MathUtil.Lerp(color1.A, color2.A, lerp));
        }
    }
}
