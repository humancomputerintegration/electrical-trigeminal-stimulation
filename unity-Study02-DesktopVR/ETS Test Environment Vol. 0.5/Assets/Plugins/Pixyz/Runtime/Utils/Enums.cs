namespace Pixyz.Import {

    /// <summary>
    /// Mode to use for LODs.
    /// </summary>
    public enum LodGroupPlacement {
        /// <summary>
        /// The LOD Group is placed on the root node of the imported model. Use this setting if you wish to control the global visibility of the entire imported model at once.
        /// </summary>
        ROOT = 0,
        /// <summary>
        /// The LOD Group is placed on the parent-node of each mesh (or object) existing in the hierarchy. Use this setting if you wish to control the visibility of each sub-part of the imported model.
        /// </summary>
        LEAVES = 1
    };

    /// <summary>
    /// LOD Quality.
    /// </summary>
    public enum LodQuality {
        /// <summary>
        /// Use this setting if you wish to obtain a very dense and precise mesh (quality is a priority over low-density) OR if you are importing a very small asset (under 1cm). A tessellation process is run.
        /// </summary>
        MAXIMUM = 0,
        /// <summary>
        /// This is a modification of the Very High preset. Pixyz will deliver a superior quality mesh. Gives high-quality results for small objects. A tessellation process is run.
        /// </summary>
        HIGH = 1,
        /// <summary>
        /// Typically the best option to obtain a balanced mesh between quality and polygon count. A tessellation process is run.
        /// </summary>
        MEDIUM = 2,
        /// <summary>
        /// Efficient setting to obtain a low-density mesh, or to process large objects while limiting polygon count. A tessellation process is run.
        /// </summary>
        LOW = 3,
        /// <summary>
        /// Setting to obtain a very low-density mesh, or to process large objects while strongly limiting polygon count. A tessellation process is run.
        /// </summary>
        POOR = 4,
        /// <summary>
        /// LOD is culled for maximum performances.
        /// </summary>
        CULLED = 5
    }
}
