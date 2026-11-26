using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace UILayout
{
    public interface IScrollable
    {
        void ScrollBackward();
        void ScrollForward();
        void ScrollPageBackward();
        void ScrollPageForward();
        void SetScrollPercent(float scrollPercent);
    }

    /// <summary>
    /// A <see cref="VerticalScrollBar"/> in a <see cref="Frame"/> (the track, in the theme's background color), with
    /// arrow buttons above and below it that scroll by an item.
    /// </summary>
    public class VerticalScrollBarWithArrows : VerticalStack
    {
        const float ArrowHeight = 20;

        public VerticalScrollBar ScrollBar { get; private set; }

        public VerticalScrollBarWithArrows()
        {
            UITheme theme = Layout.Current.Theme;

            VerticalAlignment = EVerticalAlignment.Stretch;
            ChildSpacing = 2;

            ScrollBar = new VerticalScrollBar()
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Stretch
            };

            Frame track = new Frame(ScrollBar)
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Stretch,
                FillColor = theme.Background
            };

            Children.Add(new ImageButton(theme.ScrollUpArrow)
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                DesiredHeight = ArrowHeight,
                ClickAction = ScrollBar.ScrollBackward
            });

            Children.Add(track);

            Children.Add(new ImageButton(theme.ScrollDownArrow)
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                DesiredHeight = ArrowHeight,
                ClickAction = ScrollBar.ScrollForward
            });
        }
    }

    /// <summary>
    /// A vertical scroll bar: the theme's scroll thumb, as long as the part of the list that's in view, at where it is.
    /// Drag the thumb, or press above or below it to scroll by a page. The thumb takes the accent color when the
    /// mouse is over it and while it's dragged.
    /// </summary>
    public class VerticalScrollBar : UIElementWrapper
    {
        public IScrollable Scrollable { get; set; }

        float visiblePercent = 1.0f;
        bool inDrag;
        int touchID;
        Vector2 dragStart;
        float scrollPercent = 0.0f;
        float startY;

        NinePatchWrapper bar;

        public VerticalScrollBar()
        {
            UITheme theme = Layout.Current.Theme;

            Child = bar = new NinePatchWrapper(theme.ScrollThumb)
            {
                Padding = new LayoutPadding(0),
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Absolute,
                Color = theme.Border
            };
        }

        public override void HandleInput(InputManager inputManager)
        {
            base.HandleInput(inputManager);

            UITheme theme = Layout.Current.Theme;
            UIColor color = (inDrag || bar.IsMouseOver) ? theme.Accent : theme.Border;

            if (bar.Color == color)
                return;

            bar.Color = color;

            Layout.Current.AddDirtyRect(LayoutBounds);
        }

        public void ScrollBackward()
        {
            if (Scrollable != null)
                Scrollable.ScrollBackward();
        }

        public void ScrollForward()
        {
            if (Scrollable != null)
                Scrollable.ScrollForward();
        }

        public void SetVisiblePercent(float visiblePercent)
        {
            this.visiblePercent = visiblePercent;

            UpdateContentLayout();
        }

        public void SetScrollPercent(float scrollPercent)
        {
            this.scrollPercent = scrollPercent;

            UpdateContentLayout();
        }

        public override void UpdateContentLayout()
        {
            if (visiblePercent >= 1.0f)
            {
                bar.DesiredHeight = ContentBounds.Height;

                bar.Margin = new LayoutPadding(0);
            }
            else
            {
                bar.DesiredHeight = visiblePercent * ContentBounds.Height;

                bar.Margin = new LayoutPadding(0, (int)(ContentBounds.Height * scrollPercent));
            }

            base.UpdateContentLayout();
        }

        public override bool HandleTouch(in Touch touch)
        {
            switch (touch.TouchState)
            {
                case ETouchState.Pressed:
                    if (touch.Position.Y < bar.ContentBounds.Top)
                    {
                        if (Scrollable != null)
                            Scrollable.ScrollPageBackward();
                    }
                    else if (touch.Position.Y > bar.ContentBounds.Bottom)
                    {
                        if (Scrollable != null)
                            Scrollable.ScrollPageForward();
                    }
                    else
                    {
                        inDrag = true;
                        touchID = touch.TouchID;

                        dragStart = touch.Position;
                        startY = bar.ContentBounds.Top;

                        CaptureTouch(touch);
                    }
                    break;
                case ETouchState.Moved:
                    if (inDrag)
                    {
                        float deltaY = touch.Position.Y - dragStart.Y;

                        float yOffset = startY + deltaY;

                        if (yOffset < ContentBounds.Top)
                        {
                            yOffset = ContentBounds.Top;
                        }
                        else if ((yOffset + bar.DesiredHeight) > ContentBounds.Bottom)
                        {
                            yOffset = ContentBounds.Bottom - bar.DesiredHeight;
                        }

                        scrollPercent = (yOffset - ContentBounds.Top) / ContentBounds.Height;

                        if (Scrollable != null)
                        {
                            Scrollable.SetScrollPercent(scrollPercent);
                        }

                        UpdateContentLayout();
                    }
                    break;
                case ETouchState.Released:
                case ETouchState.Invalid:
                    if (touch.TouchID == touchID)
                    {
                        inDrag = false;
                        ReleaseTouch(touchID);

                        UpdateContentLayout();
                    }
                    break;
            }

            return true;
        }
    }
}
