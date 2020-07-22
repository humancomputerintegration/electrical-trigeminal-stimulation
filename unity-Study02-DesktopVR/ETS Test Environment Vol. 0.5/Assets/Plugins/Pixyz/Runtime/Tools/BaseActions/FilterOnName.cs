using System.Collections.Generic;
using UnityEngine;
using Pixyz.Config;

namespace Pixyz.Tools.Builtin
{

    public class FilterOnName : ActionInOut<IList<GameObject>, IList<GameObject>> {

        [UserParameter]
        public bool caseSensitive;
        [UserParameter]
        public StringConstrain valueConstrain;
        [UserParameter]
        public string value;

        public override int id => 783075375;
        public override string menuPathRuleEngine => "Filter/On Name";
        public override string menuPathToolbox => null;
        public override string tooltip => "Filter input GameObjects based on their names.";

        public enum StringConstrain {
            Contains,
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
                if (valueConstrain == StringConstrain.IsEqualTo) {
                    if ((caseSensitive ? gameObject.name : gameObject.name.ToLower()) == value)
                        output.Add(gameObject);
                } else {
                    if ((caseSensitive ? gameObject.name : gameObject.name.ToLower()).Contains(value))
                        output.Add(gameObject);
                }
            }

            return output;
        }

        public override IList<string> getErrors() {
            if (string.IsNullOrEmpty(value)) {
                if (valueConstrain == StringConstrain.Contains) {
                    return new string[] { "This condition is always met. Enter a value for filtering on Name." };
                } else {
                    return new string[] { "This condition is never met. Enter a value that matches with at least one GameObject Name." };
                }
            }
            return null;
        }
    }
}