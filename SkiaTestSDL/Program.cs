using SDL2;
using UILayout;
using UILayout.Test;

namespace SkiaTestSDL
{
    internal class Program
    {
        static LayoutWindow window;

        static void Main(string[] args)
        {
            window = new LayoutWindow("SkiaTestSDL", 800, 600);

            SkiaLayout ui = new SkiaLayout();

            ui.RootUIElement = new LayoutTest();

            window.SetLayout(ui);

            // The window fits the test layout
            window.ResizeToLayout();

            window.Run();
        }
    }
}
