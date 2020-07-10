using System.Collections.Generic;
using UnityEngine;
using Pixyz.Config;

namespace Pixyz.Tools.Builtin
{

    public class SetMesh : ActionInOut<IList<GameObject>, IList<GameObject>> {

        [UserParameter]
        public Mesh remplacementMesh;

        public override int id => 645120725;
        public override string menuPathRuleEngine => "Set/Mesh";
        public override string menuPathToolbox => null;
        public override string tooltip => "Set GameObjects' Mesh (if GameObject has a MeshFilter component)";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new System.Exception("Your license doesn't allow you to execute this action");

            for (int i = 0; i < input.Count; i++) {
                MeshFilter meshFilter = input[i].GetComponent<MeshFilter>();
                if (!meshFilter)
                    continue;
                meshFilter.sharedMesh = remplacementMesh;
            }
            return input;
        }

        public override IList<string> getErrors() {
            if (remplacementMesh == null)
                return new string[] { "Remplacement Mesh is required !" };
            return null;
        }
    }
}