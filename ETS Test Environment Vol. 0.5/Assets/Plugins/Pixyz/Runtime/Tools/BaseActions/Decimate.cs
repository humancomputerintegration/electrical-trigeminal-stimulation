using Pixyz.Tools.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pixyz.Config;
using Pixyz.Utils;
using Pixyz.Plugin4Unity;

namespace Pixyz.Tools.Builtin
{
    public class Decimate : ActionInOut<IList<GameObject>, IList<GameObject>> {

        public enum DecimationCriterion {
            ToQuality,
            ToPolycount,
            ToPolycountRatio,
        }

        public enum DecimationQuality {
            VeryHigh = 0,
            High = 1,
            Medium = 2,
            Low = 3,
            Poor = 4
        }

        [UserParameter]
        public DecimationCriterion criterion = DecimationCriterion.ToQuality;

        private bool isPolycountRatioOrPolycount() => isPolycountRatio() || isPolycount();
        private bool isPolycountRatio() => criterion == DecimationCriterion.ToPolycountRatio;
        private bool isPolycount() => criterion == DecimationCriterion.ToPolycount;
        private bool isTopology() => criterion == DecimationCriterion.ToQuality;

        [UserParameter("isTopology")]
        public DecimationQuality quality = DecimationQuality.Medium;

        [UserParameter("isPolycountRatio")]
        public Range targetRatio = (Range)50f;

        [UserParameter("isPolycount")]
        public int targetPolycount = 5000;

        public override int id => 15654563;
        public override int order => 1;
        public override string menuPathRuleEngine => "Optimize/Decimate";
        public override string menuPathToolbox => "Decimate";
        public override string tooltip => "Reduces the number of polygons on the Meshes, using a specific criterion.";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new NoValidLicenseException();

            using (new CoreContext(input.ToArray(), false)) {
                switch (criterion) {
                    case DecimationCriterion.ToQuality:
                        NativeInterface.Decimate((MeshQuality)(int)quality);
                        break;
                    case DecimationCriterion.ToPolycount:
                        NativeInterface.DecimateTarget(targetPolycount);
                        break;
                    case DecimationCriterion.ToPolycountRatio:
                        int polycount = 0;
                        Mesh[] meshes = input.GetMeshes();
                        foreach (Mesh mesh in meshes) {
                            polycount += mesh.GetPolycount();
                        }
                        polycount = (int)Mathf.Clamp(0.01f * targetRatio * polycount, 1, polycount);
                        NativeInterface.DecimateTarget(polycount);
                        break;
                }
            }

            //Undo.FlushUndoRecordObjects();

            return input;
        }
    }
}