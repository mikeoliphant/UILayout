﻿using System;

namespace UILayout
{
    public struct RectF
    {
        private float x;
        private float y;
        private float width;
        private float height;

        public float X { get => x; set => x = value; }
        public float Y { get => y; set => y = value; }
        public float Width { get => width; set => width = value; }
        public float Height { get => height; set => height = value; }
        public float Top { get { return y; } }
        public float Bottom { get { return y + height; } }
        public float Left { get { return x; } }
        public float Right { get { return x + width; } }
        public bool IsEmpty { get { return (width == 0) || (height == 0); } }

        public static RectF Empty { get { return new RectF(0, 0, 0, 0); } }

        public RectF(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RectF))
                return false;

            return (this.x == ((RectF)obj).x) && (this.y == ((RectF)obj).y) && (this.width == ((RectF)obj).width) && (this.height == ((RectF)obj).height);
        }

        public PointF GetCenter()
        {
            return new PointF(x + (width / 2), y + (height / 2));
        }

        public void MakeEmpty()
        {
            x = 0;
            y = 0;
            width = 0;
            height = 0;
        }

        public void Copy(ref RectF other)
        {
            x = other.x;
            y = other.y;
            width = other.width;
            height = other.height;
        }

        public bool Contains(float x, float y)
        {
            return (x >= this.x) && (y >= this.y) && (x < Right) && (y < Bottom);
        }

        public bool Contains(ref PointF point)
        {
            return (point.X >= x) && (point.Y >= y) && (point.X < Right) && (point.Y < Bottom);
        }

        public bool Intersects(ref RectF other)
        {
            return (other.x < Right) &&
               (x < other.Right) &&
               (other.y < Bottom) &&
               (y < other.Bottom);
        }

        public void UnionWith(ref RectF other)
        {
            float maxX = Math.Max(Right, other.Right);
            float maxY = Math.Max(Bottom, other.Bottom);

            x = Math.Min(x, other.x);
            y = Math.Min(y, other.y);
            width = maxX - x;
            height = maxY - y;
        }

        public void IntersectWith(ref RectF other)
        {
            if (Intersects(ref other))
            {
                float right = Math.Min(x + width, other.x + other.width);
                float left = Math.Max(x, other.x);
                float top = Math.Max(y, other.y);
                float bottom = Math.Min(y + height, other.y + other.height);

                x = left;
                y = top;
                width = right - left;
                height = bottom - top;
            }
            else
            {
                MakeEmpty();
            }
        }

        public override string ToString()
        {
            return x + "," + y + " (" + width + "x" + height + ")";
        }
    }
}
