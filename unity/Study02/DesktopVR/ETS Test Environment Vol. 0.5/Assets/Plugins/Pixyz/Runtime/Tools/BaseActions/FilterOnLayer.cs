using System.Collections.Generic;
using UnityEngine;
using Pixyz.Config;

namespace Pixyz.Tools.Builtin {

      public class FilterOnLayer : ActionInOut<IList<GameObject>, IList<GameObject>> {

        [UserParameter]
        public LayerMask layer;

        public override int id => 456440045;
        public override string menuPathRuleEngine => "Filter/On Layer";
        public override string menuPathToolbox => null;
        public override string tooltip => "Filter input GameObjects based on its Layer.";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new NoValidLicenseException();

            List<GameObject> output = new List<GameObject>();
            foreach (GameObject gameObject in input) {
                if (gameObject.layer == layer)
                    output.Add(gameObject);
            }
            return output;
        }
    }
}