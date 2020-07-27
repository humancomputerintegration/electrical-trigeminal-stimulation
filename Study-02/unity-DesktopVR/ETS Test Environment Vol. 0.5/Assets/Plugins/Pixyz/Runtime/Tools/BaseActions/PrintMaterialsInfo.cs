using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pixyz.Config;
using Pixyz.Utils;

namespace Pixyz.Tools.Builtin
{

    public class PrintMaterialsInfo : ActionInOut<IList<GameObject>, IList<GameObject>> {

        [UserParameter]
        public bool detailedLog = false;

        public override int id => 888851100;
        public override string menuPathRuleEngine => "Debug/Materials Info";
        public override string menuPathToolbox => null;
        public override string tooltip => "Prints out information on input Materials (attached to GameObjects) in the console. With 'Detail Log', this will print one information line for each Material.";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new NoValidLicenseException();

            var materials = input.GetMaterials();

            if (detailedLog) {
                foreach (Material material in materials) {
                    BaseExtensions.LogColor(Color.magenta, $"Pixyz Tools > Material Name:{material.name}, Material Shader:{material.shader}, Material Color:{material.color}");
                }
            }

            BaseExtensions.LogColor(Color.magenta, $"Pixyz Tools > Materials Count:{input.Count()}");
            return input;
        }
    }
}