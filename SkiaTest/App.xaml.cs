using SkiaSharp;
using System.Windows;
using UILayout;
using UILayout.Skia.WPF;
using UILayout.Test;

namespace SkiaTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // The window fits the test layout (the layout control's size is the layout's)
            LayoutWindow layoutWindow = new LayoutWindow()
            {
                SizeToContent = SizeToContent.WidthAndHeight
            };

            SkiaLayout ui = new SkiaLayout();

            ui.RootUIElement = new LayoutTest();

            layoutWindow.SkiaCanvas.SetLayout(ui);

            layoutWindow.Show();
        }
    }
}
