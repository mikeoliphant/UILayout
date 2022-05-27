using System;

namespace UILayout
{
    public class Layout
    {
        public static Layout Current { get; private set; }

        public RectF Bounds { get; private set; }
        public UIElement RootUIElement { get; set; }
        public RectF DirtyRect { get { return dirtyRect; } set { dirtyRect = value; } }

        RectF dirtyRect = RectF.Empty;

        public Layout()
        {
            Layout.Current = this;
        }

        public void SetBounds(RectF bounds)
        {
            this.Bounds = bounds;
        }

        public void ClearDirtyRect()
        {
            DirtyRect = RectF.Empty;
        }

        public void AddDirtyRect(RectF dirty)
        {
            if (dirtyRect.IsEmpty)
            {
                dirtyRect = dirty;
            }
            else
            {
                dirtyRect.UnionWith(dirty);
            }
        }

        public virtual void UpdateLayout()
        {
            if (RootUIElement != null)
                RootUIElement.SetBounds(Bounds, null);
        }

        public virtual void Draw()
        {
            if (DirtyRect.IsEmpty)
                return;

            if (RootUIElement != null)
                RootUIElement.Draw();

            ClearDirtyRect();
        }

        public bool HandleTouch(Touch touch)
        {
            if (RootUIElement != null)
                return RootUIElement.HandleTouch(touch);

            return false;
        }
    }
}
