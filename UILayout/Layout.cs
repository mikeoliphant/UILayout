using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
#if WINDOWS
using System.Windows.Forms;
#endif

namespace UILayout
{
    public interface IPopup
    {
        Action CloseAction { get; set; }
        void Opened();
    }

    /// <summary>
    /// Shows any element as a popup, in a <see cref="Frame"/> (with a little more room inside than the border needs).
    /// Placed at its top left: show it at an anchor point to put it there.
    /// </summary>
    public class PopupWrapper : Frame, IPopup
    {
        public Action CloseAction { get; set; }

        public PopupWrapper(UIElement child)
            : base(child)
        {
            HorizontalAlignment = EHorizontalAlignment.Left;
            VerticalAlignment = EVerticalAlignment.Top;

            ContentPadding = new LayoutPadding(8);
        }

        public UIElement Child
        {
            get { return Content; }
            set { Content = value; }
        }

        public void Opened()
        {
            if (Content is IPopup)
            {
                (Content as IPopup).Opened();
            }
        }
    }

    public partial class Layout
    {
        public static Layout Current { get; private set; }

        public bool UseEmbeddedResources { get; set; } = true;

        /// <summary>How controls look: colors, fonts and images. Set it up before creating controls.</summary>
        public UITheme Theme { get; set; } = new UITheme();

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
        public virtual bool InputIsActive { get { return true;  } }

        object dragObject = null;
        int dragTouchID;
        UIElement dragInitiator = null;
        UIImage dragImage = null;
        UIColor dragImageColor;
        float dragImageXOffset = 0;
        float dragImageYOffset = 0;
        Vector2 dragImagePosition;

        // Sized up front, so the first capture doesn't allocate during a frame
        Dictionary<int, UIElement> touchCapture = new Dictionary<int, UIElement>(4);

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

        public bool InDrag
        {
            get
            {
                return (dragObject != null);
            }
        }

        List<UIElement> popupStack = new List<UIElement>();

        // The elements under the mouse, from the active element (the top popup, or the root) down to the deepest.
        // Sized up front for deep element trees, so hovering doesn't grow it during a frame
        List<UIElement> mouseOverPath = new List<UIElement>(32);

        public Layout()
        {
            Layout.Current = this;

            InputManager = new InputManager();

            InputManager.AddMapping("LeftArrow", new KeyMapping(InputKey.Left) { DoRepeat = true });
            InputManager.AddMapping("RightArrow", new KeyMapping(InputKey.Right) { DoRepeat = true });
            InputManager.AddMapping("UpArrow", new KeyMapping(InputKey.Up) { DoRepeat = true });
            InputManager.AddMapping("DownArrow", new KeyMapping(InputKey.Down) { DoRepeat = true });
            InputManager.AddMapping("Backspace", new KeyMapping(InputKey.Back) { DoRepeat = true });
            InputManager.AddMapping("Enter", new KeyMapping(InputKey.Enter) { DoRepeat = true });
        }

        public virtual void Exiting()
        {
        }

