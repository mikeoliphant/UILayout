#if !GENERICS_UNSUPPORTED
using UIElementCollection = System.Collections.Generic.List<UILayout.UIElement>;
#else
using UIElementCollection = System.Collections.ArrayList;
#endif

namespace UILayout
{
    public class LayoutElement : UIElement
    {
    }

    public class UIElementWrapper : LayoutElement
    {
        public virtual UIElement Child { get; set; }

        protected override void GetContentSize(out float width, out float height)
        {
            if (Child != null)
                Child.GetSize(out width, out height);
            else
            {
                width = 0;
                height = 0;
            }
        }

        public override void UpdateContentLayout()
        {
            base.UpdateContentLayout();

            if (Child != null)
                Child.SetBounds(ContentBounds, this);
        }

        protected override void DrawContents()
        {
            base.DrawContents();

            if (Child != null)
                Child.Draw();
        }

        public override bool HandleTouch(ref Touch touch)
        {
            if (Child != null)
                return Child.HandleTouch(ref touch);

            return false;
        }
    }

    public class ListUIElement : LayoutElement
    {
        protected UIElementCollection children = new UIElementCollection();

        public bool DrawInReverse { get; set; }
        public float ChildSpacing { get; set; }
        public bool ChildrenEqualSize { get; set; }

        public virtual UIElementCollection Children
        {
            get { return children; }
            set { children = value; }
        }

        protected override void DrawContents()
        {
            if (!Visible)
                return;

            if (DrawInReverse)
            {
                for (int i = children.Count - 1; i >= 0; i--)
                {
                    (children[i] as UIElement).Draw();
                }
            }
            else
            {
                foreach (UIElement child in children)
                {
                    child.Draw();
                }
            }
        }

        public UIElement FindChildContainingPoint(float x, float y)
        {
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                UIElement child = Children[i] as UIElement;

                if (child.Visible && child.ContentBounds.Contains(x, y))
                {
                    return child;
                }
            }

            return null;
        }

        public UIElement FindClosestChild(ref PointF point)
        {
            float minDist = float.MaxValue;
            UIElement minChild = null;

            for (int i = Children.Count - 1; i >= 0; i--)
            {
                UIElement child = Children[i] as UIElement;

                if (child.Visible)
                {
                    float dist =  point.Distance(child.ContentBounds.GetCenter());

                    if (dist < minDist)
                    {
                        minDist = dist;

                        minChild = child;
                    }
                }
            }

            return minChild;
        }

        public override bool HandleTouch(ref Touch touch)
        {
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                UIElement child = Children[i] as UIElement;

                if (child.Visible && child.LayoutBounds.Contains(ref touch.Position))
                {
                    if (child.HandleTouch(ref touch))
                        return true;
                }
            }

