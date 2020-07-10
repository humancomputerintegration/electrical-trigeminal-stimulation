using System.Collections.Generic;
using UnityEngine;
using Pixyz.Config;

namespace Pixyz.Tools.Builtin
{

    public class SetMaterial : ActionInOut<IList<GameObject>, IList<GameObject>> {

        [UserParameter]
        public Material material;

        public override int id => 155767537;
        public override string menuPathRuleEngine => "Set/Material";
        public override string menuPathToolbox => null;
        public override string tooltip => "Replace GameObjects' Materials with the given one.\nUse Modify > Switch Materials to switch Materials on Material name.";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new System.Exception("Your license doesn't allow you to execute this action");

            foreach (GameObject gameObject in input) {
                var meshRenderer = gameObject.GetComponent<MeshRenderer>();
                if (meshRenderer == null) {
                    continue;
                }
                var materials = meshRenderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++) {
                    materials[i] = material;
                }
                meshRenderer.sharedMaterials = materials;
            }
            return input;
        }
    }
}