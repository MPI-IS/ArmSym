using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;
using Assets.LSL4Unity.Scripts;

public class TrialMasterScript : MonoBehaviour {

    //General variables
    delegate void TrialPhaseMethodDelegate(); // helps scrolling 
    TrialPhaseMethodDelegate TrialPhaseMethod;

    // Pretrial variables
    public GameObject StartingPoint;
    public Text CanvasText;
    public GameObject CanvasRef;
    public Light MyLight;
    public Material MyRobotMaterial, MyFixedRobotMaterial;
    private float initialtimeofcounting;
    private bool iscountingstarted, iscountingfinished;  
    private bool isthreemarked, istwomarked, isonemarked; // Just for canvas drawing.
    public LSLMarkerStream MyMarkerstream; //Handles the messaging for start/stop of a trial 



    // trial variables
    public GameObject CounterBox;
    public float initialtimeoftrial;
    public int standardtrialtime;
    public Text ConsoleCanvasText;
    public int trialscore; // The score of the trial;
    private bool isfirstframe; 
    public datamanagement MyDataManagement;
    private GameObject[] RenderPieces; // We don't want the user to start driving cubes before the start cue. To ensure this, we dissapear the robot 
                                         // before the trial starts. RenderPieces helps us do this. 


    // Post-trial variables
    public bool displayquestionnaire;
    public GameObject FormularCanvas;
    private int intertrialpause; // How many seconds to wait after the trial has finished. 
    private bool isquestionnairestarted, iscanvasdisplayed;
    public GameObject controllermodel, pointerrenderer, experimentmaster;
    private AAExperimentMasterScript ExperimentMasterObject;
    public bool flushlasttrial = false;
    
    private void Start()
    {
        // 0
        ExperimentClockUtils.trialphase = 0; ExperimentClockUtils.shouldWAMmove = false;  TrialPhaseMethod = BeforeTrialPhase;
        iscountingstarted = false;
        iscountingfinished = false;
        initialtimeofcounting = 0;
        isthreemarked = false;
        istwomarked = false;
        isonemarked = false;
        CanvasText.text = "Please locate your right shoulder or your head into the capsule to start the test.";


        // 1
        initialtimeoftrial = 0f;
        trialscore = 0;
        isfirstframe = true; // to check the first frame in the trial
        ExperimentMasterObject = experimentmaster.GetComponent<AAExperimentMasterScript>();
        RenderPieces = ExperimentMasterObject.GetRenderPieces();
        foreach (var renderpiece in RenderPieces) { if (renderpiece.GetComponent<Renderer>()) { renderpiece.GetComponent<Renderer>().enabled = false; } } ; // The pieces of the robot are not displayed until the go cue.

        //2 
        intertrialpause = 3; // seconds
        isquestionnairestarted = false; // Is the questionnaire already started?
        iscanvasdisplayed = false; // is the questionnarie canvas already displayed?
    } 

    /// <summary>
    /// This also represents a state machine. Trialphase is used to denote in which fase of a trial we are running. 0 means we haven't started. In this case,
    /// the user is prompted to go to the area of the box. 
    /// 1 means that the trial is started. The cubes counted with the box and blocks test are monitored.
    /// 2 means that the trial is finished. The total number of cubes is displayed.
    /// </summary>
    void Update () {
        TrialPhaseMethod(); // This will be switch from "Before trial phase" to "during" and then "after" dpeending on the events from the clock that are happening in the code.



    }

    void BeforeTrialPhase() {
        if (StartingPoint.GetComponent<startingpointcollider>().isrobotthere) // If the robot is not there, the component startingpointcollider is on charge.
        {
            // If the robot is there, we fist check if the counting has started.
            if (!iscountingstarted) // If it hasn't, it starts it.
            {
                iscountingstarted = true;
                CanvasText.text = "Starting in..."; // Then it draws it on the canvas.
                initialtimeofcounting = Time.time;
                MyLight.color = Color.yellow; // And finally sets the light to yellow.
                MyRobotMaterial.color = MyFixedRobotMaterial.color; // Solves a bug, by which the robot's colour was red at the beginning of a trial.

                return;


            }

        }

        if (iscountingstarted & !iscountingfinished)
        { // If the counting has started and hasn't ended, we are driven into  a four second counting.
            if (Time.time - initialtimeofcounting > 4) // If four preparation seconds have passed, the user is prompted to start. In this case the canvas is set inactive, and the counting finishes.
            {

                CanvasRef.SetActive(false);
                iscountingfinished = true;
                ExperimentClockUtils.trialphase = 1; ExperimentClockUtils.shouldWAMmove = true;  TrialPhaseMethod = DuringTrialPhase;
                MyMarkerstream.Write("1"); // Sends the marker that the trial has started
                MyLight.color = Color.white; // Light also changes

                return;
            }

            if (Time.time - initialtimeofcounting > 3) // This three conditional loops are completely identical, they draw in the canvas the remaining time.
            {

                if (!isonemarked)
                {
                    isonemarked = true;
                    CanvasText.text = CanvasText.text + "1..";
                    return;
                }
            }
            if (Time.time - initialtimeofcounting > 2)
            {
                if (!istwomarked)
                {
                    istwomarked = true;
                    CanvasText.text = CanvasText.text + "2..";
                    return;
                }
            }

            if (Time.time - initialtimeofcounting > 1)
            {
                if (!isthreemarked)
                {
                    isthreemarked = true;
                    CanvasText.text = CanvasText.text + "3..";
                    
                }

            }
        }

        // If the code reaches this part of the code, it means the trial has already started. 
    }


