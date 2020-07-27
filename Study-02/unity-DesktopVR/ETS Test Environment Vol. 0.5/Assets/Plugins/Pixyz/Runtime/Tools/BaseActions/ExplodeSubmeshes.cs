using System.Collections.Generic;
using UnityEngine;
using Pixyz.Config;
using Pixyz.Utils;

namespace Pixyz.Tools.Builtin {

      public class ExplodeSubmeshes : ActionInOut<IList<GameObject>, IList<GameObject>> {

        public override int id => 91540003;
        public override int order => 3;
        public override string menuPathRuleEngine => "Scene/Explode";
        public override string menuPathToolbox => "Explode";
        public override string tooltip => "Explodes each GameObject in multiple GameObjects depending on its submesh count (if it has a Mesh)";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new NoValidLicenseException();
            List<GameObject> output = new List<GameObject>();
            foreach (GameObject gameObject in input) {
                output.Add(gameObject);
                output.AddRange(SceneExtensions.ExplodeSubmeshes(gameObject, false));
            }
            return output;
        }
    }
}