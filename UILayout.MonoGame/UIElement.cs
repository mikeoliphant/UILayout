
namespace UILayout
{
    public partial class UIElement
    {
        public Color BackgroundColor { get; set; }

        public void Draw()
        {
            // Don't draw if we aren't in the diry rectangle
            if (!Layout.Current.HaveDirty && !Layout.Current.DirtyRect.Intersects(ref layoutBounds))
                return;

            if (BackgroundColor.NativeColor.A > 0)
            {
                //if ((BackgroundRoundRadius.Width > 0) || (BackgroundRoundRadius.Height > 0))
                //    SkiaLayout.Current.Canvas.DrawRoundRect(LayoutBounds.ToSKRect(), BackgroundRoundRadius, backgroundPaint);
                //else
                //    SkiaLayout.Current.Canvas.DrawRect(LayoutBounds.ToSKRect(), backgroundPaint);
            }

            DrawContents();
        }

        protected virtual void DrawContents()
        {

        }
    }
}
