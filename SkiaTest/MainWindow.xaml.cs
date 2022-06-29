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
        Spinner spinner;

        public MainWindow()
        {
            InitializeComponent();

            ui.RootUIElement = new LayoutTest();

            SkiaCanvas.SetLayout(ui);
        }
    }

    public class Spinner : UIElement
    {
        float offset = 0;
        int dx = 1;

        SKPaint paint = new SKPaint()
        {
            Color = SKColors.White
        };

        public void UpdateSpinner()
        {
            offset += dx;

            if (offset >= (int)ContentBounds.Width)
            {
                offset = (int)ContentBounds.Width - 1;
                dx = -1;
            }
            else if (offset < 0)
            {
                offset = 0;
                dx = 1;
            }

            UpdateContentLayout();
        }

        protected override void DrawContents()
        {
            SkiaLayout.Current.Canvas.DrawLine(ContentBounds.X + offset, 0, ContentBounds.Right - offset, ContentBounds.Bottom, paint);
        }
    }
}
