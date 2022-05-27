using System;
using System.Text;

namespace UILayout
{
    public struct RectF
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Top { get { return Y; } }
        public float Bottom { get { return Y + Height; } }
        public float Left { get { return X; } }
        public float Right { get { return X + Width; } }
        public bool IsEmpty { get { return (Width == 0) || (Height == 0); } }

        public static RectF Empty { get { return new RectF(0, 0, 0, 0); } }

        public RectF(float x, float y, float width, float height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RectF))
                return false;

            return (this.X == ((RectF)obj).X) && (this.Y == ((RectF)obj).Y) && (this.Width == ((RectF)obj).Width) && (this.Height == ((RectF)obj).Height);
        }

        public bool Contains(float x, float y)
        {
            return (x >= X) && (y >= Y) && (x < Right) && (y < Bottom);
        }

        public bool Contains(PointF point)
        {
            return (point.X >= X) && (point.Y >= Y) && (point.X < Right) && (point.Y < Bottom);
        }

        public void UnionWith(RectF other)
        {
            float maxX = Math.Max(Right, other.Right);
            float maxY = Math.Max(Bottom, other.Bottom);

            X = Math.Min(X, other.X);
            Y = Math.Min(Y, other.Y);
            Width = maxX - X;
            Height = maxY - Y;
        }
    }
}