        public UIImage AddImage(ContentLoader loader, string name)
        {
            images[name] = loader.LoadImage(name);

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

        public bool TryGetFont(string name, out UIFont font)
        {
            return fonts.TryGetValue(name, out font);
        }

        public virtual void SetBounds(in RectF bounds)
        {
            this.Bounds = bounds;
        }

        public void ClearDirtyRect()
        {
            haveDirty = false;
        }

        public virtual void AddDirtyRect(in RectF dirty)
        {
            if (float.IsNaN(dirty.Width) || float.IsNaN(dirty.Height))
                return;

            if (!haveDirty)
            {
                dirtyRect.Copy(dirty);

                haveDirty = true;
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

            if (InputIsActive)
            {
                InputManager.Update(secondsElapsed);

                foreach (Touch touch in InputManager.GetTouches())
                {
                    HandleTouch(touch);
                }
            }

            UIElement activeElement = ActiveUIElement;

            UpdateMouseOver(activeElement);

            // The mouse wheel goes to the element under the mouse, then up through the elements containing it until
            // one uses it
            int wheel = InputIsActive ? InputManager.MouseWheelDelta : 0;

            if (wheel != 0)
            {
                for (int i = mouseOverPath.Count - 1; i >= 0; i--)
                {
                    if (mouseOverPath[i].HandleMouseWheel(wheel))
                        break;
                }
            }

            if (activeElement != null)
                activeElement.HandleInput(InputManager);
        }

        /// <summary>The deepest visible element under the mouse, or null (the mouse is outside the layout, or over
        /// nothing). Only elements of the active element count: while a popup is open, what's under it isn't under the
        /// mouse.</summary>
        public UIElement MouseOverElement => (mouseOverPath.Count > 0) ? mouseOverPath[mouseOverPath.Count - 1] : null;

        /// <summary>Whether the mouse is over the element: the element is under the mouse, or contains the element that
        /// is (as CSS :hover). Updated once per <see cref="Update"/>.</summary>
        public bool IsMouseOver(UIElement element)
        {
            for (int i = 0; i < mouseOverPath.Count; i++)
            {
                if (mouseOverPath[i] == element)
                    return true;
            }

            return false;
        }

        // Finds the elements under the mouse, as touches find the element they go to: the last visible child (drawn on
        // top) whose bounds contain the point, down through lists and wrappers.
        void UpdateMouseOver(UIElement activeElement)
        {
            mouseOverPath.Clear();

            Vector2 position = InputManager.MousePosition;

            if ((activeElement == null) || !Bounds.Contains(position))
                return;

            UIElement element = activeElement;

            while ((element != null) && element.Visible && element.LayoutBounds.Contains(position))
            {
                mouseOverPath.Add(element);

                UIElement next = null;

                if (element is UIElementWrapper wrapper)
                {
                    next = wrapper.Child;
                }
                else if (element is ListUIElement list)
                {
                    for (int i = list.Children.Count - 1; i >= 0; i--)
                    {
                        UIElement child = list.Children[i];

                        if (child.Visible && child.LayoutBounds.Contains(position))
                        {
                            next = child;

                            break;
                        }
                    }
                }

                element = next;
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

            if ((dragImage != null) && (dragImagePosition.X != float.MinValue))
            {
                GraphicsContext.DrawImage(dragImage, (int)(dragImagePosition.X + dragImageXOffset), (int)(dragImagePosition.Y + dragImageYOffset), dragImageColor, 1.0f);
            }

            ClearDirtyRect();
        }

        public bool HandleTextInput(char c)
        {
            UIElement activeElement = ActiveUIElement;

            if (activeElement != null)
                return activeElement.HandleTextInput(c);

            return false;
        }

        public bool HandleTouch(in Touch touch)
        {
            // Outside the layout only touches that something has captured (or is dragging) still count, so that
            // a drag released outside the window still ends.
            if (!Bounds.Contains(touch.Position) && !touchCapture.ContainsKey(touch.TouchID) &&
                !((dragObject != null) && (dragTouchID == touch.TouchID)))
                return false;

            UIElement activeElement = ActiveUIElement;

            if ((dragObject != null) && (dragTouchID == touch.TouchID))
            {
                switch (touch.TouchState)
                {
                    case ETouchState.Moved:
                        if (dragImagePosition.X != float.MinValue)
                            UpdateDragDirty();
                        dragImagePosition = touch.Position;
                        UpdateDragDirty();

                        //activeElement.DragHover(dragInitiator, dragObject, touch);
                        break;
                    case ETouchState.Released:
                        UIElement dropElement = activeElement.AcceptsDrop(dragInitiator, dragObject, touch);

                        if ((dropElement != null) && dropElement.HandleDrop(dragInitiator, dragObject, touch))
                        {
                            dragInitiator.HandleDragCompleted(dragObject);
                        }
                        else
                        {
                            dragInitiator.HandleDragCancelled(dragObject);
                        }

                        UpdateDragDirty();

                        dragObject = null;
                        dragImage = null;

                        break;
                    case ETouchState.Held:
                        //activeElement.DragHover(dragInitiator, dragObject, touch);
                        break;
                    case ETouchState.Invalid:
                        UpdateDragDirty();

                        dragInitiator.HandleDragCancelled(dragObject);

                        dragImage = null;
                        dragObject = null;

                        break;
                }

                return true;
            }

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
                if (activeElement != null)
                {
                    return activeElement.HandleTouch(touch);
                }
            }

            return false;
        }

        public void BeginDrag(UIElement initiator, int touchID, object obj, UIImage image, float offsetX, float offsetY)
        {
            BeginDrag(initiator, touchID, obj, image, offsetX, offsetY, UIColor.White);
        }

        /// <summary>Starts dragging the object, with the image (tinted with the color) following the touch.</summary>
        public void BeginDrag(UIElement initiator, int touchID, object obj, UIImage image, float offsetX, float offsetY,
            UIColor color)
        {
            dragInitiator = initiator;
            dragTouchID = touchID;
            dragObject = obj;
            dragImage = image;
            dragImageColor = color;
            dragImageXOffset = offsetX;
            dragImageYOffset = offsetY;
            dragImagePosition = new Vector2(float.MinValue);
        }

        void UpdateDragDirty()
        {
            AddDirtyRect(new RectF(dragImagePosition.X + dragImageXOffset, dragImagePosition.Y + dragImageYOffset,
                dragImage.LogicalWidth, dragImage.LogicalHeight));
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
                (popup as IPopup).CloseAction = CloseActionFor(popup);

                (popup as IPopup).Opened();
            }

            AddDirtyRect(popup.layoutBounds);
        }

        // An action closing the popup. Its own method: a lambda capturing ShowPopup's parameter would make every
        // ShowPopup call allocate the captured state, also for popups that need no close action.
        Action CloseActionFor(UIElement popup)
        {
            return delegate { ClosePopup(popup); };
        }

        public void ShowPopup(UIElement popup, in Vector2 anchorPoint)
        {
            ShowContextPopup(popup, new ContextUIElementWrapper(anchorPoint));
        }

        /// <summary>
        /// Shows the popup just below the anchor bounds (an element's), left-aligned with them; above them when it
        /// doesn't fit below. The popup should be Left- and Top-aligned (not Absolute or Stretch).
        /// </summary>
        public void ShowPopup(UIElement popup, in RectF anchorBounds)
        {
            ShowContextPopup(popup, new ContextUIElementWrapper(anchorBounds));
        }

        void ShowContextPopup(UIElement popup, ContextUIElementWrapper contextElementWrapper)
        {
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
            ShowContinuePopup(new TextBlock(text), continueAction);
        }

        public void ShowContinuePopup(UIElement contents, Action continueAction)
        {
            ShowPopup(new InputDialog(contents,
                new DialogInput { Text = "Continue", Action = continueAction, CloseOnInput = true, IsPrimary = true }));
        }

        public void ShowConfirmationPopup(string text, Action confirmAction)
        {
            ShowConfirmationPopup(new TextBlock(text), confirmAction);
        }

        public void ShowConfirmationPopup(UIElement contents, Action confirmAction)
        {
            ShowPopup(new InputDialog(contents,
                new DialogInput { Text = "Yes", Action = confirmAction, CloseOnInput = true, IsPrimary = true },
                new DialogInput { Text = "No", CloseOnInput = true }));
        }

        public void ShowBackPopup(string text)
        {
            ShowBackPopup(new TextBlock(text));
        }

        public void ShowBackPopup(UIElement contents)
        {
            ShowPopup(new InputDialog(contents, new DialogInput { Text = "Back", CloseOnInput = true }));
        }

        public void ShowTextInputPopup(string prompt, Action<string> confirmAction)
        {
            ShowTextInputPopup(prompt, null, confirmAction);
        }

        /// <summary>A dialog asking for a line of text: the prompt over a <see cref="TextBox"/>, with Ok and Cancel.
        /// Enter is Ok.</summary>
        public void ShowTextInputPopup(string prompt, string defaultText, Action<string> confirmAction)
        {
#if ANDROID
            Layout.Current.GetKeyboardInput(prompt, defaultText, delegate (string str, object obj)
            {
                confirmAction(str);
            }, null);            
#else
            VerticalStack stack = new VerticalStack
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Stretch,
                ChildSpacing = 6
            };

            stack.Children.Add(new TextBlock(prompt));

            TextBox textBox = new TextBox(256)
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                DesiredWidth = 300
            };
            stack.Children.Add(textBox);

