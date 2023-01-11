namespace UILayout
{
    public struct PointF
    {
        private float x;
        private float y;

        public float X { get => x; set => x = value; }
        public float Y { get => y; set => y = value; }

        public PointF(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PointF))
                return false;

            return (this.x == ((PointF)obj).x) && (this.y == ((PointF)obj).y);
        }

        public override string ToString()
        {
            return x + "," + y;
        }
    }
}
