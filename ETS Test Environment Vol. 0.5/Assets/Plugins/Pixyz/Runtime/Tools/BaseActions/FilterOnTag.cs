using System.Collections.Generic;
using UnityEngine;
using Pixyz.Config;

namespace Pixyz.Tools.Builtin
{

    public class FilterOnTag : ActionInOut<IList<GameObject>, IList<GameObject>> {

        [UserParameter]
        public bool caseSensitive;

        [UserParameter]
        public string tagName;

        public override int id => 87441512;
        public override string menuPathRuleEngine => "Filter/On Tag";
        public override string menuPathToolbox => null;
        public override string tooltip => "Filter input GameObjects based on its Tag.";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new NoValidLicenseException();

            List<GameObject> output = new List<GameObject>();
            var tagNameCs = caseSensitive ? tagName : tagName.ToLower();
            foreach (GameObject gameObject in input) {
                string goName = caseSensitive ? gameObject.tag : gameObject.tag.ToLower();
                if (goName == tagNameCs)
                    output.Add(gameObject);
            }
            return output;
        }
    }
}