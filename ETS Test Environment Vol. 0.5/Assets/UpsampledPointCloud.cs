using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class UpsampledPointCloud : MonoBehaviour
{

    ParticleSystem particleSystem;
    int pointCnt;
    ArrayList points;
    int nextUpdate;

    // Start is called before the first frame update
    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
        nextUpdate = 1;
        points = UpdatePoint(0);
        DrawPointCloud(points);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= nextUpdate)
        {
            points = UpdatePoint(nextUpdate);
            DrawPointCloud(points);
            nextUpdate++;
        }
    }

    Color ConcentrationToColor(float concentration)
    {
        if (concentration > 0.75f) return new Color(1, 1 - (concentration - 0.75f) * 4, 0, 30 * concentration);
        else if (concentration > 0.5f) return new Color((concentration - 0.5f) * 4, 1, 0, 30 * concentration);
        else if (concentration > 0.25f) return new Color(0, 1, 1 - (concentration - 0.25f) * 4, 30 * concentration);
        else if (concentration > 0.0f) return new Color(0, concentration * 4, 1, 30 * concentration);
        else return new Color(0, 0, 1, 0);
    }

    ArrayList UpdatePoint(int n)
    {
        string path = (Application.streamingAssetsPath + "/data/smoke_1/upsampled/point_" + n.ToString() + ".csv");
        FileInfo f = new FileInfo(path);
        string s = "";
        StreamReader r;
        ArrayList vecList = new ArrayList();

        if (f.Exists)
        {
            r = new StreamReader(path);
        }
        else
        {
            Debug.Log("no such file: " + path);
            return vecList;
        }

        while ((s = r.ReadLine()) != null)
        {
            string[] words = s.Split(new[] { "," }, System.StringSplitOptions.RemoveEmptyEntries);
            Vector4 xyzd = new Vector4(float.Parse(words[0]), float.Parse(words[1]), float.Parse(words[2]), float.Parse(words[3]));
            vecList.Add(xyzd);
        }
        Debug.Log(n.ToString() + ": " + vecList.Count.ToString());
        return vecList;
    }

    ParticleSystem.Particle[] allParticles;
    void DrawPointCloud(ArrayList drawList)
    {
        var main = particleSystem.main;
        main.startSpeed = 0.0f;
        main.startLifetime = 1000.0f;

        pointCnt = drawList.Count;
        allParticles = new ParticleSystem.Particle[pointCnt];
        main.maxParticles = pointCnt;
        particleSystem.Emit(pointCnt);
        particleSystem.GetParticles(allParticles);
        for (int i = 0; i < pointCnt; i++)
        {
            Vector4 cur = (Vector4)drawList[i];
            allParticles[i].position = new Vector3(cur.x, cur.y, cur.z);
            allParticles[i].startColor = ConcentrationToColor(cur.w);
            allParticles[i].startSize = 0.05f;
        }
        particleSystem.SetParticles(allParticles, pointCnt);
    }
}
