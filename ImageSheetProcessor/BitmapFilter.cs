using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageSheetProcessor
{
    public class BitmapFilter
    {
        protected float[,] convolution;

        protected float[,] Calculate1DSampleKernel(float deviation, int size)
        {
            float[,] ret = new float[size, 1];
            float sum = 0;
            int half = size / 2;

            for (int i = 0; i < size; i++)
            {
                ret[i, 0] = (float)(1 / (Math.Sqrt(2 * Math.PI) * deviation) * Math.Exp(-(i - half) * (i - half) / (2 * deviation * deviation)));
                sum += ret[i, 0];
            }

            return ret;
        }


        public void Apply(Bitmap sourceImage, Bitmap destImage)
        {
            int size = convolution.GetLength(0) / 2;
            float weight = 0;
            float r = 0;
            float g = 0;
            float b = 0;
            float a = 0;

            BitmapData sourceData = sourceImage.LockBits(new Rectangle(0, 0, sourceImage.Width, sourceImage.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData destData = destImage.LockBits(new Rectangle(0, 0, sourceImage.Width, sourceImage.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* srcPixels = (byte*)sourceData.Scan0;
                byte* destPixels = (byte*)destData.Scan0;

                for (int y = 0; y < sourceImage.Height; y++)
                {
                    for (int x = 0; x < sourceImage.Width; x++)
                    {
                        weight = 0;
                        b = 0;
                        g = 0;
                        r = 0;
                        a = 0;

                        for (int convY = Math.Max(0, y - size); convY <= Math.Min(sourceImage.Height - 1, y + size); convY++)
                        {
                            for (int convX = Math.Max(0, x - size); convX <= Math.Min(sourceImage.Width - 1, x + size); convX++)
                            {

                                //float aWeight = (float)c.A / 255.0f;
                                byte* offset = srcPixels + (convY * sourceData.Stride) + (convX * 4);

                                float convWeight = convolution[convX - x + size, convY - y + size];

                                b += (float)*offset++ * convWeight;

                                g += (float)*offset++ * convWeight;// *aWeight;
                                r += (float)*offset++ * convWeight;// *aWeight;
                                a += (float)*offset++ * convWeight;// *aWeight;

                                weight += convWeight;
                            }
                        }

                        byte* destOffset = destPixels + (y * sourceData.Stride) + (x * 4);

                        *destOffset++ = (byte)(b / weight);
                        *destOffset++ = (byte)(g / weight);
                        *destOffset++ = (byte)(r / weight);
                        *destOffset++ = (byte)(a / weight);
                    }
                }
            }

            sourceImage.UnlockBits(sourceData);
            destImage.UnlockBits(destData);
        }
    }

    public class GaussianBlur : BitmapFilter
    {
        float sigma;
        float sqrSigma;

        public GaussianBlur(int size, float deviation)
        {
            sigma = deviation;
            sqrSigma = sigma * sigma;

            convolution = GaussianKernel2D(size);
        }

        private float Gaussian2D(float x, float y)
        {
            return (float)(Math.Exp((x * x + y * y) / (-2 * sqrSigma)) / (2 * Math.PI * sqrSigma));
        }

        private float[,] GaussianKernel2D(int size)
        {
            // check for evem size and for out of range
            if (((size % 2) == 0) || (size < 3) || (size > 101))
            {
                throw new ArgumentException();
            }

            // raduis
            int r = size / 2;
            // kernel
            float[,] kernel = new float[size, size];

            // compute kernel
            for (int y = -r, i = 0; i < size; y++, i++)
            {
                for (int x = -r, j = 0; j < size; x++, j++)
                {
                    kernel[i, j] = Gaussian2D(x, y);
                }
            }

            return kernel;
        }
    }
}
