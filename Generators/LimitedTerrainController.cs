using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Quixel
{
    /// <summary>
    /// A generator that generates a limited amount of terrain
    /// </summary>
    public class LimitedTerrainController : BasicTerrainController
    {
        /// <summary>
        /// The maximum x value
        /// </summary>
        public Int32 x;

        /// <summary>
        /// The maximum y value
        /// </summary>
        public Int32 y;

        /// <summary>
        /// The maximum z value
        /// </summary>
        public Int32 z;

        /// <summary>
        /// Creates a new Limited Terrain Controller
        /// </summary>
        public LimitedTerrainController(Material groundMat, Int32 x, Int32 y, Int32 z) : base(new[] { groundMat })
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Builds the voxel terrain data for a given position
        /// </summary>
        /// <param name="pos">The (real world) position</param>
        public override TerrainData BuildVoxelData(Vector3 center, Vector3 pos)
        {
            Single xx = Mathf.Abs(center.x - pos.x);
            Single yy = Mathf.Abs(center.y - pos.y);
            Single zz = Mathf.Abs(center.z - pos.z);
            TerrainData data = new TerrainData()
            {
                density = (xx <= x || yy <= y || zz <= z) ? 100f : -100f,
                materialIndex = 0
            };
            return data;
        }
    }
}
