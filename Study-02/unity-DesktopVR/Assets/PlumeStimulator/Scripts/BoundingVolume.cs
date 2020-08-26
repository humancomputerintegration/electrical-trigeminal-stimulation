using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System;

/*
 * Potentially switch this to multi-threading via Tasks...
 * https://www.youtube.com/watch?v=ja63QO1Imck maybe look at this?
 */

[RequireComponent(typeof(ParticleSystem), typeof(BoxCollider), typeof(Rigidbody))]
public class BoundingVolume : MonoBehaviour
{
    private ParticleSystem volumeVisualizer;
    private BoxCollider volumeCollider;

    public int volumeI = 0;
    public int volumeJ = 0;
    public int volumeK = 0;
    public string ansysSimulationPath = "/AnsysBoundingVolumes";

    public int startFrame = 500;
    public int currFrame = 0;

    public bool isEmpty = true;
    public bool visualize = false;

    private List<SamplePoint> volumeSamples = new List<SamplePoint>(); // ArrayLists are deprecated in favor of List<T>.

    // Struct for a sample point in the plume segment.
    struct SamplePoint
    {
        public Vector3 Position { get; set; }
        public float Concentration { get; set; }

        // Constructor
        public SamplePoint(Vector3 pos, float con)
        {
            this.Position = pos;
            this.Concentration = con;
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        volumeVisualizer = GetComponent<ParticleSystem>();
        var main = volumeVisualizer.main;
        main.startSpeed = 0.0f;
        main.startLifetime = 1000.0f;
        main.startSize = 0.05f;

        volumeCollider = GetComponent<BoxCollider>();
    }

    public void InitializeVolume()
    {
        var info = new DirectoryInfo(Application.streamingAssetsPath + ansysSimulationPath
            + "/Box_i" + volumeI.ToString()
            + "_j" + volumeJ.ToString()
            + "_k" + volumeK.ToString()
            );
        FileInfo[] fileInfo = info.GetFiles();
        startFrame = Int32.Parse(Regex.Replace(fileInfo[0].Name, "[^.0-9]", "").Replace(".", string.Empty));
        VolumeUpdate(1);
        /*
            //foreach (FileInfo file in fileInfo)
            //{
            //    if (file.Name.Contains(".meta"))
            //    {
            //        continue;
            //    }
            //    int frame = Int32.Parse(Regex.Replace(file.Name, "[^.0-9]", "").Replace(".", string.Empty));
            //    if (frame < startFrame)
            //    {
            //        startFrame = frame;
            //        currFrame = startFrame-1;
            //    }
            //}
        */
    }

    public void VolumeUpdate(int frameNumber)
    {
        if ((frameNumber >= startFrame) && (frameNumber > currFrame))
        {
            currFrame = frameNumber;
            volumeSamples = ReadAnsysCSV(currFrame);
            RenderSamples();
        }
    }

    private List<SamplePoint> ReadAnsysCSV(int frameNumber)
    {
        string framePath = (Application.streamingAssetsPath + ansysSimulationPath
            + "/Box_i" + volumeI.ToString()
            + "_j" + volumeJ.ToString()
            + "_k" + volumeK.ToString()
            + "/" + frameNumber.ToString()
            + ".csv");
        FileInfo frameInfo = new FileInfo(framePath);
        string sampleString = "";
        StreamReader frameReader;
        List<SamplePoint> samples = new List<SamplePoint>();

        frameReader = new StreamReader(framePath); // This is sure to exist because we gathered the start frames for each volume.

        while ((sampleString = frameReader.ReadLine()) != null)
        {
            string[] sampleStrings = sampleString.Split(new[] { "," }, System.StringSplitOptions.RemoveEmptyEntries);
            Vector3 samplePosition = new Vector3(float.Parse(sampleStrings[0]), float.Parse(sampleStrings[1]), float.Parse(sampleStrings[2]));
            float sampleConcentration = float.Parse(sampleStrings[3]);
            samples.Add(new SamplePoint(samplePosition, sampleConcentration));
        }

        frameReader.Close(); // Need to close the file after gathering all the data.

        return samples;
    }

    private Color ConcentrationToColor(float concentration)
    {
        if (concentration > 0.75f) return new Color(1, 1 - (concentration - 0.75f) * 4, 0, 30 * concentration);
        else if (concentration > 0.5f) return new Color((concentration - 0.5f) * 4, 1, 0, 30 * concentration);
        else if (concentration > 0.25f) return new Color(0, 1, 1 - (concentration - 0.25f) * 4, 30 * concentration);
        else if (concentration > 0.0f) return new Color(0, concentration * 4, 1, 30 * concentration);
        else return new Color(0, 0, 1, 0);
    }

    private void RenderSamples()
    {
        var main = volumeVisualizer.main;
        main.startSpeed = 0.0f;
        main.startLifetime = 1000.0f;

        int particleCount = volumeSamples.Count;
        main.maxParticles = particleCount;
        //Debug.Log(particleCount.ToString());
        volumeVisualizer.Emit(particleCount);
        ParticleSystem.Particle[] segmentParticles = new ParticleSystem.Particle[particleCount];
        volumeVisualizer.Emit(particleCount);
        volumeVisualizer.GetParticles(segmentParticles);

        for (int i = 0; i < particleCount; i++)
        {
            //Debug.Log(volumeSamples[i].Position);
            //ParticleSystem.EmitParams currEmitParam = new ParticleSystem.EmitParams();

            segmentParticles[i].position = volumeSamples[i].Position;
            segmentParticles[i].startColor = ConcentrationToColor(volumeSamples[i].Concentration);
            segmentParticles[i].startSize = 0.05f;

            //volumeVisualizer.Emit(currEmitParam, 1);
        }
        volumeVisualizer.SetParticles(segmentParticles, particleCount);
    }

    public float Concentration(Bounds bound, Transform colliderTransform)
    {
        float concentration = 0f;
        foreach(SamplePoint sp in volumeSamples)
        {
            Vector3 currSample = colliderTransform.InverseTransformPoint(sp.Position);
            if (bound.Contains(currSample))
            {
                concentration += sp.Concentration;
            }

        }
        return concentration;
    }
}
