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

        public UIElement()
        {
            BackgroundColor = new UIColor(0, 0, 0, 0);
        }

        public void GetSize(out float width, out float height)
        {
            GetContentSize(out width, out height);

            if (HorizontalAlignment != EHorizontalAlignment.Absolute)
            {
                if (DesiredWidth != 0)
                    width = DesiredWidth;

                width += Margin.Left + Margin.Right + Padding.Left + Padding.Right;
            }

            if (VerticalAlignment != EVerticalAlignment.Absolute)
            {
                if (DesiredHeight != 0)
                    height = DesiredHeight;

                height += Margin.Top + Margin.Bottom + Padding.Top + Padding.Bottom;
            }
        }

        protected virtual void GetContentSize(out float width, out float height)
        {
            width = 0;
            height = 0;
        }

        public virtual void SetBounds(RectF bounds, UIElement parent)
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

            GetSize(out layoutWidth, out layoutHeight);

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

            RectF newLayoutBounds = Margin.ShrinkRectangle(new RectF(layoutLeft, layoutTop, layoutWidth, layoutHeight));
            RectF newContentBounds = Padding.ShrinkRectangle(newLayoutBounds);

            // Only call UpdateContentLayout if the layout changed
            if (!newLayoutBounds.Equals(LayoutBounds) || !newContentBounds.Equals(ContentBounds))
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

        public virtual bool HandleTouch(in Touch touch)
        {
            return false;
        }
    }
}
