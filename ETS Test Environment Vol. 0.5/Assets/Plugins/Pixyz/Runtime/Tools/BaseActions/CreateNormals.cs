using Pixyz.Tools.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pixyz.Config;
using Pixyz.Plugin4Unity;

namespace Pixyz.Tools.Builtin
{

    public class CreateNormals : ActionInOut<IList<GameObject>, IList<GameObject>> {

        [UserParameter]
        public double smoothingAngle = 25;

        [UserParameter]
        public bool areaWeighting = true;

        public override int id => 219151512;
        public override int order => 6;
        public override string menuPathRuleEngine => "Optimize/Create Normals";
        public override string menuPathToolbox => "Create Normals";
        public override string tooltip => "Creates normals for the given smoothing angle";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new NoValidLicenseException();

            using (new CoreContext(input.ToArray(), false)) {
                NativeInterface.CreateNormals(smoothingAngle, areaWeighting);
            }

            return input;
        }

        public override IList<string> getErrors() {
            var errors = new List<string>();
            if (smoothingAngle < 0) {
                errors.Add("Smoothing angle is too low ! (must be between 0 and 180)");
            }
            if (smoothingAngle > 180) {
                errors.Add("Smoothing angle is too high ! (must be between 0 and 180)");
            }
            return errors;
        }
    }
}