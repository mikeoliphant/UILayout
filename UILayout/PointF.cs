using System;
using System.Text;

namespace UILayout
{
    public struct PointF
    {
        public float X { get; set; }
        public float Y { get; set; }

        public PointF(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PointF))
                return false;

            return (this.X == ((PointF)obj).X) && (this.Y == ((PointF)obj).Y);
        }
    }
}
