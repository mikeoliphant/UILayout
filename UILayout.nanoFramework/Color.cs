namespace UILayout
{
    public partial class Color
    {
        public nanoFramework.Presentation.Media.Color NativeColor;
        public ushort NativeOpacity;

        public Color(byte r, byte g, byte b)
        {
            NativeColor = nanoFramework.Presentation.Media.ColorUtility.ColorFromRGB(r, g, b);
            NativeOpacity = nanoFramework.UI.Bitmap.OpacityOpaque;
        }

        public Color(byte r, byte g, byte b, float opacity)
        {
            NativeColor = nanoFramework.Presentation.Media.ColorUtility.ColorFromRGB(r, g, b);
            NativeOpacity = (ushort)(opacity * nanoFramework.UI.Bitmap.OpacityOpaque);
        }

        public Color(nanoFramework.Presentation.Media.Color nativeColor, ushort opacity)
        {
            this.NativeColor = nativeColor;
            this.NativeOpacity = opacity;
        }
    }
}
