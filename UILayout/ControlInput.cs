using System;

namespace UILayout
{
    /// <summary>
    /// Input settings shared by controls for a value from 0 to 1 (<see cref="Slider"/>, and AudioPlugSharp's knobs):
    /// fine adjustment while Shift is held, the mouse wheel's step, snapping to steps.
    /// </summary>
    public static class ControlInput
    {
        /// <summary>How much slower a drag or the wheel moves the value while Shift is held.</summary>
        public static float FineScale { get; set; } = 0.1f;

        /// <summary>How far one notch of the mouse wheel moves the value (0.02 is 2% of the range).</summary>
        public static float WheelStep { get; set; } = 0.02f;

        /// <summary>
        /// Whether Shift is held (fine adjustment). The layout sees key events while it has keyboard focus.
        /// </summary>
        public static bool IsFine
        {
            get
            {
                InputManager inputManager = Layout.Current.InputManager;

                return inputManager.IsKeyDown(InputKey.LeftShift) || inputManager.IsKeyDown(InputKey.RightShift);
            }
        }

        /// <summary>
        /// The value after the wheel turned by the delta (120 per notch, positive away from the user). With steps, a
        /// notch is one step.
        /// </summary>
        public static double WheelValue(double value, int delta, int stepCount = 0)
        {
            if (stepCount > 0)
            {
                int notches = (delta > 0) ? Math.Max(1, delta / 120) : Math.Min(-1, delta / 120);

                return Snap(value + ((double)notches / stepCount), stepCount);
            }

            float step = WheelStep * (IsFine ? FineScale : 1);

            return Math.Clamp(value + ((delta / 120.0) * step), 0, 1);
        }

        /// <summary>The value at its nearest step (0, 1 / stepCount, ... 1), or just kept in 0-1 without steps.</summary>
        public static double Snap(double value, int stepCount)
        {
            value = Math.Clamp(value, 0, 1);

            if (stepCount <= 0)
                return value;

            return Math.Round(value * stepCount) / stepCount;
        }
    }
}
