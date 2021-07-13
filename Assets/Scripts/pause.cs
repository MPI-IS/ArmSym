using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq; // unity list.average
using System.IO;
using VRTK;


/// <summary>

/// </summary>
/// 

public class pause : MonoBehaviour
{
    public GameObject PauseCanvas, RestartCanvas;
    public Text modifabletext;
    private float timestart; //Time at which the thingy starts
    private float currentime; //The current countdown
    private float minutes, seconds;
    public GameObject ControllerGameObject; //Here We need the game object of the controller we want to attach to the robot
    protected SteamVR_TrackedObject ControllerTackedObject;


    public SteamVR_Controller.Device ControllerDevice //this is what we handle in "update"
    {
        get
        {
            return SteamVR_Controller.Input((int)ControllerTackedObject.index);
        }
    }

    void Awake(){ControllerTackedObject = ControllerGameObject.GetComponent<SteamVR_TrackedObject>();}

    void Start() {
        timestart = Time.time;

        currentime = 120f;

    }

  

    void Update()
    {
        currentime = (120f - (Time.time - timestart));

        if (currentime > 1) {
            minutes = Mathf.Floor(currentime / 60f);
            seconds = Mathf.Floor(currentime-(60*(minutes)));


            if (seconds < 10) { modifabletext.text = minutes.ToString() + ":0" + seconds.ToString(); }
            else { modifabletext.text = minutes.ToString() + ":" + seconds.ToString(); }
            
        
        }

        if (currentime < 0) {
            PauseCanvas.SetActive(false);
            RestartCanvas.SetActive(true);
        }
    }


    public void buttonpressed()
    {
     
        SceneManager.LoadScene("LaboratoryScene");
    }

    
}





