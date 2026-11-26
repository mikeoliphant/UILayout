using System;
using System.Numerics;

namespace UILayout
{
    public enum EHorizontalAlignment
    {
        Left,
        Right,
        Center,
        Stretch,
        Absolute
    }

    public enum EVerticalAlignment
    {
        Top,
        Bottom,
        Center,
        Stretch,
        Absolute
    }

    public struct LayoutPadding
    {
        public float Left { get; set; }
        public float Top { get; set; }
        public float Right { get; set; }
        public float Bottom { get; set; }

        public static implicit operator LayoutPadding(float padding)
        {
            return new LayoutPadding(padding);
        }

        public static implicit operator LayoutPadding((float X, float Y) padding)
        {
            return new LayoutPadding(padding.X, padding.Y);
        }

        public static implicit operator LayoutPadding((float Left, float Top, float Right, float Bottom) padding)
        {
            return new LayoutPadding(padding.Left, padding.Top, padding.Right, padding.Bottom);
        }

        public LayoutPadding(float padding)
        {
            Left = Top = Right = Bottom = padding;
        }

        public LayoutPadding(float leftRight, float topBottom)
        {
            Left = Right = leftRight;
            Top = Bottom = topBottom;
        }

        public LayoutPadding(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public RectF ShrinkRectangle(RectF toShrink)
        {
            return new RectF(toShrink.Left + Left, toShrink.Top + Top, toShrink.Width - (Left + Right), toShrink.Height - (Top + Bottom));
        }

        public RectF PadRectangle(RectF toShrink)
        {
            return new RectF(toShrink.Left - Left, toShrink.Top - Top, toShrink.Width + (Left + Right), toShrink.Height + (Top + Bottom));
        }
    }

    public partial class UIElement
    {
        internal RectF layoutBounds;
        internal RectF contentBounds;

        public bool Visible { get; set; } = true;

        /// <summary>Whether what the element draws inside it is cut to its content bounds: for elements that draw
        /// more than fits, such as a scrolling list's partly visible items. Its background isn't affected.</summary>
        public bool ClipToBounds { get; set; }

        /// <summary>Whether the mouse is over this element (or an element inside it), for hover highlights. Updated
        /// once per layout update; false while a popup this element isn't part of is open.</summary>
        public bool IsMouseOver => Layout.Current.IsMouseOver(this);
        public UIColor BackgroundColor { get; set; }
        public LayoutPadding Margin { get; set; }
        public LayoutPadding Padding { get; set; }
        public EHorizontalAlignment HorizontalAlignment { get; set; }
        public EVerticalAlignment VerticalAlignment { get; set; }
        public bool MatchParentHorizontalAlignment { get; set; }
        public bool MatchParentVerticalAlignment { get; set; }
        public RectF ContentBounds { get => contentBounds; }
        public RectF LayoutBounds { get => layoutBounds; }
        public float DesiredWidth { get; set; }
        public float DesiredHeight { get; set; }
        public bool AbsorbAllInput { get; set; } = false;
        public bool HaveTouchCapture { get; private set; }
        public Vector2 TouchCaptureStartPosition { get; private set; }
        public int CapturedTouchID { get; private set; }
        public DragDropHandler DragDropHandler { get; set; }

        float lastDragY = 0;
        float totDrag = 0;
        UIElement tapElement;
        DateTime lastTapTime = DateTime.MinValue;
        Vector2 lastTapPosition;

        public UIElement()
        {
            BackgroundColor = new UIColor(0, 0, 0, 0);
        }

        public void GetSize(out float width, out float height)
        {
            if ((HorizontalAlignment == EHorizontalAlignment.Absolute) || (VerticalAlignment == EVerticalAlignment.Absolute))
            {
                width = 0;
                height = 0;
            }
            else
            {
                GetContentSize(out width, out height);

                if (DesiredWidth != 0)
                {
                    width = DesiredWidth;
                }

                if (DesiredHeight != 0)
                {
                    height = DesiredHeight;
                }

                width += Margin.Left + Margin.Right + Padding.Left + Padding.Right;
                height += Margin.Top + Margin.Bottom + Padding.Top + Padding.Bottom;
            }
        }

        protected virtual void GetContentSize(out float width, out float height)
        {
            width = 0;
            height = 0;
        }

