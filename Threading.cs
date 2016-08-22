using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using System.IO;

namespace Quixel
{
    /// <summary>
    /// Thread used for generating chunks.
    /// </summary>
    public class GeneratorThread<T>
    {
        private QuixelEngine<T> Engine;
        private Queue<MeshFactory<T>.MeshRequest<T>> genQueue;
        private Queue<MeshFactory<T>.MeshRequest<T>> finishedQueue;
        private Thread thread;

        public GeneratorThread(QuixelEngine<T> engine)
        {
            Engine = engine;
            genQueue = new Queue<MeshFactory<T>.MeshRequest<T>>();
            finishedQueue = new Queue<MeshFactory<T>.MeshRequest<T>>();

            try
            {
                thread = new Thread((System.Object obj) =>
                {
                    GenerateLoop();
                });
                
                thread.Priority = System.Threading.ThreadPriority.BelowNormal;
                thread.IsBackground = true;
                thread.Start();
            }
            catch (Exception e)
            {
                Debug.Log(e.Message + "\r\n" + e.StackTrace);
            }
        }

        /// <summary>
        /// Queue a mesh for generation.
        /// </summary>
        /// <param name="req"></param>
        public void QueueGenerateMesh(MeshFactory<T>.MeshRequest<T> req)
        {
            lock (genQueue)
                genQueue.Enqueue(req);
        }

        /// <summary>
        /// Returns a finished mesh request if there is one.
        /// </summary>
        /// <returns></returns>
        public MeshFactory<T>.MeshRequest<T> GetFinishedMesh()
        {
            if (finishedQueue.Count > 0)
                lock (finishedQueue)
                    return finishedQueue.Dequeue();
            return null;
        }

        /// <summary>
        /// Gets the count of queued mesh requests.
        /// </summary>
        /// <returns></returns>
        public int GetCount()
        {
            return genQueue.Count;
        }

        /// <summary>
        /// Continuously check if we have chunks to generate.
        /// </summary>
        private void GenerateLoop()
        {
            bool sleep = true;
            while (Engine.IsActive())
            {
                MeshFactory<T>.MeshRequest<T> req = Engine.meshFactory.GetNextRequest();
                sleep = req == null;
                if (!sleep)
                {
                    if (req.terrainData == null)
                        if (!req.hasDensities)
                            req.terrainData = new VoxelData<T>();
                        else
                            req.terrainData = req.node.data;
                    Engine.meshFactory.GenerateMeshData(req);
                    lock (finishedQueue)
                        finishedQueue.Enqueue(req);
                    Thread.Sleep(4);
                }
                else
                {
                    Thread.Sleep(30);
                }
            }
        }
    }

    /*
    /// <summary>
    /// Thread used for generating densities.
    /// Currently unused due to bugs
    /// </summary>
    public class DensityThread
    {
        private Queue<DensityData> genQueue;
        private Queue<DensityData> finishedQueue;
        private Thread thread;

        public DensityThread()
        {
            genQueue = new Queue<DensityData>();
            finishedQueue = new Queue<DensityData>();

            thread = new Thread(new ThreadStart(recycleLoop));
            thread.Priority = System.Threading.ThreadPriority.BelowNormal;
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// Queue a mesh for generation.
        /// </summary>
        /// <param name="req"></param>
        public void queueRecycleDensity(DensityData req)
        {
            lock (genQueue)
                genQueue.Enqueue(req);
        }

        /// <summary>
        /// Returns a finished mesh request if there is one.
        /// </summary>
        /// <returns></returns>
        public DensityData getFinishedDensity()
        {
            if (finishedQueue.Count > 0)
                lock (finishedQueue)
                    return finishedQueue.Dequeue();

            return null;
        }

        /// <summary>
        /// Continuously check if we have chunks to generate.
        /// </summary>
        private void recycleLoop()
        {
            while (QuixelEngine.isActive())
            {
                Thread.Sleep(1);
                if (genQueue.Count > 0)
                {
                    DensityData d = null;
                    lock (genQueue)
                        d = genQueue.Dequeue();

                    d.dispose();

                    lock (finishedQueue)
                        finishedQueue.Enqueue(d);
                }
                else
                {
                    Thread.Sleep(30);
                }
            }
        }
    }*/
}