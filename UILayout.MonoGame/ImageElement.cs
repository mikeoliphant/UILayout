using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UILayout
{
    public partial class ImageElement
    {
        protected override void DrawContents()
        {
            MonoGameLayout.Current.GraphicsContext.DrawImage(Image, ContentBounds.X, ContentBounds.Y, 0.5f);
        }
    }
}
