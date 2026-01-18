using System.Collections.Generic;
using UnityEngine;

namespace VoxelDestructionPro.Tools
{
    public static class MeshPool
    {
        private static readonly Stack<Mesh> Pool = new Stack<Mesh>();
        public static int MaxPoolSize = 64;

        public static Mesh Acquire()
        {
            Mesh mesh = Pool.Count > 0 ? Pool.Pop() : new Mesh();
            mesh.Clear(false);
            mesh.hideFlags = HideFlags.None;
            return mesh;
        }

        public static void Release(Mesh mesh)
        {
            if (mesh == null)
                return;

            if (Pool.Count >= MaxPoolSize)
            {
                Object.Destroy(mesh);
                return;
            }

            mesh.Clear(false);
            Pool.Push(mesh);
        }
    }
}
