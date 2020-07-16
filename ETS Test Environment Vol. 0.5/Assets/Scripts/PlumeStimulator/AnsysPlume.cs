using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class AnsysPlume : MonoBehaviour
{

    /*
     * RENDERER VARIABLES
     */
    private ParticleSystem plumeRenderer; // particleSystem was hiding a component variable, so switched to plumeRenderer.
    private int pointCnt;
    private ArrayList points;
    private ParticleSystem.Particle[] allParticles;

    /*
     * PLUME VARIABLES
     */
    public int plumeFrame = 0; // nextUpdate -> plumeFrame. Made public so we can see where we're at when looking at the editor.
    public int numPlumeFrames = 290;
    public int framesPerMinute = 10;
    private float secondsPerFrame;

    // Start is called before the first frame update
    void Start()
    {
        plumeRenderer = GetComponent<ParticleSystem>();

        secondsPerFrame = (60.0f / (float)framesPerMinute);

        StartCoroutine(PlumeAnimate());
    }
    
    void LateUpdate()
    {
    }

    private IEnumerator PlumeAnimate()
    {
        while (plumeFrame < numPlumeFrames)
        {
            points = UpdatePlume(plumeFrame);
            DrawPlume(points);
            plumeFrame++;

            yield return new WaitForSeconds(secondsPerFrame);
        }
    }

    private Color ConcentrationToColor(float concentration)
    {
        if (concentration > 0.75f) return new Color(1, 1 - (concentration - 0.75f) * 4, 0, 30 * concentration);
        else if (concentration > 0.5f) return new Color((concentration - 0.5f) * 4, 1, 0, 30 * concentration);
        else if (concentration > 0.25f) return new Color(0, 1, 1 - (concentration - 0.25f) * 4, 30 * concentration);
        else if (concentration > 0.0f) return new Color(0, concentration * 4, 1, 30 * concentration);
        else return new Color(0, 0, 1, 0);
    }

    private ArrayList UpdatePlume(int frameNumber)
    {
        string framePath = (Application.streamingAssetsPath + "/data/smoke_1/upsampled/point_" + frameNumber.ToString() + ".csv");
        FileInfo f = new FileInfo(framePath);
        string sampleString = "";
        StreamReader frameReader;
        ArrayList plumeSamples = new ArrayList();

        if (f.Exists)
        {
            frameReader = new StreamReader(framePath);
        }
        else
        {
            Debug.Log("No such file (" + framePath + ") found. Please check if the path is correct.");
            return plumeSamples;
        }

        while ((sampleString = frameReader.ReadLine()) != null)
        {
            string[] sampleStrings = sampleString.Split(new[] { "," }, System.StringSplitOptions.RemoveEmptyEntries);
            Vector4 sampleData = new Vector4(float.Parse(sampleStrings[0]), float.Parse(sampleStrings[1]), float.Parse(sampleStrings[2]), float.Parse(sampleStrings[3]));
            plumeSamples.Add(sampleData);
        }

        frameReader.Close(); // Need to close the file after gathering all the data.

        // Debug.Log(n.ToString() + ": " + vecList.Count.ToString());
        return plumeSamples;
    }
    
    private void DrawPlume(ArrayList drawList)
    {
        var main = plumeRenderer.main;
        main.startSpeed = 0.00001f;
        main.startLifetime = 1000.0f;

        pointCnt = drawList.Count;
        allParticles = new ParticleSystem.Particle[pointCnt];
        main.maxParticles = pointCnt;
        plumeRenderer.Emit(pointCnt);
        plumeRenderer.GetParticles(allParticles);
        for (int i = 0; i < pointCnt; i++)
        {
            Vector4 cur = (Vector4)drawList[i];
            allParticles[i].position = new Vector3(cur.x, cur.y, cur.z);
            allParticles[i].startColor = ConcentrationToColor(cur.w);
            allParticles[i].startSize = 0.05f;
        }
        plumeRenderer.SetParticles(allParticles, pointCnt);
    }
}
