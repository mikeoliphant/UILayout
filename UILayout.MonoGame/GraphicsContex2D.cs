using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UILayout
{
    public class GraphicsContext2D
    {
        Image singleWhitePixelImage = new Image("SingleWhitePixel");

        Rectangle emptySrc = new Rectangle(0, 0, 0, 0);

        public virtual void BeginDraw()
        {

        }

        public virtual void EndDraw()
        {

        }

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

        public virtual void DrawImage(Image image, float x, float y, float depth, in Rectangle srcRect)
        {
            DrawImage(image, x, y, depth, srcRect, Color.White, 1.0f, SpriteEffects.None);
        }

        public virtual void DrawImage(Image image, float x, float y, float depth, in Rectangle srcRect, Color color)
        {
            DrawImage(image, x, y, depth, srcRect, color, 1.0f, SpriteEffects.None);
        }

        public virtual void DrawImage(Image image, float x, float y, float depth, in Rectangle srcRect, Color color, float scale)
        {
            DrawImage(image, x, y, depth, srcRect, color, scale, SpriteEffects.None);
        }

        public virtual void DrawImage(Image image, float depth, in Rectangle srcRect, in Rectangle destRect)
        {
            DrawImage(image, depth, srcRect, destRect, Color.White, SpriteEffects.None);
        }

        public virtual void DrawImage(Image image, float depth, in Rectangle srcRect, in Rectangle destRect, Color color)
        {
            DrawImage(image, depth, srcRect, destRect, color, SpriteEffects.None);
        }

        public virtual void DrawImage(Image image, float x, float y, float depth, Color color, float scale, SpriteEffects spriteEffects)
        {
            throw new NotImplementedException();
        }

        public virtual void DrawImage(Image image, float x, float y, float depth, in Rectangle srcRect, Color color, float scale, SpriteEffects spriteEffects)
        {
            throw new NotImplementedException();
        }

        public virtual void DrawImage(Image image, float depth, in Rectangle srcRect, in Rectangle destRect, Color color, SpriteEffects spriteEffect)
        {
            throw new NotImplementedException();
        }

        public virtual void DrawRectangle(in Rectangle destRect, float depth, Color color)
        {
            DrawImage(singleWhitePixelImage, depth, emptySrc, destRect, color);
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

    public class MonoGameGraphicsContext2D : GraphicsContext2D
    {
        SpriteBatch spriteBatch;

        const float PiOver2 = (float)(Math.PI / 2);

        public override void BeginDraw()
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap);

            base.BeginDraw();
        }

        public override void EndDraw()
        {           
            base.EndDraw();

            spriteBatch.End();
        }

        public MonoGameGraphicsContext2D(SpriteBatch spriteBatch)
        {
            this.spriteBatch = spriteBatch;
        }

        public override void DrawImage(Image image, float x, float y, float depth, Color color)
        {
            spriteBatch.Draw(image.Texture, new Vector2(x, y), new Rectangle(image.XOffset, image.YOffset, image.Width, image.Height), color,
                0, Vector2.Zero, 1, SpriteEffects.None, depth);
        }

        public override void DrawImage(Image image, float x, float y, float depth, Color color, float scale)
        {
            spriteBatch.Draw(image.Texture, new Vector2(x, y), new Rectangle(image.XOffset, image.YOffset, image.Width, image.Height), color,
                0, Vector2.Zero, scale, SpriteEffects.None, depth);
        }

        public override void DrawImage(Image image, float x, float y, float depth, Color color, float scale, SpriteEffects spriteEffects)
        {
            spriteBatch.Draw(image.Texture, new Vector2(x, y), new Rectangle(image.XOffset, image.YOffset, image.Width, image.Height), color,
                0, Vector2.Zero, scale, spriteEffects, depth);
        }

        public override void DrawImage(Image image, float depth, in Microsoft.Xna.Framework.Rectangle srcRect, in Microsoft.Xna.Framework.Rectangle destRect, Color color)
        {
            spriteBatch.Draw(image.Texture, new Rectangle(destRect.X, destRect.Y, destRect.Width, destRect.Height), new Rectangle(srcRect.X + image.XOffset, srcRect.Y + image.YOffset, srcRect.Width, srcRect.Height), color,
                0, Vector2.Zero, SpriteEffects.None, depth);
        }

        public override void DrawImage(Image image, float depth, in Microsoft.Xna.Framework.Rectangle srcRect, in Microsoft.Xna.Framework.Rectangle destRect, Color color, SpriteEffects spriteEffects)
        {
            spriteBatch.Draw(image.Texture, new Rectangle(destRect.X, destRect.Y, destRect.Width, destRect.Height), new Rectangle(srcRect.X + image.XOffset, srcRect.Y + image.YOffset, srcRect.Width, srcRect.Height), color, 0,
                Vector2.Zero, spriteEffects, depth);
        }

        public override void DrawImage(Image image, float x, float y, float depth, in Microsoft.Xna.Framework.Rectangle srcRect, Color color)
        {
            spriteBatch.Draw(image.Texture, new Vector2(x, y), new Rectangle(srcRect.X + image.XOffset, srcRect.Y + image.YOffset, srcRect.Width, srcRect.Height),
                color, 0, Vector2.Zero, 1, SpriteEffects.None, depth);
        }

        int mod(int x, int m)
        {
            return (x % m + m) % m;
        }

        public override void DrawImage(Image image, float x, float y, float depth, in Microsoft.Xna.Framework.Rectangle srcRect, Color color, float scale)
        {
            spriteBatch.Draw(image.Texture, new Vector2(x, y), new Rectangle(srcRect.X + image.XOffset, srcRect.Y + image.YOffset, srcRect.Width, srcRect.Height),
                color, 0, Vector2.Zero, scale, SpriteEffects.None, depth);
        }

        public override void DrawImage(Image image, float x, float y, float depth, in Microsoft.Xna.Framework.Rectangle srcRect, Color color, float scale, SpriteEffects spriteEffects)
        {
            spriteBatch.Draw(image.Texture, new Vector2(x, y), new Rectangle(srcRect.X + image.XOffset, srcRect.Y + image.YOffset, srcRect.Width, srcRect.Height),
                color, 0, Vector2.Zero, scale, spriteEffects, depth);
        }

        public void DrawImage(Image image, float x, float y, float depth, in Microsoft.Xna.Framework.Rectangle srcRect, Color color, float rotation, Microsoft.Xna.Framework.Vector2 origin, float scale)
        {
            spriteBatch.Draw(image.Texture, new Vector2(x, y), new Rectangle(srcRect.X + image.XOffset, srcRect.Y + image.YOffset, srcRect.Width, srcRect.Height),
                color, PiOver2 - rotation, new Vector2(origin.X, origin.Y), scale, SpriteEffects.None, depth);
        }

        public void DrawImage(Image image, float x, float y, float depth, Color color, float rotation, Microsoft.Xna.Framework.Vector2 origin, float scale)
        {
            spriteBatch.Draw(image.Texture, new Vector2(x, y), new Rectangle(image.XOffset, image.YOffset, image.Width, image.Height), color,
                PiOver2 - rotation, new Vector2(origin.X, origin.Y), scale, SpriteEffects.None, depth);
        }
    }
}
