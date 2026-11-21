using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UILayout
{
    public class GraphicsContext2D
    {
        SpriteBatch spriteBatch;

        public UIImage SingleWhitePixelImage { get; set; }
        public float Scale { get; set; } = 1.0f;
        public SamplerState SamplerState { get; set; } = SamplerState.PointClamp;
        public BlendState BlendState { get; set; } = BlendState.AlphaBlend;

        public GraphicsContext2D(SpriteBatch spriteBatch)
        {
            this.spriteBatch = spriteBatch;
        }

        public void BeginDraw()
        {
            clips.Clear();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState, SamplerState, null, null, null, Matrix.CreateScale(Scale));
        }

        public void EndDraw()
        {
            spriteBatch.End();
        }

        // ---- clipping -----------------------------------------------------------------------------------------------

        // The clip rectangles pushed (layout units), each already cut to the one before. Draws are cut to the last one
        // here, rather than with a scissor rectangle, which would mean ending and restarting the sprite batch at every
        // change. Rotated draws aren't cut.
        List<RectF> clips = new List<RectF>();

        /// <summary>
        /// Cuts what is drawn from now on to the rectangle (within the current clip), until <see cref="PopClip"/>.
        /// </summary>
        public void PushClip(in RectF rectangle)
        {
            RectF clip = rectangle;

            if (clips.Count > 0)
            {
                RectF outer = clips[clips.Count - 1];

                float left = Math.Max(clip.X, outer.X);
                float top = Math.Max(clip.Y, outer.Y);
                float right = Math.Min(clip.Right, outer.Right);
                float bottom = Math.Min(clip.Bottom, outer.Bottom);

                clip = new RectF(left, top, Math.Max(0, right - left), Math.Max(0, bottom - top));
            }

            clips.Add(clip);
        }

        public void PopClip()
        {
            if (clips.Count > 0)
                clips.RemoveAt(clips.Count - 1);
        }

        // Cuts a draw to the clip: destination (layout units) shows source (texture pixels), and both shrink together,
        // by whole source pixels. Stretched sources at most 2 pixels across (a nine-patch's middle, a rectangle's single
        // pixel) look the same all over: they just draw smaller. False: nothing is left to draw.
        bool Clip(ref RectF destination, ref Rectangle source)
        {
            RectF clip = clips[clips.Count - 1];

            float x = destination.X;
            float width = destination.Width;
            int sourceX = source.X;
            int sourceWidth = source.Width;

            if (!ClipAxis(ref x, ref width, ref sourceX, ref sourceWidth, clip.X, clip.Right))
                return false;

            float y = destination.Y;
            float height = destination.Height;
            int sourceY = source.Y;
            int sourceHeight = source.Height;

            if (!ClipAxis(ref y, ref height, ref sourceY, ref sourceHeight, clip.Y, clip.Bottom))
                return false;

            destination = new RectF(x, y, width, height);
            source = new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight);

            return true;
        }

        static bool ClipAxis(ref float start, ref float size, ref int sourceStart, ref int sourceSize, float min,
            float max)
        {
            float end = start + size;

            if ((start >= min) && (end <= max))
                return true;

            if ((end <= min) || (start >= max) || (sourceSize <= 0))
                return false;

            if ((sourceSize <= 2) && (size > sourceSize))
            {
                start = Math.Max(start, min);
                size = Math.Min(end, max) - start;

                return size > 0;
            }

            // Whole source pixels cut from each end: at least as much as lies outside.
            float pixel = size / sourceSize;
            int cutStart = (start < min) ? (int)MathF.Ceiling(((min - start) / pixel) - 0.001f) : 0;
            int cutEnd = (end > max) ? (int)MathF.Ceiling(((end - max) / pixel) - 0.001f) : 0;
            int remaining = sourceSize - cutStart - cutEnd;

            if (remaining <= 0)
                return false;

            sourceStart += cutStart;
            sourceSize = remaining;
            start += cutStart * pixel;
            size = remaining * pixel;

            return true;
        }

        // A draw at a position and scale, its origin in source pixels (as SpriteBatch takes them).
        void DrawAt(Texture2D texture, float x, float y, Rectangle source, Color color, float scale, Vector2 origin)
        {
            if (clips.Count == 0)
            {
                spriteBatch.Draw(texture, new Vector2(x, y), source, color, 0, origin, scale, SpriteEffects.None, 0);

                return;
            }

            // The drawn rectangle: the origin's source pixel is at (x, y).
            RectF destination = new RectF(x - (origin.X * scale), y - (origin.Y * scale), source.Width * scale,
                source.Height * scale);

            if (!Clip(ref destination, ref source))
                return;

            Vector2 position = new Vector2(destination.X, destination.Y);
            Vector2 drawScale = new Vector2(destination.Width / source.Width, destination.Height / source.Height);

            spriteBatch.Draw(texture, position, source, color, 0, Vector2.Zero, drawScale, SpriteEffects.None, 0);
        }

        // A draw into a rectangle (whole layout units).
        void DrawInto(Texture2D texture, in RectF rectangle, Rectangle source, Color color)
        {
            Rectangle destination = new Rectangle((int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width,
                (int)rectangle.Height);

            if (clips.Count > 0)
            {
                RectF cut = new RectF(destination.X, destination.Y, destination.Width, destination.Height);

                if (!Clip(ref cut, ref source))
                    return;

                // Edges rounded on their own, so pieces cut at the same edge meet.
                int left = (int)MathF.Round(cut.X);
                int top = (int)MathF.Round(cut.Y);

                destination = new Rectangle(left, top, (int)MathF.Round(cut.Right) - left,
                    (int)MathF.Round(cut.Bottom) - top);
            }

            spriteBatch.Draw(texture, destination, source, color);
        }

        // ---- drawing ------------------------------------------------------------------------------------------------

        static Rectangle Source(UIImage image)
        {
            return new Rectangle(image.XOffset, image.YOffset, image.Width, image.Height);
        }

        static Rectangle Source(UIImage image, in System.Drawing.Rectangle srcRectangle)
        {
            return new Rectangle(srcRectangle.X + image.XOffset, srcRectangle.Y + image.YOffset, srcRectangle.Width,
                srcRectangle.Height);
        }

        // Drawn at a position: at the image's logical size (its pixels divided by its density), times any scale.

        public void DrawImage(UIImage image, float x, float y)
        {
            DrawAt(image.Texture, x, y, Source(image), Color.White, 1 / image.Density, Vector2.Zero);
        }

        public void DrawImage(UIImage image, float x, float y, in UIColor color)
        {
            DrawAt(image.Texture, x, y, Source(image), color.NativeColor, 1 / image.Density, Vector2.Zero);
        }

        public void DrawImage(UIImage image, float x, float y, in UIColor color, float scale)
        {
            DrawAt(image.Texture, x, y, Source(image), color.NativeColor, scale / image.Density,
                Vector2.One);
        }

        public void DrawImage(UIImage image, float x, float y, in System.Drawing.Rectangle srcRectangle)
        {
            DrawAt(image.Texture, x, y, Source(image, srcRectangle), Color.White, 1 / image.Density,
                Vector2.Zero);
        }

        public void DrawImage(UIImage image, float x, float y, in System.Drawing.Rectangle srcRectangle, in UIColor color)
        {
            DrawAt(image.Texture, x, y, Source(image, srcRectangle), color.NativeColor, 1 / image.Density,
                Vector2.Zero);
        }

        public void DrawImage(UIImage image, float x, float y, in System.Drawing.Rectangle srcRectangle, in UIColor color,
            float scale)
        {
            DrawAt(image.Texture, x, y, Source(image, srcRectangle), color.NativeColor, scale / image.Density,
                Vector2.One);
        }

        public void DrawImage(UIImage image, in System.Drawing.Rectangle srcRectangle, in RectF destRectangle)
        {
            DrawInto(image.Texture, destRectangle, Source(image, srcRectangle), Color.White);
        }

        public void DrawImage(UIImage image, in System.Drawing.Rectangle srcRectangle, in RectF destRectangle, UIColor color)
        {
            DrawInto(image.Texture, destRectangle, Source(image, srcRectangle), color.NativeColor);
        }

        // Not clipped (rotated).
        public void DrawImage(UIImage image, float x, float y, in UIColor color, float rotation,
            in System.Numerics.Vector2 origin, float scale)
        {
            spriteBatch.Draw(image.Texture, new Vector2(x, y), Source(image), color.NativeColor, rotation,
                new Vector2(origin.X, origin.Y), scale / image.Density, SpriteEffects.None, 0);
        }

        public void DrawRectangle(in RectF rectangle, in UIColor color)
        {
            DrawInto(SingleWhitePixelImage.Texture, rectangle, Source(SingleWhitePixelImage), color.NativeColor);
        }

        public void DrawText(ReadOnlySpan<char> text, UIFont font, float x, float y, in UIColor color)
        {
            font.SpriteFont.DrawString(text, this, x, y, color, 1.0f);
        }

        public void DrawText(ReadOnlySpan<char> text, UIFont font, float x, float y, in UIColor color, float scale)
        {
            font.SpriteFont.DrawString(text, this, x, y, color, scale);
        }

        public void DrawText(String text, UIFont font, float x, float y, in UIColor color)
        {
            font.SpriteFont.DrawString(text, this, x, y, color, 1.0f);
        }
        public void DrawText(String text, UIFont font, float x, float y, in UIColor color, float scale)
        {
            font.SpriteFont.DrawString(text, this, x, y, color, scale);
        }

        public void DrawText(StringBuilder text, UIFont font, float x, float y, in UIColor color)
        {
            font.SpriteFont.DrawString(text, this, x, y, color, 1.0f);
        }

        public void DrawText(StringBuilder text, UIFont font, float x, float y, in UIColor color, float scale)
        {
            font.SpriteFont.DrawString(text, this, x, y, color, scale);
        }
    }
}
