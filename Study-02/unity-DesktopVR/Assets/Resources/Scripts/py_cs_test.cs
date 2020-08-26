/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IronPython.Hosting;
using UnityEngine.UI;

public class py_cs_test : MonoBehaviour
{
    
    public Text PulseType;
    public Transform OdorSourceCube;
    public Transform Camera;
    public int StimulationInterval = 10;
    dynamic stimulator;
    dynamic py;
    Vector3 relativePosition; //the relative position of the odor source in Camera's coordinate system
    public string stimPort = "COM18";
    public string channel = "red";

    // Start is called before the first frame update
    void Start()
    {
        var engine = Python.CreateEngine();
        ICollection<string> searchPaths = engine.GetSearchPaths();
        
        
        searchPaths.Add(@"C:\Users\The Lab\Desktop\ETS test environment\ETS Test Environment Vol. 0.5\");
        // searchPaths.Add(@"C:\Users\The Lab\Desktop\ETS test environment\ETS Test Environment Vol. 0.5\rehamovelib\");
        // searchPaths.Add(@"C:\Users\The Lab\Desktop\ETS test environment\ETS Test Environment Vol. 0.5\rehamovelib\_rehamovelib.pyd");
        searchPaths.Add("C:\\Users\\The Lab\\Desktop\\ETS test environment\\ETS Test Environment Vol. 0.5\\Assets\\Plugins\\Lib\\rehamovelib");
        searchPaths.Add("C:\\Users\\The Lab\\AppData\\Local\\Programs\\Python\\Python37\\python37.zip");
        searchPaths.Add("C:\\Users\\The Lab\\AppData\\Local\\Programs\\Python\\Python37\\DLLs");
        searchPaths.Add("C:\\Users\\The Lab\\AppData\\Local\\Programs\\Python\\Python37\\lib");
        searchPaths.Add("C:\\Users\\The Lab\\AppData\\Local\\Programs\\Python\\Python37");
        searchPaths.Add("C:\\Users\\The Lab\\AppData\\Roaming\\Python\\Python37\\site-packages");
        searchPaths.Add("C:\\Users\\The Lab\\AppData\\Local\\Programs\\Python\\Python37\\lib\\site-packages");
        string p = Application.dataPath + "/Plugins/Lib/rehamovelib";
        searchPaths.Add(p);
        Debug.Log(p);
        // searchPaths.Add(Application.dataPath + "\\Plugins\\Lib\\_rehamovelib.pyd");

        searchPaths.Add(@"C:\Users\The Lab\Desktop\ETS test environment\ETS Test Environment Vol. 0.5\Assets\Plugins\Lib\");
        searchPaths.Add(@"C:\Users\The Lab\Desktop\ETS test environment\ETS Test Environment Vol. 0.5\Assets\Plugins\Lib\rehamovelib");
        searchPaths.Add(@"C:\Users\The Lab\AppData\Local\Programs\Python\Python37");
        engine.SetSearchPaths(searchPaths);

        string scriptPath = Application.dataPath + "\\Plugins\\Lib\\rehamovelib\\py_plugin_test.py";
        py = engine.ExecuteFile(scriptPath);
        stimulator = py.stimulator(stimPort, channel);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.LeftArrow)){
            PulseType.text = stimulator.stimulate("left");
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow)){
            PulseType.text = stimulator.stimulate("right");
        }
/*       
        if(Time.frameCount % StimulationInterval == 0)
        {
            relativePosition = Camera.InverseTransformPoint(OdorSourceCube.position);
            if(relativePosition.x<0){
                PulseType.text = stimulator.stimulate("left");
            }
            else if (relativePosition.x>0){
                PulseType.text = stimulator.stimulate("right");
            }
        }
 */
//    }
//}