            return base.HandleTouch(ref touch);
        }
    }

    public class VerticalStack : ListUIElement
    {
        protected override void GetContentSize(out float width, out float height)
        {
            width = 0;
            height = 0;

            for (int i = 0; i < children.Count; i++)
            {
                UIElement child = children[i] as UIElement;

                if (!child.Visible)
                    continue;

                float childWidth;
                float childHeight;

                child.GetSize(out childWidth, out childHeight);

                if (childWidth > width)
                    width = childWidth;

                height += childHeight;

                if (i < (children.Count - 1))
                {
                    height += ChildSpacing;
                }
            }
        }

        public override void UpdateContentLayout()
        {
            base.UpdateContentLayout();

            float neededHeight = 0;

            int greedyChildCount = 0;

            for (int i = 0; i < children.Count; i++)
            {
                UIElement child = children[i] as UIElement;

                if (!child.Visible)
                    continue;

                float childWidth;
                float childHeight;

                child.GetSize(out childWidth, out childHeight);

                neededHeight += childHeight;

                if (i < (children.Count - 1))
                {
                    neededHeight += ChildSpacing;
                }

                if (ChildrenEqualSize || (child.VerticalAlignment == EVerticalAlignment.Stretch))
                    greedyChildCount++;
            }

            float extraHeight = (ContentBounds.Height - neededHeight) / greedyChildCount;

            float yOffset = 0;

            for (int i = 0; i < children.Count; i++)
            {
                UIElement child = children[i] as UIElement;

                if (!child.Visible)
                    continue;

                if (ChildrenEqualSize)
                {
                    float height = ContentBounds.Height / greedyChildCount;

                    child.SetBounds(new RectF(ContentBounds.Left, ContentBounds.Top + yOffset, ContentBounds.Width, height), this);

                    yOffset += height;
                }
                else
                {
                    float childWidth;
                    float childHeight;

                    child.GetSize(out childWidth, out childHeight);

                    if (child.VerticalAlignment == EVerticalAlignment.Stretch)
                    {
                        child.SetBounds(new RectF(ContentBounds.Left, ContentBounds.Top + yOffset, ContentBounds.Width, childHeight + extraHeight), this);

                        yOffset += childHeight + extraHeight;
                    }
                    else
                    {
                        child.SetBounds(new RectF(ContentBounds.Left, ContentBounds.Top + yOffset, ContentBounds.Width, childHeight), this);

                        yOffset += childHeight;
                    }

                    if (i < (children.Count - 1))
                    {
                        yOffset += ChildSpacing;
                    }
                }
            }
        }
    }


    public class HorizontalStack : ListUIElement
    {
        protected override void GetContentSize(out float width, out float height)
        {
            width = 0;
            height = 0;

            for (int i = 0; i < children.Count; i++)
            {
                UIElement child = children[i] as UIElement;

                if (!child.Visible)
                    continue;

                float childWidth;
                float childHeight;

                child.GetSize(out childWidth, out childHeight);

                if (childHeight > height)
                    height = childHeight;

                width += childWidth;

                if (i < (children.Count - 1))
                {
                    width += ChildSpacing;
                }
            }
        }

        public override void UpdateContentLayout()
        {
            base.UpdateContentLayout();
           
            float neededWidth = 0;

            int greedyChildCount = 0;

            for (int i = 0; i < children.Count; i++)
            {
                UIElement child = children[i] as UIElement;

                if (!child.Visible)
                    continue;

                float childWidth;
                float childHeight;

                child.GetSize(out childWidth, out childHeight);

                neededWidth += childWidth;

                if (i < (children.Count - 1))
                {
                    neededWidth += ChildSpacing;
                }

                if (ChildrenEqualSize || (child.HorizontalAlignment == EHorizontalAlignment.Stretch))
                    greedyChildCount++;
            }

            float extraWidth = (ContentBounds.Width - neededWidth) / greedyChildCount;

            float xOffset = 0;

            for (int i = 0; i < children.Count; i++)
            {
                UIElement child = children[i] as UIElement;

                if (!child.Visible)
                    continue;

                if (ChildrenEqualSize)
                {
                    float width = ContentBounds.Width / greedyChildCount;

                    child.SetBounds(new RectF(ContentBounds.Left + xOffset, ContentBounds.Top, width, ContentBounds.Height), this);

                    xOffset += width;
                }
                else
                {
                    float childWidth;
                    float childHeight;

                    child.GetSize(out childWidth, out childHeight);

                    if (child.HorizontalAlignment == EHorizontalAlignment.Stretch)
                    {
                        child.SetBounds(new RectF(ContentBounds.Left + xOffset, ContentBounds.Top, childWidth + extraWidth, ContentBounds.Height), this);

                        xOffset += childWidth + extraWidth;
                    }
                    else
                    {
                        child.SetBounds(new RectF(ContentBounds.Left + xOffset, ContentBounds.Top, childWidth, ContentBounds.Height), this);

                        xOffset += childWidth;
                    }
                }

                if (i < (children.Count - 1))
                {
                    xOffset += ChildSpacing;
                }
            }
        }
    }

    public class Dock : ListUIElement
    {
        public Dock()
        {
            HorizontalAlignment = EHorizontalAlignment.Stretch;
            VerticalAlignment = EVerticalAlignment.Stretch;
        }

        protected override void GetContentSize(out float width, out float height)
        {
            width = 0;
            height = 0;

            foreach (UIElement child in children)
            {
                float childWidth;
                float childHeight;

                child.GetSize(out childWidth, out childHeight);

                if (childHeight > height)
                    height = childHeight;

                if (childWidth > width)
                    width = childWidth;
            }
        }

        public override void UpdateContentLayout()
        {
            base.UpdateContentLayout();

            foreach (UIElement child in children)
            {
                if (child.Visible)
                {
                    child.SetBounds(ContentBounds, this);
                }
            }
        }
    }

}
