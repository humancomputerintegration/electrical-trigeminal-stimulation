using Pixyz.Plugin4Unity;
using UnityEngine;

namespace Pixyz.Import {

    public enum ParameterAvailability {
        Available = 0,
        Locked = 1,
        Hidden = 2,
    }

    /// <summary>
    /// A wrapper for a parameter. This allows the creation of presets and custom behaviours for classes that require user configurable parameter setup.
    /// </summary>
    /// <typeparam name="T">Type for this parameter</typeparam>
    public struct Parameter<T> {
        public ParameterAvailability status;
        public T defaultValue;
        public string name;
        public string tooltip;
    }

    /// <summary>
    /// This is a template for the import settings.
    /// An instance can be created and registered for a given file format through the Importer class to enable custom behaviours.
    /// </summary>
    public struct ImportSettingsTemplate {

        public string name;
        public Parameter<bool> loadMetadata;
        public Parameter<bool> importLines;
        public Parameter<bool> importPatchBorders;
        public Parameter<bool> importPoints;
        public Parameter<float> scaleFactor;
        public Parameter<bool> isLeftHanded;
        public Parameter<bool> isZUp;
        public Parameter<bool> mergeFinalLevel;
        public Parameter<TreeProcessType> treeProcess;
        public Parameter<int> voxelizeGridSize;
        public Parameter<bool> hasLODs;
        public Parameter<LodGroupPlacement> lodsMode;
        public Parameter<LodsGenerationSettings> qualities;
        public Parameter<MeshQuality> quality;
        public Parameter<bool> mapUV;
        public Parameter<float> mapUV3dSize;
        public Parameter<bool> createLightmapUV;
        public Parameter<int> lightmapResolution;
        public Parameter<int> uvPadding;
        public Parameter<bool> orient;
        public Parameter<bool> splitTo16BytesIndex;
        public Parameter<Shader> shader;
        public Parameter<bool> useMaterialsInResources;
        public Parameter<bool> singularizeSymmetries;
        public Parameter<bool> repair;
        public Parameter<bool> combinePatchesByMaterial;
        public Parameter<bool> importVariants;