        public virtual void SetBounds(in RectF bounds, UIElement parent)
        {
            if (parent != null)
            {
                if (MatchParentHorizontalAlignment)
                {
                    HorizontalAlignment = parent.HorizontalAlignment;
                }

                if (MatchParentVerticalAlignment)
                {
                    VerticalAlignment = parent.VerticalAlignment;
                }
            }

            if (!Visible)
                return;

            float layoutWidth;
            float layoutHeight;

            if ((HorizontalAlignment == EHorizontalAlignment.Absolute) || (VerticalAlignment == EVerticalAlignment.Absolute))
            {
                GetContentSize(out layoutWidth, out layoutHeight);

                if (DesiredWidth != 0)
                {
                    layoutWidth = DesiredWidth;
                }

                if (DesiredHeight != 0)
                {
                    layoutHeight = DesiredHeight;
                }

                layoutWidth += Padding.Left + Padding.Right;
                layoutHeight += Padding.Top + Padding.Bottom;
            }
            else
            {
                GetSize(out layoutWidth, out layoutHeight);
            }

            float layoutLeft = 0;
            float layoutTop = 0;

            switch (HorizontalAlignment)
            {
                case EHorizontalAlignment.Left:
                    layoutLeft = bounds.Left;
                    break;

                case EHorizontalAlignment.Right:
                    layoutLeft = bounds.Right - layoutWidth;
                    break;

                case EHorizontalAlignment.Center:
                    layoutLeft = bounds.Left + (bounds.Width - layoutWidth) / 2;
                    break;

                case EHorizontalAlignment.Stretch:
                    layoutLeft = bounds.Left;
                    layoutWidth = bounds.Width;
                    break;

                case EHorizontalAlignment.Absolute:
                    layoutLeft = bounds.Left;
                    break;
            }

            switch (VerticalAlignment)
            {
                case EVerticalAlignment.Top:
                    layoutTop = bounds.Top;
                    break;

                case EVerticalAlignment.Bottom:
                    layoutTop = bounds.Bottom - layoutHeight;
                    break;

                case EVerticalAlignment.Center:
                    layoutTop = bounds.Top + (bounds.Height - layoutHeight) / 2;
                    break;

                case EVerticalAlignment.Stretch:
                    layoutTop = bounds.Top;
                    layoutHeight = bounds.Height;
                    break;

                case EVerticalAlignment.Absolute:
                    layoutTop = bounds.Top;
                    break;
            }

            RectF newLayoutBounds = new RectF(layoutLeft, layoutTop, layoutWidth, layoutHeight);

            if ((HorizontalAlignment == EHorizontalAlignment.Absolute) || (VerticalAlignment == EVerticalAlignment.Absolute))
            {
                newLayoutBounds.X += Margin.Left;
                newLayoutBounds.Y += Margin.Top;
            }
            else
            {
                newLayoutBounds = Margin.ShrinkRectangle(newLayoutBounds);
            }

            RectF newContentBounds = Padding.ShrinkRectangle(newLayoutBounds);

            // Only call UpdateContentLayout if the layout changed
            if (true) //!newLayoutBounds.Equals(LayoutBounds) || !newContentBounds.Equals(ContentBounds))
            {
                layoutBounds = newLayoutBounds;
                contentBounds = newContentBounds;

                UpdateContentLayout();
            }
        }

        public virtual void UpdateContentLayout()
        {
            Layout.Current.AddDirtyRect(layoutBounds);
        }

        public virtual bool HandleTextInput(char c)
        {
            return AbsorbAllInput;
        }

        public virtual bool HandleTouch(in Touch touch)
        {
            if (DragDropHandler != null)
            {
                return DragDropHandler.HandleTouch(touch);
            }

            return AbsorbAllInput;
        }

        public void CaptureTouch(in Touch touch)
        {
            if (Layout.Current.CaptureTouch(touch.TouchID, this))
            {
                HaveTouchCapture = true;
                CapturedTouchID = touch.TouchID;

                TouchCaptureStartPosition = touch.Position;
            }
        }

        public void ReleaseTouch()
        {
            ReleaseTouch(CapturedTouchID);
        }

        public void ReleaseTouch(int touchID)
        {
            HaveTouchCapture = false;
            Layout.Current.ReleaseTouch(touchID, this);
        }

