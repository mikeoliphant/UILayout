using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UILayout
{
    public partial class UIFont
    {
        public SpriteFont SpriteFont { get; set; }

        public float TextHeight
        {
            get { return SpriteFont.LineHeight; }
        }

        public void MeasureString(string text, out float width, out float height)
        {
            SpriteFont.MeasureString(text, out width, out height);
        }

        public void MeasureString(StringBuilder sb, out float width, out float height)
        {
            SpriteFont.MeasureString(sb, out width, out height);
        }
    }
}
