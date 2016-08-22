using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using System.IO;

namespace Quixel
{
    /// <summary>
    /// A class that loads chunks from an object pool
    /// </summary>
    public class ChunkPool<T>
    {
        /// <summary>
        /// A list of unused chunk objects
        /// </summary>
        public List<GameObject> chunkList = new List<GameObject>();
        public int totalCreated = 0;

        /// <summary>
        /// The Quixel Engine
        /// </summary>
        public QuixelEngine<T> Engine { get; set; }

        /// <summary>
        /// Initialises a new chunk pool
        /// </summary>s
        public ChunkPool(QuixelEngine<T> engine)
        {
            Engine = engine;
        }

        /// <summary>
        /// Gets a chunk from the chunk pool, creates one if the chunk pool is empty
        /// </summary>
        /// <returns></returns>
        public GameObject GetChunk()
        {
            if (chunkList.Count == 0)
            {
                totalCreated++;
                GameObject obj = (GameObject)GameObject.Instantiate(Engine.meshFactory.chunkObj);
                obj.transform.parent = Engine.TerrainObject.transform;
                return obj;
            }

            GameObject chunk = chunkList[0];
            chunkList.RemoveAt(0);
            chunk.SetActive(true);
            return chunk;
        }

        /// <summary>
        /// Recycles a chunk gameobject to the chunk pool
        /// </summary>
        /// <param name="chunk"></param>
        public void RecycleChunk(GameObject chunk)
        {
            chunk.SetActive(false);
            chunkList.Add(chunk);
        }
    }
}