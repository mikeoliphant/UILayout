using System;

namespace UILayout
{
    public partial class ImageElement : UIElement
    {
        public Image Image
        {
            get; set;
        }

        protected override void GetContentSize(out float width, out float height)
        {
            width = 0;
            height = 0;

            if (Image != null)
            {
                width = Image.Width;
                height = Image.Height;

            }
        }
    }

    public partial class NinePatchWrapper : UIElementWrapper
    {
        Image image;

        public Image Image
        {
            get => image;

            set
            {
                image = value;

                UpdateNintePatch();
            }
        }

        int[] imageWidths = new int[3];
        int[] imageHeights = new int[3];
        float[] destWidths = new float[3];
        float[] destHeights = new float[3];

        public NinePatchWrapper()
        {

        }

        public NinePatchWrapper(Image ninePatchImage)
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
                Padding = new LayoutPadding(image.Width / 2, image.Height / 2);

                imageWidths[0] = imageWidths[2] = (image.Width / 2) - 1;
                imageWidths[1] = 2;

                imageHeights[0] = imageHeights[2] = (image.Height / 2) - 1;
                imageHeights[1] = 2;

                destWidths[0] = destWidths[2] = imageWidths[0];
                destWidths[1] = layoutBounds.Width - (destWidths[0] + destWidths[2]);

                destHeights[0] = destHeights[2] = imageHeights[0];
                destHeights[1] = layoutBounds.Height - (destHeights[0] + destHeights[2]);
            }
        }
    }
}