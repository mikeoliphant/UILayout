using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
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
            Style = SKPaintStyle.StrokeAndFill,            
        };

        SKFont textFont = new SKFont()
        {

        };

        // Images drawn with a color are tinted by it (image x color), as the MonoGame backend draws them: SkiaSharp
        // tints a bitmap only through a color filter (a paint's color gives it only its alpha). One filter per color,
        // kept.
        SKPaint tintPaint = new SKPaint();
        Dictionary<uint, SKColorFilter> tintFilters = new Dictionary<uint, SKColorFilter>();

        SKPaint TintPaint(in UIColor color)
        {
            SKColor nativeColor = color.NativeColor;

            if (!tintFilters.TryGetValue((uint)nativeColor, out SKColorFilter filter))
            {
                filter = SKColorFilter.CreateBlendMode(nativeColor, SKBlendMode.Modulate);
                tintFilters[(uint)nativeColor] = filter;
            }

            tintPaint.ColorFilter = filter;

            return tintPaint;
        }

        public void BeginDraw()
        {
            clipDepth = 0;
        }

        // Clip rectangles pushed: the canvas's own clipping, saved and restored.
        int clipDepth;

        /// <summary>
        /// Cuts what is drawn from now on to the rectangle (within the current clip), until <see cref="PopClip"/>.
        /// </summary>
        public void PushClip(in RectF rectangle)
        {
            Canvas.Save();
            Canvas.ClipRect(new SKRect(rectangle.X, rectangle.Y, rectangle.Right, rectangle.Bottom));

            clipDepth++;
        }

        public void PopClip()
        {
            if (clipDepth == 0)
                return;

            Canvas.Restore();

            clipDepth--;
        }

        public void EndDraw()
        {
        }

        // Source rectangles in the image's bitmap: an image can be part of a sheet (its offset), and an SKRect takes
        // edges, not a size.
        static SKRect Source(UIImage image)
        {
            return SKRect.Create(image.XOffset, image.YOffset, image.Width, image.Height);
        }

        static SKRect Source(UIImage image, in Rectangle srcRectangle)
        {
            return SKRect.Create(srcRectangle.X + image.XOffset, srcRectangle.Y + image.YOffset, srcRectangle.Width,
                srcRectangle.Height);
        }

        // Drawn at a position: at the logical size (pixels divided by the image's density), times any scale.
        static SKRect At(UIImage image, float x, float y, float width, float height, float scale)
        {
            return SKRect.Create((int)x, (int)y, (width / image.Density) * scale, (height / image.Density) * scale);
        }

        static SKRect Into(in RectF destRectangle)
        {
            return new SKRect((int)destRectangle.X, (int)destRectangle.Y, (int)destRectangle.Right,
                (int)destRectangle.Bottom);
        }

        public void DrawImage(UIImage image, float x, float y)
        {
            Canvas.DrawBitmap(image.Bitmap, Source(image), At(image, x, y, image.Width, image.Height, 1));
        }

        public void DrawImage(UIImage image, float x, float y, in UIColor color)
        {
            Canvas.DrawBitmap(image.Bitmap, Source(image), At(image, x, y, image.Width, image.Height, 1),
                TintPaint(color));
        }

        public void DrawImage(UIImage image, float x, float y, in UIColor color, float scale)
        {
            Canvas.DrawBitmap(image.Bitmap, Source(image), At(image, x, y, image.Width, image.Height, scale),
                TintPaint(color));
        }

        public void DrawImage(UIImage image, float x, float y, in Rectangle srcRectangle)
        {
            Canvas.DrawBitmap(image.Bitmap, Source(image, srcRectangle),
                At(image, x, y, srcRectangle.Width, srcRectangle.Height, 1));
        }

        public void DrawImage(UIImage image, float x, float y, in Rectangle srcRectangle, in UIColor color, float scale)
        {
            Canvas.DrawBitmap(image.Bitmap, Source(image, srcRectangle),
                At(image, x, y, srcRectangle.Width, srcRectangle.Height, scale), TintPaint(color));
        }

        public void DrawImage(UIImage image, in Rectangle srcRectangle, in RectF destRectangle)
        {
            Canvas.DrawBitmap(image.Bitmap, Source(image, srcRectangle), Into(destRectangle));
        }

        public void DrawImage(UIImage image, in Rectangle srcRectangle, in RectF destRectangle, in UIColor color)
        {
            Canvas.DrawBitmap(image.Bitmap, Source(image, srcRectangle), Into(destRectangle), TintPaint(color));
        }

        public void DrawImage(UIImage image, float x, float y, in UIColor color, float rotation, in Vector2 origin, float scale)
        {
            throw new NotImplementedException();
        }

        public void DrawRectangle(in RectF rectangle, in UIColor color)
        {
            colorPaint.Color = color.NativeColor;

            Canvas.DrawRect(new SKRect(rectangle.X, rectangle.Y, rectangle.Right, rectangle.Bottom), colorPaint);
        }

        public void DrawText(String text, UIFont font, float x, float y, in UIColor color)
        {
            textPaint.Color = color.NativeColor;
            textPaint.Typeface = font.Typeface;
            textPaint.TextSize = font.TextSize;

            Canvas.DrawText(text, x, y + font.TextHeight, textPaint);
        }

        public void DrawText(String text, UIFont font, float x, float y, in UIColor color, float scale)
        {
            textPaint.Color = color.NativeColor;
            textPaint.Typeface = font.Typeface;
            textPaint.TextSize = font.TextSize;

            Canvas.DrawText(text, x, y + font.TextHeight, textPaint);
        }

        public void DrawText(ReadOnlySpan<char> text, UIFont font, float x, float y, in UIColor color)
        {
            DrawTextBlob(text, font, x, y, color);
        }

        public void DrawText(ReadOnlySpan<char> text, UIFont font, float x, float y, in UIColor color, float scale)
        {
            DrawTextBlob(text, font, x, y, color);
        }

        // Spans are drawn as text blobs, laid out with textFont (its typeface and size too). Skia gives no blob for
        // empty text (an empty text box): nothing to draw.
        void DrawTextBlob(ReadOnlySpan<char> text, UIFont font, float x, float y, in UIColor color)
        {
            if (text.IsEmpty)
                return;

            textFont.Typeface = font.Typeface;
            textFont.Size = font.TextSize;

            textPaint.Color = color.NativeColor;
            textPaint.Typeface = font.Typeface;
            textPaint.TextSize = font.TextSize;

            using SKTextBlob blob = SKTextBlob.Create(text, textFont);

            if (blob != null)
                Canvas.DrawText(blob, x, y + font.TextHeight, textPaint);
        }

        public void DrawText(StringBuilder text, UIFont font, float x, float y, in UIColor color)
        {
            textPaint.Color = color.NativeColor;
            textPaint.Typeface = font.Typeface;
            textPaint.TextSize = font.TextSize;

            Canvas.DrawText(text.ToString(), x, y + font.TextHeight, textPaint);
        }

        public void DrawText(StringBuilder text, UIFont font, float x, float y, in UIColor color, float scale)
        {
            textPaint.Color = color.NativeColor;
            textPaint.Typeface = font.Typeface;
            textPaint.TextSize = font.TextSize;

            Canvas.DrawText(text.ToString(), x, y + font.TextHeight, textPaint);
        }
    }
}
