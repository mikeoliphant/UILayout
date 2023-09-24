using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace UILayout
{
    public class SwipeList : UIElement
    {
        public UIFont Font { get; set; }
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

        //        public override void HandleInput(PixInputManager inputManager)
        //        {
        //            base.HandleInput(inputManager);

        //#if SHARPDX
        //            int delta = inputManager.GetMouseWheelDelta();

        //            if (delta > 0)
        //            {
        //                PreviousItem();
        //            }
        //            else if (delta < 0)
        //            {
        //                NextItem();
        //            }
        //#endif
        //        }

        public override bool HandleTouch(in Touch touch)
        {
            if ((touch.TouchState == ETouchState.Pressed) || (touch.TouchState == ETouchState.Moved) || (touch.TouchState == ETouchState.Held))
            {
                haveTouch = true;

                touchItem = (int)Math.Floor(((touch.Position.Y - ContentBounds.Y) + offset) / ItemHeight);
            }

            return base.HandleTouch(touch);
        }

        //public override bool HandleGesture(PixGesture gesture)
        //{
        //    int itemPos = (int)Math.Floor(((gesture.Position.Y - contentLayout.Offset.Y) + offset) / ItemHeight);

        //    switch (gesture.GestureType)
        //    {
        //        case EPixGestureType.Tap:
        //            if ((SelectAction != null) && (items != null))
        //            {
        //                if (itemPos < items.Count)
        //                    SelectAction(itemPos);
        //            }
        //            break;
        //        case EPixGestureType.Hold:
        //            if ((HoldAction != null) && (itemPos < items.Count))
        //            {
        //                HoldAction(itemPos);
        //            }

        //            break;
        //        case EPixGestureType.Drag:
        //            offset -= gesture.Delta.Y;
        //            break;

        //        case EPixGestureType.Flick:
        //            velocity = -gesture.Delta.Y;
        //            break;
        //    }

        //    return true;
        //}

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
