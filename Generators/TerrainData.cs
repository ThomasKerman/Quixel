using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quixel
{
    /// <summary>
    /// The data that is stored in a voxel
    /// </summary>
    public class TerrainData
    {
        public Single density = -10000f;
        public Byte materialIndex;
    }
}
