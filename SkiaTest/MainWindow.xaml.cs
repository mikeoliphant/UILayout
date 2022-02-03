using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System.Drawing;
using UILayout;

namespace SkiaTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKCanvas canvas = e.Surface.Canvas;

            float scale = (float)System.Windows.PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;
            SKSize scaledSize = new SKSize(e.Info.Width / scale, e.Info.Height / scale);

            canvas.Scale(scale);

            UIElement.Canvas = canvas;

            Dock dock = new Dock()
            {
                BackgroundColor = SKColors.Yellow,
                Margin = new LayoutPadding(5),
                Padding = new LayoutPadding(10)
            };

            HorizontalStack stack = new HorizontalStack
            {
                BackgroundColor = SKColors.Blue,
                BackgroundRoundRadius = new SKSize(10, 10),
                HorizontalAlignment = EHorizontalAlignment.Right,
                VerticalAlignment = EVerticalAlignment.Top,
                Padding = new LayoutPadding(10),
                DesiredHeight = 100,
                DesiredWidth = 100,
                ChildSpacing = 10                
            };
            dock.Children.Add(stack);

            for (int i = 0; i < 5; i++)
            {
                stack.Children.Add(new UIElement
                {
                    BackgroundColor = SKColors.Red,
                    HorizontalAlignment = EHorizontalAlignment.Stretch,
                    VerticalAlignment = EVerticalAlignment.Stretch,
                    //Margin = new LayoutPadding(10)
                });
            }

            dock.Children.Add(new TextBlock
            {
                Text = "Hello World",
                TextPaint = new SKPaint
                {
                    TextSize = 24,
                    Color = SKColors.Black,
                    IsAntialias = true,
                    Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
                },
                BackgroundColor = SKColors.Green,
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Bottom,
                Padding = new LayoutPadding(10)
            });

            dock.SetBounds(new RectangleF(0, 0, scaledSize.Width, scaledSize.Height), null);

            dock.Draw();
        }
    }
}
