using System;

namespace UILayout
{
    public class Layout
    {
        public static Layout Current { get; private set; }
        public static Image DefaultPressedNinePatch { get; set; }
        public static Image DefaultUnpressedNinePatch { get; set; }

        protected bool haveDirty = false;
        protected RectF dirtyRect = RectF.Empty;

        public RectF Bounds { get; private set; }
        public UIElement RootUIElement { get; set; }
        public bool HaveDirty { get => haveDirty; }
        public RectF DirtyRect { get { return dirtyRect; } set { dirtyRect = value; } }

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
            haveDirty = false;
        }

        public void AddDirtyRect(ref RectF dirty)
        {
            if (!haveDirty)
            {
                dirtyRect.Copy(ref dirty);

                haveDirty = true;
            }
            else
            {
                dirtyRect.UnionWith(ref dirty);
            }
        }

        public virtual void UpdateLayout()
        {
            if (RootUIElement != null)
                RootUIElement.SetBounds(Bounds, null);
        }

        public virtual void Draw()
        {
            Draw(RootUIElement);
        }

        public virtual void Draw(UIElement startElement)
        {
            if (!haveDirty)
                return;

            if (startElement != null)
                startElement.Draw();

            ClearDirtyRect();
        }

        public bool HandleTouch(ref Touch touch)
        {
            if (RootUIElement != null)
                return RootUIElement.HandleTouch(ref touch);

            return false;
        }
    }
}
