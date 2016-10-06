using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Quixel
{
    /// <summary>
    /// A generator that generates a spherical terrain
    /// </summary>
    public class SphericalTerrainController : BasicTerrainController
    { 
        /// <summary>
        /// The radius of the sphere
        /// </summary>
        public Single radius;

        /// <summary>
        /// Creates a new Circular Terrain Controller
        /// </summary>
        public SphericalTerrainController(Material groundMat, Single radius) : base(new[] { groundMat })
        {
            this.radius = radius;
        }

        /// <summary>
        /// Builds the voxel terrain data for a given position
        /// </summary>
        /// <param name="pos">The (real world) position</param>
        public override TerrainData BuildVoxelData(Vector3 center, Vector3 pos)
        {
            Vector3 dist = pos - center;
            Single density = (-dist.magnitude + radius) * 50;
            TerrainData data = new TerrainData
            {
                density = density,
                materialIndex = 0
            };
            return data;
        }

        public override Vector3 ShiftVector(Node<TerrainData> node)
        {
            Vector3 center = node.manager.Engine.TerrainObject_Position;
            Vector3 direction = (node.position - center).normalized;
            return direction * node.manager.LODSize[node.LOD] / 2f;
        }
    }
}
