using System.Collections.Generic;
using UnityEngine;
using Pixyz.Config;

namespace Pixyz.Tools.Builtin
{
    public class SetEnabled : ActionInOut<IList<GameObject>, IList<GameObject>> {

        [UserParameter]
        public bool isEnabled = false;

        public override int id => 154316520;
        public override string menuPathRuleEngine => "Set/Enabled";
        public override string menuPathToolbox => null;
        public override string tooltip => "Sets GameObjects' enabled state (Enabled/Disabled).\nMake sure input contains disabled items if [Is Visible] is true.";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new System.Exception("Your license doesn't allow you to execute this action");

            foreach (GameObject gameObject in input) {
                gameObject.SetActive(isEnabled);
            }
            return input;
        }
    }
}