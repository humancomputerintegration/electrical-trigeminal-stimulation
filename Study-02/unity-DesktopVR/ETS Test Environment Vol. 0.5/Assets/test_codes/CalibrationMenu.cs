using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CalibrationMenu : MonoBehaviour
{
    

    public void ConfirmCalibration(){
        SceneManager.LoadScene("ETS Enviroment");
    }
}
