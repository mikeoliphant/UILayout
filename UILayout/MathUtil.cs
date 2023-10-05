using System;
using System.Collections.Generic;
using System.Text;

namespace UILayout
{
    public static class MathUtil
    {
        public const float PiOver2 = (float)(Math.PI / 2.0);

        public static float ToDegrees(float radians)
        {
            return (float)(radians * 57.295779513082320876798154814105);
        }

        public static float ToRadians(float degrees)
        {
            return (float)(degrees * 0.017453292519943295769236907684886);
        }

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
