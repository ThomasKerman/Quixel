using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using System.IO;

namespace Quixel
{
    public class VoxelData<T>
    {
        /// <summary> Reference to another density data that contains the change in densities used for paging. </summary>
        private VoxelData<T> changeData;

        /// <summary> Terrain values </summary>
        public T[, ,] Values;

        /// <summary>
        /// Creates a new instance 
        /// </summary>
        public VoxelData()
        {
            Values = new T[19, 19, 19];
        }

        /// <summary>
        /// Returns the terrain value at the given coordinates.
        /// </summary>
        public T Get(int x, int y, int z)
        {
            if (changeData != null)
                if (!changeData.Get(x, y, z).Equals(default(T)))
                    return changeData.Get(x, y, z);
            return Values[x + 1, y + 1, z + 1];
        }

        /// <summary>
        /// Returns the density value at the given coordinates.
        /// </summary>
        public T Get(Vector3I pos)
        {
            if (changeData != null)
                if (!changeData.Get(pos).Equals(default(T)))
                    return changeData.Get(pos);
            return Get(pos.x, pos.y, pos.z);
        }

        /// <summary>
        /// Sets the density value at the given coordinates.
        /// </summary>
        public void Set(int x, int y, int z, T val)
        {
            Values[x + 1, y + 1, z + 1] = val;
        }

        /// <summary>
        /// Sets the density value at the given coordinates.
        /// </summary>
        public void Set(Vector3I pos, T val)
        {
            Set(pos.x, pos.y, pos.z, val);
        }

        /// <summary>
        /// Applies changes (additive) using another Density Data
        /// </summary>
        /// <param name="other"></param>
        public void ApplyChanges(VoxelData<T> other)
        {
            for (int x = 0; x < 19; x++)
                for (int y = 0; y < 19; y++)
                    for (int z = 0; z < 19; z++)
                        if (!other.Values[x, y, z].Equals(default(T)))
                            Values[x, y, z] = other.Values[x, y, z];
        }

        /// <summary>
        /// Sets the change data for this density array.
        /// Values are pulled from here if available.
        /// </summary>
        public void SetChangeData(VoxelData<T> data)
        {
            changeData = data;
        }

        /// <summary>
        /// Disposes of the density array.
        /// </summary>
        public void Dispose()
        {
            for (int x = 0; x < 19; x++)
                for (int y = 0; y < 19; y++)
                    for (int z = 0; z < 19; z++)
                        Values[x, y, z] = default(T);
        }
    }

    public struct Triangle
    {
        public Vector3 pointOne, pointTwo, pointThree;
        public Vector3 nOne, nTwo, nThree;

        /// <summary>
        /// Creates a triangle consisting of 6 vectors. 3 for points, 3 for normals.
        /// </summary>
        /// <param name="PointOne"></param>
        /// <param name="PointTwo"></param>
        /// <param name="PointThree"></param>
        /// <param name="nOne"></param>
        /// <param name="nTwo"></param>
        /// <param name="nThree"></param>
        public Triangle(Vector3 PointOne, Vector3 PointTwo, Vector3 PointThree, Vector3 nOne, Vector3 nTwo, Vector3 nThree)
        {
            this.pointOne = PointOne;
            this.pointTwo = PointTwo;
            this.pointThree = PointThree;

            this.nOne = nOne;
            this.nTwo = nTwo;
            this.nThree = nThree;
        }
    }

    public struct Vector3I
    {
        public int x;
        public int y;
        public int z;

        public Vector3I(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Checks equality with another Vector3I object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Vector3I o = (Vector3I)obj;
            return (x == o.x && y == o.y && z == o.z);
        }

        /// <summary>
        /// Only because Unity complained if I didn't
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Adds another vector's values to this
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public Vector3I Add(Vector3I other)
        {
            return new Vector3I(x + other.x, y + other.y, z + other.z);
        }

        /// <summary>
        /// Subtracts another vectory's values from this
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public Vector3I Subtract(Vector3I other)
        {
            return new Vector3I(x - other.x, y - other.y, z - other.z);
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("({0},{1},{2})", x, y, z);
        }
    }

    public class MeshData
    {
        public Vector3[] triangleArray;
        public Vector2[] uvArray;
        public int[][] indexArray;
        public Vector3[] normalArray;

        /// <summary>
        /// Disposes the meshdata.
        /// </summary>
        public void Dispose()
        {
            triangleArray = null;
            uvArray = null;
            indexArray = null;
            normalArray = null;
        }
    }
}