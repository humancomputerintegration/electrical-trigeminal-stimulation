using Pixyz.Tools.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pixyz.Config;
using Pixyz.Utils;
using Pixyz.Plugin4Unity;

namespace Pixyz.Tools.Builtin
{

    public enum TextureQuality {
        High = 2048,
        Medium = 1024,
        Low = 512,
        Custom = 0
    }

    public class Retopologize : ActionInOut<IList<GameObject>, IList<GameObject>> {

        public enum MeshReconstructionQuality {
            VeryHigh = 200,
            High = 100,
            Medium = 50,
            Low = 25,
            Poor = 10,
            Custom = 0
        }

        public enum RetopologyMode {
            ProxyMesh,
            Voxelized,
            MarchingCubes,
        }

        private bool isProxy() => mode == RetopologyMode.ProxyMesh;
        private bool isCustomVoxelSize() => meshQuality == MeshReconstructionQuality.Custom;

        [UserParameter]
        public RetopologyMode mode = RetopologyMode.ProxyMesh;

        [UserParameter]
        public MeshReconstructionQuality meshQuality = MeshReconstructionQuality.Medium;

        [UserParameter]
        public int gridResolution = 100;

        [UserParameter]
        public bool runOncePerObject = false;

        [UserParameter]
        public bool isPointCloud = false;

        [UserParameter("isProxy")]
        public bool bakeMaps = false;

        private bool isBakingMaps() => bakeMaps && isProxy();

        [UserParameter("isBakingMaps")]
        public MapType[] mapsToBake = new MapType[] { MapType.Diffuse, MapType.Normal };

        [UserParameter("isBakingMaps")]
        public TextureQuality texturesQuality = TextureQuality.Medium;

        [UserParameter("isBakingMaps")]
        public int texturesResolution = 1024;

        public override int id => 128365740;
        public override int order => 11;
        public override string menuPathRuleEngine => "Optimize/Retopologize";
        public override string menuPathToolbox => "Retopologize";
        public override string tooltip => "Creates a remeshed version of the given mesh, following input topology";

        private MeshReconstructionQuality previousMeshQuality;
        private int previousGridReso = -1;
        private TextureQuality previousTexturesQuality;
        private int previousTexturesResolution = -1;

        public override void onBeforeDraw() {
            base.onBeforeDraw();
            BaseExtensions.MatchEnumWithCustomValue(ref previousMeshQuality, ref meshQuality, ref previousGridReso, ref gridResolution);
            BaseExtensions.MatchEnumWithCustomValue(ref previousTexturesQuality, ref texturesQuality, ref previousTexturesResolution, ref texturesResolution);
        }

        public override IList<string> getErrors() {
            var errors = new List<string>();
            if (gridResolution > 1000) {
                errors.Add("Grid resolution is too high ! (must be between 1 and 1000)");
            }
            if (gridResolution < 1) {
                errors.Add("Grid resolution is too low ! (must be between 1 and 1000)");
            }
            if (isBakingMaps()) {
                if (texturesResolution < 64) {
                    errors.Add("Textures resolution is too low ! (must be between 64 and 8192)");
                }
                if (texturesResolution > 8192) {
                    errors.Add("Textures resolution is too high ! (must be between 64 and 8192)");
                }
            }
            return errors.ToArray();
        }

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new NoValidLicenseException();

            if (!runOncePerObject) {
                Merge merge = new Merge();
                merge.keepParent = false;
                input = merge.run(input);
            }

            float voxelSize;
            if (!isCustomVoxelSize()) {
                gridResolution = (int)meshQuality;
            }

            Bounds bounds = input.GetBoundsWorldSpace();
            voxelSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) / gridResolution;

            using (new CoreContext(input.ToArray(), isBakingMaps())) {

                switch (mode) {
                    case RetopologyMode.ProxyMesh:
                        NativeInterface.Retopologize(isPointCloud, voxelSize * 1000d, bakeMaps ? new NativeInterface.MapTypeList(mapsToBake) : new NativeInterface.MapTypeList(0), texturesResolution);
                        break;
                    case RetopologyMode.MarchingCubes:
                        NativeInterface.MarchingCubes(voxelSize * 1000d);
                        break;
                    case RetopologyMode.Voxelized:
                        NativeInterface.Voxelize(voxelSize * 1000d);
                        break;
                }
            }

            if (isPointCloud) {
                // We should set an unlit shader with albedo texture if the result is a baked point cloud (because light is already in the point data)
                foreach (Material material in input.GetMaterials()) {
                    material.shader = Shader.Find("Unlit/Texture");
                }
            }

            return input;
        }
    }
}