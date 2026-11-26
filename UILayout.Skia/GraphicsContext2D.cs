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

        SKPaint colorPaint = new SKPaint();

        // Text gives the paint only its color: the typeface, size and edging are the UIFont's SKFont.
        SKPaint textPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };

        // Images are filtered bilinearly when scaled, as the MonoGame backend's LinearClamp sampler does (an image drawn
        // 1:1 on whole pixels is the same either way).
        static readonly SKSamplingOptions imageSampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None);

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
            Canvas.DrawBitmap(image.Bitmap, Source(image), At(image, x, y, image.Width, image.Height, 1), imageSampling);
        }

        public void DrawImage(UIImage image, float x, float y, in UIColor color)
        {
            Canvas.DrawBitmap(image.Bitmap, Source(image), At(image, x, y, image.Width, image.Height, 1),
                imageSampling, TintPaint(color));
        }

        public void DrawImage(UIImage image, float x, float y, in UIColor color, float scale)
        {
            Canvas.DrawBitmap(image.Bitmap, Source(image), At(image, x, y, image.Width, image.Height, scale),
                imageSampling, TintPaint(color));
        }

        public void DrawImage(UIImage image, float x, float y, in Rectangle srcRectangle)
        {
            Canvas.DrawBitmap(image.Bitmap, Source(image, srcRectangle),
                At(image, x, y, srcRectangle.Width, srcRectangle.Height, 1), imageSampling);
        }

        public void DrawImage(UIImage image, float x, float y, in Rectangle srcRectangle, in UIColor color, float scale)
        {
            Canvas.DrawBitmap(image.Bitmap, Source(image, srcRectangle),
                At(image, x, y, srcRectangle.Width, srcRectangle.Height, scale), imageSampling, TintPaint(color));
        }

        public void DrawImage(UIImage image, in Rectangle srcRectangle, in RectF destRectangle)
        {
            Canvas.DrawBitmap(image.Bitmap, Source(image, srcRectangle), Into(destRectangle), imageSampling);
        }

        public void DrawImage(UIImage image, in Rectangle srcRectangle, in RectF destRectangle, in UIColor color)
        {
            Canvas.DrawBitmap(image.Bitmap, Source(image, srcRectangle), Into(destRectangle), imageSampling,
                TintPaint(color));
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

        // Where text drawn from y has its baseline: the font's ascent below, on a whole device pixel (as the MonoGame
        // backend snaps its draws), so text placed alike lines up exactly.
        float Baseline(float y, UIFont font)
        {
            SKMatrix matrix = Canvas.TotalMatrix;

            float scale = (matrix.ScaleY != 0) ? matrix.ScaleY : 1;
            float deviceY = MathF.Round(((y + font.Ascent) * scale) + matrix.TransY);

            return (deviceY - matrix.TransY) / scale;
        }

        public void DrawText(String text, UIFont font, float x, float y, in UIColor color)
        {
            DrawText(text.AsSpan(), font, x, y, color, 1);
        }

        public void DrawText(String text, UIFont font, float x, float y, in UIColor color, float scale)
        {
            DrawText(text.AsSpan(), font, x, y, color, scale);
        }

        public void DrawText(ReadOnlySpan<char> text, UIFont font, float x, float y, in UIColor color)
        {
            DrawText(text, font, x, y, color, 1);
        }

        /// <summary>
        /// Draws text from its top left, a line for each newline, as <see cref="UIFont.MeasureString(ReadOnlySpan{char},
        /// out float, out float)"/> measures it. A scale other than 1 scales it about (x, y).
        /// </summary>
        public void DrawText(ReadOnlySpan<char> text, UIFont font, float x, float y, in UIColor color, float scale)
        {
            if (text.IsEmpty)
                return;

            BeginText(color, scale, ref x, ref y);

            while (true)
            {
                int newLine = text.IndexOf('\n');
                ReadOnlySpan<char> line = (newLine >= 0) ? text.Slice(0, newLine) : text;

                DrawLine(line, font, x, y);

                if (newLine < 0)
                    break;

                y += font.TextHeight;
                text = text.Slice(newLine + 1);
            }

            EndText(scale);
        }

        public void DrawText(StringBuilder text, UIFont font, float x, float y, in UIColor color)
        {
            DrawText(text, font, x, y, color, 1);
        }

        /// <summary>
        /// Draws the text's chunks one after another on one line, as <see cref="UIFont.MeasureString(StringBuilder,
        /// out float, out float)"/> measures it (without copying the text).
        /// </summary>
        public void DrawText(StringBuilder text, UIFont font, float x, float y, in UIColor color, float scale)
        {
            if (text.Length == 0)
                return;

            BeginText(color, scale, ref x, ref y);

            foreach (ReadOnlyMemory<char> chunk in text.GetChunks())
            {
                DrawLine(chunk.Span, font, x, y);

                x += font.NativeFont.MeasureText(chunk.Span);
            }

            EndText(scale);
        }

        // Text is drawn in its color, and scaled about its position: drawn at the origin of a canvas translated there
        // and scaled (the baseline is still snapped in device pixels).
        void BeginText(in UIColor color, float scale, ref float x, ref float y)
        {
            textPaint.Color = color.NativeColor;

            if (scale == 1)
                return;

            Canvas.Save();
            Canvas.Translate(x, y);
            Canvas.Scale(scale);

            x = 0;
            y = 0;
        }

        void EndText(float scale)
        {
            if (scale != 1)
                Canvas.Restore();
        }

        // One line, as a text blob laid out with the font's SKFont. Skia gives no blob for empty text (an empty line or
        // text box): nothing to draw.
        void DrawLine(ReadOnlySpan<char> line, UIFont font, float x, float y)
        {
            if (line.IsEmpty)
                return;

            using SKTextBlob blob = SKTextBlob.Create(line, font.NativeFont);

            if (blob != null)
                Canvas.DrawText(blob, x, Baseline(y, font), textPaint);
        }
    }
}
