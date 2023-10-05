using System;
#if !GENERICS_UNSUPPORTED
using System.Collections.Generic;
using System.Numerics;
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

        Dictionary<string, UIImage> images = new Dictionary<string, UIImage>();
        Dictionary<string, UIFont> fonts = new Dictionary<string, UIFont>();

        public GraphicsContext2D GraphicsContext { get; protected set; }
        public InputManager InputManager { get; protected set; }

        public float SecondsElapsed { get; private set; }

        protected bool haveDirty = false;
        protected RectF dirtyRect = RectF.Empty;

        public RectF Bounds { get; private set; }
        public UIElement RootUIElement { get; set; }
        public bool HaveDirty { get => haveDirty; }
        public RectF DirtyRect { get { return dirtyRect; } set { dirtyRect = value; } }

        Dictionary<int, UIElement> touchCapture = new Dictionary<int, UIElement>();

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

        public UIImage AddImage(string name)
        {
            images[name] = new UIImage(name);

            return images[name];
        }

        public UIImage AddImage(string name, UIImage image)
        {
            images[name] = image;

            return images[name];
        }

        public UIImage GetImage(string name)
        {
            return images[name];
        }

        public UIFont AddFont(string name, UIFont font)
        {
            fonts[name] = font;

            return fonts[name];
        }

        public UIFont GetFont(string name)
        {
            return fonts[name];
        }

        public virtual void SetBounds(RectF bounds)
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

        public virtual void Update(float secondsElapsed)
        {
            SecondsElapsed = secondsElapsed;

            InputManager.Update(secondsElapsed);

            foreach (Touch touch in InputManager.GetTouches())
            {
                HandleTouch(touch);
            }
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
            if (touchCapture.ContainsKey(touch.TouchID))
            {
                bool result = touchCapture[touch.TouchID].HandleTouch(touch);

                if ((touch.TouchState == ETouchState.Invalid) || (touch.TouchState == ETouchState.Released))
                {
                    touchCapture.Remove(touch.TouchID);
                }

                return result;
            }
            else
            {
                UIElement active = ActiveUIElement;

                if (active != null)
                    return active.HandleTouch(touch);
            }

            return false;
        }

        internal bool CaptureTouch(int touchID, UIElement captureElement)
        {
            if (touchCapture.ContainsKey(touchID))
                return false;

            touchCapture[touchID] = captureElement;

            return true;
        }

        internal void ReleaseTouch(int touchID, UIElement captureElement)
        {
            if (touchCapture.ContainsKey(touchID) && touchCapture[touchID] == captureElement)
            {
                touchCapture.Remove(touchID);
            }
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

        public void ShowPopup(UIElement popup, Vector2 anchorPoint)
        {
            ContextUIElementWrapper contextElementWrapper = new ContextUIElementWrapper(anchorPoint);
            contextElementWrapper.Child = popup;

            popupStack.Add(contextElementWrapper);

            contextElementWrapper.SetBounds(Bounds, null);

            if (popup is IPopup)
            {
                (popup as IPopup).CloseAction = delegate { ClosePopup(contextElementWrapper); };
            }

            AddDirtyRect(popup.layoutBounds);
        }

        public void ShowContinuePopup(string text)
        {
            ShowContinuePopup(text, null);
        }

        public void ShowContinuePopup(UIElement contents)
        {
            ShowContinuePopup(contents, null);
        }

        public void ShowContinuePopup(string text, Action continueAction)
        {
            ShowPopup(new InputDialog(Layout.DefaultOutlineNinePatch, new TextBlock(text), new DialogInput { Text = "Continue", WaitForRelease = true, Action = continueAction, CloseOnInput = true }));
        }

        public void ShowContinuePopup(UIElement contents, Action continueAction)
        {
            ShowPopup(new InputDialog(Layout.DefaultOutlineNinePatch, contents, new DialogInput { Text = "Continue", WaitForRelease = true, Action = continueAction, CloseOnInput = true }));
        }

        public void ShowConfirmationPopup(string text, Action confirmAction)
        {
            ShowPopup(new InputDialog(Layout.DefaultOutlineNinePatch, new TextBlock(text),
                new DialogInput { Text = "Yes", Action = confirmAction, WaitForRelease = true, CloseOnInput = true },
                new DialogInput { Text = "No", WaitForRelease = true, CloseOnInput = true }));
        }

        public void ShowConfirmationPopup(UIElement contents, Action confirmAction)
        {
            ShowPopup(new InputDialog(Layout.DefaultOutlineNinePatch, contents,
                new DialogInput { Text = "Yes", Action = confirmAction, WaitForRelease = true, CloseOnInput = true },
                new DialogInput { Text = "No", WaitForRelease = true, CloseOnInput = true }));
        }

        public void ShowBackPopup(string text)
        {
            ShowPopup(new InputDialog(Layout.DefaultOutlineNinePatch, new TextBlock(text), new DialogInput { Text = "Back", WaitForRelease = true, CloseOnInput = true }));
        }

        public void ShowBackPopup(UIElement contents)
        {
            ShowPopup(new InputDialog(Layout.DefaultOutlineNinePatch, contents, new DialogInput { Text = "Back", WaitForRelease = true, CloseOnInput = true }));
        }

        public void ClosePopup(UIElement popup)
        {
            popupStack.Remove(popup);

            AddDirtyRect(popup.layoutBounds);
        }
    }

    public class ContextUIElementWrapper : UIElementWrapper
    {
        Vector2 anchorPoint;

        public ContextUIElementWrapper(Vector2 point)
        {
            HorizontalAlignment = EHorizontalAlignment.Absolute;
            VerticalAlignment = EVerticalAlignment.Absolute;

            anchorPoint = point;
        }

        public override void SetBounds(RectF bounds, UIElement parent)
        {
            float horizontalOffset = anchorPoint.X - bounds.X;
            float verticalOffset = anchorPoint.Y - bounds.Y;

            float width, height;

            base.GetContentSize(out width, out height);

            if ((horizontalOffset + width) > bounds.Width)
            {
                horizontalOffset = anchorPoint.X - bounds.X - width;

                if (horizontalOffset < 0)
                    horizontalOffset = 0;
            }

            if ((verticalOffset + height) > bounds.Height)
            {
                verticalOffset = anchorPoint.Y - bounds.Y - height;

                if (verticalOffset < 0)
                    verticalOffset = 0;
            }

            Margin = new LayoutPadding(horizontalOffset, verticalOffset);

            base.SetBounds(bounds, parent);
        }

        public override bool HandleTouch(in Touch touch)
        {
            if ((touch.TouchState == ETouchState.Invalid) || ((touch.TouchState == ETouchState.Released) && !Child.ContentBounds.Contains(touch.Position)))
            {
                Layout.Current.ClosePopup(this);

                return true;
            }

            return base.HandleTouch(touch);
        }
    }
}
