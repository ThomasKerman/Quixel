using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using System.IO;

namespace Quixel
{
    public interface IVoxelTerrainController<T>
    {
        /// <summary>
        /// Builds the voxel terrain data for a given position
        /// </summary>
        /// <param name="pos">The (real world) position</param>
        T BuildVoxelData(Vector3 pos);

        /// <summary>
        /// Extracts the density from the voxel terrain data
        /// </summary>
        Single ExtractDensity(T data);

        /// <summary>
        /// Extracts the material from the voxel terrain data
        /// </summary>
        Int32 ExtractMaterial(T data);

        /// <summary>
        /// Extracts the color from the voxel terrain data
        /// </summary>
        Color ExtractColor(T data);

        /// <summary>
        /// Returns the maximum amount of materials available.
        /// </summary>
        Int32 GetMaterialCount();

        /// <summary>
        /// Applies the voxel mesh to the chunk
        /// </summary>
        void ApplyVoxelData(Mesh mesh, Node<T> node);

        /// <summary>
        /// Destroys the voxel mesh of the chunk
        /// </summary>
        void DisposeVoxelData(Node<T> node);

        /// <summary>
        /// Defines whether the node should be rendered
        /// </summary>
        void SetRenderState(Node<T> node, Boolean state);
    }
}