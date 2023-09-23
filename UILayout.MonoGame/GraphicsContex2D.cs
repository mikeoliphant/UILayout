using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UILayout
{
    public class GraphicsContext2D
    {
        public Image SingleWhitePixelImage { get; set; }

        Rectangle emptySrc = new Rectangle(0, 0, 0, 0);

        public virtual void DrawImage(Image image, float x, float y, float depth)
        {
            DrawImage(image, x, y, depth, Color.White, 1.0f, SpriteEffects.None);
        }

        public virtual void DrawImage(Image image, float x, float y, float depth, Color color)
        {
            DrawImage(image, x, y, depth, color, 1.0f, SpriteEffects.None);
        }

        public virtual void DrawImage(Image image, float x, float y, float depth, Color color, float scale)
        {
            DrawImage(image, x, y, depth, color, scale, SpriteEffects.None);
        }

        public virtual void DrawImage(Image image, float x, float y, float depth, ref Rectangle srcRect)
        {
            DrawImage(image, x, y, depth, ref srcRect, Color.White, 1.0f, SpriteEffects.None);
        }

        public virtual void DrawImage(Image image, float x, float y, float depth, ref Rectangle srcRect, Color color)
        {
            DrawImage(image, x, y, depth, ref srcRect, color, 1.0f, SpriteEffects.None);
        }

        public virtual void DrawImage(Image image, float x, float y, float depth, ref Rectangle srcRect, Color color, float scale)
        {
            DrawImage(image, x, y, depth, ref srcRect, color, scale, SpriteEffects.None);
        }

        public virtual void DrawImage(Image image, float depth, ref Rectangle srcRect, ref Rectangle destRect)
        {
            DrawImage(image, depth, ref srcRect, ref destRect, Color.White, SpriteEffects.None);
        }

        public virtual void DrawImage(Image image, float depth, ref Rectangle srcRect, ref Rectangle destRect, Color color)
        {
            DrawImage(image, depth, ref srcRect, ref destRect, color, SpriteEffects.None);
        }

        public virtual void DrawImage(Image image, float x, float y, float depth, Color color, float scale, SpriteEffects spriteEffects)
        {
            throw new NotImplementedException();
        }

        public virtual void DrawImage(Image image, float x, float y, float depth, ref Rectangle srcRect, Color color, float scale, SpriteEffects spriteEffects)
        {
            throw new NotImplementedException();
        }

        public virtual void DrawImage(Image image, float depth, ref Rectangle srcRect, ref Rectangle destRect, Color color, SpriteEffects spriteEffect)
        {
            throw new NotImplementedException();
        }

        public virtual void DrawRectangle(ref Rectangle destRect, float depth, Color color)
        {
            DrawImage(SingleWhitePixelImage, depth, ref emptySrc, ref destRect, color);
        }

        public virtual void DrawText(String text, SpriteFont font, float x, float y, float depth, Color color, float scale)
        {
            font.DrawString(text, this, x, y, depth, color, scale);
        }

        public virtual void DrawText(StringBuilder text, SpriteFont font, float x, float y, float depth, Color color, float scale)
        {
            font.DrawString(text, this, x, y, depth, color, scale);
        }
    }
}
