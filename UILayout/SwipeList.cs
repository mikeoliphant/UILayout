using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace UILayout
{
    public class SwipeList : UIElement
    {
        public UIFont Font { get; set; } = UIFont.DefaultFont;
        public float FontScale { get; set; } = 1.0f;
        public UIColor TextColor { get; set; } = UIColor.White;
        public UIColor HighlightColor { get; set; } = new UIColor(200, 200, 200, 255);

        public Action<int> SelectAction { get; set; }
        public Action<int> HoldAction { get; set; }
        public float ItemHeight { get; set; }
        public float ItemXOffset { get; set; }
        public float ItemYOffset { get; set; }

        IList items;

        float offset = 0;
        float velocity;
        protected StringBuilder sb = new StringBuilder();
        bool haveTouch = false;
        int touchItem;
        int firstVisibleItem;
        int lastVisibleItem;

        public IList Items
        {
            get { return items; }
            set
            {
                items = value;
                offset = 0;
            }
        }

        public SwipeList()
        {
            TextColor = UIColor.White;

            ItemXOffset = 10;
        }

        public override void UpdateContentLayout()
        {
            base.UpdateContentLayout();

            ItemHeight = (Font.TextHeight * FontScale) * 1.1f;
            ItemYOffset = (ItemHeight - (Font.TextHeight * FontScale)) / 2;
        }

        protected override void DrawContents()
        {
            base.DrawContents();

            firstVisibleItem = -1;
            lastVisibleItem = -1;

            if (items == null)
                return;

            if (offset < 0)
                offset = 0;

            float maxOffset = (items.Count * ItemHeight) - ContentBounds.Height;

            if (maxOffset < 0)
            {
                maxOffset = 0;
            }

            if (offset > maxOffset)
            {
                offset = maxOffset;
            }


            int item = (int)Math.Floor(offset / ItemHeight);
            float itemOffset = (item * ItemHeight) - offset;

            float y = ContentBounds.Y;

            while ((y + itemOffset) < ContentBounds.Bottom)
            {
                if (item > (items.Count - 1))
                    break;

                if (item >= 0)
                {
                    if (firstVisibleItem == -1)
                        firstVisibleItem = item;

                    lastVisibleItem = item;

                    DrawItem(item, y + itemOffset);
                }

                y += ItemHeight;
                item++;
            }

            haveTouch = false;
        }

        protected virtual void DrawItem(int item, float y)
        {
            if (haveTouch && (item == touchItem))
            {
                Layout.Current.GraphicsContext.DrawRectangle(new RectF(ContentBounds.X, y, ContentBounds.Width, ItemHeight), HighlightColor);
            }

            DrawItemContents(item, ContentBounds.X + ItemXOffset, y + ItemYOffset);
        }

        public virtual void GetItemText(int item, StringBuilder stringBuilder)
        {
            sb.Append(items[item].ToString());
        }

        public virtual void DrawItemContents(int item, float x, float y)
        {
            sb.Remove(0, sb.Length);

            GetItemText(item, sb);

            Layout.Current.GraphicsContext.DrawText(sb, Font, (int)x, (int)y, TextColor, FontScale);
        }

        float dragStartY = 0;
        float lastDragY = 0;
        float dragStartOffset = 0;
        float totDrag = 0;

        public override bool HandleTouch(in Touch touch)
        {
            int itemPos = (int)Math.Floor(((touch.Position.Y - ContentBounds.Y) + offset) / ItemHeight);

            if ((touch.TouchState == ETouchState.Pressed) || (touch.TouchState == ETouchState.Moved) || (touch.TouchState == ETouchState.Held))
            {
                haveTouch = true;

                touchItem = itemPos;
            }

            if ((touch.TouchState == ETouchState.Pressed))
            {
                dragStartY = lastDragY = touch.Position.Y;
                dragStartOffset = offset;
                totDrag = 0;
            }

            if ((touch.TouchState == ETouchState.Moved))
            {
                float diff = touch.Position.Y - dragStartY;

                offset = dragStartOffset - diff;

                totDrag += Math.Abs(touch.Position.Y - lastDragY);

                lastDragY = touch.Position.Y;
            }

            if ((touch.TouchState == ETouchState.Released))
            {
                if (totDrag < 5)
                {
                    if (itemPos < items.Count)
                        SelectAction(itemPos);
                }
            }

            return true;
        }

        public override void HandleInput(InputManager inputManager)
        {
            base.HandleInput(inputManager);

            int delta = inputManager.MouseWheelDelta;

            if (delta > 0)
            {
                PreviousItem();
            }
            else if (delta < 0)
            {
                NextItem();
            }
        }

        public void NextItem()
        {
            if (Items != null)
            {
                offset += (ItemHeight * 1.1f);

                EnforceEvenItemBounds();
            }
        }

        public void PreviousItem()
        {
            if (Items != null)
            {
                offset -= (ItemHeight * 0.9f);

                EnforceEvenItemBounds();
            }
        }

        public void NextPage()
        {
            if (Items != null)
            {
                offset += ContentBounds.Height;

                EnforceEvenItemBounds();
            }
        }

        public void PreviousPage()
        {
            if (items != null)
            {
                offset -= ContentBounds.Height;

                EnforceEvenItemBounds();
            }
        }

        public void FirstItem()
        {
            offset = 0;
        }

        public void LastItem()
        {
            if (Items != null)
            {
                offset = (Items.Count * (ItemHeight + 1)) - ContentBounds.Height;

                EnforceEvenItemBounds();
            }
        }

        void EnforceEvenItemBounds()
        {
            if (offset < 0)
            {
                offset = 0;
            }
            else
            {
                offset = (int)(offset / ItemHeight) * ItemHeight;

                int itemsDisplayed = (int)(ContentBounds.Height / ItemHeight);

                if ((int)(offset / ItemHeight) > (Items.Count - itemsDisplayed))
                {
                    offset = (Items.Count - itemsDisplayed) * ItemHeight;
                }
            }
        }
    }
}
