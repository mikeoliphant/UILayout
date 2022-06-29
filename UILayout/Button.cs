using System;
using System.Collections.Generic;
using System.Text;

namespace UILayout
{
    public partial class Button : UIElement
    {
        bool down = false;

        public void Toggle()
        {
            down = !down;

            BackgroundColor = down ? Color.Green : Color.Red;

            UpdateContentLayout();
        }

        public override bool HandleTouch(ref Touch touch)
        {
            if (touch.TouchState == ETouchState.Released)
            {
                if (down)
                {
                    Toggle();
                }
            }
            else
            {
                if (!down)
                {
                    Toggle();
                }
            }

            return true;
        }
    }
}
