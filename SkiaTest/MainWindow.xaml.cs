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
        Dock dock;
        SkiaLayout ui = new SkiaLayout();
        TextBlock text;
        HorizontalStack stack;

        public MainWindow()
        {
            InitializeComponent();

            dock = new Dock()
            {
                BackgroundPaint = new SKPaint { Color = SKColors.Yellow, Style = SKPaintStyle.Stroke, StrokeWidth = 1 },
                BackgroundRoundRadius = new SKSize(5, 5),
                Margin = new LayoutPadding(5),
                Padding = new LayoutPadding(10)
            };

            ui.RootUIElement = dock;

            stack = new HorizontalStack
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

            dock.Children.Add(text = new TextBlock
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
                //Padding = new LayoutPadding(10)
            });

            SkiaCanvas.SetLayout(ui);
        }

        bool toggle;

        private void SkiaCanvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            stack.BackgroundColor = toggle ? SKColors.Blue : SKColors.Green;
            stack.UpdateContentLayout();

            toggle = !toggle;
        }

        private void SkiaCanvas_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }
}
