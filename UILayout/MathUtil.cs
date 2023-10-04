using System;
using System.Collections.Generic;
using System.Text;

namespace UILayout
{
    public static class MathUtil
    {
        public const float PiOver2 = (float)(Math.PI / 2.0);

        public static float Clamp(float val, float min, float max)
        {
            if (val < min)
                return min;

            if (val > max)
                return max;

            return val;
        }

        public static double Clamp(double val, double min, double max)
        {
            if (val < min)
                return min;

            if (val > max)
                return max;

            return val;
        }

        public static int Clamp(int val, int min, int max)
        {
            if (val < min)
                return min;

            if (val > max)
                return max;

            return val;
        }

        public static float Saturate(float val)
        {
            return Clamp(val, 0, 1);
        }

        public static double Lerp(double val1, double val2, double lerp)
        {
            return val1 + (lerp * (val2 - val1));
        }

        public static float Lerp(float val1, float val2, float lerp)
        {
            return val1 + (lerp * (val2 - val1));
        }
    }
}
