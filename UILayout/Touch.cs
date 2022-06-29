using System;

namespace UILayout
{
    public enum ETouchState
    {
        Pressed,
        Moved,
        Held,
        Released,
        Invalid
    }

    public struct Touch
    {
        public PointF Position;
        public ETouchState TouchState;
        public int TouchID;
    }
}