            textBox.SetText(defaultText);
            textBox.Focus();

            var dialog = new InputDialog(stack,
                new DialogInput
                {
                    Text = "Ok",
                    Action = delegate
                    {
                        confirmAction(textBox.GetText());
                    },
                    CloseOnInput = true,
                    IsPrimary = true
                },
                new DialogInput { Text = "Cancel", CloseOnInput = true });

            textBox.EnterAction = delegate
            {
                dialog.Exit();
                confirmAction(textBox.GetText());
            };

            ShowPopup(dialog);
#endif
        }

        public void ClosePopup(UIElement popup)
        {
            popupStack.Remove(popup);

            AddDirtyRect(popup.layoutBounds);
        }

        public void GetKeyboardInput(string title, string defaultText, Action<string, object> callback, object userData)
        {
            GetKeyboardInputAsync(title, defaultText).ContinueWith(t => callback(t.Result, userData));
        }

        public virtual Task<string> GetKeyboardInputAsync(string title, string defaultText)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Places a popup at an anchor, inside the bounds it's given. Anchored at a point (a context menu), the popup's top
    /// left goes there, flipped to the point's left or above it when it doesn't fit. Anchored to an element's bounds
    /// (a drop-down's list), it goes just below them, left-aligned with them; above them when it doesn't fit below, and
    /// moved left as far as it needs to fit across.
    /// </summary>
    public class ContextUIElementWrapper : UIElementWrapper
    {
        bool anchoredToBounds;
        RectF anchorBounds;

