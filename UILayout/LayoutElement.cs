using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace UILayout
{
    public class LayoutElement : UIElement
    {
    }

    public class ListUIElement : LayoutElement
    {
        protected List<UIElement> children = new List<UIElement>();

        public bool DrawInReverse { get; set; }
        public float ChildSpacing { get; set; }
        public bool ChildrenEqualSize { get; set; }

        public virtual List<UIElement> Children
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
                    children[i].Draw();
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
                UIElement child = Children[i];

                if (child.Visible && child.ContentBounds.Contains(x, y))
                {
                    return child;
                }
            }

            return null;
        }

        public UIElement FindClosestChild(PointF point)
        {
            float minDist = float.MaxValue;
            UIElement minChild = null;

            for (int i = Children.Count - 1; i >= 0; i--)
            {
                UIElement child = Children[i];

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
    }

    public class VerticalStack : ListUIElement
    {
        protected override void GetContentSize(out float width, out float height)
        {
            width = 0;
            height = 0;

            for (int i = 0; i < children.Count; i++)
            {
                UIElement child = children[i];

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
                UIElement child = children[i];

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
                UIElement child = children[i];

                if (!child.Visible)
                    continue;

                if (ChildrenEqualSize)
                {
                    float height = ContentBounds.Height / greedyChildCount;

                    child.SetBounds(new RectangleF(ContentBounds.Left, ContentBounds.Top + yOffset, ContentBounds.Width, height), this);

                    yOffset += height;
                }
                else
                {
                    float childWidth;
                    float childHeight;

                    child.GetSize(out childWidth, out childHeight);

                    if (child.VerticalAlignment == EVerticalAlignment.Stretch)
                    {
                        child.SetBounds(new RectangleF(ContentBounds.Left, ContentBounds.Height + yOffset, ContentBounds.Width, childHeight + extraHeight), this);

                        yOffset += childHeight + extraHeight;
                    }
                    else
                    {
                        child.SetBounds(new RectangleF(ContentBounds.Left, ContentBounds.Height + yOffset, ContentBounds.Width, childHeight), this);

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
                UIElement child = children[i];

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
                UIElement child = children[i];

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
                UIElement child = children[i];

                if (!child.Visible)
                    continue;

                if (ChildrenEqualSize)
                {
                    float width = ContentBounds.Width / greedyChildCount;

                    child.SetBounds(new RectangleF(ContentBounds.Left + xOffset, ContentBounds.Top, width, ContentBounds.Height), this);

                    xOffset += width;
                }
                else
                {
                    float childWidth;
                    float childHeight;

                    child.GetSize(out childWidth, out childHeight);

                    if (child.HorizontalAlignment == EHorizontalAlignment.Stretch)
                    {
                        child.SetBounds(new RectangleF(ContentBounds.Left + xOffset, ContentBounds.Top, childWidth + extraWidth, ContentBounds.Height), this);

                        xOffset += childWidth + extraWidth;
                    }
                    else
                    {
                        child.SetBounds(new RectangleF(ContentBounds.Left + xOffset, ContentBounds.Top, childWidth, ContentBounds.Height), this);

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
