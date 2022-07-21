using System;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System.Windows.Threading;
using UILayout;
using UILayout.Test;

namespace SkiaTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        SkiaLayout ui = new SkiaLayout();

        public MainWindow()
        {
            InitializeComponent();

            ui.RootUIElement = new LayoutTest();

            SkiaCanvas.SetLayout(ui);
        }
    }
}