        protected bool IsTap(in Touch touch, UIElement element)
        {
            switch (touch.TouchState)
            {
                case ETouchState.Pressed:
                    lastDragY = touch.Position.Y;
                    totDrag = 0;
                    tapElement = element;
                    break;
                case ETouchState.Moved:
                    if (element == tapElement)
                    {
                        totDrag += Math.Abs(touch.Position.Y - lastDragY);

                        lastDragY = touch.Position.Y;
                    }
                    break;
                case ETouchState.Released:
                    if (element == tapElement)
                    {
                        tapElement = null;

                        if (totDrag < 5)
                        {
                            return true;
                        }
                    }
                    break;
                case ETouchState.Invalid:
                    tapElement = null;
                    break;
            }

            return false;
        }

        public bool IsDoubleTap(in Touch touch, UIElement element)
        {
            if (IsTap(touch, element))
            {
                DateTime now = DateTime.Now;

                if (((now - lastTapTime).TotalSeconds < 0.5) && (Vector2.Distance(touch.Position, lastTapPosition) < 5))
                {
                    return true;
                }

                lastTapTime = now;
                lastTapPosition = touch.Position;
            }

            return false;
        }

        public virtual void HandleInput(InputManager inputManager)
        {

        }

        /// <summary>
        /// The mouse wheel turned while the mouse is over the element (or an element inside it that didn't use the
        /// wheel). Delta: 120 per notch, positive away from the user. Returns whether the element used it; if not, the
        /// element containing it is asked next.
        /// </summary>
        public virtual bool HandleMouseWheel(int delta)
        {
            return false;
        }

        /// <summary>
        /// The mouse moved and is still over the element. Called only when the layout redraws just its dirty area
        /// (<see cref="Layout.RedrawsDirtyArea"/>): an element that looks different depending on where in it the mouse
        /// is (as a list highlighting the item under the mouse) adds the area that changed.
        /// </summary>
        public virtual void HandleMouseMoveOver(in Vector2 lastPosition, in Vector2 position)
        {
        }

        /// <summary>Starts dragging the object, shown by the theme's frame fill in its accent color.</summary>
        public void BeginDrag(int touchID, object obj)
        {
            UITheme theme = Layout.Current.Theme;
            UIImage image = theme.FrameFill;

            Layout.Current.BeginDrag(this, touchID, obj, image, -image.LogicalWidth / 2, -image.LogicalHeight / 2,
                theme.Accent);
        }

        public void BeginDrag(int touchID, object obj, UIImage image)
        {
            BeginDrag(touchID, obj, image, -image.LogicalWidth / 2, -image.LogicalHeight / 2);
        }

        public void BeginDrag(int touchID, object obj, UIImage image, float imageXOffset, float imageYOffset)
        {
            Layout.Current.BeginDrag(this, touchID, obj, image, imageXOffset, imageYOffset);
        }

        public virtual UIElement AcceptsDrop(UIElement dragElement, object dropObject, in Touch touch)
        {
            if (DragDropHandler != null)
                return DragDropHandler.AcceptsDrop(dragElement, dropObject, touch);

            return null;
        }

        public virtual bool HandleDrop(UIElement dragElement, object dropObject, in Touch touch)
        {
            if (DragDropHandler != null)
                return DragDropHandler.HandleDrop(dragElement, dropObject, touch);

            return false;
        }

        public virtual void HandleDragCompleted(object dropObject)
        {
            if (DragDropHandler != null)
                DragDropHandler.HandleDragCompleted(dropObject);
        }

        public virtual void HandleDragCancelled(object dropObject)
        {
            if (DragDropHandler != null)
                DragDropHandler.HandleDragCancelled(dropObject);
        }

        public void Draw()
        {
            if (!Visible)
                return;

            // Don't draw if we aren't in the diry rectangle
            if (!Layout.Current.HaveDirty || !Layout.Current.DirtyRect.Intersects(layoutBounds))
                return;

            if (BackgroundColor.A > 0)
            {
                Layout.Current.GraphicsContext.DrawRectangle(LayoutBounds, BackgroundColor);
            }

            if (!ClipToBounds)
            {
                DrawContents();

                return;
            }

            GraphicsContext2D graphicsContext = Layout.Current.GraphicsContext;

            graphicsContext.PushClip(ContentBounds);

            DrawContents();

            graphicsContext.PopClip();
        }

        protected virtual void DrawContents()
        {

        }
    }
}
