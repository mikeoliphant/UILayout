using System;

namespace UILayout
{
    public static class Extensions
    {
        public static float Distance(this PointF p1, PointF p2)
        {
            return (float)Math.Sqrt(((p1.X - p2.X) + (p1.Y - p2.Y)) * ((p1.X - p2.X) + (p1.Y - p2.Y)));
        }
    }
}
