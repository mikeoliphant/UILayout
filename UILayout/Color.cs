namespace UILayout
{
    public partial class UIColor
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

        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; }

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
    }
}
