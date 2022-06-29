namespace UILayout
{
    public partial class UIElement
    {
        nanoFramework.Presentation.Media.Color backgroundColor;
        ushort backgroundOpacity;

        public Color BackgroundColor
        {
            get { return new Color(backgroundColor, backgroundOpacity); }
            set
            {
                backgroundColor = value.NativeColor;
                backgroundOpacity = value.NativeOpacity;
            }
        }

        public void Draw()
        {
            if (!Visible)
                return;

            // Don't draw if we aren't in the diry rectangle
            if (!Layout.Current.HaveDirty || !Layout.Current.DirtyRect.Intersects(ref layoutBounds))
                return;

            if (backgroundOpacity != nanoFramework.UI.Bitmap.OpacityTransparent)
            {
                BitmapLayout.Current.FullScreenBitmap.DrawRectangle(backgroundColor, 0, (int)LayoutBounds.X, (int)LayoutBounds.Y, (int)LayoutBounds.Width, (int)LayoutBounds.Height, 0, 0, backgroundColor, 0, 0, backgroundColor, 0, 0, backgroundOpacity);
            }

            DrawContents();
        }

        protected virtual void DrawContents()
        {
        }
    }
}
