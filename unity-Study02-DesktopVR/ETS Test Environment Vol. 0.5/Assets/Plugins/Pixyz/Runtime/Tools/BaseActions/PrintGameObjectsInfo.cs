using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pixyz.Config;
using Pixyz.Utils;

namespace Pixyz.Tools.Builtin
{
    public class PrintGameObjectsInfo : ActionInOut<IList<GameObject>, IList<GameObject>> {

        [UserParameter]
        public bool detailedLog = false;

        public override int id => 848484845;
        public override string menuPathRuleEngine => "Debug/GameObjects Info";
        public override string menuPathToolbox => null;
        public override string tooltip => "Prints out information on input GameObjects in the console. With 'Detail Log', this will print one information line for each GameObject.";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new NoValidLicenseException();

            if (detailedLog) {
                foreach (GameObject gameObject in input) {
                    BaseExtensions.LogColor(Color.magenta, $"Pixyz Tools > GameObject Name:{gameObject.name}");
                }
            }
            BaseExtensions.LogColor(Color.magenta, $"Pixyz Tools > GameObjects Count:{input.Count()}");
            return input;
        }
    }
}