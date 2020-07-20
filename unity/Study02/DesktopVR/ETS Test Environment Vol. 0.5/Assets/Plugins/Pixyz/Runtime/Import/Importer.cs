using Pixyz.Config;
using Pixyz.Interface;
using Pixyz.Plugin4Unity;
using Pixyz.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Pixyz.Import {

    public delegate void ExceptionHandler(Exception exception);

    /// <summary>
    /// Single-use class for importing 3D data to Unity.<br/>
    /// This class can be used in the editor as well as in runtime, if the license requirements are met.
    /// </summary>
    public sealed class Importer {

        private static ImportStamp _LatestModelImportedObject;
        /// <summary>
        /// The GameObject reference to the latest imported model. Returns null if no model was imported during this session.
        /// </summary>
        public static ImportStamp LatestModelImportedObject {
            get {
                if (_LatestModelImportedObject == null) {
                    _LatestModelImportedObject = GameObject.FindObjectsOfType<ImportStamp>().OrderByDescending(x => x.importTime).FirstOrDefault();
                }
                return _LatestModelImportedObject;
            }
        }

        /// <summary>
        /// The file path to the latest imported model. Returns null if no model was imported during this session.
        /// </summary>
        public static string LatestModelImportedPath { get; private set; }

        private static Dictionary<string, ImportSettingsTemplate> _SettingsTemplate = new Dictionary<string, ImportSettingsTemplate>();
        /// <summary>
        /// Add a pre-process action to run for a specific file format.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="preprocessingAction"></param>
        public static void AddOrSetTemplate(string format, ImportSettingsTemplate template) {
            format = format.ToLower();
            if (_SettingsTemplate.ContainsKey(format.ToLower())) {
                _SettingsTemplate[format] = template;
            } else {
                _SettingsTemplate.Add(format, template);
            }
        }

        private static Dictionary<string, SubProcess> _Preprocesses = new Dictionary<string, SubProcess>();
        /// <summary>
        /// Add a pre-process action to run for a specific file format.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="preprocessingAction"></param>
        public static void AddOrSetPreprocess(string format, SubProcess preprocessingAction) {
            format = format.ToLower();
            if (_Preprocesses.ContainsKey(format.ToLower())) {
                _Preprocesses[format] = preprocessingAction;
            } else {
                _Preprocesses.Add(format, preprocessingAction);
            }
        }

        private static Dictionary<string, SubProcess> _Postprocesses = new Dictionary<string, SubProcess>();
        /// <summary>
        /// Add a post-process action to run for a specific file format.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="postprocessingAction"></param>
        public static void AddOrSetPostprocess(string format, SubProcess postprocessingAction) {
            format = format.ToLower();
            if (_Postprocesses.ContainsKey(format.ToLower())) {
                _Postprocesses[format] = postprocessingAction;
            } else {
                _Postprocesses.Add(format, postprocessingAction);
            }
        }

        public static ImportSettingsTemplate GetSettingsTemplate(string filePath) {
            ImportSettingsTemplate template;
            if (!string.IsNullOrEmpty(filePath) && _SettingsTemplate.TryGetValue(Path.GetExtension(filePath).ToLower(), out template)) {
                return template;
            } else {
                return ImportSettingsTemplate.Default;
            }
        }

        public static SubProcess GetPreprocess(string filePath) {
            SubProcess preprocess;
            if (!string.IsNullOrEmpty(filePath) && _Preprocesses.TryGetValue(Path.GetExtension(filePath).ToLower(), out preprocess)) {
                return preprocess;
            } else {
                return null;
            }
        }

        public static SubProcess GetPostprocess(string filePath) {
            SubProcess postprocess;
            if (!string.IsNullOrEmpty(filePath) && _Postprocesses.TryGetValue(Path.GetExtension(filePath).ToLower(), out postprocess)) {
                return postprocess;
            } else {
                return null;
            }
        }

        /// <summary>
        /// Callback function triggered everytime the importer has progressed.
        /// Always occurs in the main thread.
        /// </summary>
        public event ProgressHandler progressed;

        /// <summary>
        /// Callback function trigerred when the import failed
        /// </summary>
        public event ExceptionHandler failed;

        private System.Diagnostics.Stopwatch _stopwatch;
        /// <summary>
        /// Elasped ticks since the begining of the import.
        /// </summary>
        public long elaspedTicks { get { return _stopwatch.ElapsedTicks; } }

        /// <summary>
        /// Callback function triggered when the importer has finished importing.
        /// In Async mode, this callback is triggered only when everything is finished.
        /// Always occurs in the main thread.
        /// </summary>
        public event GameObjectToVoidHandler completed;

        private string _file;
        /// <summary>
        /// The file to import
        /// </summary>
        public string filePath {
            get { return _file; }
            set { if (_hasStarted) { throw new AccessViolationException("Can only set Importer properties before it runs"); } _file = value; } }

        private bool _isAsynchronous = true;
        /// <summary>
        /// [Default is True]
        /// If set to true, the import process will run as much as it can on different threads than the main one, so that it won't freeze the Editor/Application and performances are kept to the maximum.
        /// In Asynchronous mode, it is recommended to use callback methods to get information on the import status.
        /// </summary>
        public bool isAsynchronous {
            get { return _isAsynchronous; }
            set { if (_hasStarted) { throw new AccessViolationException("Can only set Importer properties before it runs"); } _isAsynchronous = value; } }

        private bool _printMessageOnCompletion = false;
        /// <summary>
        /// [Default is True]
        /// If set to true, the importer will print a message in the console on import completion.
        /// </summary>
        public bool printMessageOnCompletion {
            get { return _printMessageOnCompletion; }
            set { if (_hasStarted) { throw new AccessViolationException("Can only set Importer properties before it runs"); } _printMessageOnCompletion = value; }
        }

        private ImportSettings _importSettings;
        /// <summary>
        /// Returns the ImportSettings reference 
        /// </summary>
        public ImportSettings importSettings { get { return _importSettings; } set { if (_hasStarted) { throw new AccessViolationException("Can only set Importer properties before it runs"); } _importSettings = value; } }

        public SubProcess preprocess => GetPreprocess(filePath);

        public SubProcess postprocess => GetPostprocess(filePath);

        private ImportStamp _importStamp;
        public ImportStamp importedModel => _importStamp;

        public int polycount => _sceneConverter.PolyCount;
        public int gameObjectCount => _sceneConverter.ObjectCount;

        private ImportSettings _importSettingsCopy;

        private NativeInterface.SceneExtract _scene;
        private SceneExtractToUnity _sceneConverter;
        private bool _hasStarted = false;
        private string _seed;

        const string CANCEL_MESSAGE = "Import has been canceled.";

        public Importer(string file, ImportSettings importSettings) {

            filePath = file;
            if (importSettings)
                this.importSettings = importSettings;
            else
                this.importSettings = ScriptableObject.CreateInstance<ImportSettings>();

            if (!File.Exists(file)) {
                throw new Exception($"File '{file}' does not exist");
            }
            if (!Formats.IsFileSupported(file)) {
                throw new Exception($"File '{file}' is not supported by Pixyz");
            }
        }

        /// <summary>
        /// Starts importing the file. Can be executed only once per Importer instance.
        /// </summary>
        public void run() {
            try {
                if (!Configuration.CheckLicense())
                    throw new NoValidLicenseException();
                NativeInterface.SetFile(filePath);
                Dispatcher.StartCoroutine(runCoreCommands());
            } catch (Exception exception) {
                reportImportProgressed(1f, "Exception");
                Debug.LogException(exception);
            }
        }

        delegate uint ImportFileDelegate(string file);

        private bool checkIfCanceled() {
            if (_isStopped) {
                NativeInterface.ClearScene();
                reportImportProgressed(1f, CANCEL_MESSAGE);
                Debug.LogWarning(CANCEL_MESSAGE);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Read the file in the Pixyz Core native assemblies
        /// </summary>
        private IEnumerator runCoreCommands() {

//#if UNITY_EDITOR
//            UnityEditor.EditorPrefs.SetBool("kAutoRefresh", false);
//#endif

            if (_hasStarted) {
                throw new Exception("An Importer instance can only import once. Please create a new Importer instance to import another file.");
            }

            _hasStarted = true;
            _stopwatch = new System.Diagnostics.Stopwatch();
            _stopwatch.Start();

            _seed = UnityEngine.Random.Range(0, int.MaxValue).ToString();

            Profiling.Start("Importer " + _seed);
            reportImportProgressed(0f, "Initializing...");

            /// Freezing the ImportSettings, and eventually change some settings depending on context
            /// For example,
            /// - Ensures it's not making LODs and doing some tree processing if file is .pxz
            /// - Ensures it's not loading metadata if there is some tree processing
            /// - Loads default shader if not override shader is specified
            _importSettingsCopy = UnityEngine.Object.Instantiate(importSettings);

            if (_importSettingsCopy.shader == null) {
                _importSettingsCopy.shader = SceneExtensions.GetDefaultShader();
            }

            if (isAsynchronous)
                yield return Dispatcher.GoThreadPool();

            /// This part runs in another thread to keep the Editor/App smooth and improve performances.
            try {
                /// Assembly
                reportImportProgressed(0.10f, "Reading file...");
                NativeInterface.ClearScene();

                if (checkIfCanceled())
                    yield break;

                NativeInterface.ImportFile();

                if (checkIfCanceled())
                    yield break;

                if (preprocess != null) {
                    try {
                        reportImportProgressed(0.30f, "Pre-processing...");
                        preprocess.run(this);
                    } catch (Exception exception) {
                        Debug.LogError("An exception occurred in the pre-process : " + exception);
                        yield break;
                    }
                }

                reportImportProgressed(0.40f, "Processing...");
                var settings = _importSettingsCopy.ToInterfaceObject(GetSettingsTemplate(filePath));
                var rootChild = NativeInterface.RunAutomaticProcess(settings);

                if (checkIfCanceled())
                    yield break;

                /// Scene
                reportImportProgressed(0.60f, "Extracting...");
                _scene = NativeInterface.GetSceneExtract(rootChild, true, true, true, MatrixExtractMode.LOCAL, MaterialExtractMode.PART_ONLY, VisibilityExtractMode.VISIBLE_ONLY);

                NativeInterface.ClearScene();
            } catch (Exception coreException) {
                /// An exception has occured in the Core
                NativeInterface.ClearScene();
                reportImportProgressed(1f, "Core exception");
                invokeFailed(coreException);
                Debug.LogException(coreException);
                yield break;
            }

            if (_isStopped) {
                reportImportProgressed(1f, CANCEL_MESSAGE);
                Debug.LogWarning(CANCEL_MESSAGE + " before creating objects");
                yield break;
            }

            reportImportProgressed(0.70f, "Converting...");
            if (isAsynchronous)
                yield return Dispatcher.GoMainThread();

            /// Running this Part in the main Unity thread.
            reportImportProgressed(0.80f, "Creating objects...");
            try {
                /// Converting the Scene from Core data to Unity data structure
                /// Also tracking all created dependencies such as Materials and Meshes, useful for prefab making
                _sceneConverter = new SceneExtractToUnity(_scene, filePath, _importSettingsCopy, finalize, isAsynchronous);
                _sceneConverter.convert();
            } catch (Exception sceneConversionException) {
                /// An exception has occured while trying to convert the Scene
                reportImportProgressed(1f, "Extraction exception");
                invokeFailed(sceneConversionException);
                Debug.LogException(sceneConversionException);
            }
        }

        private void finalize() {

            if (checkIfCanceled())
                return;

            try {
                GameObject gameObject = _sceneConverter.gameObject;

                /// Sets LatestModelImported (useful for RuleEngine or any other script running after an import)
                TimeSpan time = Profiling.End("Importer " + _seed);
                _stopwatch.Stop();

                /// Recreating ImportedModel stamp
                _importStamp = gameObject.AddComponent<ImportStamp>();
                _importStamp.stamp(filePath, elaspedTicks);
                _importStamp.importSettings = _importSettingsCopy;

                _LatestModelImportedObject = _importStamp;

                /// Post-Process
                if (postprocess != null) {
                    try {
                        reportImportProgressed(0.90f, "Post-processing...");
                        postprocess.run(this);
                    } catch (Exception exception) {
                        Debug.LogError("An exception occurred in the post-process : " + exception);
                    }
                }

                /// Import is finished. Sets progress to 100% and runs callbackEnded.
                reportImportProgressed(1f, "Done !");
                if (printMessageOnCompletion)
                    BaseExtensions.LogColor(Color.green, $"Pixyz Import > File imported in {time.FormatNicely()}");

                invokeCompleted(gameObject);

            } catch (Exception exception) {
                reportImportProgressed(1f, "Finalization exception");
                invokeFailed(exception);
                Debug.LogException(exception);
            }

            clear();
        }

        private void invokeFailed(Exception exception)
        {
//#if UNITY_EDITOR
//            UnityEditor.EditorPrefs.SetBool("kAutoRefresh", true);
//#endif
            failed?.Invoke(exception);
        }

        private void invokeCompleted(GameObject gameObject)
        {
//#if UNITY_EDITOR
//            UnityEditor.EditorPrefs.SetBool("kAutoRefresh", true);
//#endif
            completed?.Invoke(gameObject);
        }

        private void reportImportProgressed(float progress, string message) {
            progressed?.Invoke(progress, message);
        }

        private void clear() {
            _scene = new NativeInterface.SceneExtract();
            _sceneConverter = null;
        }

        private bool _isStopped = false;
        public bool isStopped => _isStopped;

        public void stop() {
            _isStopped = true;
        }
    }

    /// <summary>
    /// Serializable container class for all LODs
    /// </summary>
    [Serializable]
    public struct LodsGenerationSettings {

        public static LodsGenerationSettings Default()
        {
            return new LodsGenerationSettings(new LodGenerationSettings[] {
                new LodGenerationSettings { threshold = 0.50, quality = LodQuality.MAXIMUM },
                new LodGenerationSettings { threshold = 0.20, quality = LodQuality.MEDIUM },
                new LodGenerationSettings { threshold = 0.05, quality = LodQuality.LOW },
                new LodGenerationSettings { threshold = 0.0, quality = LodQuality.POOR } });
        }

        public LodsGenerationSettings(LodGenerationSettings[] lods)
        {
            _locked = false;
            _lods = lods;
        }

        [SerializeField]
        private LodGenerationSettings[] _lods;

        /// <summary>
        /// Get or Set settings for each LOD.
        /// Check @link Pixyz.LODSettings @endlink for information on how to set up a LOD.
        /// </summary>
        public LodGenerationSettings[] lods {
            get {
                if (_lods == null || _lods.Length == 0) {
                    _lods = new LodGenerationSettings[] { new LodGenerationSettings { threshold = 1, quality = LodQuality.MAXIMUM } };
                }
                return _lods;
            }
            set {
                if (_lods == value || value == null)
                    return;
                _lods = value;
            }
        }

        public LodGenerationSettings quality {
            get {
                return lods[0];
            }
            set {
                lods[0] = value;
            }
        }

        public bool isLocked {
            get { return _locked; }
            set { _locked = value; }
        }

        [SerializeField]
        private bool _locked;
    }

    /// <summary>
    /// Serializable container class for a single LOD.
    /// </summary>
    [Serializable]
    public struct LodGenerationSettings {

        /// <summary>
        /// The quality for that LOD.
        /// </summary>
        public LodQuality quality;

        /// <summary>
        /// The threshold [0 to 1] at which this LOD ends.
        /// For example : 
        /// A threshold of 0 means that this LOD will be visible between (previousLOD.threshold * 100)% and 0% visibility.
        /// A threshold of 0.3 means that this LOD will be visible between (previousLOD.threshold * 100)% and 30% visibility.
        /// </summary>
        public double threshold;
    }
}