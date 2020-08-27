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

public class RehaMoveInterface : MonoBehaviour
{
    [Header("RehaMove Parameters")]
    public int something = 0;

    /*
     * PYTHON SERVER COMMUNICATION.
     * Brief: for communicating with the RehaMove system via Python script over a server.
     */
    private OSCServer myServer;
    [Header("OSC Settings")]
    public string outIP = "127.0.0.1";
    public int outPort = 9001;
    public int inPort = 9998;
    public int bufferSize = 100;        // Buffer size of the application (stores 100 messages from different servers).


    void Start()
    {
        InitializeOSC();
    }

    void InitializeOSC()
    {
        // Initialize OSC.
        OSCHandler.Instance.Init();

        // Initialize OSC clients (transmitters).
        OSCHandler.Instance.CreateClient("myClient", IPAddress.Parse(outIP), outPort);

        // Initialize OSC servers (listeners).
        myServer = OSCHandler.Instance.CreateServer("myServer", inPort);

        // Set buffer size (bytes) of the server (default 1024).
        myServer.ReceiveBufferSize = 1024;

        // Set the sleeping time of the thread (default 10).
        myServer.SleepMilliseconds = 10;
    }
    
    void Update()
    {
    }

    public void SendStimulation()
    {
        OSCHandler.Instance.SendMessageToClient("myClient", "/stimulation_S", "");
    }
    
}