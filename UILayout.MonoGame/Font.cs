using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UILayout
{
    public partial class Font
    {
        public SpriteFont SpriteFont { get; set; }

        public float TextHeight
        {
            get { return SpriteFont.TextHeight; }
        }
    }
}
