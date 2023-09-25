﻿using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static System.Net.Mime.MediaTypeNames;

namespace UILayout
{
    public class GraphicsContext2D
    {
        SpriteBatch spriteBatch;

        public UIImage SingleWhitePixelImage { get; set; }

        public GraphicsContext2D(SpriteBatch spriteBatch)
        {
            this.spriteBatch = spriteBatch;
        }

        public void BeginDraw()
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap);
        }

        public void EndDraw()
        {
            spriteBatch.End();
        }

        public void DrawImage(UIImage image, float x, float y)
        {
            spriteBatch.Draw(image.Texture, new Vector2(x, y), new Rectangle(image.XOffset, image.YOffset, image.Width, image.Height), Color.White);
        }

        public void DrawImage(UIImage image, float x, float y, UIColor color)
        {
            spriteBatch.Draw(image.Texture, new Vector2(x, y), new Rectangle(image.XOffset, image.YOffset, image.Width, image.Height), color.NativeColor);
        }

        public void DrawImage(UIImage image, float x, float y, UIColor color, float scale)
        {
            spriteBatch.Draw(image.Texture, new Vector2(x, y), new Rectangle(image.XOffset, image.YOffset, image.Width, image.Height), color.NativeColor, 0, Vector2.One, scale, SpriteEffects.None, 0);
        }

        public void DrawImage(UIImage image, float x, float y, in System.Drawing.Rectangle srcRectangle)
        {
            spriteBatch.Draw(image.Texture, new Vector2(x, y), new Rectangle(srcRectangle.X + image.XOffset, srcRectangle.Y + image.YOffset, srcRectangle.Width, srcRectangle.Height), Color.White);
        }

        public void DrawImage(UIImage image, float x, float y, in System.Drawing.Rectangle srcRectangle, UIColor color)
        {
            spriteBatch.Draw(image.Texture, new Vector2(x, y), new Rectangle(srcRectangle.X + image.XOffset, srcRectangle.Y + image.YOffset, srcRectangle.Width, srcRectangle.Height), color.NativeColor);
        }

        public void DrawImage(UIImage image, float x, float y, in System.Drawing.Rectangle srcRectangle, UIColor color, float scale)
        {
            spriteBatch.Draw(image.Texture, new Vector2(x, y), new Rectangle(srcRectangle.X + image.XOffset, srcRectangle.Y + image.YOffset, srcRectangle.Width, srcRectangle.Height), color.NativeColor, 0, Vector2.One, scale, SpriteEffects.None, 0);
        }

        public void DrawImage(UIImage image, in System.Drawing.Rectangle srcRectangle, in RectF destRectangle)
        {
            spriteBatch.Draw(image.Texture, new Rectangle((int)destRectangle.X, (int)destRectangle.Y, (int)destRectangle.Width, (int)destRectangle.Height),
                new Rectangle(srcRectangle.X + image.XOffset, srcRectangle.Y + image.YOffset, srcRectangle.Width, srcRectangle.Height), Color.White);
        }

        public void DrawRectangle(in RectF rectangle, UIColor color)
        {
            spriteBatch.Draw(SingleWhitePixelImage.Texture, new Rectangle((int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height),
                new Rectangle(SingleWhitePixelImage.XOffset, SingleWhitePixelImage.YOffset, SingleWhitePixelImage.Width, SingleWhitePixelImage.Height), color.NativeColor);
        }
        public void DrawText(String text, UIFont font, float x, float y, UIColor color)
        {
            font.SpriteFont.DrawString(text, this, x, y, color, 1.0f);
        }
        public void DrawText(String text, UIFont font, float x, float y, UIColor color, float scale)
        {
            font.SpriteFont.DrawString(text, this, x, y, color, scale);
        }
        public void DrawText(StringBuilder text, UIFont font, float x, float y, UIColor color)
        {
            font.SpriteFont.DrawString(text, this, x, y, color, 1.0f);
        }

        public void DrawText(StringBuilder text, UIFont font, float x, float y, UIColor color, float scale)
        {
            font.SpriteFont.DrawString(text, this, x, y, color, scale);
        }
    }
}