        /// <summary>
        /// The default template for the ImportSettings
        /// </summary>
        public static readonly ImportSettingsTemplate Default = new ImportSettingsTemplate {
            name = "Default",
            loadMetadata = new Parameter<bool> { defaultValue = true, name = "Import Metadata", tooltip = "Import metadata from the imported model. Only available fi model has metadata and no processing is applied on the hierarchy." },
            importLines = new Parameter<bool> { defaultValue = false, name = "Import Lines", tooltip = "Import lines such as laser marking. Only works if model has lines." },
            importPatchBorders = new Parameter<bool> { defaultValue = false, name = "Import Patch Boundaries", tooltip = "Import patch boundaries as lines. Only works if model is a CAD model (has BReps)." },
            importPoints = new Parameter<bool> { defaultValue = false, name = "Import Points", tooltip = "Import points, also called 'free vertices'. Points can be CAD position information or point cloud data for instance." },
            scaleFactor = new Parameter<float> { defaultValue = 0.001f, name = "Scale", tooltip = "Scale factor applied to the whole assembly." },
            isLeftHanded = new Parameter<bool> { defaultValue = false, name = "Left Handed", tooltip = "Use this setting if the model was designed in a Left Handed environment." },
            isZUp = new Parameter<bool> { defaultValue = true, name = "Z-up", tooltip = "Use this setting if the model was designed in a Z-up environment." },
            mergeFinalLevel = new Parameter<bool> { defaultValue = false, name = "Merge Final Level", tooltip = "Merge objects on the last levels of the hierarchy. This will for instance merge brep patches contained in the same geometrical set." },
            voxelizeGridSize = new Parameter<int> { status = ParameterAvailability.Hidden, defaultValue = 0, name = "Grid Size", tooltip = "The point cloud is divided in a set of smaller point clouds to allow speedups thanks to Unity frustrum culling (objects outside of the field of view are not renderered).\n" },
            treeProcess = new Parameter<TreeProcessType> { defaultValue = TreeProcessType.FULL, name = "Hierarchy", tooltip = "Use these functions to simplify the tree(or hierarchy) of your model.\n" +
                "> Full: Hierarchy is fully kept.\n" +
                "> Clean-up intermediary nodes: Compresses the hierarchy by removing empty nodes, or any node containing only one sub-node.\n" +
                "> Transfer all under root: Simplifies the hierarchy by transferring all renderers under the root node of the imported model.\n" +
                "> Merge all: All objects contained in the original model will be merged together, as one single object.\n" +
                "> Merge by material: All objects contained in the original model that share the same material will be merged together." },
            hasLODs = new Parameter<bool> { defaultValue = false, name = "Create LODs", tooltip = "The LODs (Level Of Detail) is an optimization feature that allows Unity to automatically switch a model quality depending on its occupancy relatively to the screen (in %)." },
            lodsMode = new Parameter<LodGroupPlacement> { defaultValue = LodGroupPlacement.ROOT, name = "LODs Mode", tooltip = "The LODs level tells if the LOD should be managed at assembly (Root) or at model (Leaves) level." },
            qualities = new Parameter<LodsGenerationSettings> { defaultValue = LodsGenerationSettings.Default(), name = "LODs", tooltip = "The LODs settings describes LODs to be generated with their associated threshold and quality." },
            quality = new Parameter<MeshQuality> { defaultValue = MeshQuality.MAXIMUM, name = "Mesh Quality", tooltip = "Quality defines the density of the mesh that Pixyz creates.\nDepending if you import a CAD model (exact geometry) or a mesh model (tessellated geometry), Pixyz will either perform a Tessellation or a Decimation on the model (see documentation for more information and presets details)." },
            mapUV = new Parameter<bool> { defaultValue = false, name = "Create Ch.0 UVs", tooltip = "Create a new projected primary UV set (channel #0).\n" +
                "Caution: Pixyz will override the existing UV set. Do not use this setting if you wish to preserve the UVs embedded in the imported model." },
            mapUV3dSize = new Parameter<float> { defaultValue = 1f, name = "UVs Size", tooltip = "The size of the projection box used to create UVs." },
            createLightmapUV = new Parameter<bool> { defaultValue = false, name = "Create UVs for Lightmapper", tooltip = "Create UVs for the lightmapper, so that light can be properly stored after being computed by Unity." },
            lightmapResolution = new Parameter<int> { defaultValue = 1024, name = "Resolution", tooltip = "Resolution (in pixels)." },
            uvPadding = new Parameter<int> { defaultValue = 4, name = "Padding", tooltip = "Set the padding (in pixels) between UV islands." },
            orient = new Parameter<bool> { defaultValue = true, name = "Re-orient Faces", tooltip = "Orient normals of adjacent faces consistently (unification of all triangles orientation)." },
            splitTo16BytesIndex = new Parameter<bool> { defaultValue = false, name = "Use 16bit Buffers", tooltip = "Use this setting if you wish to create meshes limited to 65k vertices. Meshes will be created with a 16 bit index buffer (32 bit by default).\n" +
                "Consider using this setting if you wish to publish the model on platform with limited power." },
            shader = new Parameter<Shader> { defaultValue = null, name = "Override Shader", tooltip = "If specified, the import will use the given shader to create materials contained in the imported model. If not specified, it will use the default shader for the current Render Pipeline." },
            useMaterialsInResources = new Parameter<bool> { defaultValue = false, name = "Use Materials in Resources", tooltip = "If true, Pixyz will attempt to load materials whose name match the name of the imported one.\n" +
                "Materials to load needs to be placed at the root of a Resources folder, somewhere in the project Assets." },
            singularizeSymmetries = new Parameter<bool> { defaultValue = true, name = "Singularize Symmetries", tooltip = "If true, all GameObjects that are considered as symmetries will have their transform baked in their mesh vertices.\n" +
                "This will break instances for those GameObject's meshes, but avoid negative scales in transform that could cause issues such as lightmapping issues." },
            repair = new Parameter<bool> { defaultValue = true, name = "Repair", status = ParameterAvailability.Hidden, tooltip = "Repairs breps and meshes." },
            combinePatchesByMaterial = new Parameter<bool> { defaultValue = true, name = "Combine Patches by Material", status = ParameterAvailability.Hidden, tooltip = "Combine patches (or submeshes) by materials." },
        };
    }
}