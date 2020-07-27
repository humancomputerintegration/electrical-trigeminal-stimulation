using Pixyz.Tools.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pixyz.Config;
using Pixyz.Plugin4Unity;

namespace Pixyz.Tools.Builtin
{

    public class RepairMesh : ActionInOut<IList<GameObject>, IList<GameObject>> {

        [UserParameter]
        public double tolerance = 0.1;

        [UserParameter]
        public bool crackNonManifold = true;

        [UserParameter]
        public bool orient = false;

        public override int id => 94374481;
        public override int order => 10;
        public override string menuPathRuleEngine => "Optimize/Repair Mesh";
        public override string menuPathToolbox => "Repair Mesh";
        public override string tooltip => "Attempts a mesh repair.";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new NoValidLicenseException();

            using (new CoreContext(input.ToArray(), false)) {
                NativeInterface.RepairMesh(new NativeInterface.OccurrenceList(new uint[] { 1 }), tolerance, crackNonManifold, orient);
            }

            return input;
        }

        public override IList<string> getErrors() {
            var errors = new List<string>();
            if (tolerance <= 0) {
                errors.Add("Tolerance is too low ! (must be between above 0)");
            }
            return errors;
        }
    }
}