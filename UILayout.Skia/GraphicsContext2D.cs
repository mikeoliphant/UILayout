using System;
using System.Text;
using SkiaSharp;

namespace UILayout
{
    public class GraphicsContext2D
    {
        public SKCanvas Canvas { get; set; }

        SKPaint colorPaint = new SKPaint()
        {
        };

        SKPaint textPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.StrokeAndFill
        };


        public void BeginDraw()
        {
        }

        public void EndDraw()
        {
        }

        public void DrawImage(UIImage image, float x, float y)
        {
            Canvas.DrawBitmap(image.Bitmap, x, y);
        }

        public void DrawImage(UIImage image, float x, float y, UIColor color)
        {
            Canvas.DrawBitmap(image.Bitmap, x, y);
        }

        public void DrawImage(UIImage image, float x, float y, UIColor color, float scale)
        {
            colorPaint.Color = color.NativeColor;

            Canvas.DrawBitmap(image.Bitmap, x, y, colorPaint);
        }

        public void DrawImage(UIImage image, float x, float y, in System.Drawing.Rectangle srcRectangle)
        {
            Canvas.DrawBitmap(image.Bitmap, new SKRect(srcRectangle.X, srcRectangle.Y, srcRectangle.Right, srcRectangle.Bottom), new SKRect((int)x, (int)y, (int)x + image.Width, (int)y + image.Height));
        }

        public void DrawImage(UIImage image, float x, float y, in System.Drawing.Rectangle srcRectangle, UIColor color, float scale)
        {
        }

        public void DrawImage(UIImage image, in System.Drawing.Rectangle srcRectangle, in RectF destRectangle)
        {
            Canvas.DrawBitmap(image.Bitmap, new SKRect(srcRectangle.X, srcRectangle.Y, srcRectangle.Right, srcRectangle.Bottom), new SKRect((int)destRectangle.X, (int)destRectangle.Y, (int)destRectangle.Right, (int)destRectangle.Bottom));
        }

        public void DrawRectangle(in RectF rectangle, UIColor color)
        {
            colorPaint.Color = color.NativeColor;

            Canvas.DrawRect(new SKRect(rectangle.X, rectangle.Y, rectangle.Right, rectangle.Bottom), colorPaint);
        }

        public void DrawText(String text, Font font, float x, float y, UIColor color)
        {
            textPaint.Color = color.NativeColor;
            textPaint.Typeface = font.Typeface;
            textPaint.TextSize = font.TextSize;

            Canvas.DrawText(text, x, y + font.TextHeight, textPaint);
        }

        public void DrawText(String text, Font font, float x, float y, UIColor color, float scale)
        {
        }

        public void DrawText(StringBuilder text, Font font, float x, float y, UIColor color)
        {
            textPaint.Color = color.NativeColor;
            textPaint.Typeface = font.Typeface;
            textPaint.TextSize = font.TextSize;

            Canvas.DrawText(text.ToString(), x, y + font.TextHeight, textPaint);
        }

        public void DrawText(StringBuilder text, Font font, float x, float y, UIColor color, float scale)
        {
        }
    }
}
