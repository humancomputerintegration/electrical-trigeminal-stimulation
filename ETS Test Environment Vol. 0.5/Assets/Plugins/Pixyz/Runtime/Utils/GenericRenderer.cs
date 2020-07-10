using UnityEngine;

namespace Pixyz.Utils
{

    public class GenericRenderer
    {
        public Mesh mesh;
        public Material[] materials;

        public bool IsValid => mesh.subMeshCount > 0 && mesh.subMeshCount == materials.Length;

        public GenericRenderer(Mesh mesh, Material[] materials)
        {
            this.mesh = mesh;
            this.materials = materials;
        }

        public GenericRenderer(MeshFilter meshFilter, MeshRenderer meshRenderer)
            : this(meshFilter?.sharedMesh, meshRenderer?.sharedMaterials ?? new Material[0])
        {
        }

        public GenericRenderer(SkinnedMeshRenderer skinnedMeshRenderer)
            : this(skinnedMeshRenderer.sharedMesh, skinnedMeshRenderer.sharedMaterials)
        {
        }

        public void AssignTo(SkinnedMeshRenderer skinnedMeshRenderer)
        {
            skinnedMeshRenderer.sharedMesh = mesh;
            skinnedMeshRenderer.sharedMaterials = materials;
        }

        public void AssignTo(GameObject gameObject)
        {
            SkinnedMeshRenderer skmr = gameObject.GetComponent<SkinnedMeshRenderer>();
            if (skmr) {
                skmr.sharedMesh = mesh;
                skmr.sharedMaterials = materials;
            } else {
                MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
                MeshFilter mf = gameObject.GetComponent<MeshFilter>();
                if (mr && mf) {
                    mf.sharedMesh = mesh;
                    mr.sharedMaterials = materials;
                }
            }
        }

        public void AssignMeshTo(GameObject gameObject)
        {
            SkinnedMeshRenderer skmr = gameObject.GetComponent<SkinnedMeshRenderer>();
            if (skmr) {
                skmr.sharedMesh = mesh;
            } else {
                var mf = gameObject.GetComponent<MeshFilter>();
                if (mf) {
                    mf.sharedMesh = mesh;
                }
            }
        }

        public void AssignMaterialsTo(GameObject gameObject)
        {
            SkinnedMeshRenderer skmr = gameObject.GetComponent<SkinnedMeshRenderer>();
            if (skmr) {
                skmr.sharedMaterials = materials;
            } else {
                var mr = gameObject.GetComponent<MeshRenderer>();
                if (mr) {
                    mr.sharedMaterials = materials;
                }
            }
        }

        public void AssignTo(MeshFilter meshFilter, MeshRenderer meshRenderer)
        {
            meshFilter.sharedMesh = mesh;
            meshRenderer.sharedMaterials = materials;
        }

        public static GenericRenderer CreateFrom(GameObject gameObject)
        {
            SkinnedMeshRenderer skmr = gameObject.GetComponent<SkinnedMeshRenderer>();
            if (skmr) {
                return new GenericRenderer(skmr.sharedMesh, skmr.sharedMaterials);
            } else {
                MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
                MeshFilter mf = gameObject.GetComponent<MeshFilter>();
                if (mr && mf) {
                    return new GenericRenderer(mf.sharedMesh, mr.sharedMaterials);
                } else {
                    return null;
                }
            }
        }
    }
}