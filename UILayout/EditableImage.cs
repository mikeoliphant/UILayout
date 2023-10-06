using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace UILayout
{
    public struct LineSegment
    {
        public Vector2 Start;
        public Vector2 End;

        public LineSegment(in Vector2 start, in Vector2 end)
        {
            this.Start = start;
            this.End = end;
        }
    }

    public class UICanvas2D<T>
    {
        int pointsInDraw;
        Point lastDrawPoint;
        Point lastLastDrawPoint;
        T lastColor;
        T lastLastColor;
        protected T penColor;

        protected virtual bool CanDraw
        {
            get { return true; }
        }

        public virtual Rectangle ImageRectangle
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual int ImageWidth
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual int ImageHeight
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public static Vector2 GetPointOnCircle(in Vector2 centerPoint, float radius, float angle)
        {
            return centerPoint + MathUtil.GetAngleUnitVector(angle) * radius;
        }

        public bool IsInBounds(int x, int y)
        {
            return ImageRectangle.Contains(x, y);
        }

        public virtual T GetPixel(int x, int y)
        {
            throw new NotImplementedException();
        }

        public virtual void SetPen(in T pen)
        {
            this.penColor = pen;
        }

        public virtual bool SetPixel(int x, int y)
        {
            return SetPixel(x, y, penColor);
        }

        public virtual bool SetPixel(int x, int y, in T color)
        {
            throw new NotImplementedException();
        }

        public virtual void Clear(in T color)
        {
            SetPen(color);

            for (int x = 0; x < ImageWidth; x++)
            {
                for (int y = 0; y < ImageHeight; y++)
                {
                    SetPixel(x, y);
                }
            }
        }

        public virtual void DrawScanLine(int startX, int endX, int y, in T color)
        {
            for (int x = startX; x <= endX; x++)
            {
                SetPixel(x, y, color);
            }
        }

        public virtual void DrawScanLine(int startX, int endX, int y)
        {
            for (int x = startX; x <= endX; x++)
            {
                SetPixel(x, y);
            }
        }

        public virtual void DrawScanLine(int startX, int endX, int y, in T color, in T oldColor)
        {
            DrawScanLine(startX, endX, y, color);
        }

        public void DrawRectangle(in Rectangle rectangle, in T color, bool fill)
        {
            SetPen(color);

            for (int y = 0; y < rectangle.Height; y++)
            {
                if (fill || (y == 0) || (y == (rectangle.Height - 1)))
                {
                    DrawScanLine(rectangle.X, rectangle.Right - 1, rectangle.Y + y);
                }
                else
                {
                    SetPixel(rectangle.X, rectangle.Y + y);
                    SetPixel(rectangle.Right - 1, rectangle.Y + y);
                }
            }
        }

        public void DrawCircle(int xc, int yc, int r, in T color, bool fill)
        {
            DrawEllipse(xc, yc, r, r, color, fill);
        }

        // From "There Is No Royal Road to Programs, A Trilogy on Raster Ellipses and Programming Methodology by M. Douglas McIlroy
        public void DrawEllipse(int xc, int yc, int a, int b, in T color, bool fill)
        {
            int x = 0, y = b;
            long a2 = (long)a * a, b2 = (long)b * b;
            long crit1 = -(a2 / 4 + a % 2 + b2);
            long crit2 = -(b2 / 4 + b % 2 + a2);
            long crit3 = -(b2 / 4 + b % 2);
            long t = -a2 * y; /* t = e(x+1/2,y-1/2) - (aˆ2+bˆ2)/4 */
            long dxt = 2 * b2 * x, dyt = -2 * a2 * y;
            long d2xt = 2 * b2, d2yt = 2 * a2;

            while (y >= 0 && x <= a)
            {
                if (fill)
                {
                    DrawScanLine(xc - x, xc + x, yc - y, color);
                    DrawScanLine(xc - x, xc + x, yc + y, color);
                }
                else
                {
                    SetPixel(xc + x, yc + y, color);

                    if (x != 0 || y != 0)
                    {
                        SetPixel(xc - x, yc - y, color);
                    }

                    if (x != 0 && y != 0)
                    {
                        SetPixel(xc + x, yc - y, color);
                        SetPixel(xc - x, yc + y, color);
                    }
                }

                if (t + b2 * x <= crit1 || /* e(x+1,y-1/2) <= 0 */
                    t + a2 * y <= crit3) /* e(x+1/2,y) <= 0 */
                {
                    x++; dxt += d2xt; t += dxt;
                }
                else if (t - a2 * y > crit2) /* e(x+1/2,y-1) > 0 */
                {
                    y--; dyt += d2yt; t += dyt;
                }
                else
                {
                    x++; dxt += d2xt; t += dxt;
                    y--; dyt += d2yt; t += dyt;
                }
            }
        }

        struct FillScan
        {
            public UInt16 X1;
            public UInt16 X2;
            public UInt16 Y;
            public bool? IsDown;
            public bool ExtendLeft;
            public bool ExtendRight;
        }

        public void Fill(in Vector2 fillPoint, in T fillColor)
        {
            Fill((int)fillPoint.X, (int)fillPoint.Y, fillColor);
        }

        public void Fill(int x, int y, T fillColor)
        {
            if (!IsInBounds(x, y))
                return;

            T oldColor = GetPixel(x, y);

            if (oldColor.Equals(fillColor))
                return;

            Stack<FillScan> fillStack = new Stack<FillScan>();

            fillStack.Push(new FillScan { X1 = (UInt16)x, X2 = (UInt16)x, Y = (UInt16)y, IsDown = null, ExtendLeft = true, ExtendRight = true });

            SetPixel(x, y, fillColor);

            UInt16 minX = 0;
            UInt16 maxX = 0;
            UInt16 drawY = 0;
            FillScan r = new FillScan();

            Action<UInt16, bool, bool> AddNextLine = delegate (UInt16 newY, bool isNext, bool downwards)
            {
                UInt16 rMinX = minX;
                bool inRange = false;

                UInt16 addX = minX;

                int startDrawX = -1;
                int endDrawX = -1;

                for (; addX <= maxX; addX++)
                {
                    // skip testing, if testing previous line within previous range
                    //bool empty = (isNext || ((addX < r.X1) || (addX > r.X2))) && (image.GetPixel(addX, newY) == oldColor);
                    bool empty = GetPixel(addX, newY).Equals(oldColor);

                    if (!inRange && empty)
                    {
                        rMinX = addX;
                        inRange = true;
                    }
                    else if (inRange && !empty)
                    {
                        if (startDrawX != -1)
                        {
                            DrawScanLine(startDrawX, endDrawX, newY, fillColor, oldColor);
                        }

                        startDrawX = endDrawX = -1;

                        fillStack.Push(new FillScan { X1 = rMinX, X2 = (UInt16)(addX - 1), Y = newY, IsDown = downwards, ExtendLeft = (rMinX == minX), ExtendRight = false });
                        inRange = false;
                    }

                    if (inRange)
                    {
                        if (startDrawX == -1)
                        {
                            startDrawX = addX;
                            endDrawX = addX;
                        }
                        else
                        {
                            endDrawX = addX;
                        }
                    }

                    // skip
                    //if (!isNext && (addX == r.X1))
                    //{
                    //    addX = r.X2;
                    //}
                }

                if (inRange)
                {
                    if (startDrawX != -1)
                    {
                        DrawScanLine(startDrawX, endDrawX, newY, fillColor, oldColor);
                    }

                    startDrawX = endDrawX = -1;

                    fillStack.Push(new FillScan { X1 = rMinX, X2 = (UInt16)(addX - 1), Y = newY, IsDown = downwards, ExtendLeft = (rMinX == minX), ExtendRight = true });
                }

                if (startDrawX != -1)
                {
                    DrawScanLine(startDrawX, endDrawX, newY, fillColor, oldColor);
                }
            };

            while (fillStack.Count > 0)
            {
                r = fillStack.Pop();

                bool down = r.IsDown ?? true;
                bool up = !r.IsDown ?? true;

                // extendLeft
                minX = r.X1;
                drawY = r.Y;

                int startDrawX = -1;
                int endDrawX = -1;

                if (r.ExtendLeft)
                {
                    while ((minX > ImageRectangle.X) && GetPixel(minX - 1, drawY).Equals(oldColor))
                    {
                        minX--;

                        if (endDrawX == -1)
                        {
                            endDrawX = minX;
                            startDrawX = minX;
                        }
                        else
                        {
                            startDrawX = minX;
                        }
                    }
                }

                if (startDrawX != -1)
                {
                    DrawScanLine(startDrawX, endDrawX, drawY, fillColor, oldColor);
                }

                startDrawX = endDrawX = -1;

                // extendRight
                maxX = r.X2;

                if (r.ExtendRight)
                {
                    while ((maxX < (ImageRectangle.Right - 1)) && GetPixel(maxX + 1, drawY).Equals(oldColor))
                    {
                        maxX++;

                        if (startDrawX == -1)
                        {
                            startDrawX = maxX;
                            endDrawX = maxX;
                        }
                        else
                        {
                            endDrawX = maxX;
                        }
                    }
                }

                if (startDrawX != -1)
                {
                    DrawScanLine(startDrawX, endDrawX, drawY, fillColor, oldColor);
                }

                // extend range ignored from previous line
                r.X1--;
                r.X2++;

                if (drawY < (ImageRectangle.Bottom - 1))
                    AddNextLine((UInt16)(drawY + 1), !up, true);

                if (drawY > ImageRectangle.Y)
                    AddNextLine((UInt16)(drawY - 1), !down, false);
            }
        }

        public void FillAll(int fx, int fy, in T fillColor)
        {
            T existingColor = GetPixel(fx, fy);

            int startX = -1;
            int endX = -1;

            for (int y = ImageRectangle.Y; y < ImageRectangle.Bottom; y++)
            {
                for (int x = ImageRectangle.X; x < ImageRectangle.Right; x++)
                {
                    if (GetPixel(x, y).Equals(existingColor))
                    {
                        if (startX == -1)
                        {
                            startX = x;
                        }

                        endX = x;
                    }
                    else
                    {
                        if (startX != -1)
                        {
                            DrawScanLine(startX, endX, y, fillColor, existingColor);

                            startX = -1;
                        }
                    }
                }

                if (startX != -1)
                {
                    DrawScanLine(startX, endX, y, fillColor, existingColor);

                    startX = -1;
                }
            }
        }

        public void DrawLine(in Vector2 start, in Vector2 end, in T drawColor)
        {
            AddPoint(new Point((int)start.X, (int)start.Y), drawColor);

            foreach (Point p in RenderLine(start, end))
            {
                AddPoint(p, drawColor);
            }

            AddPoint(new Point((int)end.X, (int)end.Y), drawColor);
        }

        public void DrawLine(Vector2 start, Vector2 end, Action<Point> drawAction)
        {
            drawAction(new Point((int)start.X, (int)start.Y));

            RenderLine(start, end, drawAction);

            drawAction(new Point((int)end.X, (int)end.Y));
        }

        public void DrawLines(IEnumerable<LineSegment> lines, in T drawColor)
        {
            foreach (LineSegment line in lines)
            {
                DrawLine(line.Start, line.End, drawColor);
            }
        }

        public static IEnumerable<Point> RenderLine(Vector2 begin, Vector2 end)
        {
            Point last = new Point((int)begin.X, (int)begin.Y);

            Vector2 pF = begin;
            Vector2 delta = Vector2.Normalize(end - begin);
            delta *= 0.1f;

            if (delta.Length() == 0)
            {
                yield return last;
                yield break;
            }
            else
            {
                while (Vector2.Distance(pF, begin) <= Vector2.Distance(end, begin))
                {
                    Point p = new Point((int)pF.X, (int)pF.Y);

                    if (p != last)
                    {
                        yield return p;

                        last = p;
                    }

                    pF += delta;
                }
            }
        }

        public static void RenderLine(in Vector2 begin, in Vector2 end, Action<Point> drawAction)
        {
            Point last = new Point((int)begin.X, (int)begin.Y);

            Vector2 pF = begin;
            Vector2 delta = Vector2.Normalize(end - begin);
            delta *= 0.1f;

            if (delta.Length() == 0)
            {
                drawAction(last);

                return;
            }
            else
            {
                while (Vector2.Distance(pF, begin) <= Vector2.Distance(end, begin))
                {
                    Point p = new Point((int)pF.X, (int)pF.Y);

                    if (p != last)
                    {
                        drawAction(p);

                        last = p;
                    }

                    pF += delta;
                }
            }
        }

        void AddPoint(in Point p, T color)
        {
            if (p == lastDrawPoint)
                return;

            pointsInDraw++;

            if (pointsInDraw > 2)
            {
                if ((Math.Abs(p.X - lastLastDrawPoint.X) == 1) &&
                    (Math.Abs(p.Y - lastLastDrawPoint.Y) == 1))
                {
                    SetPixel(lastDrawPoint.X, lastDrawPoint.Y, lastColor);

                    lastDrawPoint = lastLastDrawPoint;
                    lastColor = lastLastColor;
                    pointsInDraw = 1;
                }
            }

            lastLastDrawPoint = lastDrawPoint;
            lastLastColor = lastColor;
            lastDrawPoint = p;
            lastColor = GetPixel(p.X, p.Y);

            SetPixel(p.X, p.Y, color);
        }
    }

    public class EditableImage : UICanvas2D<UIColor>
    {
        public UIImage Image { get; private set; }

        public override int ImageWidth { get { return Image.Width; } }
        public override int ImageHeight { get { return Image.Height; } }
        public override Rectangle ImageRectangle { get { return new Rectangle(0, 0, Image.Width, Image.Height); } }

        UIColor penColor = UIColor.White;

        UIColor[] imageData = null;

        public EditableImage()
        {
        }

        public EditableImage(UIImage image)
        {
            this.Image = image;

            imageData = image.GetData();
        }

        //public EditableImage(int width, int height)
        //{
        //}

        //public EditableImage(int width, int height, bool createTexture)
        //{
        //    CreateImage(width, height, createTexture);
        //}

        //public virtual void CreateImage(int width, int height, bool createTexture)
        //{
        //    Image = createTexture ? new UIImage(width, height) : new DummyImage(width, height, cacheData: true);
        //}

        public override void Clear(in UIColor color)
        {
            for (int y = 0; y < Image.Height; y++)
            {
                DrawScanLine(Image.XOffset, Image.XOffset + Image.Width, y + Image.YOffset, color);
            }
        }

        public override void SetPen(in UIColor pen)
        {
            this.penColor = pen;
        }

        public override bool SetPixel(int x, int y, in UIColor color)
        {
            if (!IsInBounds(x, y))
                return false;

            imageData[x + Image.XOffset + (y + Image.YOffset) * Image.ActualWidth] = color;

            return true;
        }

        public override bool SetPixel(int x, int y)
        {
            if (!IsInBounds(x, y))
                return false;

            imageData[x + Image.XOffset + (y + Image.YOffset) * Image.ActualWidth] = penColor;

            return true;
        }

        public override UIColor GetPixel(int x, int y)
        {
            if (!IsInBounds(x, y))
                return UIColor.Transparent;

            return imageData[x + Image.XOffset + (y + Image.YOffset) * Image.ActualWidth];
        }

        public void UpdateImageData()
        {
            Image.SetData(imageData);
        }
    }

}
