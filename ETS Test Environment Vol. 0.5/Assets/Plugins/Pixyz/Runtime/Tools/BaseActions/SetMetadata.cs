using Pixyz.Import;
using System.Collections.Generic;
using UnityEngine;
using Pixyz.Config;

namespace Pixyz.Tools.Builtin
{

    public class SetMetadata : ActionInOut<IList<GameObject>, IList<GameObject>> {

        [UserParameter]
        public bool addComponentIfMissing = true;
        [UserParameter]
        public string propertyName;
        [UserParameter]
        public string propertyValue;

        public override int id => 994562210;
        public override string menuPathRuleEngine => "Set/Metadata";
        public override string menuPathToolbox => null;
        public override string tooltip => "Sets GameObjects' Metadata (Pixyz) property";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new System.Exception("Your license doesn't allow you to execute this action");

            for (int i = 0; i < input.Count; i++) {

                Metadata metadata = input[i].GetComponent<Metadata>();

                if (addComponentIfMissing) {
                    if (!metadata)
                        metadata = input[i].AddComponent<Metadata>();
                } else {
                    if (!metadata)
                        continue;
                }

                //if (metadata.containsProperty(propertyName)) {
                metadata.addOrSetProperty(propertyName, propertyValue);
                //}
            }
            return input;
        }
    }
}