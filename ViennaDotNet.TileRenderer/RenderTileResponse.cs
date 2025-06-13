using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViennaDotNet.TileRenderer;

public sealed record RenderTileResponse(int TileX, int TileY, int Zoom, string TileObjectId);
