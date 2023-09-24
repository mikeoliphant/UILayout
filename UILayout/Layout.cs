using System;
#if !GENERICS_UNSUPPORTED
using System.Collections.Generic;
#endif

namespace UILayout
{
    public interface IPopup
    {
        Action CloseAction { get; set; }
        void Opened();
    }

    public class Layout
    {
        public static Layout Current { get; private set; }

        public static UIImage DefaultOutlineNinePatch { get; set; }
        public static UIImage DefaultPressedNinePatch { get; set; }
        public static UIImage DefaultUnpressedNinePatch { get; set; }

        public GraphicsContext2D GraphicsContext { get; protected set; }
        public InputManager InputManager { get; protected set; }

        protected bool haveDirty = false;
        protected RectF dirtyRect = RectF.Empty;

        public RectF Bounds { get; private set; }
        public UIElement RootUIElement { get; set; }
        public bool HaveDirty { get => haveDirty; }
        public RectF DirtyRect { get { return dirtyRect; } set { dirtyRect = value; } }

        public UIElement ActiveUIElement
        {
            get
            {
                if (popupStack.Count > 0)
                {
                    return (popupStack[popupStack.Count - 1] as UIElement);
                }

                return RootUIElement;
            }
        }

#if !GENERICS_UNSUPPORTED
        List<UIElement> popupStack = new List<UIElement>();
#else
        ArrayList popupStack = new ArrayList();
#endif

        public Layout()
        {
            Layout.Current = this;

            InputManager = new InputManager();
        }

        public void SetBounds(RectF bounds)
        {
            this.Bounds = bounds;
        }

        public void ClearDirtyRect()
        {
            haveDirty = false;
        }

        public void AddDirtyRect(in RectF dirty)
        {
            if (!haveDirty)
            {
                dirtyRect.Copy(dirty);

                haveDirty = true;
            }
            else
            {
                dirtyRect.UnionWith(in dirty);
            }
        }

        public virtual void UpdateLayout()
        {
            if (RootUIElement != null)
                RootUIElement.SetBounds(Bounds, null);

            foreach (UIElement popup in popupStack)
            {
                popup.SetBounds(Bounds, null);
            }
        }

        public virtual void Draw()
        {
            Draw(RootUIElement);
        }

        public void Update(float secondsElapsed)
        {
            InputManager.Update(secondsElapsed);

            UIElement active = ActiveUIElement;

            if (active != null)
                active.HandleInput(InputManager);
        }

        public virtual void Draw(UIElement startElement)
        {
            if (!haveDirty)
                return;

            if (startElement != null)
                startElement.Draw();

            foreach (UIElement popup in popupStack)
            {
                popup.Draw();
            }

            ClearDirtyRect();
        }

        public bool HandleTouch(in Touch touch)
        {
            UIElement active = ActiveUIElement;

            if (active != null)
                return active.HandleTouch(touch);

            return false;
        }

        public void ShowPopup(UIElement popup)
        {
            popupStack.Add(popup);

            popup.SetBounds(Bounds, null);

            if (popup is IPopup)
            {
                (popup as IPopup).CloseAction = delegate { ClosePopup(popup); };
            }

            AddDirtyRect(popup.layoutBounds);
        }

        public void ClosePopup(UIElement popup)
        {
            popupStack.Remove(popup);

            AddDirtyRect(popup.layoutBounds);
        }
    }
}
