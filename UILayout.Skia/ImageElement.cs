using System;
using SkiaSharp;

namespace UILayout
{
    public partial class ImageElement
    {
        protected override void DrawContents()
        {
            SkiaLayout.Current.Canvas.DrawBitmap(Image.Bitmap, ContentBounds.X, ContentBounds.Y);
        }
    }

    public partial class NinePatchWrapper : UIElementWrapper
    {
        protected override void DrawContents()
        {
            if (Image != null)
            {
                SKBitmap bitmap = Image.Bitmap;

                int srcOffsetY = 0;
                float destOffsetY = layoutBounds.Y;

                for (int y = 0; y < 3; y++)
                {
                    int srcOffsetX = 0;
                    float destOffsetX = layoutBounds.X;

                    for (int x = 0; x < 3; x++)
                    {
                        SkiaLayout.Current.Canvas.DrawBitmap(bitmap, SKRectI.Create(srcOffsetX, srcOffsetY, imageWidths[x], imageHeights[y]), SKRect.Create(destOffsetX, destOffsetY, destWidths[x], destHeights[y]));

                        srcOffsetX += imageWidths[x];
                        destOffsetX += destWidths[x];
                    }

                    srcOffsetY += imageHeights[y];
                    destOffsetY += destHeights[y];
                }
            }

            base.DrawContents();
        }
    }
}
