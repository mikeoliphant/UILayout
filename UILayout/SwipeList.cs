using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace UILayout
{
    public class SwipeList : UIElement
    {
        public Font Font { get; set; }
        public float FontScale { get; set; }
        public UIColor TextColor { get; set; }

        //public Image ItemOutlineUnpressed
        //{
        //    get { return unpressedNinePatch.Image; }
        //    set
        //    {
        //        if (value == null)
        //        {
        //            unpressedNinePatch = null;
        //        }
        //        else
        //        {
        //            unpressedNinePatch.Image = value;
        //        }
        //    }
        //}

        //public Image ItemOutlinePressed
        //{
        //    get { return pressedNinePatch.Image; }
        //    set { pressedNinePatch.Image = value; }
        //}

        public Action<int> SelectAction { get; set; }
        public Action<int> HoldAction { get; set; }
        public float ItemHeight { get; set; }
        public float ItemXOffset { get; set; }
        public float ItemYOffset { get; set; }

        //NinePatchDrawable unpressedNinePatch;
        //NinePatchDrawable pressedNinePatch;

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
            //unpressedNinePatch = new NinePatchDrawable(PixUI.DefaultUnpressedNinePatch);
            //pressedNinePatch = new NinePatchDrawable(PixUI.DefaultPressedNinePatch);

            TextColor = UIColor.White;

            ScaleItemHeightToFont();

            ItemXOffset = 10;
        }

        public void ScaleItemHeightToFont()
        {
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
//            NinePatchDrawable drawNinePatch = unpressedNinePatch;

//            if (haveTouch && (item == touchItem))
//            {
//                drawNinePatch = pressedNinePatch;
//            }

//            if (drawNinePatch != null)
//            {
//                drawNinePatch.X = contentLayout.Offset.X;
//                drawNinePatch.Y = y;
//                drawNinePatch.Width = ContentLayout.Width;
//                drawNinePatch.Height = ItemHeight;

//#if UNITY
//                drawNinePatch.Depth = ID;
//#else
//                drawNinePatch.Depth = PixUI.MidgroundDepth;
//#endif
//                drawNinePatch.Update(0);
//                drawNinePatch.SetScene(PixGame.Instance.UIScene);
//                drawNinePatch.Draw();
//            }

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

#if UNITY
            PixGame.Instance.UIGraphicsContext.DrawText(sb, Font, (int)x, (int)y, ID, TextColor, FontScale);
#else
            PixGame.Instance.UIGraphicsContext.DrawText(sb, Font, (int)x, (int)y, PixUI.MidgroundDepth, TextColor, FontScale);
#endif
        }

        public override void HandleInput(PixInputManager inputManager)
        {
            base.HandleInput(inputManager);

#if SHARPDX
            int delta = inputManager.GetMouseWheelDelta();

            if (delta > 0)
            {
                PreviousItem();
            }
            else if (delta < 0)
            {
                NextItem();
            }
#endif
        }

        public override bool HandleTouch(PixTouch touch)
        {
            if ((touch.TouchState == EPixTouchState.Pressed) || (touch.TouchState == EPixTouchState.Moved) || (touch.TouchState == EPixTouchState.Held))
            {
                haveTouch = true;

                touchItem = (int)Math.Floor(((touch.Position.Y - contentLayout.Offset.Y) + offset) / ItemHeight);
            }

            return base.HandleTouch(touch);
        }

        public override bool HandleGesture(PixGesture gesture)
        {
            int itemPos = (int)Math.Floor(((gesture.Position.Y - contentLayout.Offset.Y) + offset) / ItemHeight);

            switch (gesture.GestureType)
            {
                case EPixGestureType.Tap:
                    if ((SelectAction != null) && (items != null))
                    {
                        if (itemPos < items.Count)
                            SelectAction(itemPos);
                    }
                    break;
                case EPixGestureType.Hold:
                    if ((HoldAction != null) && (itemPos < items.Count))
                    {
                        HoldAction(itemPos);
                    }

                    break;
                case EPixGestureType.Drag:
                    offset -= gesture.Delta.Y;
                    break;

                case EPixGestureType.Flick:
                    velocity = -gesture.Delta.Y;
                    break;
            }

            return true;
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
                offset += ContentLayout.Height;

                EnforceEvenItemBounds();
            }
        }

        public void PreviousPage()
        {
            if (items != null)
            {
                offset -= ContentLayout.Height;

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
                offset = (Items.Count * (ItemHeight + 1)) - ContentLayout.Height;

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

                int itemsDisplayed = (int)(ContentLayout.Height / ItemHeight);

                if ((int)(offset / ItemHeight) > (Items.Count - itemsDisplayed))
                {
                    offset = (Items.Count - itemsDisplayed) * ItemHeight;
                }
            }
        }
    }
}
