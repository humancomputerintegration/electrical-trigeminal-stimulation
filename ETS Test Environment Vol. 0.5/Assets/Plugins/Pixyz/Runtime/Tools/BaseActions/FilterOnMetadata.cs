using Pixyz.Import;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Pixyz.Config;

namespace Pixyz.Tools.Builtin
{

    public class FilterOnMetadata : ActionInOut<IList<GameObject>, IList<GameObject>> {

        public struct PropName {
            public enum PropertyNameConstrain {
                Contains,
                IsEqualTo,
            }
            public PropertyNameConstrain constrain;
            public string value;
        }

        public struct PropValue {
            public enum PropertyValueConstrain {
                Contains,
                IsEqualTo,
                IsGreaterThan,
                IsLowerThan,
                IsGreaterOrEqualTo,
                IsLowerOrEqualTo,
            }
            public PropertyValueConstrain constrain;
            public string value;
        }

        [UserParameter]
        public bool caseSensitive;

        [UserParameter]
        public PropName propertyName;

        [UserParameter]
        public PropValue propertyValue;

        public override int id => 523752222;
        public override string menuPathRuleEngine => "Filter/On Property";
        public override string menuPathToolbox => null;
        public override string tooltip => "Filter input GameObjects based on their Metadata (Component created by Pixyz at import time).";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new NoValidLicenseException();

            if (propertyName.value == null)
                propertyName.value = "";

            if (propertyValue.value == null)
                propertyValue.value = "";

            List<GameObject> output = new List<GameObject>();
            for (int i = 0; i < input.Count; i++) {
                Metadata metadata = input[i].GetComponent<Metadata>();
                if (!metadata)
                    continue;

                var properties = metadata.getProperties();
                if (!caseSensitive) {
                    Dictionary<string, string> lowerCaseProps = new Dictionary<string, string>();
                    foreach (var pair in properties) {
                        if (!lowerCaseProps.ContainsKey(pair.Key.ToLower())) // Security. Metadata may have two properties with the same name.
                            lowerCaseProps.Add(pair.Key.ToLower(), pair.Value.ToLower());
                    }
                    properties = lowerCaseProps;
                    propertyName.value = propertyName.value.ToLower();
                    propertyValue.value = propertyValue.value.ToLower();
                }

                string value;

                if (propertyName.constrain == PropName.PropertyNameConstrain.IsEqualTo) {
                    if (!properties.TryGetValue(propertyName.value, out value))
                        continue;
                    if (checkValue(value, propertyValue))
                        output.Add(input[i]);
                } else {
                    foreach (var pair in properties) {
                        /// First one ? Keep if true ?
                        if (pair.Key.Contains(propertyName.value)) {
                            if (checkValue(pair.Value, propertyValue)) {
                                output.Add(input[i]);
                                break;
                            }
                        }
                    }
                }
            }
            return output;
        }

        private bool checkValue(string value, PropValue propertyValue) {
            switch (propertyValue.constrain) {
                case PropValue.PropertyValueConstrain.Contains:
                    if (value.Contains(propertyValue.value))
                        return true;
                    break;
                case PropValue.PropertyValueConstrain.IsEqualTo:
                    if (value == propertyValue.value)
                        return true;
                    break;
                case PropValue.PropertyValueConstrain.IsGreaterThan:
                    double propValue;
                    double queryValue;
                    double.TryParse(propertyValue.value.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out queryValue);
                    double.TryParse(value.Replace(',', '.'), out propValue);
                    if (queryValue < propValue)
                        return true;
                    break;
                case PropValue.PropertyValueConstrain.IsLowerThan:
                    double propValue2;
                    double queryValue2;
                    double.TryParse(propertyValue.value.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out queryValue2);
                    double.TryParse(value.Replace(',', '.'), out propValue2);
                    if (queryValue2 > propValue2)
                        return true;
                    break;
                case PropValue.PropertyValueConstrain.IsGreaterOrEqualTo:
                    double propValue3;
                    double queryValue3;
                    double.TryParse(propertyValue.value.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out queryValue3);
                    double.TryParse(value.Replace(',', '.'), out propValue3);
                    if (queryValue3 <= propValue3)
                        return true;
                    break;
                case PropValue.PropertyValueConstrain.IsLowerOrEqualTo:
                    double propValue4;
                    double queryValue4;
                    double.TryParse(propertyValue.value.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out queryValue4);
                    double.TryParse(value.Replace(',', '.'), out propValue4);
                    if (queryValue4 >= propValue4)
                        return true;
                    break;
            }
            return false;
        }
    }
}