        public ContextUIElementWrapper(Vector2 point)
        {
            HorizontalAlignment = EHorizontalAlignment.Absolute;
            VerticalAlignment = EVerticalAlignment.Absolute;

            AnchorPoint = point;
        }

        public ContextUIElementWrapper(in RectF anchor)
        {
            HorizontalAlignment = EHorizontalAlignment.Absolute;
            VerticalAlignment = EVerticalAlignment.Absolute;

            AnchorBounds = anchor;
        }

        /// <summary>Where the child is placed (moved to fit the bounds it's given), so the wrapper can be shown again
        /// elsewhere.</summary>
        public Vector2 AnchorPoint
        {
            get { return new Vector2(anchorBounds.X, anchorBounds.Y); }

            set
            {
                anchorBounds = new RectF(value.X, value.Y, 0, 0);
                anchoredToBounds = false;
            }
        }

        /// <summary>The bounds the child is placed below (or above), so the wrapper can be shown again
        /// elsewhere.</summary>
        public RectF AnchorBounds
        {
            get { return anchorBounds; }

            set
            {
                anchorBounds = value;
                anchoredToBounds = true;
            }
        }

        public override void SetBounds(in RectF bounds, UIElement parent)
        {
            float horizontalOffset = anchorBounds.X - bounds.X;
            float verticalOffset = anchorBounds.Bottom - bounds.Y;

            float width, height;

            base.GetContentSize(out width, out height);

            if ((horizontalOffset + width) > bounds.Width)
            {
                if (anchoredToBounds)
                    horizontalOffset = bounds.Width - width;
                else
                    horizontalOffset = anchorBounds.X - bounds.X - width;

                if (horizontalOffset < 0)
                    horizontalOffset = 0;
            }

            if ((verticalOffset + height) > bounds.Height)
            {
                verticalOffset = anchorBounds.Y - bounds.Y - height;

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

            if (Child.ContentBounds.Contains(touch.Position))
                return base.HandleTouch(touch);

            return false;
        }
    }
}
