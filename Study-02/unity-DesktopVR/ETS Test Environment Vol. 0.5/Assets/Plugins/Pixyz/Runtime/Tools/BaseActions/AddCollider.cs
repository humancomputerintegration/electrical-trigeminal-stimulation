using System.Collections.Generic;
using UnityEngine;
using Pixyz.Config;
using Pixyz.Tools.Editor;
using System.Linq;
using Pixyz.Plugin4Unity;
using Pixyz.Utils;

namespace Pixyz.Tools.Builtin
{

    public class AddCollider : ActionInOut<IList<GameObject>, IList<GameObject>> {

        public enum ColliderType {
            ProxyMesh,
            OriginalMesh,
            AxisAlignedBoundingBox,
        }

        public enum ProxyMeshQuality
        {
            VeryHigh = 200,
            High = 100,
            Medium = 50,
            Low = 25,
            Poor = 10,
            Custom = 0
        }

        [UserParameter]
        public ColliderType type = ColliderType.ProxyMesh;

        private bool isProxy() => type == ColliderType.ProxyMesh;

        [UserParameter("isProxy")]
        public ProxyMeshQuality meshQuality = ProxyMeshQuality.Medium;

        private bool isCustomVoxelSize() => meshQuality == ProxyMeshQuality.Custom;

        [UserParameter("isProxy")]
        public int gridResolution = 100;

        [UserParameter("isProxy")]
        public bool isPointCloud = false;

        public override int id => 951016102;
        public override string menuPathRuleEngine => "Add/Collider";
        public override string menuPathToolbox => "Create Collider";
        public override string tooltip => "Adds Colliders to GameObjects (if no collider is present).";

        private Dictionary<GameObject, GameObject> cloneToOriginal = new Dictionary<GameObject, GameObject>();

        private ProxyMeshQuality previousMeshQuality;
        private int previousGridReso = -1;

        public override void onBeforeDraw()
        {
            base.onBeforeDraw();
            BaseExtensions.MatchEnumWithCustomValue(ref previousMeshQuality, ref meshQuality, ref previousGridReso, ref gridResolution);
        }

        public override IList<string> getErrors()
        {
            var errors = new List<string>();
            if (gridResolution > 1000)
            {
                errors.Add("Grid resolution is too high ! (must be between 1 and 1000)");
            }
            if (gridResolution < 1)
            {
                errors.Add("Grid resolution is too low ! (must be between 1 and 1000)");
            }
            return errors.ToArray();
        }

        public override IList<GameObject> run(IList<GameObject> input) {
            if (!Configuration.CheckLicense()) throw new NoValidLicenseException();

            cloneToOriginal.Clear();

            foreach (GameObject gameObject in input) {
                MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
                if (!meshFilter)
                    continue;
                Mesh mesh = meshFilter.sharedMesh;
                MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
                if (!mesh || !meshRenderer)
                    continue;

                switch (type)
                {
                    case ColliderType.ProxyMesh:
                        var clone = GameObject.Instantiate(gameObject, gameObject.transform.parent);
                        cloneToOriginal.Add(clone, gameObject);
                        break;
                    case ColliderType.OriginalMesh:
                        MeshCollider meshCollider = gameObject.GetOrAddComponent<MeshCollider>();
                        meshCollider.sharedMesh = mesh;
                        break;
                    case ColliderType.AxisAlignedBoundingBox:
                        BoxCollider boxCollider = gameObject.GetOrAddComponent<BoxCollider>();
                        boxCollider.center = mesh.bounds.center;
                        boxCollider.size = mesh.bounds.size;
                        break;
                }
            }

            if (type == ColliderType.ProxyMesh)
            {
                float voxelSize;
                if (!isCustomVoxelSize()) {
                    gridResolution = (int)meshQuality;
                }

                Bounds bounds = input.GetBoundsWorldSpace();
                voxelSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) / gridResolution;

                using (new CoreContext(cloneToOriginal.Keys.ToArray(), false))
                {
                    NativeInterface.Retopologize(isPointCloud, voxelSize * 1000d, null, 1024);
                }

                foreach (var pair in cloneToOriginal)
                {
                    MeshFilter meshFilter = pair.Key.GetComponent<MeshFilter>();
                    if (!meshFilter)
                        continue;
                    Mesh mesh = meshFilter.sharedMesh;
                    mesh.name = "Collider_" + mesh.GetInstanceID();

                    if (mesh)
                    {
                        MeshCollider collider = pair.Value.GetOrAddComponent<MeshCollider>();
                        collider.sharedMesh = mesh;
                    }
                }

                foreach (var pair in cloneToOriginal)
                {
                    GameObject.DestroyImmediate(pair.Key);
                }

                cloneToOriginal.Clear();
            }

            return input;
        }
    }
}