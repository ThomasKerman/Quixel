using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using System.IO;

namespace Quixel
{
    /// <summary>
    /// A terrain generator that uses voxels and marching cubes
    /// </summary>
    /// <typeparam name="T">The type of the data assigned to each voxel</typeparam>
    public class QuixelEngine<T>
    {
        /// <summary>
        /// The name of the world
        /// </summary>
        public String WorldName { get; private set; }

        /// <summary>
        /// Whether the engine is active
        /// </summary>
        private Boolean _active { get; set; }

        /// <summary>
        /// The terrain controller
        /// </summary>
        public VoxelTerrainController<T> controller { get; private set; }

        /// <summary>
        /// The parent object of the terrain
        /// </summary>
        public GameObject TerrainObject { get; private set; }
        public Vector3 TerrainObject_Position { get; private set; }

        /// <summary>
        /// The player object that is used to determine the position for LOD
        /// </summary>
        public Func<GameObject> PlayerObject { get; private set; }

        /// <summary>
        /// The maximum LOD for the voxel
        /// </summary>
        public Int32 MaxLOD { get; private set; }
        public Int32 Size { get; private set; }

        /// <summary>
        /// The amount of generator threads
        /// </summary>
        public Int32 GeneratorThreadCount { get; private set; }

        // Managers
        public MeshFactory<T> meshFactory;
        public NodeManager<T> nodeManager;
        public ChunkPool<T> chunkPool;

        /// <summary>
        /// Initializes the Quixel Engine
        /// </summary>
        /// <param name="terrainObject">Parent terrain object. (empty)</param>
        /// <param name="name">Name of the world. Used for paging. (empty)</param>
        public QuixelEngine(GameObject terrainObject, String name)
        {
            TerrainObject = terrainObject;
            WorldName = name;
        }

        /// <summary>
        /// Creates the voxel managers
        /// </summary>
        public void Init()
        {
            // Checks
            if (PlayerObject?.Invoke() == null)
                throw new Exception("The player object is null.");
            if (TerrainObject == null)
                throw new Exception("The terrain object is null.");
            if (String.IsNullOrEmpty(WorldName))
                throw new Exception("The world name is null or empty.");
            if (controller == null)
                throw new Exception("The terrain controller is null.");
            if (GeneratorThreadCount == 0)
                throw new Exception("The amount of generator threads is zero.");

            // Manager
            _active = true;
            meshFactory = new MeshFactory<T>(this);
            nodeManager = new NodeManager<T>(this);
            chunkPool = new ChunkPool<T>(this);
        }

        /// <summary>
        /// Sets the width/length/height of the smallest LOD voxel.
        /// The width of a single voxel will be 2^(size + LOD)
        /// </summary>
        /// <param name="size">The size (units).</param>
        public void SetVoxelSize(int size, int maxLOD)
        {
            MaxLOD = maxLOD;
            Size = size;
        }

        /// <summary>
        /// Sets the amount of threads used to generate the terrain data
        /// </summary>
        public void SetGeneratorThreadCount(Int32 count)
        {
            GeneratorThreadCount = count;
        }
        
        /// <summary>
        /// Sets the terrain generator to use when generating terrain.
        /// </summary>
        public void SetTerrainController(VoxelTerrainController<T> gen)
        {
            controller = gen;
        }
        
        /// <summary>
        /// Updates the Quixel system. Should be called every step.
        /// </summary>
        public void Update()
        {
            TerrainObject_Position = TerrainObject.transform.position;
            meshFactory.Update();
            if (PlayerObject != null)
                nodeManager.SetViewPosition(PlayerObject().transform.position);
            _active = Application.isPlaying;
        }

        /// <summary>
        /// Sets the object to follow for the LOD system.
        /// </summary>
        public void SetCameraObj(Func<GameObject> obj)
        {
            PlayerObject = obj;
        }

        /// <summary>
        /// Returns true if the player is still active.
        /// </summary>
        public bool IsActive()
        {
            return _active;
        }

        /// <summary>
        /// Terminates the engine.
        /// </summary>
        public void Terminate()
        {
            _active = false;
        }
    }
}
