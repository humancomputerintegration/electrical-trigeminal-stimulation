using System.Collections.Generic;
using UnityEngine;

namespace Pixyz.Utils  {

    public sealed class VerticesData {

        public List<Vector3> vertices = null;
        public List<Vector2> uv1 = null;
        public List<Vector2> uv2 = null;
        public List<Vector2> uv3 = null;
        public List<Vector2> uv4 = null;
        public List<Vector3> normals = null;
        public List<Vector4> tangents = null;
        public List<Color32> colors = null;
        public List<BoneWeight> boneWeights = null;

        public int Length => vertices.Count;

        public VerticesData() {
            vertices = new List<Vector3>();
        }

        public VerticesData(Mesh aMesh) {
            vertices = CreateList(aMesh.vertices);
            uv1 = CreateList(aMesh.uv);
            uv2 = CreateList(aMesh.uv2);
            uv3 = CreateList(aMesh.uv3);
            uv4 = CreateList(aMesh.uv4);
            normals = CreateList(aMesh.normals);
            tangents = CreateList(aMesh.tangents);
            colors = CreateList(aMesh.colors32);
            boneWeights = CreateList(aMesh.boneWeights);
        }

        private List<T> CreateList<T>(T[] aSource) {
            if (aSource == null || aSource.Length == 0)
                return null;
            return new List<T>(aSource);
        }

        private void Copy<T>(ref List<T> aDest, List<T> aSource, int aIndex) {
            if (aSource == null)
                return;
            if (aDest == null)
                aDest = new List<T>();
            aDest.Add(aSource[aIndex]);
        }

        public int Add(VerticesData aOther, int aIndex) {
            int i = vertices.Count;
            Copy(ref vertices, aOther.vertices, aIndex);
            Copy(ref uv1, aOther.uv1, aIndex);
            Copy(ref uv2, aOther.uv2, aIndex);
            Copy(ref uv3, aOther.uv3, aIndex);
            Copy(ref uv4, aOther.uv4, aIndex);
            Copy(ref normals, aOther.normals, aIndex);
            Copy(ref tangents, aOther.tangents, aIndex);
            Copy(ref colors, aOther.colors, aIndex);
            Copy(ref boneWeights, aOther.boneWeights, aIndex);
            return i;
        }

        public void AssignTo(Mesh aTarget) {
            if (vertices.Count > 65535)
                aTarget.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            aTarget.SetVertices(vertices);
            if (uv1 != null) aTarget.SetUVs(0, uv1);
            if (uv2 != null) aTarget.SetUVs(1, uv2);
            if (uv3 != null) aTarget.SetUVs(2, uv3);
            if (uv4 != null) aTarget.SetUVs(3, uv4);
            if (normals != null) aTarget.SetNormals(normals);
            if (tangents != null) aTarget.SetTangents(tangents);
            if (colors != null) aTarget.SetColors(colors);
            if (boneWeights != null) aTarget.boneWeights = boneWeights.ToArray();
        }
    }
}