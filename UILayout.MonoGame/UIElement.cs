using Microsoft.Xna.Framework;

namespace UILayout
{
    public partial class UIElement
    {
        public UIColor BackgroundColor { get; set; }

        public void Draw()
        {
            // Don't draw if we aren't in the diry rectangle
            if (!Layout.Current.HaveDirty && !Layout.Current.DirtyRect.Intersects(layoutBounds))
                return;

            if (BackgroundColor.NativeColor.A > 0)
            {
                MonoGameLayout.Current.GraphicsContext.DrawRectangle(new Rectangle((int)ContentBounds.X, (int)ContentBounds.Y, (int)ContentBounds.Width, (int)ContentBounds.Height), 0.5f, BackgroundColor.NativeColor);
            }

            DrawContents();
        }

        protected virtual void DrawContents()
        {

        }
    }
}
