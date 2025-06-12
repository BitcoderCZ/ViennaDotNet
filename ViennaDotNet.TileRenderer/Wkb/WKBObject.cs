using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViennaDotNet.TileRenderer.Wkb;

internal class WKBObject
{
    public bool ByteOrder { get; set; }
    public uint WkbType { get; set; }

    public virtual void Load(BinaryReader reader)
    {
    }

    public virtual void Render(Tile tile, double r, double g, double b, double strokeWidth)
    {
    }
}
