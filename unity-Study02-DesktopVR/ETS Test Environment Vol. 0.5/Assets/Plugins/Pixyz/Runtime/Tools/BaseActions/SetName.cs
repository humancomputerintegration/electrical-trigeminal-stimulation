using System.Collections.Generic;
using UnityEngine;
using Pixyz.Config;

namespace Pixyz.Tools.Builtin
{

    public class SetName : ActionInOut<IList<GameObject>, IList<GameObject>> {

        [UserParameter]
        public string newName;

        public override int id => 647578605;
        public override string menuPathRuleEngine => "Set/Name";
        public override string menuPathToolbox => null;
        public override string tooltip => "Sets GameObjects' names";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new System.Exception("Your license doesn't allow you to execute this action");

            //string[] oldNames = new string[input.Count];
            for (int i = 0; i < input.Count; i++) {
                //oldNames[i] = input[i].name;
                input[i].name = newName;
            }
            //action = () => {
            //    for (int i = 0; i < input.Count; i++) {
            //        input[i].name = oldNames[i];
            //    }
            //};
            return input;
        }

        public override IList<string> getErrors() {
            if (string.IsNullOrEmpty(newName))
                return new string[] { "New name can't be empty !" };
            return null;
        }
    }
}