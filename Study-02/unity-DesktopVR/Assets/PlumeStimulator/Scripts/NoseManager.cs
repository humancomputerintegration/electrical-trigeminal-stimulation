using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
/*for IP*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityOSC;
/*for Unity*/

public class NoseManager : MonoBehaviour
{

    /*
     * Basic idea:
     *      - The nose manager keeps track of the frame number AND frame rate (non the Ansys Plume).
     *      - While a nostril is in a bounding volume:
     *              + check the frame discrepancy (if nose frame > bounding volume frame). If yes, update.
     *              + call bounding volume's comparison function using the Bounds 
     */

    /*
     * PLUME VARIABLES
     */
    [Header("ANSYS Settings")]
    public int ansysFrame = 0; // nextUpdate -> plumeFrame. Made public so we can see where we're at when looking at the editor.
    public int numAnsysFrames = 300;
    public int framesPerSecond = 1;
    private float secondsPerFrame;
    public int breathsPerMinute = 18;
    public float secondsPerBreath;

    [Header("Odor Thresholds")]
    public double detectionThreshold_kgmc = 0.0000014;

    [Header("Visualization")]
    public Transform latViz;

    [Header("OSC Settings")]
    public string message = "l 2.5";
    public string outIP = "127.0.0.1";
    public int outPort = 9001;
    public int inPort = 9998;
    public int bufferSize = 100; // buffer size of the application (stores 100 messages from different servers)
    private OSCServer myServer;

    // Start is called before the first frame update
    void Start()
    {
        InitializeOSC();
        secondsPerFrame = (1.0f / (float)framesPerSecond);
        secondsPerBreath = (60.0f / (float)breathsPerMinute);
        StartCoroutine(NoseAnimate());
        StartCoroutine(NoseBreathe());
    }

    void InitializeOSC()
    {
        // Initialize OSC
        OSCHandler.Instance.Init();

        // Initialize OSC clients (transmitters)
        OSCHandler.Instance.CreateClient("myClient", IPAddress.Parse(outIP), outPort);

        // Initialize OSC servers (listeners)
        myServer = OSCHandler.Instance.CreateServer("myServer", inPort);

        // Set buffer size (bytes) of the server (default 1024)
        myServer.ReceiveBufferSize = 1024;

        // Set the sleeping time of the thread (default 10)
        myServer.SleepMilliseconds = 10;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private System.Collections.IEnumerator NoseAnimate()
    {
        while (ansysFrame < numAnsysFrames)
        {
            ansysFrame++;
            yield return new WaitForSeconds(secondsPerFrame);
        }
    }

    private float ConcentrationToIntensity(double concentration_kgmc)
    {
        float intensity = 0f;
        double concentration_ppm = concentration_kgmc * 1000.0;

        intensity = 2.5f * Mathf.Pow((float)(concentration_ppm / 1.7), 0.617f);

        return intensity;
    }

    private System.Collections.IEnumerator NoseBreathe()
    {
        while (true)
        {
            float left = 0f;
            float right = 0f;
            foreach (Transform nostril in transform)
            {
                if (nostril.gameObject.layer == LayerMask.NameToLayer("Debug"))
                    continue;
                NostrilCollision nC = nostril.GetComponent<NostrilCollision>();
                float conc = nC.CheckConcentration();
                if (nostril.name.Contains("left"))
                {
                    left = conc;
                } else
                {
                    right = conc;
                }
            }

            double totalConcentration = (double)(left + right);

            if (totalConcentration < detectionThreshold_kgmc)
            {
                latViz.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            }
            else
            {
                float totalIntensity = ConcentrationToIntensity(totalConcentration);
                if (left < right) // Right stimulation
                {
                    latViz.localPosition = new Vector3(0.0f, 0.0f, -0.2f);
                    message = "l " + totalIntensity.ToString();
                }
                else if (right < left) // Left stimulation
                {
                    latViz.localPosition = new Vector3(0.0f, 0.0f, 0.2f);
                    message = "r " + totalIntensity.ToString();
                }
                else // Middle stimulation
                {
                    latViz.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                    message = "m " + totalIntensity.ToString();
                }
                OSCHandler.Instance.SendMessageToClient("myClient", "/stimulation", message);
            }
            yield return new WaitForSeconds(secondsPerBreath);
        }
    }
}
