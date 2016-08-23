using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Quixel
{
    /// <summary>
    /// A generator that generates a circular terrain
    /// </summary>
    public class CircularTerrainController : BasicTerrainController
    {
        /// <summary>
        /// The height of the terrain
        /// </summary>
        public Single height;

        /// <summary>
        /// The radius of the circle
        /// </summary>
        public Single radius;

        /// <summary>
        /// Creates a new Circular Terrain Controller
        /// </summary>
        public CircularTerrainController(Material groundMat, Single height, Single radius) : base(new[] { groundMat })
        {
            this.height = height;
            this.radius = radius;
        }

        /// <summary>
        /// Builds the voxel terrain data for a given position
        /// </summary>
        /// <param name="pos">The (real world) position</param>
        public override TerrainData BuildVoxelData(Vector3 center, Vector3 pos)
        {
            Single distance = Vector2.Distance(new Vector2(center.x, center.z), new Vector2(pos.x, pos.z));
            Single yy = Math.Abs(pos.y - center.y);
            TerrainData data = new TerrainData
            {
                density = distance <= radius ? -(yy + height) : -100f,
                materialIndex = 0
            };
            return data;
        }
    }
}
