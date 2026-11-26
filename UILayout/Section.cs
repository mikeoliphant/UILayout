namespace UILayout
{
    /// <summary>
    /// A group of controls set off by a thin rounded outline: a <see cref="Frame"/> without its fill, with a little
    /// more room inside.
    /// </summary>
    public class Section : Frame
    {
        public Section()
            : this(null)
        {
        }

        public Section(UIElement content)
            : base(content)
        {
            DrawFill = false;
            HorizontalAlignment = EHorizontalAlignment.Stretch;
            VerticalAlignment = EVerticalAlignment.Top;

            // More room inside than the outline needs.
            ContentPadding = new LayoutPadding(6);
        }
    }
}
