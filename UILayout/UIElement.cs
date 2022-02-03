using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

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

        public RectangleF ShrinkRectangle(RectangleF toShrink)
        {
            return new RectangleF(toShrink.Left + Left, toShrink.Top + Top, toShrink.Width - (Left + Right), toShrink.Height - (Top + Bottom));
        }
    }

    public partial class UIElement
    {
        public bool Visible { get; set; }
        public LayoutPadding Margin { get; set; }
        public EHorizontalAlignment HorizontalAlignment { get; set; }
        public EVerticalAlignment VerticalAlignment { get; set; }
        public bool MatchParentHorizontalAlignment { get; set; }
        public bool MatchParentVerticalAlignment { get; set; }
        public RectangleF ContentBounds { get; protected set; }
        public float DesiredWidth { get; set; }
        public float DesiredHeight { get; set; }

        public UIElement()
        {
            Visible = true;
        }
        
        public void GetSize(out float width, out float height)
        {
            GetContentSize(out width, out height);

            if (HorizontalAlignment != EHorizontalAlignment.Absolute)
            {
                if (DesiredWidth != 0)
                    width = DesiredWidth;

                width += Margin.Left + Margin.Right;
            }

            if (VerticalAlignment != EVerticalAlignment.Absolute)
            {
                if (DesiredHeight != 0)
                    height = DesiredHeight;

                height += Margin.Top + Margin.Bottom;
            }
        }

        protected virtual void GetContentSize(out float width, out float height)
        {
            width = 0;
            height = 0;
        }

        public virtual void SetBounds(RectangleF bounds, UIElement parent)
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

            float contentWidth;
            float contentHeight;

            GetSize(out contentWidth, out contentHeight);

            float contentLeft = 0;
            float contentTop = 0;

            switch (HorizontalAlignment)
            {
                case EHorizontalAlignment.Left:
                    contentLeft = bounds.Left + Margin.Left;
                    break;

                case EHorizontalAlignment.Right:
                    contentLeft = bounds.Right - contentWidth - Margin.Right;
                    break;

                case EHorizontalAlignment.Center:
                    contentLeft = bounds.Left + (bounds.Width - contentWidth) / 2;
                    break;

                case EHorizontalAlignment.Stretch:
                    contentLeft = bounds.Left + Margin.Left;
                    contentWidth = bounds.Width - (Margin.Left + Margin.Right);
                    break;

                case EHorizontalAlignment.Absolute:
                    contentLeft = bounds.Left + Margin.Left;
                    break;
            }

            switch (VerticalAlignment)
            {
                case EVerticalAlignment.Top:
                    contentTop = bounds.Top + Margin.Top;
                    break;

                case EVerticalAlignment.Bottom:
                    contentTop = bounds.Bottom - contentHeight - Margin.Bottom;
                    break;

                case EVerticalAlignment.Center:
                    contentTop = bounds.Top + (bounds.Height - contentHeight) / 2;
                    break;

                case EVerticalAlignment.Stretch:
                    contentTop = bounds.Top + Margin.Top;
                    contentHeight = bounds.Height - (Margin.Top + Margin.Bottom);
                    break;

                case EVerticalAlignment.Absolute:
                    contentTop = bounds.Top + Margin.Top;
                    break;
            }

            ContentBounds = new RectangleF(contentLeft, contentTop, contentWidth, contentHeight);

            UpdateContentLayout();
        }

        public virtual void UpdateContentLayout()
        {
        }
    }
}
