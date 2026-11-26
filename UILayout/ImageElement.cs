using System.Drawing;
using System.Numerics;

namespace UILayout
{
    public partial class ImageElement : UIElement
    {
        public UIImage Image { get; set; }

        UIColor color = UIColor.White;

        /// <summary>The color the image is tinted with (white: as it is). A changed color is redrawn.</summary>
        public UIColor Color
        {
            get { return color; }

            set
            {
                if (color == value)
                    return;

                color = value;

                Layout.Current.AddDirtyRect(LayoutBounds);
            }
        }

        public Rectangle? SourceRectangle { get; set; } = null;

        public ImageElement(string imageName)
            : this(Layout.Current.GetImage(imageName))
        {
        }

        public ImageElement(UIImage image)
        {
            Image = image;
        }

        protected override void GetContentSize(out float width, out float height)
        {
            width = 0;
            height = 0;

            if (Image != null)
            {
                if (SourceRectangle.HasValue)
                {
                    width = SourceRectangle.Value.Width / Image.Density;
                    height = SourceRectangle.Value.Height / Image.Density;
                }
                else
                {
                    width = Image.LogicalWidth;
                    height = Image.LogicalHeight;
                }
            }
        }

        protected override void DrawContents()
        {
            base.DrawContents();

            if (SourceRectangle.HasValue)
            {
                Layout.Current.GraphicsContext.DrawImage(Image, SourceRectangle.Value, new RectF(ContentBounds.X, ContentBounds.Y, ContentBounds.Width, ContentBounds.Height), Color);
            }
            else
            {
                Layout.Current.GraphicsContext.DrawImage(Image, new Rectangle(0, 0, Image.Width, Image.Height), new RectF(ContentBounds.X, ContentBounds.Y, ContentBounds.Width, ContentBounds.Height), Color);
            }
        }
    }

    public partial class RotatingImageElement : UIElement
    {
        public UIImage Image { get; set; }

        UIColor color = UIColor.White;

        /// <summary>The color the image is tinted with (white: as it is). A changed color is redrawn.</summary>
        public UIColor Color
        {
            get { return color; }

            set
            {
                if (color == value)
                    return;

                color = value;

                Layout.Current.AddDirtyRect(LayoutBounds);
            }
        }

        public float Rotation { get; set; } = 0;

        public RotatingImageElement(string imageName)
            : this(Layout.Current.GetImage(imageName))
        {
        }

        public RotatingImageElement(UIImage image)
        {
            Image = image;
        }

        protected override void GetContentSize(out float width, out float height)
        {
            width = 0;
            height = 0;

            if (Image != null)
            {
                width = Image.LogicalWidth;
                height = Image.LogicalHeight;
            }
        }

        protected override void DrawContents()
        {
            base.DrawContents();

            Layout.Current.GraphicsContext.DrawImage(Image, ContentBounds.CenterX, ContentBounds.CenterY, Color, Rotation, new Vector2(Image.Width / 2.0f, Image.Height / 2.0f), 1.0f);
        }
    }

    public partial class NinePatchWrapper : UIElementWrapper
    {
        UIImage image;

        public UIImage Image
        {
            get => image;

            set
            {
                image = value;

                Padding = new LayoutPadding(((image.Width / 2) - 1) / image.Density,
                    ((image.Height / 2) - 1) / image.Density);
                UpdateNintePatch();
            }
        }

        public bool DrawCenter { get; set; } = true;

        UIColor color = UIColor.White;

        /// <summary>The color the image is tinted with (white: as it is). A changed color is redrawn.</summary>
        public UIColor Color
        {
            get { return color; }

            set
            {
                if (color == value)
                    return;

                color = value;

                Layout.Current.AddDirtyRect(LayoutBounds);
            }
        }

        int[] imageWidths = new int[3];
        int[] imageHeights = new int[3];
        float[] destWidths = new float[3];
        float[] destHeights = new float[3];

        public NinePatchWrapper()
        {

        }

        public NinePatchWrapper(UIImage ninePatchImage)
            : this()
        {
            this.Image = ninePatchImage;
        }

        public override void UpdateContentLayout()
        {
            base.UpdateContentLayout();

            UpdateNintePatch();
        }

        void UpdateNintePatch()
        {
            if (Image != null)
            {
                imageWidths[0] = imageWidths[2] = (image.Width / 2) - 1;
                imageWidths[1] = 2;

                imageHeights[0] = imageHeights[2] = (image.Height / 2) - 1;
                imageHeights[1] = 2;

                // Corners at the image's density; the middle takes the rest
                destWidths[0] = destWidths[2] = imageWidths[0] / image.Density;
                destWidths[1] = (int)(layoutBounds.Width - (destWidths[0] + destWidths[2]));

                destHeights[0] = destHeights[2] = imageHeights[0] / image.Density;
                destHeights[1] = (int)(layoutBounds.Height - (destHeights[0] + destHeights[2]));
            }
        }

        protected override void DrawContents()
        {
            if (Image != null)
            {
                int srcOffsetY = 0;
                float destOffsetY = (int)layoutBounds.Y;

                for (int y = 0; y < 3; y++)
                {
                    // On whole pixels across as down (at a fractional position, as a centered popup's, a filtering
                    // backend would spread the edges over two pixels)
                    int srcOffsetX = 0;
                    float destOffsetX = (int)layoutBounds.X;

                    for (int x = 0; x < 3; x++)
                    {
                        if (DrawCenter || (x != 1) || (y != 1))
                            Layout.Current.GraphicsContext.DrawImage(Image, new System.Drawing.Rectangle(srcOffsetX, srcOffsetY, imageWidths[x], imageHeights[y]), new RectF(destOffsetX, destOffsetY, destWidths[x], destHeights[y]), Color);

                        srcOffsetX += imageWidths[x];
                        destOffsetX += destWidths[x];
                    }

                    srcOffsetY += imageHeights[y];
                    destOffsetY += destHeights[y];
                }
            }

            base.DrawContents();
        }
    }
}