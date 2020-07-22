using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pixyz.Config;

namespace Pixyz.Tools.Builtin
{

    public class FilterOnMaterial : ActionInOut<IList<GameObject>, IList<GameObject>> {

        [UserParameter]
        public bool caseSensitive;

        [UserParameter]
        public StringConstrain valueConstrain;

        [UserParameter]
        public string value;

        public override int id => 156420455;
        public override string menuPathRuleEngine => "Filter/On Material";
        public override string menuPathToolbox => null;
        public override string tooltip => "Filter input GameObjects based on its Materials names.\nIf constrain is [Contains], GameObjects without any Materials or without at least one Material whose name contains [Value] will be filtered out.\nIf constrain is [Does Not Contains], GameObjects with at least one Material whose name contains [Value] will be filtered out.\nIf constrain is [Is Equal To], GameObjects without any Materials or without at least one Material whose name is equal to [Value] will be filtered out.";

        public enum StringConstrain {
            Contains,
            DoesNotContain,
            IsEqualTo,
        }

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new NoValidLicenseException();

            List<GameObject> output = new List<GameObject>();

            if (value == null)
                value = "";

            if (!caseSensitive)
                value = value.ToLower();

            foreach (GameObject gameObject in input) {

                MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
                if (renderer == null)
                    continue;

                foreach (Material material in renderer.sharedMaterials) {
                    if (valueConstrain == StringConstrain.IsEqualTo) {
                        if ((caseSensitive ? material.name : material.name.ToLower()) == value) {
                            output.Add(gameObject);
                            break;
                        }
                    } else {
                        if ((caseSensitive ? material.name : material.name.ToLower()).Contains(value)) {
                            output.Add(gameObject);
                            break;
                        }
                    }
                }
            }

            if (valueConstrain == StringConstrain.DoesNotContain) {
                output = input.Except(output).ToList();
            }

            return output;
        }
    }
}