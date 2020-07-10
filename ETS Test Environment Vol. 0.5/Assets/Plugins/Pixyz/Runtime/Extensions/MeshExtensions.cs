using System.Collections.Generic;
using UnityEngine;

namespace Pixyz.Utils  {

    public static class MeshExtensions {

        /// <summary>
        /// Returns the number of vertices in a submesh.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="submesh"></param>
        /// <returns></returns>
        public static uint GetVerticesCount(this Mesh mesh, int submesh) {
            if (submesh < mesh.subMeshCount - 1) {
                return mesh.GetBaseVertex(submesh + 1) - mesh.GetBaseVertex(submesh);
            } else {
                return (uint)mesh.vertexCount - mesh.GetBaseVertex(submesh);
            }
        }

        /// <summary>
        /// Returns the polycount of a given mesh.
        /// Only counts triangles.
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static int GetPolycount(this Mesh mesh) {
            if (mesh == null)
                return 0;
            uint polycount = 0;
            for (int s = 0; s < mesh.subMeshCount; s++) {
                if (mesh.GetTopology(s) == MeshTopology.Triangles) {
                    polycount += mesh.GetIndexCount(s) / 3;
                }
            }
            return (int)polycount;
        }

        /// <summary>
        /// Extracts a submesh from a mesh to create a new mesh.
        /// The source mesh is untouched.
        /// </summary>
        /// <param name="aMesh"></param>
        /// <param name="aSubMeshIndex"></param>
        /// <returns></returns>
        public static FloatingMesh GetSubmesh(this Mesh aMesh, int aSubMeshIndex) {

            if (aSubMeshIndex < 0 || aSubMeshIndex >= aMesh.subMeshCount)
                return null;

            int[] indices = aMesh.GetIndices(aSubMeshIndex);
            MeshTopology topology = aMesh.GetTopology(aSubMeshIndex);
            //int[] indices = aMesh.GetTriangles(aSubMeshIndex);
            VerticesData source = new VerticesData(aMesh);
            VerticesData dest = new VerticesData();
            Dictionary<int, int> map = new Dictionary<int, int>();
            int[] newIndices = new int[indices.Length];
            for (int i = 0; i < indices.Length; i++) {
                int o = indices[i];
                int n;
                if (!map.TryGetValue(o, out n)) {
                    n = dest.Add(source, o);
                    map.Add(o, n);
                }
                newIndices[i] = n;
            }
            return new FloatingMesh { verticesData = dest, topology = topology, indices = newIndices };
        }
    }

    public class FloatingMesh
    {
        public VerticesData verticesData;
        public MeshTopology topology;
        public int[] indices;

        public Mesh ToMesh()
        {
            Mesh m = new Mesh();
            verticesData.AssignTo(m);
            m.SetIndices(indices, topology, 0);
            return m;
        }
    }
}