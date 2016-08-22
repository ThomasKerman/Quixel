using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using System.IO;

namespace Quixel
{
    /// <summary>
    /// Controls the 27 top-level nodes. Handles node searching.
    /// </summary>
    public class NodeManager<T>
    {
        public QuixelEngine<T> Engine;
        public Vector3I curBottomNode = new Vector3I(-111, 0, 0);
        public Vector3I curTopNode = new Vector3I(0, 0, 0);

        public Vector3I[] viewChunkPos;

        //public static Node[,,] topNodes = new Node[3,3,3];
        public Node<T>[, ,] topNodes = new Node<T>[3, 3, 3];

        /// <summary> 
        /// Mask used for positioning subnodes
        /// </summary>
        public Vector3[] mask = new Vector3[8] 
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 0, 1),
            new Vector3(1, 1, 0),
            new Vector3(1, 0, 1),
            new Vector3(0, 1, 1),
            new Vector3(1, 1, 1)
        };

        /// <summary>
        /// Refers to the tri size of each vertex
        /// </summary>
        public int[] LODSize = new int[11] 
        {
            1,
            2, 
            4, 
            8,
            16,
            32,
            64,
            128,
            256,
            512,
            1024
        };

        //DEPRECATED
        /// <summary>
        /// Radius for each LOD value
        /// </summary>
        public int[] LODRange = new int[11] 
        {
            0, 
            7, 
            12, 
            26,
            50,
            100,
            210,
            400,
            700,
            1200,
            2400
        };

        public int[] nodeCount = new int[11];

        /// <summary>
        /// Density array size
        /// </summary>
        public int nodeSize = 16;

        /// <summary>
        /// The maximum allowed LOD. The top level nodes will be this LOD.
        /// </summary>
        public int maxLOD = 10;

        /// <summary>
        /// Initializes the node manager.
        /// </summary>
        public NodeManager(QuixelEngine<T> engine)
        {
            Engine = engine;
            float nSize = LODSize[Engine.MaxLOD] * nodeSize;
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    for (int z = -1; z < 2; z++)
                    {
                        topNodes[x + 1, y + 1, z + 1] = new Node<T>(null, this, new Vector3(x * nSize, y * nSize, z * nSize), 0, Engine.MaxLOD, Node<T>.RenderType.FRONT);
                    }
                }
            }

            viewChunkPos = new Vector3I[Engine.MaxLOD + 1];
            for (int i = 0; i <= Engine.MaxLOD; i++)
                viewChunkPos[i] = new Vector3I();
        }

        /// <summary>
        /// Sets the view position, and checks if chunks need to be updated
        /// </summary>
        public void SetViewPosition(Vector3 pos)
        {
            for (int i = 0; i <= Engine.MaxLOD; i++)
            {
                float nWidth = LODSize[i] * nodeSize;
                viewChunkPos[i].x = (int)(pos.x / nWidth);
                viewChunkPos[i].y = (int)(pos.y / nWidth);
                viewChunkPos[i].z = (int)(pos.z / nWidth);
            }

            float sWidth = LODSize[0] * nodeSize * 0.5f;
            Vector3I newPos = new Vector3I((int)(pos.x / sWidth), (int)(pos.y / sWidth), (int)(pos.z / sWidth));

            if (!curTopNode.Equals(GetTopNode(pos)))
            {
                float nodeWidth = LODSize[Engine.MaxLOD] * nodeSize;
                Vector3I diff = GetTopNode(pos).Subtract(curTopNode);
                curTopNode = GetTopNode(pos);
                while (diff.x > 0)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        for (int z = 0; z < 3; z++)
                        {
                            topNodes[0, y, z].Dispose();
                            topNodes[0, y, z] = topNodes[1, y, z];
                            topNodes[1, y, z] = topNodes[2, y, z];
                            topNodes[2, y, z] = new Node<T>(null, this,
                                                new Vector3((curTopNode.x * nodeWidth) + nodeWidth,
                                                    (curTopNode.y * nodeWidth) + ((y - 1) * nodeWidth),
                                                    (curTopNode.z * nodeWidth) + ((z - 1) * nodeWidth)),
                                                0, Engine.MaxLOD, Node<T>.RenderType.FRONT);
                        }
                    }
                    diff.x--;
                }

                while (diff.x < 0)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        for (int z = 0; z < 3; z++)
                        {
                            topNodes[2, y, z].Dispose();
                            topNodes[2, y, z] = topNodes[1, y, z];
                            topNodes[1, y, z] = topNodes[0, y, z];
                            topNodes[0, y, z] = new Node<T>(null, this,
                                                new Vector3((curTopNode.x * nodeWidth) - nodeWidth,
                                                    (curTopNode.y * nodeWidth) + ((y - 1) * nodeWidth),
                                                    (curTopNode.z * nodeWidth) + ((z - 1) * nodeWidth)),
                                                0, Engine.MaxLOD, Node<T>.RenderType.FRONT);
                        }
                    }
                    diff.x++;
                }

                while (diff.y > 0)
                {
                    for (int x = 0; x < 3; x++)
                    {
                        for (int z = 0; z < 3; z++)
                        {
                            topNodes[x, 0, z].Dispose();
                            topNodes[x, 0, z] = topNodes[x, 1, z];
                            topNodes[x, 1, z] = topNodes[x, 2, z];
                            topNodes[x, 2, z] = new Node<T>(null, this,
                                                new Vector3((curTopNode.x * nodeWidth) + ((x - 1) * nodeWidth),
                                                    (curTopNode.y * nodeWidth) + nodeWidth,
                                                    (curTopNode.z * nodeWidth) + ((z - 1) * nodeWidth)),
                                                0, Engine.MaxLOD, Node<T>.RenderType.FRONT);
                        }
                    }
                    diff.y--;
                }

                while (diff.y < 0)
                {
                    for (int x = 0; x < 3; x++)
                    {
                        for (int z = 0; z < 3; z++)
                        {
                            topNodes[x, 2, z].Dispose();
                            topNodes[x, 2, z] = topNodes[x, 1, z];
                            topNodes[x, 1, z] = topNodes[x, 0, z];
                            topNodes[x, 0, z] = new Node<T>(null, this,
                                                new Vector3((curTopNode.x * nodeWidth) + ((x - 1) * nodeWidth),
                                                    (curTopNode.y * nodeWidth) - nodeWidth,
                                                    (curTopNode.z * nodeWidth) + ((z - 1) * nodeWidth)),
                                                0, Engine.MaxLOD, Node<T>.RenderType.FRONT);
                        }
                    }

                    diff.y++;
                }

                while (diff.z > 0)
                {
                    for (int x = 0; x < 3; x++)
                    {
                        for (int y = 0; y < 3; y++)
                        {
                            topNodes[x, y, 0].Dispose();
                            topNodes[x, y, 0] = topNodes[x, y, 1];
                            topNodes[x, y, 1] = topNodes[x, y, 2];
                            topNodes[x, y, 2] = new Node<T>(null, this,
                                                new Vector3((curTopNode.x * nodeWidth) + ((x - 1) * nodeWidth),
                                                    (curTopNode.y * nodeWidth) + ((y - 1) * nodeWidth),
                                                    (curTopNode.z * nodeWidth) + nodeWidth),
                                                0, Engine.MaxLOD, Node<T>.RenderType.FRONT);
                        }
                    }

                    diff.z--;
                }

                while (diff.z < 0)
                {
                    for (int x = 0; x < 3; x++)
                    {
                        for (int y = 0; y < 3; y++)
                        {
                            topNodes[x, y, 2].Dispose();
                            topNodes[x, y, 2] = topNodes[x, y, 1];
                            topNodes[x, y, 1] = topNodes[x, y, 0];
                            topNodes[x, y, 0] = new Node<T>(null, this,
                                                new Vector3((curTopNode.x * nodeWidth) + ((x - 1) * nodeWidth),
                                                    (curTopNode.y * nodeWidth) + ((y - 1) * nodeWidth),
                                                    (curTopNode.z * nodeWidth) - nodeWidth),
                                                0, Engine.MaxLOD, Node<T>.RenderType.FRONT);
                        }
                    }
                    diff.z++;
                }
            }

            if (curBottomNode.x != newPos.x || curBottomNode.y != newPos.y || curBottomNode.z != newPos.z)
            {
                Vector3 setPos = new Vector3(newPos.x * sWidth + (sWidth / 1f), newPos.y * sWidth + (sWidth / 1f), newPos.z * sWidth + (sWidth / 1f));
                for (int x = 0; x < 3; x++)
                    for (int y = 0; y < 3; y++)
                        for (int z = 0; z < 3; z++)
                        {
                            topNodes[x, y, z].ViewPosChanged(setPos);
                        }

                curBottomNode = newPos;
            }
        }

        /// <summary>
        /// Returns a node containing the point as close as possible to the requested LOD.
        /// </summary>
        /// <returns>Null if no such node exists.</returns>
        public Node<T>[] SearchNodeContainingDensity(Vector3 pos, int searchLOD)
        {
            Node<T>[] ret = new Node<T>[8];
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int z = 0; z < 3; z++)
                    {
                        if (topNodes[x, y, z] != null)
                            topNodes[x, y, z].SearchNodeCreate(pos, searchLOD, ref ret);
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Returns a node containing the point as close as possible to the requested LOD.
        /// </summary>
        /// <returns>Null if no such node exists.</returns>
        public Node<T> SearchNode(Vector3 pos, int searchLOD)
        {
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int z = 0; z < 3; z++)
                    {
                        if (topNodes[x, y, z] != null)
                            if (topNodes[x, y, z].ContainsPoint(pos))
                                return topNodes[x, y, z].SearchNode(pos, searchLOD);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Calculates the offset position given a parent's node and the subnode ID.
        /// </summary>
        /// <param name="parentNode">Parent that contains t</param>
        /// <param name="subNodeID">Index of the node in the subnode array</param>
        /// <returns></returns>
        public Vector3 GetOffsetPosition(Node<T> parentNode, int subNodeID)
        {
            int parentWidth = nodeSize * LODSize[parentNode.LOD];
            return new Vector3
            {
                x = parentNode.position.x + ((parentWidth / 2) * mask[subNodeID].x),
                y = parentNode.position.y + ((parentWidth / 2) * mask[subNodeID].y),
                z = parentNode.position.z + ((parentWidth / 2) * mask[subNodeID].z)
            };
        }

        /// <summary>
        /// Returns a 3d integer vector position of the "top" (highest LOD) node that contains the given position.
        /// </summary>
        public Vector3I GetTopNode(Vector3 pos)
        {
            Vector3I ret = new Vector3I();
            ret.x = (int)Mathf.Floor(pos.x / (LODSize[Engine.MaxLOD] * nodeSize));
            ret.y = (int)Mathf.Floor(pos.y / (LODSize[Engine.MaxLOD] * nodeSize));
            ret.z = (int)Mathf.Floor(pos.z / (LODSize[Engine.MaxLOD] * nodeSize));

            return ret;
        }
    }

    /// <summary>
    /// Octree node (chunk)
    /// </summary>
    public class Node<T>
    {
        /// <summary> 
        /// When we are already generating a new mesh and need a new regeneration. 
        /// </summary>
        public bool regenFlag = false;
        private bool regenReq = false;
        public bool permanent = false;
        public bool hasDensityChangeData = false;

        /// <summary> 
        /// Array that contains the mesh data. 
        /// </summary>
        public VoxelData<T> data;
        public VoxelData<T> changeData;

        /// <summary>
        /// Index of this node in the parent node's subnode array
        /// </summary>
        public int subNodeID;

        /// <summary>
        /// Level of Detail value
        /// </summary>
        public int LOD;

        /// <summary> 
        /// The integer position of the chunk (Not real) 
        /// </summary>
        public Vector3I chunkPos;

        /// <summary>
        /// Real position
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// Subnodes under this parent node
        /// </summary>
        public Node<T>[] subNodes = new Node<T>[8];

        /// <summary>
        /// Neighbor nodes of the same LOD
        /// Null means the neighbor node either doesn't exist or hasn't been allocated yet.
        /// </summary>
        public Node<T>[] neighborNodes = new Node<T>[6];

        /// <summary>
        /// The parent that owns this node. Null if top-level
        /// </summary>
        public Node<T> parent;

        /// <summary>
        /// Gameobject that contains the mesh
        /// </summary>
        public GameObject chunk;

        /// <summary> 
        /// Center of the chunk in real pos 
        /// </summary>
        public Vector3 center;

        public NodeManager<T> manager;

        public bool disposed = false;
        public bool hasMesh = false;
        public bool collides = false;
        public bool empty = true;

        public RenderType renderType;
        public enum RenderType
        {
            FRONT, BACK
        }

        /// <summary> 
        /// Mask used for positioning subnodes
        /// </summary>
        public static Vector3[] neighborMask = new Vector3[6] 
        {
            new Vector3(-1, 0, 0),
            new Vector3(0, -1, 0),
            new Vector3(0, 0, -1),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 0, 1),
        };

        public static int[] oppositeNeighbor = new int[6] 
        {
            3,
            4,
            5,
            0,
            1,
            2
        };

        public Node(Node<T> parent, NodeManager<T> manager, Vector3 position, int subNodeID, int LOD, RenderType renderType)
        {
            changeData = new VoxelData<T>();
            this.manager = manager;
            this.parent = parent;
            this.position = position;
            this.subNodeID = subNodeID;
            this.LOD = LOD;

            float chunkWidth = (manager.LODSize[LOD] * manager.nodeSize) / 2f;
            center = new Vector3(position.x + chunkWidth, position.y + chunkWidth, position.z + chunkWidth);

            SetRenderType(renderType);

            if (parent != null && parent.permanent)
                permanent = true;

            manager.nodeCount[LOD]++;

            float nWidth = manager.LODSize[LOD] * manager.nodeSize;
            chunkPos.x = (int)(center.x / nWidth);
            chunkPos.y = (int)(center.y / nWidth);
            chunkPos.z = (int)(center.z / nWidth);

            regenReq = true;
            manager.Engine.meshFactory.RequestMesh(this);
        }

        /// <summary>
        /// Called when the viewpoint has changed.
        /// </summary>
        /// <param name="pos"></param>
        public void ViewPosChanged(Vector3 pos)
        {
            Vector3I viewPos = manager.viewChunkPos[LOD];
            int size = 1;
            if ((viewPos.x >= chunkPos.x - size && viewPos.x <= chunkPos.x + size)
                && (viewPos.y >= chunkPos.y - size && viewPos.y <= chunkPos.y + size)
                && (viewPos.z >= chunkPos.z - size && viewPos.z <= chunkPos.z + size))
            {
                if (IsBottomLevel())
                    CreateSubNodes(RenderType.FRONT);

                for (int i = 0; i < 8; i++)
                {
                    if (subNodes[i] != null)
                        subNodes[i].ViewPosChanged(pos);
                }
            }
            else
            {
                size += 2;
                if (LOD < 3 && (viewPos.x < chunkPos.x - size || viewPos.x > chunkPos.x + size)
                    || (viewPos.y < chunkPos.y - size || viewPos.y > chunkPos.y + size)
                    || (viewPos.z < chunkPos.z - size || viewPos.z > chunkPos.z + size))
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (subNodes[i] != null)
                        {
                            subNodes[i].Dispose();
                            subNodes[i] = null;
                        }
                    }
                }
                else if (LOD >= 3)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (subNodes[i] != null)
                        {
                            subNodes[i].Dispose();
                            subNodes[i] = null;
                        }
                    }
                }
            }

            if (LOD == 0)
            {
                float nodeSize = (float)manager.LODSize[0] * (float)manager.nodeSize;
                Vector3I viewChunk = new Vector3I((int)(pos.x / nodeSize),
                    (int)(pos.y / nodeSize),
                    (int)(pos.z / nodeSize));

                Vector3I curChunk = new Vector3I((int)(position.x / nodeSize),
                    (int)(position.y / nodeSize),
                    (int)(position.z / nodeSize));

                if (curChunk.x >= viewChunk.x - 3 && curChunk.x <= viewChunk.x + 3 &&
                    curChunk.y >= viewChunk.y - 3 && curChunk.y <= viewChunk.y + 3 &&
                    curChunk.z >= viewChunk.z - 3 && curChunk.z <= viewChunk.z + 3)
                {
                    collides = true;
                    if (chunk != null)
                    {
                        // chunk.GetComponent<MeshCollider>().sharedMesh = chunk.GetComponent<MeshFilter>().sharedMesh;
                    }
                }
            }

            RenderCheck();
        }

        /// <summary>
        /// Searches for a node containing the given point and LOD, creating it if none is found.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public void SearchNodeCreate(Vector3 pos, int searchLOD, ref Node<T>[] list)
        {
            if (ContainsDensityPoint(pos))
            {
                if (LOD == searchLOD)
                {
                    for (int i = 0; i < list.Length; i++)
                        if (list[i] == null)
                        {
                            list[i] = this;
                            return;
                        }
                    Debug.Log("A");
                }
                else
                {
                    if (IsBottomLevel())
                        CreateSubNodes(RenderType.FRONT);

                    for (int i = 0; i < 8; i++)
                    {
                        subNodes[i].SearchNodeCreate(pos, searchLOD, ref list);
                    }
                }
            }
        }

        /// <summary>
        /// Searches for a node containing the given point and LOD.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Node<T> SearchNode(Vector3 pos, int searchLOD)
        {
            if (ContainsDensityPoint(pos))
            {
                if (searchLOD == LOD)
                    return this;

                if (!IsBottomLevel())
                {
                    for (int i = 0; i < 8; i++)
                        if (subNodes[i] != null)
                        {
                            if (subNodes[i].ContainsPoint(pos))
                                return subNodes[i].SearchNode(pos, searchLOD);
                        }
                }
                return this;
            }

            if (parent != null)
                return parent.SearchNode(pos, searchLOD);

            return manager.SearchNode(pos, searchLOD);
        }

        /// <summary>
        /// Returns whether or not the point is within the chunk.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool ContainsPoint(Vector3 pos)
        {
            float chunkWidth = manager.LODSize[LOD] * manager.nodeSize;
            if ((pos.x >= position.x && pos.y >= position.y && pos.z >= position.z)
                && (pos.x <= position.x + chunkWidth && pos.y <= position.y + chunkWidth && pos.z <= position.z + chunkWidth))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns whether or not the point is within the chunk.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool ContainsDensityPoint(Vector3 pos)
        {
            float chunkWidth = manager.LODSize[LOD] * manager.nodeSize;
            Vector3 corner1 = new Vector3(position.x - manager.LODSize[LOD],
                                        position.y - manager.LODSize[LOD],
                                        position.z - manager.LODSize[LOD]);
                                        
            Vector3 corner2 = new Vector3(position.x + chunkWidth + manager.LODSize[LOD],
                                        position.y + chunkWidth + manager.LODSize[LOD],
                                        position.z + chunkWidth + manager.LODSize[LOD]);
                                        
            if((pos.x >= corner1.x && pos.y >= corner1.y && pos.z >= corner1.z) &&
                (pos.x <= corner2.x && pos.y <= corner2.y && pos.z <= corner2.z)) {
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Checks whether this chunk should render and where.
        /// </summary>
        public void RenderCheck()
        {
            if (disposed)
            {
                if (chunk != null)
                    manager.Engine.controller.SetRenderState(this, false);
                return;
            }

            if (chunk != null)
            {
                if (IsBottomLevel())
                {
                    manager.Engine.controller.SetRenderState(this, true);
                    SetRenderType(RenderType.FRONT);
                }
                else
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (subNodes[i] != null && !subNodes[i].hasMesh)
                        {
                            SetRenderType(RenderType.FRONT);
                        }
                    }
                    if (chunk != null)
                        manager.Engine.controller.SetRenderState(this, false);
                }
            }
        }

        /// <summary>
        /// Called when a mesh has been generated
        /// </summary>
        /// <param name="mesh">Mesh data</param>
        public void SetMesh(MeshData meshData)
        {
            data.SetChangeData(changeData);

            regenReq = false;
            if (regenFlag)
            {
                regenFlag = false;
                regenReq = true;
                manager.Engine.meshFactory.RequestMesh(this);
            }

            hasMesh = true;
            if (meshData.indexArray.Length == 0)
                return;

            empty = false;
            Mesh mesh = new Mesh();
            mesh.subMeshCount = manager.Engine.controller.GetMaterialCount();
            mesh.vertices = meshData.triangleArray;

            for (int i = 0; i < manager.Engine.controller.GetMaterialCount(); i++)
            {
                if (meshData.indexArray[i].Length > 0)
                    mesh.SetTriangles(meshData.indexArray[i], i);
            }
            mesh.uv = meshData.uvArray;
            mesh.normals = meshData.normalArray;
            mesh.Optimize();
            manager.Engine.controller.ApplyVoxelData(mesh, this);
            meshData.Dispose();

            RenderCheck();
            switch (renderType)
            {
                case RenderType.BACK:
                    if (chunk != null)
                        chunk.layer = 9;
                    break;

                case RenderType.FRONT:
                    if (chunk != null)
                        chunk.layer = 8;
                    break;
            }
            if (parent != null)
                parent.RenderCheck();
        }

        /// <summary>
        /// Sets the render type of the chunk.
        /// Front will be rendered last, on top of Back.
        /// </summary>
        /// <param name="r"></param>
        public void SetRenderType(RenderType r)
        {
            switch (r)
            {
                case RenderType.BACK:
                    if (chunk != null)
                        chunk.layer = 9;
                    break;

                case RenderType.FRONT:
                    if (chunk != null)
                        chunk.layer = 8;
                    break;
            }

            renderType = r;
        }

        /// <summary>
        /// Returns whether or not this is the bottom level of the tree.
        /// </summary>
        /// <returns></returns>
        public bool IsBottomLevel()
        {
            for (int i = 0; i < 8; i++)
            {
                if (subNodes[i] == null)
                    return true;
                else if (subNodes[i].disposed)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if this is a transitional node.
        /// True if an adjacent node of the same LOD has subnodes.
        /// </summary>
        /// <returns></returns>
        public bool IsTransitional()
        {
            if (LOD == 1 || LOD == 0)
                return false;

            for (int i = 0; i < 6; i++)
            {
                if (neighborNodes[i] != null)
                    if (neighborNodes[i].LOD == this.LOD && neighborNodes[i].IsBottomLevel() && !IsBottomLevel())
                        return true;
            }

            return false;
        }

        /// <summary>
        /// Populates the subnode array
        /// </summary>
        public void CreateSubNodes(RenderType type)
        {
            if (LOD == 0)
                return;

            if (subNodes[0] != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (subNodes[i].renderType != type)
                        subNodes[i].SetRenderType(type);

                    subNodes[i].disposed = false;
                }
                return;
            }

            for (int i = 0; i < 8; i++)
            {
                if (subNodes[i] == null)
                    subNodes[i] = new Node<T>(this, manager, manager.GetOffsetPosition(this, i), i, LOD - 1, type);
            }
        }
        /// <summary>
        /// Sets the data of a point, given a world pos.
        /// </summary>
        /// <param name="worldPos"></param>
        public void SetDataFromWorldPos(Vector3 worldPos, T val)
        {
            worldPos = worldPos - position;
            Vector3I arrayPos = new Vector3I((int)Math.Round(worldPos.x) / manager.LODSize[LOD],
                                            (int)Math.Round(worldPos.y) / manager.LODSize[LOD],
                                            (int)Math.Round(worldPos.z) / manager.LODSize[LOD]);

            if (arrayPos.x < -1 || arrayPos.x > 17 ||
                arrayPos.y < -1 || arrayPos.y > 17 ||
                arrayPos.z < -1 || arrayPos.z > 17)
            {
                Debug.Log("Wrong node. " + arrayPos + ":" + worldPos + ":" + ContainsDensityPoint(worldPos).ToString());
                return;
            }

            changeData.Set(arrayPos.x, arrayPos.y, arrayPos.z, val);
            setPermanence(true);

            hasDensityChangeData = true;
            // manager.Engine.meshFactory.requestSave(this);
        }

        /// <summary>
        /// Regenerates the chunk without threading.
        /// </summary>
        public void RegenerateChunk()
        {
            if (regenReq)
            {
                regenFlag = true;
            }
            else
            {
                manager.Engine.meshFactory.RequestMesh(this);
                regenReq = true;
            }
        }

        /// <summary>
        /// Disposes of subnodes
        /// </summary>
        public void Dispose()
        {
            //If already disposed, exit.
            if (disposed)
                return;

            disposed = true;

            for (int i = 0; i < 8; i++)
            {
                if (subNodes[i] != null)
                {
                    subNodes[i].Dispose();
                    subNodes[i] = null;
                }
            }

            if (permanent)
            {
                if (chunk != null)
                    manager.Engine.controller.SetRenderState(this, false);
                return;
            }

            manager.nodeCount[LOD]--;

            hasMesh = false;
            if (parent != null)
                parent.RenderCheck();

            //Remove this node from the neighbor of existing nodes.
            for (int i = 0; i < 6; i++)
            {
                if (neighborNodes[i] != null)
                {
                    neighborNodes[i].neighborNodes[oppositeNeighbor[i]] = null;
                }
            }

            manager.Engine.controller.DisposeVoxelData(this);
        }

        /// <summary>
        /// Attempts to find any neighbors that we don't have a reference to.
        /// </summary>
        private void findNeighbors()
        {
            float nodeWidth = manager.LODSize[LOD] * manager.nodeSize;
            Vector3 searchPos = new Vector3();
            for (int i = 0; i < 6; i++)
            {
                if (neighborNodes[i] != null)
                {
                    if (neighborNodes[i].LOD == LOD)
                        continue;
                }
                searchPos.x = center.x + (neighborMask[i].x * nodeWidth);
                searchPos.y = center.y + (neighborMask[i].y * nodeWidth);
                searchPos.z = center.z + (neighborMask[i].z * nodeWidth);
                neighborNodes[i] = manager.SearchNode(searchPos, LOD);

                if (neighborNodes[i] != null && neighborNodes[i].LOD == LOD)
                    neighborNodes[i].neighborNodes[oppositeNeighbor[i]] = this;
            }
        }

        /// <summary>
        /// Sets the permanence of this node. If true, it will not be disposed of when out of range.
        /// </summary>
        private void setPermanence(bool perm)
        {
            if (perm == true)
                if (parent != null)
                    parent.setPermanence(true);

            if (perm == false)
                for (int i = 0; i < 8; i++)
                    if (subNodes[i] != null)
                        subNodes[i].setPermanence(false);

            permanent = perm;
        }

        /// <summary>
        /// Checks if the node intersects with a boundary box of the given size around the player.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        private bool CheckRangeIntersection(Vector3 pos, float width)
        {
            float nodeWidth = manager.nodeSize * manager.LODSize[LOD];
            Vector3[] corners = new Vector3[8] 
            {
                new Vector3(position.x, position.y, position.z),
                new Vector3(position.x + nodeWidth, position.y, position.z),
                new Vector3(position.x, position.y + nodeWidth, position.z),
                new Vector3(position.x, position.y, position.z + nodeWidth),
                new Vector3(position.x + nodeWidth, position.y + nodeWidth, position.z),
                new Vector3(position.x + nodeWidth, position.y, position.z + nodeWidth),
                new Vector3(position.x, position.y + nodeWidth, position.z + nodeWidth),
                new Vector3(position.x + nodeWidth, position.y + nodeWidth, position.z + nodeWidth),
            };

            for (int i = 0; i < 8; i++)
            {
                //float distance = (float)Math.Sqrt((corners[i].x - pos.x) * (corners[i].x - pos.x) +
                //(corners[i].y - pos.y) * (corners[i].y - pos.y) +
                //(corners[i].z - pos.z) * (corners[i].z - pos.z));
                float distance = Vector3.Distance(pos, corners[i]);
                if (distance < width)
                    return true;
            }

            return false;
        }

        public override string ToString()
        {
            return "LOD: " + LOD + ", Pos: " + position.ToString();
        }
    }
}