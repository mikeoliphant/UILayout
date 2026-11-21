namespace UILayout
{
    /// <summary>A group of controls set off by a thin rounded outline (the theme's input border nine-patch, in its border
    /// color), with a little padding inside.</summary>
    public class Section : NinePatchWrapper
    {
        public Section()
            : this(null)
        {
        }

        public Section(UIElement content)
            : base(Layout.Current.DefaultInputBorderNinePatch)
        {
            DrawCenter = false;
            Color = Layout.Current.Theme.Border;
            HorizontalAlignment = EHorizontalAlignment.Stretch;

            // More room inside than the outline needs.
            Padding = new LayoutPadding(6);

            Child = content;
        }
    }
}
