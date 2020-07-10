using Pixyz.Interface;
using Pixyz.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class ProxyOcclusionCuller : MonoBehaviour {

    public enum ScalingMode {
        Fixed,
        Variable,
    }

    public enum RatioMode {
        WidthDrivesHeight,
        HeightDrivesWidth,
    }

    public int resolution = 196;

    public float voxelSize = 2f;

    public ScalingMode scalingMode = ScalingMode.Fixed;
    public RatioMode ratioMode = RatioMode.WidthDrivesHeight;
    public float scaleFactor = 0.5f;

    public int maximumReadbackRequests = 1;

    public string saveTo;

    public bool mirrorX = false;
    public bool isZup = false;

    public bool hashedColors;

    public bool debugBuffer;
    public bool debugBounds;
    public bool _debugStats;

    [HideInInspector]
    [NonSerialized]
    public Material proxyMaterial;

    [HideInInspector]
    [NonSerialized]
    public int visibleCells;

    [HideInInspector]
    [NonSerialized]
    public int idenficationTime;

    [HideInInspector]
    [NonSerialized]
    public int activationTime;

    [HideInInspector]
    [NonSerialized]
    public int textureWidth;

    [HideInInspector]
    [NonSerialized]
    public int textureHeight;

    [HideInInspector]
    [NonSerialized]
    public int screenWidth;

    [HideInInspector]
    [NonSerialized]
    public int screenHeight;

    private CommandBuffer commandBuffer;
    private RenderTexture renderTexture;
    private Renderer[] renderers;
    private Color32[] rcolors;
    private Vector3[] positions;
    private Queue<AsyncGPUReadbackRequest> requests;

    public int renderersCount => (renderers != null) ? renderers.Length : 0;

    public MeshFilter proxy => gameObject.GetComponentsInChildren<MeshFilter>(true).Where(x => x.name.StartsWith("ProxyMesh")).FirstOrDefault();

    private void Start() {
        commandBuffer = new CommandBuffer();
        requests = new Queue<AsyncGPUReadbackRequest>();
        // Add command buffer to camera. It can be fill afterwards
        Camera.main.AddCommandBuffer(CameraEvent.AfterImageEffects, commandBuffer);
        Initialize();
    }

    private void OnDisable() {
        for (int i = 0; i < renderers.Length; i++) {
            renderers[i].enabled = true;
        }
    }

    public void Initialize() {

        screenWidth = Camera.main.pixelWidth;
        screenHeight = Camera.main.pixelHeight;

        // Prevents initialization if script hasn't started
        if (commandBuffer == null)
            return;

        // --- Create render texture ---
        if (scalingMode == ScalingMode.Variable) {
            textureWidth = Mathf.RoundToInt(scaleFactor * screenWidth);
            textureHeight = Mathf.RoundToInt(scaleFactor * screenHeight);
        } else {
            textureWidth = resolution;
            textureHeight = resolution;
        }
        float ratio = 1f * screenWidth / screenHeight;
        if (ratioMode == RatioMode.WidthDrivesHeight) {
            textureHeight = Mathf.RoundToInt(1f * textureWidth / ratio);
        } else {
            textureWidth = Mathf.RoundToInt(1f * textureHeight * ratio);
        }
        // Create render texture that will serve to check visibility based on rasterized proxy's pixels
        renderTexture = new RenderTexture(textureWidth, textureHeight, 0, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm);
        renderTexture.filterMode = FilterMode.Point;

        // --- Prepare command buffer ---
        requests.Clear();
        commandBuffer.Clear();

        // Use white color as background as it will be hashed as int's max value and thus will never used for shading
        // the model as long as the voxelization is under 256 * 256 * 256
        commandBuffer.SetRenderTarget(renderTexture);
        commandBuffer.ClearRenderTarget(true, true, Color.white);

        Bounds bounds = gameObject.GetBoundsWorldSpace(true);

        // --- Prepares the material for proxy rendering ---
        proxyMaterial = new Material(Shader.Find("Pixyz/Worldspace Unlit Voxels"));
        proxyMaterial.SetFloat("_VoxelSize", voxelSize);
        proxyMaterial.SetFloat("_HashedColors", hashedColors ? 1f : 0f);
        // The voxelization process may has been done from either side of an axis. By specificying weither
        // it has been inverted on a axis or not, we are able to garantee that the proxy shader perfectly matches
        // the point cloud voxels
        proxyMaterial.SetFloat("_MinX", bounds.min.x + (mirrorX ? (bounds.size.x % voxelSize) : 0));
        proxyMaterial.SetFloat("_MinY", bounds.min.y + (isZup ? (bounds.size.z % voxelSize) : 0));
        proxyMaterial.SetFloat("_MinZ", bounds.min.z + (isZup ? (bounds.size.y % voxelSize) : 0));
        // Draws the proxy using a special shader that is going to have one unlit color per voxel
        var proxyIns = proxy;
        if (!proxyIns) throw new Exception("No proxy mesh was found. A proxy mesh is required to perform proxy based point cloud GPU culling.");
        commandBuffer.DrawMesh(proxyIns.sharedMesh, proxyIns.transform.localToWorldMatrix, proxyMaterial, 0, 0);

        // --- Gather renderers and associated colors for later correspondance from read pixels in buffer
        // Gather all concerned renderers
        renderers = gameObject.GetComponentsInChildren<Renderer>(true);
        rcolors = new Color32[renderers.Length];
        positions = new Vector3[renderers.Length];
        for (int r = 0; r < renderers.Length; r++) {
            // Find x, y, and z that represents the actual voxel position indices
            int x = Mathf.FloorToInt((renderers[r].bounds.center.x - bounds.min.x) / voxelSize);
            int y = Mathf.FloorToInt((renderers[r].bounds.center.y - bounds.min.y) / voxelSize);
            int z = Mathf.FloorToInt((renderers[r].bounds.center.z - bounds.min.z) / voxelSize);
            positions[r] = new Vector3((x + 0.5f) * voxelSize + bounds.min.x, (y + 0.5f) * voxelSize + bounds.min.y, (z + 0.5f) * voxelSize + bounds.min.z);
            if (x > 255 || y > 255 || z > 255)
                throw new System.Exception("There are more than 256 voxels in a single dimension. Please use a larger voxel size.");
            if (x < 0 || y < 0 || z < 0)
                throw new System.Exception("There was an issue computing a voxels position indices.");
            // Get and store the color assignated to that voxel
            rcolors[r] = new Color32((byte)x, (byte)y, (byte)z, byte.MaxValue);
            if (hashedColors) {
                // If we chose to use hashed colors instead of a 3d 24bit linear gradient, we store the hashed color instead.
                rcolors[r] = GetColor(GetHash(GetInt(rcolors[r])));
            }
            // We assign colors to the mesh colors to visualize if voxels perfectly match the voxels as seen in the
            // worldspace shader applied on the proxy mesh.
            //Mesh mesh = renderers[r].GetComponent<MeshFilter>().sharedMesh;
            //mesh.colors32 = Enumerable.Repeat(rcolors[r], mesh.vertexCount).ToArray();
        }

        if (debugBuffer) {
            commandBuffer.Blit(renderTexture, Camera.main.targetTexture);
        }
    }

    public void Update() {
       
        if (screenWidth != Camera.main.pixelWidth || screenWidth != Camera.main.pixelWidth) {
            Initialize();
        }

        if (requests.Count < maximumReadbackRequests)
            requests.Enqueue(AsyncGPUReadback.Request(renderTexture));

        while (requests.Count > 0) {
            var request = requests.Peek();
            if (request.hasError) {
                // A readback error has occurred.
                // We ignore it but it is dequeued.
                requests.Dequeue();
            } else if (request.done) {
                var buffer = request.GetData<Color32>();
                UpdateRenderers(buffer.ToArray());
                requests.Dequeue();
            } else {
                // The request is still in progress.
                break;
            }
        }
    }

    private void UpdateRenderers(Color32[] colors) {

        if (!string.IsNullOrEmpty(saveTo)) {
            // Save colors to a .png texture (for debugging)
            Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            tex.SetPixels32(colors);
            File.WriteAllBytes(saveTo, tex.EncodeToPNG());
            saveTo = null;
        }

        if (_debugStats)
            Profiling.Start("Identifying");

        var ucolors = new HashSet<int>();
        for (int i = 0; i < colors.Length; i++) {
            int hash = GetInt(colors[i]);
            if (hash < 16777215)
                ucolors.Add(hash);
        }

        if (_debugStats) {
            idenficationTime = Profiling.End("Identifying").Milliseconds;
            Profiling.Start("Activating");
        }

        // Renderers are being enabled / disabled depending on their current visibility
        for (int i = 0; i < renderers.Length; i++) {
            renderers[i].enabled = ucolors.Contains(GetInt(rcolors[i]));
        }

        if (_debugStats)
            activationTime = Profiling.End("Activating").Milliseconds;

        visibleCells = ucolors.Count;
    }

    void OnDrawGizmos() {
        if (debugBounds) {
            Vector3 size = Vector3.one * voxelSize;
            for (int i = 0; i < renderers.Length; i++) {
                if (renderers[i].isVisible) {
                    Gizmos.color = rcolors[i];
                    Gizmos.DrawCube(positions[i], size);
                }
            }
        }
    }

    private Color32 GetColor(int i) {
        return new Color32(
            (byte)((i >> 16) & 255),
            (byte)((i >> 8) & 255),
            (byte)(i & 255),
            byte.MaxValue
        );
    }

    private int GetInt(Color32 c) {
        return (c.r << 16) + (c.g << 8) + c.b;
    }

    private int GetHash(int x) {
        x = ((x >> 16) ^ x) * 0x45d9f3b;
        x = ((x >> 16) ^ x) * 0x45d9f3b;
        x = (x >> 16) ^ x;
        return x;
    }
}
