using Pixyz.Tools.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pixyz.Config;
using Pixyz.Plugin4Unity;

namespace Pixyz.Tools.Builtin
{

    public class CreateUVs : ActionInOut<IList<GameObject>, IList<GameObject>> {

        [UserParameter]
        public bool useLocalBoundingBox = true;

        [UserParameter]
        public float uvSize = 1f;

        [UserParameter]
        public int uvChannel = 0;

        public override int id => 451666;
        public override int order => 4;
        public override string menuPathRuleEngine => "Modify/Create UVs";
        public override string menuPathToolbox => "Create UVs";
        public override string tooltip => "Create projected UVs. As this function changes data at Mesh level, any modification to a Mesh will be visible for each GameObject using that Mesh, regardless of the input.";

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new NoValidLicenseException();

            using (new CoreContext(input.ToArray(), false)) {
                var occurrences = new NativeInterface.OccurrenceList( new uint[] { NativeInterface.GetSceneRoot() });
                NativeInterface.MapUvOnAABB(occurrences, useLocalBoundingBox, uvSize, uvChannel, true);
                //Plugin4UnityI.CreateTangents(occurrences, -1, uvChannel, true);
            }

            return input;
        }
    }
}