using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace UILayout
{
    /// <summary>
    /// A list of text items that scrolls (mouse wheel, drag, or a <see cref="VerticalScrollBar"/>), drawn in the
    /// theme's font and text color. The item under the mouse and the selected one are highlighted. Put it in a
    /// <see cref="Frame"/> for a box around it.
    /// </summary>
    public class SwipeList : UIElement, IScrollable
    {
        public UIFont Font
        {
            get => font;

            set
            {
                font = value;

                UpdateItemHeight();
            }
        }

        public float FontScale
        {
            get => fontScale;
            set
            {
                fontScale = value;

                UpdateItemHeight();
            }
        }

        public UIColor TextColor { get; set; }

        /// <summary>Behind the selected item and the one being pressed (default: the theme's active color).</summary>
        public UIColor HighlightColor { get; set; }

        /// <summary>Behind the item under the mouse (default: the theme's active color, half transparent).</summary>
        public UIColor HoverColor { get; set; }
        public Action<int> SelectAction { get; set; }
        public Action<int> HoldAction { get; set; }
        public float ItemHeight { get; set; }
        public float ItemXOffset { get; set; }
        public float ItemYOffset { get; set; }
        int lastSelectedItem = -1;

        /// <summary>The selected item (highlighted), or -1. A change redraws the item it left and the one it's on.</summary>
        public int LastSelectedItem
        {
            get { return lastSelectedItem; }

            set
            {
                if (value == lastSelectedItem)
                    return;

                AddItemDirty(lastSelectedItem);

                lastSelectedItem = value;

                AddItemDirty(lastSelectedItem);
            }
        }

        public int CurrentTopItemIndex
        {
            get
            {
                return (int)(offset / ItemHeight);
            }
        }

        public int NumDisplayedItems
        {
            get
            {
                return (int)(ContentBounds.Height / ItemHeight);
            }
        }

        IList items;

        UIFont font = Layout.Current.Theme.Font;
        float fontScale = 1.0f;
        float offset = 0;
        float shownOffset = 0;
        protected StringBuilder sb = new StringBuilder();
        int touchItem;
        int firstVisibleItem;
        int lastVisibleItem;
        VerticalScrollBar scrollBar;

        public int ItemCount
        {
            get
            {
                if (items != null)
                {
                    return items.Count;
                }

                return 0;
            }
        }

        public IList Items
        {
            get { return items; }
            set
            {
                items = value;
                SetOffset(0);

                if (scrollBar != null)
                {
                    scrollBar.SetVisiblePercent((ContentBounds.Height / ItemHeight) / (float)ItemCount);
                }

                // Other items: redrawn (also when the offset was already at the top)
                Layout.Current.AddDirtyRect(ContentBounds);
            }
        }

        public SwipeList()
        {
            // Items scrolled partly out of view are cut at the edges.
            ClipToBounds = true;

            UITheme theme = Layout.Current.Theme;

            TextColor = theme.Text;
            HighlightColor = theme.Active;
            HoverColor = theme.Active * 0.5f;

            ItemXOffset = 10;

            UpdateItemHeight();
        }

        public void SetScrollBar(VerticalScrollBar scrollBar)
        {
            this.scrollBar = scrollBar;

            if (items != null)
            {
                scrollBar.SetVisiblePercent((ContentBounds.Height / ItemHeight) / (float)ItemCount);
            }
            else
            {
                scrollBar.SetVisiblePercent(1.0f);
            }

            scrollBar.Scrollable = this;
        }

        public void ScrollPageBackward()
        {
            PreviousPage();
        }

        public void ScrollPageForward()
        {
            NextPage();
        }

        public void SetScrollPercent(float scrollPercent)
        {
            if (Items != null)
            {
                SetOffset((float)ItemCount * scrollPercent * ItemHeight);
            }
        }

        void SetOffset(float newOffset)
        {
            this.offset = newOffset;

            if (offset < 0)
                offset = 0;

            float maxOffset = (ItemCount * ItemHeight) - ContentBounds.Height;

            if (maxOffset < 0)
            {
                maxOffset = 0;
            }

            if (offset > maxOffset)
            {
                offset = maxOffset;
            }

            // The scrolling functions move the offset and then settle it here: compared with where it was last shown
            if (offset == shownOffset)
                return;

            shownOffset = offset;

            if (scrollBar != null)
            {
                scrollBar.SetScrollPercent((offset / ItemHeight) / (float)ItemCount);
            }

            // Only the items move (drawn here: nothing to lay out)
            Layout.Current.AddDirtyRect(ContentBounds);
        }

        public void SetTopItem(int topItem)
        {
            SetOffset(topItem * ItemHeight);
        }

        void UpdateItemHeight()
        {
            ItemHeight = (Font.TextHeight * FontScale) * 1.1f;
            ItemYOffset = (ItemHeight - (Font.TextHeight * FontScale)) / 2;
        }

        public override void UpdateContentLayout()
        {
            base.UpdateContentLayout();

            if (scrollBar != null)
            {
                scrollBar.SetVisiblePercent((ContentBounds.Height / ItemHeight) / (float)ItemCount);
            }
        }

        protected override void DrawContents()
        {
            base.DrawContents();

            firstVisibleItem = -1;
            lastVisibleItem = -1;

            if (items == null)
                return;

            int item = (int)Math.Floor(offset / ItemHeight);
            float itemOffset = (item * ItemHeight) - offset;

            float y = ContentBounds.Y;

            while ((y + itemOffset) < ContentBounds.Bottom)
            {
                if (item > (ItemCount - 1))
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
        }

        // The item at a height (as drawn: scrolled by the offset)
        int ItemAt(float y)
        {
            return (int)Math.Floor(((y - ContentBounds.Y) + offset) / ItemHeight);
        }

        // The hover highlight follows the mouse from item to item: the item it left and the one it is over are redrawn
        public override void HandleMouseMoveOver(in Vector2 lastPosition, in Vector2 position)
        {
            if (HoverColor.A == 0)
                return;

            int lastItem = ItemAt(lastPosition.Y);
            int item = ItemAt(position.Y);

            if (item == lastItem)
                return;

            AddItemDirty(lastItem);
            AddItemDirty(item);
        }

        // Only the item under the mouse is highlighted: the one it came over, or the one it left, is redrawn
        public override void HandleMouseOverChanged(bool isOver, in Vector2 lastPosition, in Vector2 position)
        {
            if (HoverColor.A == 0)
                return;

            AddItemDirty(ItemAt(isOver ? position.Y : lastPosition.Y));
        }

        /// <summary>Redraws the item (the part of it showing; nothing when it's scrolled out).</summary>
        protected void AddItemDirty(int item)
        {
            float y = (ContentBounds.Y + (item * ItemHeight)) - offset;

            RectF itemRect = new RectF(ContentBounds.X, y, ContentBounds.Width, ItemHeight);

            // Only what shows (none of it, when it's scrolled out: an empty rectangle, which adds nothing)
            itemRect.IntersectWith(ContentBounds);

            Layout.Current.AddDirtyRect(itemRect);
        }

        protected virtual void DrawItem(int item, float y)
        {
            if (IsMouseOver && (HoverColor.A > 0))
            {
                float mouseY = Layout.Current.InputManager.MousePosition.Y;

                if ((mouseY >= y) && (mouseY < (y + ItemHeight)))
                    Layout.Current.GraphicsContext.DrawRectangle(new RectF(ContentBounds.X, y, ContentBounds.Width, ItemHeight), HoverColor);
            }

            if (HaveTouchCapture && (item == touchItem))
            {
                Layout.Current.GraphicsContext.DrawRectangle(new RectF(ContentBounds.X, y, ContentBounds.Width, ItemHeight), HighlightColor);
            }

            if (item == LastSelectedItem)
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
        float lastDragY = 0;
        float dragStartOffset = 0;

        public override bool HandleTouch(in Touch touch)
        {
            int itemPos = (int)Math.Floor(((touch.Position.Y - ContentBounds.Y) + offset) / ItemHeight);

            if ((touch.TouchState == ETouchState.Pressed) || (touch.TouchState == ETouchState.Moved) || (touch.TouchState == ETouchState.Held))
            {
                // The pressed item's highlight follows the touch (shown from the press)
                if ((itemPos != touchItem) || (touch.TouchState == ETouchState.Pressed))
                {
                    AddItemDirty(touchItem);
                    AddItemDirty(itemPos);
                }

                touchItem = itemPos;
            }

            if (touch.TouchState == ETouchState.Pressed)
            {
                CaptureTouch(touch);

                lastDragY = touch.Position.Y;
                dragStartOffset = offset;
            }

            if (touch.TouchState == ETouchState.Moved)
            {
                float diff = touch.Position.Y - TouchCaptureStartPosition.Y;

                // Redrawn if it scrolled
                SetOffset(dragStartOffset - diff);

                lastDragY = touch.Position.Y;
            }

            if ((touch.TouchState == ETouchState.Released) || (touch.TouchState == ETouchState.Invalid))
            {
                ReleaseTouch();

                // The pressed item's highlight goes
                AddItemDirty(touchItem);
            }

            if (IsTap(touch, this))
            {
                if (itemPos < ItemCount)
                {
                    SelectItem(itemPos);
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
                ScrollBackward();
            }
            else if (delta < 0)
            {
                ScrollForward();
            }
        }

        public void SelectItem(int itemPos)
        {
            LastSelectedItem = itemPos;

            if (SelectAction != null)
                SelectAction(itemPos);

            EnsureSelectedItemIsVisible();
        }

        public void NextItem()
        {
            if (Items != null)
            {
                if (LastSelectedItem == -1)
                {
                    ScrollForward();
                }
                else
                {
                    if (LastSelectedItem < (ItemCount - 1))
                    {
                        SelectItem(LastSelectedItem + 1);
                    }
                }
            }
        }

        public void PreviousItem()
        {
            if (Items != null)
            {
                if (LastSelectedItem == -1)
                {
                    ScrollBackward();
                }
                else
                {
                    if (LastSelectedItem > 0)
                    {
                        SelectItem(LastSelectedItem - 1);
                    }
                }
            }
        }

        public void ScrollBackward()
        {
            offset -= (ItemHeight * 0.9f);

            EnforceEvenItemBounds();
        }

        public void ScrollForward()
        {
            offset += (ItemHeight * 1.1f);

            EnforceEvenItemBounds();
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

        public void GoToFirstItem()
        {
            SetOffset(0);
        }

        public void GoToLastItem()
        {
            if (Items != null)
            {
                SetOffset((ItemCount * (ItemHeight + 1)) - ContentBounds.Height);

                EnforceEvenItemBounds();
            }
        }

        void EnsureSelectedItemIsVisible()
        {
            if (LastSelectedItem != -1)
            {
                float itemOffset = LastSelectedItem * ItemHeight;

                if (offset > itemOffset)
                    SetTopItem(LastSelectedItem);
                else
                {
                    float diff = itemOffset + ItemHeight - (offset + ContentBounds.Height);

                    if (diff > 0)
                    {
                        SetOffset(offset + diff);
                    }
                }
            }
        }

        void EnforceEvenItemBounds()
        {
            if (offset < 0)
            {
                SetOffset(0);
            }
            else
            {
                SetOffset((int)(offset / ItemHeight) * ItemHeight);

                int itemsDisplayed = (int)(ContentBounds.Height / ItemHeight);

                if ((int)(offset / ItemHeight) > (ItemCount - itemsDisplayed))
                {
                    SetOffset((ItemCount - itemsDisplayed) * ItemHeight);
                }
            }
        }
    }
}
