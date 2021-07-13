using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.LSL4Unity.Scripts;


public class PracticeMasterScript : MonoBehaviour {
    // This script runs only when the subject is in a free practice trial 
    public GameObject StartingPoint; // The Capsule from the start is going to be used as a "transport" device
    public GameObject Box; // All the elements from the box will be dissapeared 
    public GameObject PracticeTable; // This will be appeared
    public Material CapsuleYellow, CapsuleGreen;
    private Vector3 InitialCapsuleposition, ModifiedCapsulePosition;
    public Text UIText;
    public AAExperimentMasterScript ExperimentMaster;
    public LSLMarkerStream MyMarkerstream; //Handles the messaging for start/stop of a trial 
    private GameObject[] RenderPieces;
    private bool SendStartmarker;
    private float TimeStart;
    

    public datamanagement MyDataManagement;


    // Use this for initialization
    void Start() {
        Box.SetActive(false);
        PracticeTable.SetActive(true);
        StartingPoint.GetComponent<Renderer>().material = CapsuleGreen;
        InitialCapsuleposition = StartingPoint.transform.localPosition;
        ModifiedCapsulePosition.y = InitialCapsuleposition.y;
        ModifiedCapsulePosition.z = InitialCapsuleposition.z ;
        ModifiedCapsulePosition.x = InitialCapsuleposition.x - (float)0.8;
        StartingPoint.transform.localPosition = ModifiedCapsulePosition;
        UIText.text = "Feel free to practice and locate your head or your shoulder over the green capsule when you are ready.";
        SendStartmarker = false;
        TimeStart = Time.time;
        ExperimentClockUtils.shouldWAMmove = true;
        RenderPieces = ExperimentMaster.GetRenderPieces(); 
        foreach (var renderpiece in RenderPieces) { if (renderpiece.GetComponent<Renderer>()) { renderpiece.GetComponent<Renderer>().enabled =true; } }; // we show render pieces


    }

    // Update is called once per frame
    void Update ()
    {

        if (!SendStartmarker & (Time.time-TimeStart) > 2) // This time of 2 seconds seems to be necessary for the system to start correctly. 
        {
            MyMarkerstream.Write("0"); // Experiment started

            MyMarkerstream.Write("1"); // First trial started!!
            MyDataManagement.ArmSym_Message("Practice trial started");

            SendStartmarker = true;

        }


        if (StartingPoint.GetComponent<startingpointcollider>().isrobotthere) {
            MyDataManagement.ArmSym_Message("Practice trial ended");

            MyMarkerstream.Write("2"); // First trial ended!!
            ExperimentMaster.ReloadScene = true;

        }


    }
}
