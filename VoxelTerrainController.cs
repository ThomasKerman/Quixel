using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using System.IO;

namespace Quixel
{
    public abstract class VoxelTerrainController<T>
    {
        /// <summary>
        /// Builds the voxel terrain data for a given position
        /// </summary>
        /// <param name="pos">The (real world) position</param>
        public abstract T BuildVoxelData(Vector3 center, Vector3 pos);

        /// <summary>
        /// Extracts the density from the voxel terrain data
        /// </summary>
        public abstract Single ExtractDensity(T data);

        /// <summary>
        /// Extracts the material from the voxel terrain data
        /// </summary>
        public abstract Int32 ExtractMaterial(T data);

        /// <summary>
        /// Extracts the color from the voxel terrain data
        /// </summary>
        public abstract Color ExtractColor(T data);

        /// <summary>
        /// Returns the maximum amount of materials available.
        /// </summary>
        public abstract Int32 GetMaterialCount();

        /// <summary>
        /// Applies the voxel mesh to the chunk
        /// </summary>
        public abstract void ApplyVoxelData(Mesh mesh, Node<T> node);

        /// <summary>
        /// Destroys the voxel mesh of the chunk
        /// </summary>
        public abstract void DisposeVoxelData(Node<T> node);

        /// <summary>
        /// Defines whether the node should be rendered
        /// </summary>
        public abstract void SetRenderState(Node<T> node, Boolean state);

        /// <summary>
        /// Defines whether the mesh of the node should have a collider
        /// </summary>
        public abstract void SetCollisionState(Node<T> node, Boolean state);
    }
}