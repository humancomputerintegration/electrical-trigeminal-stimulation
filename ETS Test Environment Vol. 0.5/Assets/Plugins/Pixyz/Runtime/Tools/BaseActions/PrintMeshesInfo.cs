using System.Collections.Generic;
using UnityEngine;
using Pixyz.Config;
using Pixyz.Utils;

namespace Pixyz.Tools.Builtin {

  public class PrintMeshesInfo : ActionInOut<IList<GameObject>, IList<GameObject>> {

        [UserParameter]
        public bool detailedLog = false;

        public override int id => 86594984;
        public override string menuPathRuleEngine => "Debug/Meshes Info";
        public override string menuPathToolbox => null;
        public override string tooltip => "Prints out information on input Meshes (attached to GameObjects) in the console. With 'Detail Log', this will print one information line for each Mesh.";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new NoValidLicenseException();

            int ucount = 0;
            int tcount = 0;
            int vcount = 0;
            int mcount = 0;

            Dictionary<Mesh, int> meshes = new Dictionary<Mesh, int>();
            for (int i = 0; i < input.Count; i++) {
                MeshFilter meshFilter = input[i].GetComponent<MeshFilter>();
                if (!meshFilter)
                    continue;
                mcount++;
                Mesh mesh = meshFilter.sharedMesh;
                if (!mesh)
                    continue;
                int tris = mesh.GetPolycount() / 3;
                if (!meshes.ContainsKey(mesh)) {
                    meshes.Add(mesh, 1);
                    ucount += tris;
                } else {
                    meshes[mesh]++;
                }
                tcount += tris;
                vcount += mesh.vertices.Length;
            }

            if (detailedLog) {
                foreach (var pair in meshes) {
                    BaseExtensions.LogColor(Color.magenta, $"Pixyz Tools > Mesh Name:{pair.Key}, Mesh Instances:{pair.Value}");
                }
            }

            BaseExtensions.LogColor(Color.magenta, $"Pixyz Tools > Meshes Count:{mcount}, Unique Meshes Count:{meshes.Count}, Total Triangles:{tcount}, Unique Triangles:{ucount}, Total Vertices:{vcount}");
            return input;
        }
    }
}