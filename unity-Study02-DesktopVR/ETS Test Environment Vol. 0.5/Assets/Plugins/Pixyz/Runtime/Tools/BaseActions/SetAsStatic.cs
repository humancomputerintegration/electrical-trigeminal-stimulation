using System.Collections.Generic;
using UnityEngine;
using Pixyz.Config;

namespace Pixyz.Tools.Builtin
{
    public class SetAsStatic : ActionInOut<IList<GameObject>, IList<GameObject>> {

        [UserParameter]
        public bool isStatic = true;

        public override int id => 415461000;
        public override string menuPathRuleEngine => "Set/As Static";
        public override string menuPathToolbox => null;
        public override string tooltip => "Makes GameObjects Static (or not)";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new System.Exception("Your license doesn't allow you to execute this action");

            foreach (GameObject gameObject in input) {
                gameObject.isStatic = isStatic;
            }
            return input;
        }
    }
}