using System.Collections.Generic;
using UnityEngine;
using Pixyz.Config;

namespace Pixyz.Tools.Builtin
{

    public class SetLayer : ActionInOut<IList<GameObject>, IList<GameObject>> {

        [UserParameter]
        public LayerMask layer;

        public override int id => 1697425464;
        public override string menuPathRuleEngine => "Set/Layer";
        public override string menuPathToolbox => null;
        public override string tooltip => "Sets GameObjects' layer";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new System.Exception("Your license doesn't allow you to execute this action");

            for (int i = 0; i < input.Count; i++) {
                input[i].layer = layer;
            }
            return input;
        }
    }
}