    void DuringTrialPhase() {
        if (isfirstframe)
        {
            foreach (var renderpiece in RenderPieces) { if (renderpiece.GetComponent<Renderer>()) { renderpiece.GetComponent<Renderer>().enabled = true; } }; // display robot
            initialtimeoftrial = Time.time;
            MyDataManagement.ArmSym_Message("TrialStarted");
            isfirstframe = false;
        }
        ConsoleCanvasText.text = (Time.time - initialtimeoftrial).ToString("f2") + "... cubes:" + CounterBox.GetComponent<countermaster>().objectcounting.ToString();
        if (Time.time - initialtimeoftrial > standardtrialtime)
        {
            trialscore = CounterBox.GetComponent<countermaster>().objectcounting;
            // The next three lines change the trial phase in the experiment clock, prevent the WAM from moving, and  changes the method of the moving delegate. 
            ExperimentClockUtils.trialphase = 2;  ExperimentClockUtils.shouldWAMmove = false; TrialPhaseMethod = AfterTrialPhase; // These three lines update the trial phase.

            MyMarkerstream.Write("2"); // Sends the marker that the trial has ended
            MyLight.color = Color.gray;
            CanvasRef.SetActive(true);
            CanvasText.text = "Score: " + trialscore.ToString();



        }

     



    }

    void AfterTrialPhase()
    {
        if (Time.time - initialtimeoftrial > standardtrialtime + intertrialpause & !isquestionnairestarted) // after a pause, if there is a bool to start a questionnaire, it starts it. Otherwise it doesn't.
        {
            CanvasRef.SetActive(false);
            isquestionnairestarted = true;
            MyDataManagement.ArmSym_Message("TrialFinished");




        }

        if (isquestionnairestarted & !iscanvasdisplayed)
        {
            iscanvasdisplayed = true;
            foreach (var renderpiece in RenderPieces) { if (renderpiece.GetComponent<Renderer>()) { renderpiece.GetComponent<Renderer>().enabled = false; } }; // The pieces of the robot are dissapeared.
            controllermodel.SetActive(true); // the controller is shown
            if (flushlasttrial)
            {
                MyMarkerstream.Write("3"); // Sends the marker that the experiment is finished
            }
            //pointerrenderer.GetComponent<VRTK_ControllerEvents>().enabled=true;




            if (displayquestionnaire)
            { // The questionary is shown if the option is activated.
                pointerrenderer.GetComponent<VRTK_Pointer>().enabled = true;
                CounterBox.gameObject.SetActive(false); // gets rid of this
                pointerrenderer.GetComponent<VRTK_StraightPointerRenderer>().tracerVisibility = VRTK.VRTK_StraightPointerRenderer.VisibilityStates.AlwaysOn;
                pointerrenderer.GetComponent<VRTK_StraightPointerRenderer>().cursorVisibility = VRTK.VRTK_StraightPointerRenderer.VisibilityStates.AlwaysOn;
                pointerrenderer.GetComponent<VRTK_UIPointer>().activationMode = VRTK.VRTK_UIPointer.ActivationMethods.AlwaysOn;
                CanvasRef.SetActive(false);
                FormularCanvas.SetActive(true);
                FormularCanvas.GetComponent<aftertrialquestionnaire>().isquestionnairestarted = true;
            }

            if (!displayquestionnaire)
            {
                ExperimentMasterObject.ReloadScene = true;

            }

        }

    }



}

public static class ExperimentClockUtils{
    public static int trialphase; // trialphase is one int that represent three cases: 0 if trial has not started, 1 if trial is started, and 2 if trial is finished. 
    public static bool shouldWAMmove; // Only true if trialphase==1 or if we are in practicetrial 

}
