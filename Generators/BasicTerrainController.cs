﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Quixel
{
    /// <summary>
    /// Terrain Controller implementation that creates a 50m high terrain and that 
    /// uses standalone MeshRendering
    /// </summary>
    public class BasicTerrainController : VoxelTerrainController<TerrainData>
    {
        /// <summary>
        /// The materials for the terrain
        /// </summary>
        public Material[] Materials { get; set; }

        /// <summary>
        /// Creates a new terrain controller
        /// </summary>
        public BasicTerrainController(Material[] mats)
        {
            Materials = mats;
        }

        /// <summary>
        /// Builds the voxel terrain data for a given position
        /// </summary>
        /// <param name="pos">The (real world) position</param>
        public override TerrainData BuildVoxelData(Vector3 center, Vector3 pos)
        {
            Vector3 dist = pos - center;
            TerrainData data = new TerrainData
            {
                density = -(dist.y - 50f),
                materialIndex = 0
            };
            return data;
        }

        /// <summary>
        /// Extracts the density from the voxel terrain data
        /// </summary>
        public override Single ExtractDensity(TerrainData data)
        {
            return -data.density;
        }

        /// <summary>
        /// Extracts the material from the voxel terrain data
        /// </summary>
        public override Int32 ExtractMaterial(TerrainData data)
        {
            return data.materialIndex;
        }

        /// <summary>
        /// Extracts the color from the voxel terrain data
        /// </summary>
        public override Color ExtractColor(TerrainData data)
        {
            return Color.clear;
        }

        /// <summary>
        /// Returns the maximum amount of materials available.
        /// </summary>
        public override Int32 GetMaterialCount()
        {
            return Materials.Length;
        }

        /// <summary>
        /// Applies the voxel mesh to the chunk
        /// </summary>
        public override void ApplyVoxelData(Mesh mesh, Node<TerrainData> node)
        {
            if (node.chunk == null)
            {
                node.chunk = node.manager.Engine.chunkPool.GetChunk();
                if (node.LOD > 2)
                    node.chunk.transform.position = node.position - ShiftVector(node);
                else
                    node.chunk.transform.position = node.position;
                node.chunk.GetComponent<MeshRenderer>().materials = Materials;
            }
            node.chunk.GetComponent<MeshFilter>().mesh = mesh;
            node.chunk.GetComponent<MeshCollider>().sharedMesh = mesh;
            node.chunk.GetComponent<MeshCollider>().enabled = node.LOD == 0 && node.collides;
        }

        /// <summary>
        /// Destroys the voxel mesh of the chunk
        /// </summary>
        public override void DisposeVoxelData(Node<TerrainData> node)
        {
            if (node.chunk != null)
            {
                UnityEngine.Object.Destroy(node.chunk.GetComponent<MeshFilter>().sharedMesh);
                Mesh mesh = node.chunk.GetComponent<MeshCollider>().sharedMesh;
                if (mesh != null)
                    UnityEngine.Object.Destroy(mesh);
                node.chunk.GetComponent<MeshFilter>().sharedMesh = null;
                node.chunk.GetComponent<MeshCollider>().sharedMesh = null;

                node.manager.Engine.chunkPool.RecycleChunk(node.chunk);
            }
        }

        /// <summary>
        /// Defines whether the node should be rendered
        /// </summary>
        public override void SetRenderState(Node<TerrainData> node, Boolean state)
        {
            node.chunk.GetComponent<Renderer>().enabled = state;
        }

        /// <summary>
        /// Defines whether the mesh of the node should have a collider
        /// </summary>
        public override void SetCollisionState(Node<TerrainData> node, Boolean state)
        {
            if (node.chunk != null)
                node.chunk.GetComponent<MeshCollider>().enabled = state;
        }

        /// <summary>
        /// Returns a vector that is used to move higher LOD levels
        /// </summary>
        public virtual Vector3 ShiftVector(Node<TerrainData> node)
        {
            return new Vector3(0f, node.manager.LODSize[node.LOD] / 2f, 0f);
        }
    }
}
