﻿using System;
using System.Collections;
#if !GENERICS_UNSUPPORTED
using System.Collections.Generic;
#endif

namespace UILayout
{
    public class Layout
    {
        public static Layout Current { get; private set; }
        public static Image DefaultOutlineNinePatch { get; set; }
        public static Image DefaultPressedNinePatch { get; set; }
        public static Image DefaultUnpressedNinePatch { get; set; }

        protected bool haveDirty = false;
        protected RectF dirtyRect = RectF.Empty;

        public RectF Bounds { get; private set; }
        public UIElement RootUIElement { get; set; }
        public bool HaveDirty { get => haveDirty; }
        public RectF DirtyRect { get { return dirtyRect; } set { dirtyRect = value; } }

#if !GENERICS_UNSUPPORTED
        List<UIElement> popupStack = new List<UIElement>();
#else
        ArrayList popupStack = new ArrayList();
#endif

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

            foreach (UIElement popup in popupStack)
            {
                popup.Draw();
            }

            ClearDirtyRect();
        }

        public bool HandleTouch(ref Touch touch)
        {
            if (popupStack.Count > 0)
            {
                popupStack[popupStack.Count - 1].HandleTouch(ref touch);
            }

            if (RootUIElement != null)
                return RootUIElement.HandleTouch(ref touch);

            return false;
        }

        public void ShowPopup(UIElement popup)
        {
            popupStack.Add(popup);

            popup.SetBounds(Bounds, null);

            if (popup is InputDialog)
            {
                (popup as InputDialog).CloseAction = delegate { ClosePopup(popup); };
            }

            AddDirtyRect(ref popup.layoutBounds);
        }

        public void ClosePopup(UIElement popup)
        {
            popupStack.Remove(popup);

            AddDirtyRect(ref popup.layoutBounds);
        }
    }
}
