using System.IO;
using System.Xml.Serialization;
using UILayout;

namespace UILayout.DefaultTextures
{
    public class TextureLoader
    {
        public static void LoadDefaultTextures()
        {
            Layout.Current.ResourceAssembly = typeof(TextureLoader).Assembly;
            Layout.Current.ResourceNamespace = "UILayout";

            Layout.Current.AddImage("ScrollBar");
            Layout.Current.AddImage("ScrollBarGutter");
            Layout.Current.AddImage("ScrollUpArrow");
            Layout.Current.AddImage("ScrollDownArrow");

            Layout.Current.AddImage("TabPanelBackground");
            Layout.Current.AddImage("TabBackground");
            Layout.Current.AddImage("TabForeground");

            Layout.Current.DefaultOutlineNinePatch = Layout.Current.AddImage("OutlineNinePatch");

            Layout.Current.DefaultPressedNinePatch = Layout.Current.AddImage("ButtonPressed");
            Layout.Current.DefaultUnpressedNinePatch = Layout.Current.AddImage("ButtonUnpressed");

            Layout.Current.DefaultDragImage = Layout.Current.GetImage("ButtonUnpressed");

            Layout.Current.DefaultForegroundColor = UIColor.White;

        }
    }
}
