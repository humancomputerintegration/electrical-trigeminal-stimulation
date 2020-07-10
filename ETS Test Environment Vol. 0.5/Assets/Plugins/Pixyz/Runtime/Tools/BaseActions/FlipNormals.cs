using System.Collections.Generic;
using UnityEngine;
using Pixyz.Config;
using Pixyz.Utils;

namespace Pixyz.Tools.Builtin {

      public class FlipNormals : ActionInOut<IList<GameObject>, IList<GameObject>> {

        [UserParameter]
        public bool includeTriangleNormals = true;

        public override int id => 10191513;
        public override int order => 7;
        public override string menuPathRuleEngine => "Modify/Flip Normals";
        public override string menuPathToolbox => "Flip Normals";
        public override string tooltip => "Flips Meshes Normals. As this function changes data at Mesh level, any modification to a Mesh will be visible for each GameObject using that Mesh, regardless of the input.";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new NoValidLicenseException();

            foreach (Mesh mesh in input.GetMeshesUnique()) {
                if (includeTriangleNormals) {
                    for (int s = 0; s < mesh.subMeshCount; s++) {
                        if (mesh.GetTopology(s) != MeshTopology.Triangles)
                            continue;
                        int[] triangles = mesh.GetTriangles(s);
                        for (int i = 0; i < triangles.Length; i += 3) {
                            int x = triangles[i];
                            triangles[i] = triangles[i + 1];
                            triangles[i + 1] = x;
                        }
                        mesh.SetTriangles(triangles, s);
                    }

                }
                Vector3[] normals = mesh.normals;
                for (int n = 0; n < normals.Length; n++) {
                    normals[n] = -normals[n];
                }
                mesh.normals = normals;
            }
            return input;
        }
    }
}